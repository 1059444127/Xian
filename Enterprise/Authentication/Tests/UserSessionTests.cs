#if UNIT_TESTS

using System;
using System.Collections.Generic;
using System.Text;
using ClearCanvas.Common;
using ClearCanvas.Common.Utilities;
using ClearCanvas.Enterprise.Common;
using NUnit.Framework;
using System.Threading;

namespace ClearCanvas.Enterprise.Authentication.Tests
{
	[TestFixture]
	public class UserSessionTests
	{
    	private const string DefaultPassword = "password";
		private const string AlternatePassword = "clearcanvas";
		private const string UserName = "foo";
		private const string DisplayName = "Mr. Foo";

		public UserSessionTests()
        {
            // set the extension factory to special test factory
            Platform.SetExtensionFactory(new NullExtensionFactory());
        }

		[Test]
		public void Test_GetToken()
		{
			TimeSpan timeout = TimeSpan.FromMinutes(10);
			UserSession session = CreateSession(timeout);
			Assert.IsNotNull(session);
			Assert.IsNotNull(session.SessionId);
			Assert.IsTrue(RoughlyEqual(DateTime.Now + timeout, session.ExpiryTime));

			SessionToken token = session.GetToken();
			Assert.IsNotNull(token);

			Assert.AreEqual(session.SessionId, token.Id);
			Assert.AreEqual(session.ExpiryTime, token.ExpiryTime);
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void Test_GetToken_Terminated()
		{
			TimeSpan timeout = TimeSpan.FromMinutes(10);
			UserSession session = CreateSession(timeout);

			session.Terminate();
			session.GetToken();
		}

		[Test]
		public void Test_Validate_Normal_NoCheckExpiry()
		{
			TimeSpan timeout = TimeSpan.FromMinutes(10);
			UserSession session = CreateSession(timeout);

			session.Validate(UserName, false);
		}

		[Test]
		public void Test_Validate_Normal_CheckExpiry()
		{
			TimeSpan timeout = TimeSpan.FromMinutes(10);
			UserSession session = CreateSession(timeout);

			session.Validate(UserName, true);
		}

		[Test]
		[ExpectedException(typeof(Exception))]
		public void Test_Validate_InvalidUserName()
		{
			TimeSpan timeout = TimeSpan.FromMinutes(10);
			UserSession session = CreateSession(timeout);

			session.Validate(UserName, true);
		}

		[Test]
		[ExpectedException(typeof(Exception))]
		public void Test_Validate_InactiveUser()
		{
			TimeSpan timeout = TimeSpan.FromMinutes(10);
			UserSession session = CreateSession(timeout);

			session.Validate(UserName, true);
		}

		[Test]
		[ExpectedException(typeof(Exception))]
		public void Test_Validate_ExpiredSession()
		{
			TimeSpan timeout = TimeSpan.FromMinutes(10);
			UserSession session = CreateSession(timeout);

			session.Validate(UserName, true);
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void Test_Validate_Terminated()
		{
			TimeSpan timeout = TimeSpan.FromMinutes(10);
			UserSession session = CreateSession(timeout);

			session.Terminate();
			session.Validate(UserName, true);
		}

		[Test]
		public void Test_Terminate()
		{
			TimeSpan timeout = TimeSpan.FromMinutes(10);
			UserSession session = CreateSession(timeout);
			User user = session.User;

			// initally the sessions collection contains the session
			Assert.IsTrue(user.Sessions.Contains(session));

			// terminate the session
			session.Terminate();

			Assert.IsFalse(user.Sessions.Contains(session));
			Assert.IsNull(session.User);
			Assert.IsTrue(session.IsTerminated);
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void Test_Terminate_Terminated()
		{
			TimeSpan timeout = TimeSpan.FromMinutes(10);
			UserSession session = CreateSession(timeout);

			session.Terminate();

			// attempting to terminate again should throw
			session.Terminate();
		}

		[Test]
		public void Test_IsExpired()
		{
			TimeSpan timeout = TimeSpan.FromMilliseconds(100); // expires in 100 msec
			UserSession session = CreateSession(timeout);

			Assert.IsFalse(session.IsExpired);
			Thread.Sleep(150);	// sleep slightly more than 100 msec, to be sure
			Assert.IsTrue(session.IsExpired);
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void Test_IsExpired_Terminated()
		{
			TimeSpan timeout = TimeSpan.FromMinutes(10);
			UserSession session = CreateSession(timeout);
			session.Terminate();

			bool expired = session.IsExpired;
		}

		[Test]
		public void Test_Renew()
		{
			TimeSpan timeout = TimeSpan.FromMinutes(10);
			UserSession session = CreateSession(timeout);

			// renew for a longer period
			TimeSpan renewalPeriod = TimeSpan.FromMinutes(20);
			session.Renew(renewalPeriod);
			Assert.IsTrue(RoughlyEqual(DateTime.Now + renewalPeriod, session.ExpiryTime));

			// renew for a shorter period - should this throw an exception??
			// (does it make sense to be able to renew for a shorter period??)
			renewalPeriod = TimeSpan.FromMinutes(1);
			session.Renew(renewalPeriod);
			Assert.IsTrue(RoughlyEqual(DateTime.Now + renewalPeriod, session.ExpiryTime));
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void Test_Renew_Expired()
		{
			TimeSpan timeout = TimeSpan.FromMilliseconds(100); // expires in 100 msec
			UserSession session = CreateSession(timeout);

			Thread.Sleep(150);	// sleep a bit longer than 100 msec

			session.Renew(timeout);
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void Test_Renew_Terminated()
		{
			TimeSpan timeout = TimeSpan.FromMinutes(10);
			UserSession session = CreateSession(timeout);
			session.Terminate();

			session.Renew(timeout);
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void Test_Renew_ZeroTimeout()
		{
			TimeSpan timeout = TimeSpan.FromMinutes(10);
			UserSession session = CreateSession(timeout);
			session.Renew(TimeSpan.Zero);
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void Test_Renew_NegativeTimeout()
		{
			TimeSpan timeout = TimeSpan.FromMinutes(10);
			UserSession session = CreateSession(timeout);
			session.Renew(TimeSpan.FromMinutes(-10));
		}

		private static UserSession CreateSession(TimeSpan timeout)
		{
			UserInfo userInfo = new UserInfo(
				UserName,
				DisplayName,
				null,
				null);
			User user = User.CreateNewUser(userInfo, DefaultPassword);

			// change password so it never expires
			user.ChangePassword(AlternatePassword, DateTime.MaxValue);

			// create a session
			return user.InitiateSession("app", "host", AlternatePassword, timeout);
		}

		private static bool RoughlyEqual(DateTime? x, DateTime? y)
		{
			if (!x.HasValue && !y.HasValue)
				return true;

			if (!x.HasValue || !y.HasValue)
				return false;

			DateTime xx = x.Value;
			DateTime yy = y.Value;

			// for these purposes, if the times are within 1 second, that is good enough
			return Math.Abs((xx - yy).TotalSeconds) < 1;
		}
	}
}

#endif
