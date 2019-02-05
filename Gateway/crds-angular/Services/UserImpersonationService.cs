using System;
using System.Data.Entity.Core;
using crds_angular.Exceptions;
using crds_angular.Services.Interfaces;
using Crossroads.Utilities.Services;
using Crossroads.Web.Auth.Models;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;

namespace crds_angular.Services
{
    /// <summary>
    /// A service for impersonating another MinistryPlatform user.
    /// </summary>
    public class UserImpersonationService : IUserImpersonationService
    {
        private readonly IUserRepository _userService;

        /// <summary>
        /// Construct a new UserImpersonationService with the given IUserService.
        /// </summary>
        /// <param name="userService">An IUserService for performing user lookups</param>
        public UserImpersonationService(IUserRepository userService)
        {
            _userService = userService;
        }

        public TOutput WithImpersonation<TOutput>(AuthDTO authDTO, string usernameToImpersonate, Func<TOutput> action)
        {
            ImpersonatedUserGuid.Clear();

            if (!authDTO.UserInfo.Mp.CanImpersonate.Value)
            {
                throw new ImpersonationNotAllowedException();
            }

            return DoImpersonation(usernameToImpersonate, action);
        }

        public TOutput WithImpersonation<TOutput>(string accessToken, string usernameToImpersonate, Func<TOutput> action)
        {
            ImpersonatedUserGuid.Clear();

            var authUser = _userService.GetByAuthenticationToken(accessToken);
            if (authUser == null || !authUser.CanImpersonate)
            {
                throw new ImpersonationNotAllowedException();
            }

            return DoImpersonation(usernameToImpersonate, action);
        }

        private TOutput DoImpersonation<TOutput>(string usernameToImpersonate, Func<TOutput> action)
        {
            MpUser user = GetUser(usernameToImpersonate);

            if (user == null)
            {
                throw new ImpersonationUserNotFoundException(usernameToImpersonate);
            }

            ImpersonatedUserGuid.Set(user.Guid, usernameToImpersonate);

            try
            {
                return (action());
            }
            finally
            {
                ImpersonatedUserGuid.Clear();
            }
        }

        private MpUser GetUser(string username)
        {
            MpUser user = _userService.GetByUserId(username);
            if (user == null)
            {
                throw (new ImpersonationUserNotFoundException(username));
            }

            return user;
        }
    }
}
