#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using ClearCanvas.Enterprise.Common;

namespace ClearCanvas.Web.Enterprise.Authentication
{
    public class SessionInfo
    {
        private readonly CustomPrincipal _user;
        private bool _valid;

        public SessionInfo(CustomPrincipal user)
        {
            _user = user;
        }

        public SessionInfo(string loginId, string name, SessionToken token)
            : this(new CustomPrincipal(new CustomIdentity(loginId, name),
                                       CreateLoginCredentials(loginId, name, token)))
        {

        }

        /// <summary>
        /// Gets a value indicating whether or not the session information is valid.
        /// </summary>
        /// <remarks>
        /// Exception will be thrown if session cannot be validated in the process.
        /// </remarks>
        public bool Valid
        {
            get
            {
                Validate();
                return _valid;
            }
        }

        public CustomPrincipal User
        {
            get { return _user; }
        }

        public LoginCredentials Credentials
        {
            get
            {
                return _user.Credentials;
            }
        }

        private static LoginCredentials CreateLoginCredentials(string loginId, string name, SessionToken token)
        {
            LoginCredentials credentials = new LoginCredentials
                                               {
                                                   UserName = loginId, 
                                                   DisplayName = name, 
                                                   SessionToken = token
                                               };
            return credentials;
        }

        public void Validate()
        {
            _valid = false;

            using(LoginService service = new LoginService())
            {
                SessionInfo sessionInfo = service.Query(Credentials.SessionToken.Id);

                if (sessionInfo == null)
                {
                    throw new SessionValidationException();
                }

                _user.Credentials = sessionInfo.Credentials;
                SessionToken newToken = service.Renew(Credentials.SessionToken.Id);
                _user.Credentials.SessionToken = newToken;
                _valid = true;
            }   
        }
    }
}