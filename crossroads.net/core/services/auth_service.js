
/* eslint-disable no-param-reassign */
(() => {
  function AuthService(
    $http,
    $rootScope,
    Session,
    AUTH_EVENTS,
    Impersonate
    ) {
    const authService = {};

    authService.login = (credentials) => {
      return $http
        .post(__GATEWAY_CLIENT_ENDPOINT__ + 'api/login', credentials).then((res) => {
          console.log('AUTH: LOGGED IN')
          Session.create(
            res.data.refreshToken,
            res.data.userToken,
            res.data.userTokenExp,
            res.data.userId,
            res.data.username
          );

          // The username from the credentials is really the email address
          // In a future story, the contact email address will always be in sync with the user email address.
          $rootScope.email = credentials.username;
          $rootScope.phone = res.data.userPhone;

          // TODO: we really need to refactor "username" to be nickname
          $rootScope.username = res.data.username;
          $rootScope.roles = res.data.roles;
          $rootScope.userid = res.data.userId;
          $rootScope.canImpersonate = res.data.canImpersonate;
          return res.data.username;
        });
    };

    authService.logout = () => {
      Session.clear();
      Session.resetCredentials();
      Impersonate.clear();
      $rootScope.$broadcast(AUTH_EVENTS.logoutSuccess);
    };

    // We are pretty sure they are copied from an example of how WE SHOULD be doing this,
    // instead we are using the above rootScope
    authService.isAuthenticated = () => Session.isActive();

    authService.isAuthorized = (authorizedRoles) => {
      if (!angular.isArray(authorizedRoles)) {
        authorizedRoles = [authorizedRoles];
      }

      return (authService.isAuthenticated() &&
          (_.find($rootScope.roles, role => authorizedRoles.indexOf(role.Id) >= 0) !== undefined));
    };

    return authService;
  }

  AuthService.$inject = [
    '$http',
    '$rootScope',
    'Session',
    'AUTH_EVENTS',
    'Impersonate'
  ];

  angular.module('crossroads.core').factory('AuthService', AuthService);
})();
