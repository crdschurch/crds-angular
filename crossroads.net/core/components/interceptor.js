(function() {
  'use strict';
  
  const cookieNames = require('crds-constants').COOKIES;
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
        const expDate = new Date();
        const sessionLength = 1800000;
        expDate.setTime(expDate.getTime() + sessionLength);

        console.log("In interceptor");

        
        if (response.headers(cookieNames.SESSION_ID)) {
          console.log("injector if");
          var Session = $injector.get('Session');
          Session.getNewRefreshTokenAndSessionFromHeaders(response, sessionLength, expDate);
        } 
        else {
          console.log("injector else");
          var Session = $injector.get('Session');
          Session.updateSessionExpiration(sessionLength, expDate);
        }
        console.log("WTF MATE???");

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
