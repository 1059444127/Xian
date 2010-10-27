﻿#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Collections.Generic;
using ClearCanvas.Common;

namespace ClearCanvas.Desktop
{
	/// <summary>
	/// Provides a simple mechanism for application components to execute code asynchronously.
	/// </summary>
	public static class Async
	{
		#region ComponentTaskManager class

		class ComponentTaskManager : IDisposable
		{
			private readonly IApplicationComponent _component;
			private readonly List<AsyncTask> _asyncTasks;

			public ComponentTaskManager(IApplicationComponent component)
			{
				_component = component;
				_component.Stopped += ComponentStoppedEventHandler;
				_asyncTasks = new List<AsyncTask>();
			}

			public void Invoke(
				AsyncTask.Action asyncCode,
				AsyncTask.Action successHandler,
				Action<Exception> errorHandler)
			{
				var task = new AsyncTask();
				_asyncTasks.Add(task);

				task.Run(asyncCode,
					delegate
					{
						_asyncTasks.Remove(task);
						task.Dispose();
						successHandler();
					},
					delegate(Exception e)
					{
						_asyncTasks.Remove(task);
						task.Dispose();
						errorHandler(e);
					});
			}

			public void Request<TServiceContract, TResponse>(
				Converter<TServiceContract, TResponse> asyncCode,
				Action<TResponse> successHandler,
				Action<Exception> errorHandler)
			{
				var response = default(TResponse);
				Invoke(
					delegate
					{
						Platform.GetService<TServiceContract>(
							service => response = asyncCode(service));
					},
					delegate
					{
						successHandler(response);
					},
					errorHandler);
			}

			public void CancelPending()
			{
				foreach (var task in _asyncTasks)
				{
					task.Dispose();
				}
				_asyncTasks.Clear();
			}

			public void Dispose()
			{
				CancelPending();
			}

			private void ComponentStoppedEventHandler(object source, EventArgs args)
			{
				CancelPending();
			}
		}

		#endregion

		private static readonly Dictionary<IApplicationComponent, ComponentTaskManager> _componentTaskManagers
			= new Dictionary<IApplicationComponent, ComponentTaskManager>();

		#region Public API

		/// <summary>
		/// Invokes an arbitrary block of code asynchronously, executing a continuation upon completion or error handler upon failure.
		/// </summary>
		/// <remarks>
		/// The invocation is tied to the lifetime of the specified application component.  That is, if the component is stopped, any
		/// asynchronous invocations pending completion will be discarded.
		/// </remarks>
		/// <param name="component"></param>
		/// <param name="asyncCode"></param>
		/// <param name="continuationCode"></param>
		/// <param name="errorHandler"></param>
		public static void Invoke(
			IApplicationComponent component,
			AsyncTask.Action asyncCode,
			AsyncTask.Action continuationCode,
			Action<Exception> errorHandler)
		{
			var ctm = GetComponentTaskManager(component);

			ctm.Invoke(asyncCode, continuationCode, errorHandler);
		}

		/// <summary>
		/// Invokes an arbitrary block of code asynchronously, executing a continuation upon completion.
		/// </summary>
		/// <remarks>
		/// The invocation is tied to the lifetime of the specified application component.  That is, if the component is stopped, any
		/// asynchronous invocations pending completion will be discarded.
		/// </remarks>
		/// <param name="component"></param>
		/// <param name="asyncCode"></param>
		/// <param name="continuationCode"></param>
		public static void Invoke(
			IApplicationComponent component,
			AsyncTask.Action asyncCode,
			AsyncTask.Action continuationCode)
		{
			var ctm = GetComponentTaskManager(component);

			ctm.Invoke(asyncCode, continuationCode, AsyncTask.DefaultErrorHandler);
		}

		/// <summary>
		/// Makes an asynchronous request, executing a continuation upon completion or error handler upon failure.
		/// </summary>
		/// <remarks>
		/// The request is tied to the lifetime of the specified application component.  That is, if the component is stopped, any
		/// asynchronous requests pending completion will be discarded.
		/// </remarks>
		/// <typeparam name="TServiceContract"></typeparam>
		/// <typeparam name="TResponse"></typeparam>
		/// <param name="component"></param>
		/// <param name="asyncCode"></param>
		/// <param name="continuationCode"></param>
		/// <param name="errorHandler"></param>
		public static void Request<TServiceContract, TResponse>(
			IApplicationComponent component,
			Converter<TServiceContract, TResponse> asyncCode,
			Action<TResponse> continuationCode,
			Action<Exception> errorHandler)
		{
			var ctm = GetComponentTaskManager(component);

			ctm.Request(asyncCode, continuationCode, errorHandler);
		}

		/// <summary>
		/// Makes an asynchronous request, executing a continuation upon completion.
		/// </summary>
		/// <remarks>
		/// The request is tied to the lifetime of the specified application component.  That is, if the component is stopped, any
		/// asynchronous requests pending completion will be discarded.
		/// </remarks>
		/// <typeparam name="TServiceContract"></typeparam>
		/// <typeparam name="TResponse"></typeparam>
		/// <param name="component"></param>
		/// <param name="asyncCode"></param>
		/// <param name="continuationCode"></param>
		public static void Request<TServiceContract, TResponse>(
			IApplicationComponent component,
			Converter<TServiceContract, TResponse> asyncCode,
			Action<TResponse> continuationCode)
		{
			var ctm = GetComponentTaskManager(component);

			ctm.Request(asyncCode, continuationCode, AsyncTask.DefaultErrorHandler);
		}

		/// <summary>
		/// Cancels any pending invocations or requests made by the specified application component.
		/// </summary>
		/// <param name="component"></param>
		public static void CancelPending(IApplicationComponent component)
		{
			ComponentTaskManager ctm;
			if (_componentTaskManagers.TryGetValue(component, out ctm))
			{
				ctm.CancelPending();
			}
		}

		#endregion

		#region Helpers

		private static ComponentTaskManager GetComponentTaskManager(IApplicationComponent component)
		{
			ComponentTaskManager taskManager;
			if (!_componentTaskManagers.TryGetValue(component, out taskManager))
			{
				taskManager = new ComponentTaskManager(component);
				_componentTaskManagers.Add(component, taskManager);
				component.Stopped += ((source, args) => _componentTaskManagers.Remove(component));
			}
			return taskManager;
		}

		#endregion
	}
}
