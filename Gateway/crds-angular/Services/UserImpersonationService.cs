﻿using crds_angular.Exceptions;
using crds_angular.Services.Interfaces;
using Crossroads.Utilities.Services;
using Crossroads.Web.Auth.Models;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;
using System;

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
           
            if (authDTO.UserInfo.Mp.CanImpersonate.GetValueOrDefault())
            {
                return DoImpersonation(usernameToImpersonate, ImpersonatedUserGuid.NewAuthImpersonateToken, action);
            }
            throw new ImpersonationNotAllowedException();
        }

        public TOutput WithImpersonation<TOutput>(string accessToken, string usernameToImpersonate, Func<TOutput> action)
        {
            ImpersonatedUserGuid.Clear();

            var authUser = _userService.GetByAuthenticationToken(accessToken);
            if (authUser == null || !authUser.CanImpersonate)
            {
                throw new ImpersonationNotAllowedException();
            }

            return DoImpersonation(usernameToImpersonate, accessToken, action);
        }

        private TOutput DoImpersonation<TOutput>(string usernameToImpersonate, string token, Func<TOutput> action)
        {
            if (usernameToImpersonate == null)
            {
                throw new ImpersonationUserNotFoundException();
            }

            MpUser user = _userService.GetByUserId(usernameToImpersonate);
            if (user == null)
            {
                throw new ImpersonationUserNotFoundException(usernameToImpersonate);
            }

            ImpersonatedUserGuid.Set(user.Guid, token);
            try
            {
                return (action());
            }
            finally
            {
                ImpersonatedUserGuid.Clear();
            }
        }
    }
}
