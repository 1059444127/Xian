﻿#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Security.Principal;
using System.Threading;
using ClearCanvas.Enterprise.Common;
using ClearCanvas.Web.Common;
using ClearCanvas.Common;
using ClearCanvas.Web.Common.Events;
using ClearCanvas.Common.Utilities;

namespace ClearCanvas.Web.Services
{
	internal static class SR
	{
		public const string MessageAccessDenied = "You are not authorized to view images.";
		public const string MessagePasswordExpired = "Your password has expired.";
		public const string MessageSessionEnded  = "Your session has ended.";
		public const string MessageUnexpectedError = "An unexpected error has occurred on the server.";
        public const string MessageNoCommunicationFromClientError = "No communication has been received from the client since {0}";
	}

	[ExtensionOf(typeof(ExceptionTranslatorExtensionPoint))]
	internal class UserSessionExceptionTranslator : IExceptionTranslator
	{
		#region IExceptionTranslator Members

		public string Translate(Exception e)
		{
			//TODO (CR May 2010): string resources
			if (e.GetType().Equals(typeof(UserAccessDeniedException)))
				return SR.MessageAccessDenied;
			if (e.GetType().Equals(typeof(PasswordExpiredException)))
				return SR.MessagePasswordExpired;
			if (e.GetType().Equals(typeof(InvalidUserSessionException)))
				return SR.MessageSessionEnded;
			if (e.GetType().Equals(typeof(RequestValidationException)))
				return SR.MessageUnexpectedError;

			return default(string);
		}

		#endregion
	}

	public class ApplicationAttribute : Attribute
	{
		public ApplicationAttribute(Type startRequestType)
		{
			StartRequestType = startRequestType;
		}

		public Type StartRequestType { get; private set; }

		/// <summary>
		/// Determines whether or not this instance is the same as <paramref name="obj"/>, which is itself an <see cref="Attribute"/>.
		/// </summary>
		public override bool Match(object obj)
		{
			var attribute = obj as ApplicationAttribute;
			return attribute != null && attribute.StartRequestType == StartRequestType;
		}
	}

	[ExtensionPoint]
	public class ApplicationExtensionPoint : ExtensionPoint<IApplication>
	{
	}

	public interface IApplication
	{
		Guid Identifier { get; }
		IPrincipal Principal { get; }
		IApplicationContext Context { get; }

		void Start(StartApplicationRequest request);
        ProcessMessagesResult ProcessMessages(MessageSet messages);
		void Stop();
		void Stop(string message);
	    EventSet GetPendingOutboundEvent(int wait);

	    void SetProperty(string key, object value);

        void Shutdown();
	}

    public enum MessageBatchMode
    {
        PerType,
        PerTarget
    }

	public abstract partial class Application : IApplication
	{
		[ThreadStatic]
		private static Application _current;

		protected ApplicationContext _context;

		private string _userName;
		private volatile SessionToken _sessionToken;
		private const int _sessionRenewalOffsetMinutes = 1; // renew the session 1 min before it is expired.
		private TimeSpan _sessionPollingIntervalSeconds;
		private volatile int _lastSessionCheckTicks;

		private TimeSpan? _inactivityTimeoutMinutes;

        private volatile int _lastActivityTicks = Environment.TickCount;
        private DateTime _lastGetPendingTimestamp = DateTime.Now;
		private volatile bool _timedOut;

		private readonly object _syncLock = new object();
		private bool _stop;
		private event EventHandler _stopped;
		internal WebSynchronizationContext _synchronizationContext;

		private readonly IncomingMessageQueue _incomingMessageQueue;

		private static readonly TimeSpan _timerInterval = TimeSpan.FromSeconds(5);
		private System.Threading.Timer _timer;
		private bool _timerMethodExecuting;

		protected Application()
		{
		    Identifier = Guid.NewGuid();
			_incomingMessageQueue = new IncomingMessageQueue(
				messageSet => _synchronizationContext.Send(nothing => DoProcessMessages(messageSet), null));
		}

		internal static Application Current
		{
			get { return _current; }
			set
			{
				_current = value;
				if (_current != null && _current.Principal != null)
					Thread.CurrentPrincipal = _current.Principal;
			}
		}

		public Guid Identifier { get; private set; }

		public IApplicationContext Context
		{
			get { return _context; }
		}

		#region Authentication

		public IPrincipal Principal { get; private set; }

		private bool IsSessionShared { get; set; }

        public MessageBatchMode BatchMode { get; protected set; }

	    private void AuthenticateUser(StartApplicationRequest request)
		{
			_userName = request.Username;
			if (!String.IsNullOrEmpty(request.SessionId))
			{
				IsSessionShared = request.IsSessionShared;
				//Have to do this so we can log out right away when the session is not shared.
				_sessionToken = new SessionToken(request.SessionId);
			}

			UserSessionInfo sessionInfo = UserAuthentication.ValidateSession(request.Username, request.SessionId);
			if (sessionInfo == null)
				return;

			if (sessionInfo.Principal != null)
				Thread.CurrentPrincipal = Principal = sessionInfo.Principal;

			_sessionToken = sessionInfo.SessionToken;
		}

		private void Logout()
		{
			if (IsSessionShared || _sessionToken == null)
				return;

			UserAuthentication.Logout(_userName, _sessionToken);
		}

		protected void EnsureSessionIsValid()
		{
			if (_sessionToken == null)
				return;

            bool nearExpiry = Platform.Time.Add(TimeSpan.FromMinutes(_sessionRenewalOffsetMinutes)) > _sessionToken.ExpiryTime;
			TimeSpan timeSinceLastCheck = TimeSpan.FromMilliseconds(Environment.TickCount - _lastSessionCheckTicks);
			if (nearExpiry || timeSinceLastCheck > _sessionPollingIntervalSeconds)
			{
				_lastSessionCheckTicks = Environment.TickCount;
				_sessionToken = UserAuthentication.RenewSession(_userName, _sessionToken);

				OnSessionRenewed();
			}
		}

		private void OnSessionRenewed()
		{
			if (_sessionToken == null)
				return;

			_context.FireEvent(new SessionUpdatedEvent
			{
				Identifier = Guid.NewGuid(),
				SenderId = Identifier,
				ExpiryTimeUtc = _sessionToken.ExpiryTime.ToUniversalTime(),
				Username = _userName
			});
		}

		#endregion

		#region IApplication Members

		void IApplication.Start(StartApplicationRequest request)
		{
			throw new InvalidOperationException("Start must be called internally.");
		}

		internal void InternalStart(StartApplicationRequest request)
		{
            try
			{
				//TODO: do this here (which will fault the channel), or inside DoStart and stop the app?
				AuthenticateUser(request);
				_synchronizationContext = new WebSynchronizationContext(this);
				_synchronizationContext.Send(nothing => DoStart(request), null);
			}
			catch (Exception)
			{
				Logout();

				// need to dispose _context.				
				// DisposeMembers(); // Commented out for non-duplex binding since app is ow managed by the cache
                
				throw;
			}

			lock (_syncLock)
			{
				//DoStart can call Stop.
				if (_stop)
					return;
			}

			_timer = new System.Threading.Timer(OnTimer, null, _timerInterval, _timerInterval);
		}

		private void DoStart(StartApplicationRequest request)
		{
            //_context.SuspendEvents = true;

			try
			{
				_context.FireEvent(new ApplicationStartedEvent
				{
					Identifier = Guid.NewGuid(),
					SenderId = Identifier,
					StartRequestId = request.Identifier
				});

				OnStart(request);

				string logMessage;
				if (_sessionToken == null)
					logMessage = String.Format("Application {0} has started.", Identifier);
				else
					logMessage = String.Format("Application {0} has started (user={1}, session={2}, expiry={3}).", Identifier, _userName, _sessionToken.Id, _sessionToken.ExpiryTime);
				
				ConsoleHelper.Log(LogLevel.Info, ConsoleColor.Green, logMessage);

				OnSessionRenewed();
			}
			finally
			{
				//_context.SuspendEvents = false;
			}
		}

		public void Stop()
		{
			Stop("");
		}

        public void Shutdown()
        {
            Stop();
            DisposeMembers();
            //TODO: Remove from cache immediately?
        }

	    public void Stop(string message)
		{
			lock (_syncLock)
			{
				if (_stop)
					return;

				_stop = true;
			}

			_synchronizationContext.Send(ignore => DoStop(message), null);

			try
			{
				lock (_syncLock)
				{
					EventsHelper.Fire(_stopped, this, EventArgs.Empty);
				}
			}
			catch (Exception e)
			{
				Platform.Log(LogLevel.Debug, e);
			}

			Logout();

			// DisposeMembers(); // Commented out for non-duplex binding since app is ow managed by the cache
		}
        
	    public void SetProperty(string key, object value)
	    {
	        Platform.Log(LogLevel.Debug, "Setting {0}={1}", key, value);
	        _context.SetProperty(key, value);
	    }

	    internal void Stop(Exception e)
		{
            Platform.Log(LogLevel.Error, e);
			Stop(ExceptionTranslator.Translate(e));
		}

		private void DoStop(string message)
		{
			try
			{
				OnStop();
			}
			catch (Exception e)
			{
				Platform.Log(LogLevel.Debug, e);
			}
			finally
			{
				_context.FireEvent(new ApplicationStoppedEvent
				{
					Identifier = Guid.NewGuid(),
					SenderId = Identifier,
					Message = message,
                    IsTimedOut = _timedOut
				});

				//TODO (CR May 2010): just use the ConsoleHelper's formatting
				string logMessage;
				string stopMessage = String.IsNullOrEmpty(message) ? "<none>" : message;
				if (_sessionToken == null)
					logMessage = String.Format("Application {0} has stopped (message={1}).", Identifier, stopMessage);
				else
					logMessage = String.Format("Application {0} has stopped (user={1}, session={2}, message={3}).",
						Identifier, _userName, _sessionToken.Id, stopMessage);

				ConsoleHelper.Log(LogLevel.Info, ConsoleColor.Red, logMessage);
			}
		}

		private void OnTimer(object nothing)
		{
			try
			{
				lock (_syncLock)
				{
					if (_stop || _timerMethodExecuting)
						return;

					_timerMethodExecuting = true;
				}

				EnsureSessionIsValid();

				if (!_inactivityTimeoutMinutes.HasValue)
					return;

                // TODO: Review this
                // Per MSDN: Environment.TickCount wraps to Int32.MinValue (negative)
                TimeSpan timeSinceLastActivity = TimeSpan.FromMilliseconds(Environment.TickCount - _lastActivityTicks);
				if (timeSinceLastActivity > _inactivityTimeoutMinutes)
				{
                    _timedOut = true;
				    Stop(SR.MessageSessionEnded);
				}
                else
				{
                    // TODO: REVIEW THIS
                    // This is a temporary workaround to ensure the app shuts down when the connection is lost.
                    // Although the app will eventually timed out after 11 min (_inactivityTimeoutMinutes),
                    // for security reason it's better to detect this situation asap. 
                    // Without a permanent connection, we now rely on the timeout and GetPendingEvent 
                    // to guess if the client is still alive.
                    //
                    // Assumption: The client is supposed to send a GetPendingEvent request repeatedly while
                    // it is idle. Assume max waiting time for GetPendingEvent is 10 seconds,
                    // we can assume the client browser is closed or connection is lost if we don't receive 
                    // one for 20 seconds
                    if (DateTime.Now - _lastGetPendingTimestamp > TimeSpan.FromSeconds(20))
				    {
                        Stop(String.Format(SR.MessageNoCommunicationFromClientError, _lastGetPendingTimestamp));
				    }
				}

			}
			catch (Exception e)
			{
				Stop(e);
			}
			finally
			{
				lock (_syncLock)
				{
					_timerMethodExecuting = false;
				}
			}
		}


		private void DisposeMembers()
		{
            //NOTE: For non-duplex binding, this method should be called when the client has received all the pending messages.

			//NOTE: This class has purposely been designed such that this method (and these members) do not need to be synchronized.
            //If you were to synchronize this method, it could deadlock with the other objects that can call
			//back into this class.

			try
			{
				if (_timer != null)
				{
					_timer.Dispose();
					_timer = null;
				}

				if (_synchronizationContext != null)
				{
					_synchronizationContext.Dispose();
					_synchronizationContext = null;
				}

				if (_context == null)
					return;

				_context.Dispose();
				_context = null;

			}
			catch (Exception e)
			{
				Platform.Log(LogLevel.Debug, e);
			}
		}

		internal event EventHandler Stopped
		{
			add { lock (_syncLock) { _stopped += value; } }
			remove { lock (_syncLock) { _stopped -= value; } }
		}

		ProcessMessagesResult IApplication.ProcessMessages(MessageSet messageSet)
		{
			lock (_syncLock)
			{
				if (_stop)
					return null;
			}

            //TODO: do this here (which will fault the channel), or inside DoProcessMessages and stop the app?
			EnsureSessionIsValid();

			bool processed = _incomingMessageQueue.ProcessMessageSet(messageSet);

		    return OnProcessMessageEnd(messageSet, processed);
		}


	    private void DoProcessMessages(MessageSet messageSet)
		{
            _lastActivityTicks = Environment.TickCount;
            foreach (var message in messageSet.Messages)
                ProcessMessage(message);
		}

        protected abstract ProcessMessagesResult OnProcessMessageEnd(MessageSet messageSet, bool processed);
		protected abstract void OnStart(StartApplicationRequest request);
		protected abstract void OnStop();
	    protected abstract EventSet OnGetPendingOutboundEvent(int wait);
        

        public EventSet GetPendingOutboundEvent(int wait)
        {
            _lastGetPendingTimestamp = DateTime.Now;
            EventSet result = OnGetPendingOutboundEvent(wait);
            return result;
        }

		protected virtual void ProcessMessage(Message message)
		{
			IEntityHandler handler = Context.EntityHandlers[message.TargetId];
			if (handler != null)
			{
				handler.ProcessMessage(message);
			}
			else
			{
				string msg = String.Format("Invalid message target: {0}", message.TargetId);
				Platform.Log(LogLevel.Debug, msg);
				//throw new ArgumentException(msg);
			}
		}

		protected abstract Common.Application GetContractObject();

		#endregion

		#region Static Helpers

		internal static Application Start(StartApplicationRequest request)
		{
			var filter = new AttributeExtensionFilter(new ApplicationAttribute(request.GetType()));
			var app = new ApplicationExtensionPoint().CreateExtension(filter) as IApplication;
			if (app == null)
				throw new NotSupportedException(String.Format("No application extension exists for start request type {0}", request.GetType().FullName));

			if (!(app is Application))
				throw new NotSupportedException("Application class must derive from Application base class.");

			#region Setup

			var application = (Application)app;
			var context = new ApplicationContext(application);
			application._context = context;

			application._sessionPollingIntervalSeconds = TimeSpan.FromSeconds(ApplicationServiceSettings.Default.SessionPollingIntervalSeconds);

			if (ApplicationServiceSettings.Default.InactivityTimeoutMinutes > 0)
				application._inactivityTimeoutMinutes = TimeSpan.FromMinutes(ApplicationServiceSettings.Default.InactivityTimeoutMinutes);

			#endregion

			//NOTE: must call start before adding to the cache; we want the app to be fully initialized before it can be accessed from outside this method.
            
            application.InternalStart(request);
			
            Cache.Instance.Add(application);

			return application;
		}

		public static Application Find(Guid identifier)
		{
			return Cache.Instance.Find(identifier);
		}

		public static void StopAll(string message)
		{
		    Cache.Instance.StopAndClearAll(message);
		}

		#endregion
	}
}