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
using System.Management;
using System.Net;
using System.Security.Principal;
using System.ServiceModel;
using System.Threading;
using ClearCanvas.Common;
using ClearCanvas.Common.Utilities;
using ClearCanvas.Enterprise.Common;
using ClearCanvas.Enterprise.Common.Authentication;
using ClearCanvas.Ris.Application.Common;
using ClearCanvas.Ris.Application.Common.Login;
using ChangePasswordRequest=ClearCanvas.Ris.Application.Common.Login.ChangePasswordRequest;

namespace ClearCanvas.Ris.Client
{
	/// <summary>
	/// Holds information related to the current login session.
	/// </summary>
	public sealed class LoginSession
	{
		private static LoginSession _current;
		private static bool _risServerDown;

		/// <summary>
		/// Gets the current <see cref="LoginSession"/>.
		/// </summary>
		public static LoginSession Current
		{
			get { return _current; }
		}

		public static void Create(string userName, string password)
		{
			Create(userName, password, null, false);
		}

		/// <summary>
		/// Creates a new <see cref="LoginSession"/>.
		/// </summary>
		/// <remarks>
		/// Contacts the server and requests login using the specified credentials.  An exception will be thrown
		/// if the credentials are not valid.
		/// </remarks>
		/// <param name="userName"></param>
		/// <param name="password"></param>
		/// <param name="facility"></param>
		/// <param name="risServerDown"></param>
		public static void Create(string userName, string password, FacilitySummary facility, bool risServerDown)
		{
			try
			{
				Platform.Log(LogLevel.Debug, "Attempting login...");

				if (risServerDown)
				{
					Platform.GetService(
						delegate(IAuthenticationService service)
						{
							var request = new InitiateSessionRequest(userName, GetMachineID(), Dns.GetHostName(), password) {GetAuthorizations = true};
							InitiateSessionResponse response = service.InitiateSession(request);

							if (response.SessionToken == null)
								throw new Exception("Invalid session token returned from authentication service.");

							var nonRisAuthorityTokens = StripRisTokens(response.AuthorityTokens);

							Thread.CurrentPrincipal = DefaultPrincipal.CreatePrincipal(
								new GenericIdentity(userName),
								response.SessionToken,
								nonRisAuthorityTokens);

							// set the current session before attempting to access other services, as these will require authentication
							_current = new LoginSession(userName, response.SessionToken, null, null);
						});
				}
				else
				{
					Platform.GetService(
						delegate(ILoginService service)
							{
								LoginResponse response = service.Login(
									new LoginRequest(
										userName,
										password,
										Dns.GetHostName(),
										GetIPAddress(),
										GetMachineID()));

								// if the call succeeded, set a default principal object on this thread, containing
								// the set of authority tokens for this user
								Thread.CurrentPrincipal = DefaultPrincipal.CreatePrincipal(
									new GenericIdentity(userName),
									response.SessionToken,
									response.UserAuthorityTokens);

								// set the current session before attempting to access other services, as these will require authentication
								_current = new LoginSession(userName, response.SessionToken, response.StaffSummary, facility);
							});
				}

				// Login session created successfully.  Remembers it.
				_risServerDown = risServerDown;

				Platform.Log(LogLevel.Debug, "Login attempt was successful.");
			}
			catch (FaultException<UserAccessDeniedException> e)
			{
				Platform.Log(LogLevel.Debug, e.Detail, "Login attempt failed.");
				throw e.Detail;
			}
			catch (FaultException<PasswordExpiredException> e)
			{
				Platform.Log(LogLevel.Debug, e.Detail, "Login attempt failed.");
				throw e.Detail;
			}
		}

		internal static void ChangePassword(string userName, string oldPassword, string newPassword)
		{
			try
			{
				if (_risServerDown)
				{
					Platform.GetService(
						delegate(IAuthenticationService service)
						{
							var request = new Enterprise.Common.Authentication.ChangePasswordRequest(userName, oldPassword, newPassword);
							service.ChangePassword(request);
						});
				}
				else
				{
					Platform.GetService(
						delegate(ILoginService service)
						{
							service.ChangePassword(new ChangePasswordRequest(userName, oldPassword, newPassword, GetIPAddress(), GetMachineID()));
						});
				}
			}
			catch (FaultException<UserAccessDeniedException> e)
			{
				throw e.Detail;
			}
			catch (FaultException<RequestValidationException> e)
			{
				throw e.Detail;
			}
		}

		private readonly string _userName;
		private readonly SessionToken _sessionToken;
		private readonly StaffSummary _staff;
		private readonly FacilitySummary _workingFacility;

		private LoginSession(string userName, SessionToken sessionToken, StaffSummary staff, FacilitySummary workingFacility)
		{
			_userName = userName;
			_sessionToken = sessionToken;
			_staff = staff;
			_workingFacility = workingFacility;
		}

		/// <summary>
		/// Terminates the current login session, setting the <see cref="Current"/> property to null.
		/// </summary>
		public void Terminate()
		{
			try
			{
				if (_risServerDown)
				{
					Platform.GetService(
						delegate(IAuthenticationService service)
						{
							var request = new TerminateSessionRequest(_userName, _sessionToken);
							service.TerminateSession(request);
						});
				}
				else
				{
					Platform.GetService(
						delegate(ILoginService service)
						{
							service.Logout(new LogoutRequest(_userName, _sessionToken, GetIPAddress(), GetMachineID()));
						});
				}
			}
			finally
			{
				_current = null;
			}
		}

		/// <summary>
		/// Gets the user name of the logged on user.
		/// </summary>
		public string UserName
		{
			get { return _userName; }
		}

		/// <summary>
		/// Gets the full person name of the logged on user.
		/// </summary>
		public PersonNameDetail FullName
		{
			get { return _staff == null ? null : _staff.Name; }
		}

		/// <summary>
		/// Gets the <see cref="StaffSummary"/> of the logged on user.
		/// </summary>
		public StaffSummary Staff
		{
			get { return _staff; }
		}

		/// <summary>
		/// Gets if the user is associated with a RIS staff person.
		/// </summary>
		public bool IsStaff
		{
			get { return _staff != null; }
		}

		/// <summary>
		/// Gets the current working facility.
		/// </summary>
		public FacilitySummary WorkingFacility
		{
			get { return _workingFacility; }
		}

		/// <summary>
		/// Gets the session token.  This property is internal in order to limit exposure of the session
		/// token.
		/// </summary>
		internal string SessionToken
		{
			get { return _sessionToken.Id; }
		}

		/// <summary>
		/// Utility method to get the local IP address to report to the server.
		/// </summary>
		/// <returns></returns>
		private static string GetIPAddress()
		{
			string hostName = Dns.GetHostName();
			IPAddress[] addresses = Dns.GetHostAddresses(hostName);

			// just use the first address
			// we don't care very much because this is just for auditing purposes, 
			// it serves no technical purpose
			return addresses.Length > 0 ? addresses[0].ToString() : null;
		}

		private static string GetMachineID()
		{
			try
			{
				// Use the serial number of the mother board
				string id = null;
				ManagementClass mc = new ManagementClass("Win32_Baseboard");
				ManagementObjectCollection moc = mc.GetInstances();
				foreach (ManagementObject mo in moc)
				{
					id = mo.Properties["SerialNumber"].Value.ToString().Trim();
					if (!string.IsNullOrEmpty(id))
						break;
				}

				return id;
			}
			catch (Exception e)
			{
				Platform.Log(LogLevel.Warn, e);
				return null;
			}
		}

		private static string[] StripRisTokens(IEnumerable<string> authorityTokens)
		{
			var nonRisTokens = CollectionUtils.Select(authorityTokens, token => token.StartsWith("RIS") == false);
			nonRisTokens.Remove(AuthorityTokens.Desktop.FolderOrganization);
			return nonRisTokens.ToArray();
		}
	}
}
