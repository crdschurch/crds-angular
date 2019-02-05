using System;

namespace crds_angular.Services.Interfaces
{
    /// <summary>
    /// A service for impersonating another MinistryPlatform user.
    /// </summary>
    public interface IUserImpersonationService
    {
        /// <summary>
        /// Execute the given function while impersonating another MinistryPlatform user.
        /// </summary>
        /// <typeparam name="TOutput">The output type of the 'action' function</typeparam>
        /// <param name="userIdLoggedIn">The user id of the logged-in user, must have the "Can Impersonate" property set in order to impersonate.</param>
        /// <param name="useridToImpersonate">The user id of the user to impersonate, typically the user's email address.</param>
        /// <param name="action">The action to run as the impersonated user</param>
        /// <returns>The output of the 'action' function</returns>
        TOutput WithImpersonation<TOutput>(string userIdLoggedIn, string useridToImpersonate, Func<TOutput> action);
    }
}
