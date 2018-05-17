(function() {
  'use strict';
  angular.module('crossroads.core').factory('InterceptorFactory', InterceptorFactory);

  InterceptorFactory.$inject = ['$injector'];

  function InterceptorFactory($injector) {
    return {
      request: function(config) {
        // Make sure Crds-Api-Key is set on all requests using $http,
        // even those that explicitly specify other headers
        if(config.headers && (config.headers['Crds-Api-Key'] === undefined ||
            config.headers['Crds-Api-Key'].length === 0) && 
            __CROSSROADS_API_TOKEN__.length > 0) {
          config.headers['Crds-Api-Key'] = __CROSSROADS_API_TOKEN__;
        }
        return config;
      },

      response: function(response) {
        if (response.headers('refreshToken')) {
          var Session = $injector.get('Session');
          Session.getNewRefreshTokenAndSessionFromHeaders(response);
        }

        return response;
      }
    };
  }

  var app = angular.module('crossroads.core');
  app.config(AppConfig);
  AppConfig.$inject = ['$httpProvider'];
  function AppConfig($httpProvider) {
    $httpProvider.interceptors.push('InterceptorFactory');
  }

})();
