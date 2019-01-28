'use strict()';
(function () {
  var app = angular.module('crossroads.core');
  var cookieNames = require('crds-constants').COOKIES;
  app.config(AppConfig);

  AppConfig.$inject = [
    '$provide',
    '$httpProvider',
    '$locationProvider',
    'datepickerConfig',
    'datepickerPopupConfig',
    '$cookiesProvider',
    '$sceDelegateProvider'];

  function AppConfig($provide,
    $httpProvider,
    $locationProvider,
    datepickerConfig,
    datepickerPopupConfig,
    $cookiesProvider,
    $sceDelegateProvider) {
    $sceDelegateProvider.resourceUrlWhitelist([
      'self',
      'https://*crds-angular.netlify.com/'
    ]);
    $provide.decorator('$state', function ($delegate, $rootScope) {

      $rootScope.$on('$stateChangeStart', function (event, state, params) {
        $delegate.next = state;
        $delegate.toParams = params;
      });

      return $delegate;
    });

    $locationProvider.html5Mode({
      enabled: true,
      requireBase: false
    });

    let commonHeaders = $httpProvider.defaults.headers.common;

    $httpProvider.defaults.useXDomain = true;
    commonHeaders.Authorization = crds_utilities.getCookie(cookieNames.SESSION_ID);
    commonHeaders.RefreshToken = crds_utilities.getCookie(cookieNames.REFRESH_TOKEN);

    // Set Client API Key, if one is defined
    if (__CROSSROADS_API_TOKEN__.length > 0) {
      commonHeaders['Crds-Api-Key'] = __CROSSROADS_API_TOKEN__;
    }

    // This is a dummy header that will always be returned
    // in any 'Allow-Header' from any CORS request
    // This needs to be here because of IE.
    commonHeaders['X-Use-The-Force'] = true;

    configureDefaultCookieScope($cookiesProvider);
    configureDatePickersDefaults(datepickerConfig, datepickerPopupConfig);
  }

  var configureDefaultCookieScope = function ($cookiesProvider) {
    $cookiesProvider.defaults.path = '/';
    if (__COOKIE_DOMAIN__) {
      $cookiesProvider.defaults.domain = __COOKIE_DOMAIN__;
    }
  };

  var configureDatePickersDefaults = function (datepickerConfig, datepickerPopupConfig) {
    datepickerConfig.showWeeks = false;
    datepickerPopupConfig.showWeeks = false;
  };

})();
