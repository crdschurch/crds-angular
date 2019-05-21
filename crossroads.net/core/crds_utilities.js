'use strict()';
var cookieNames = require('crds-constants').COOKIES;

var getCookie =  function(cname) {
  var name = cname + '=';
  var ca = document.cookie.split(';');
  for (var i = 0; i < ca.length; i++) {
    var c = ca[i];
    while (c.charAt(0) === ' ') { c = c.substring(1); }
    if (c.indexOf(name) === 0) { return c.substring(name.length, c.length); }
  }
  return undefined;
};

var convertStringToDate = function convertDate(value) {
  if (typeof value === 'string' || value instanceof String) {
    var parts = value.split('/');
    if (parts.length === 2) {
      value = new Date(parts[1], parts[0] - 1, 1);
    } else if (parts.length === 3) {
      value = new Date(parts[2], parts[0] - 1, parts[1]);
    }
  }

  return value;
};

// This custom type is needed to allow us to NOT URLEncode slashes when using ui-sref
// See this post for details: https://github.com/angular-ui/ui-router/issues/1119
var preventRouteTypeUrlEncoding = function(urlMatcherFactory, routeType, urlPattern) {
  return (urlMatcherFactory.type(routeType, {
    encode: function (val) {
      return val !== null ? val.toString() : val;
    },
    decode: function (val) {
      return val !== null ? val.toString() : val;
    },
    is: function (val) {
      return this.pattern.test(val);
    },
    pattern: urlPattern
  }));
};



//================================================
// Check if the user is connected
//================================================
var checkLoggedin = function ($q, $timeout, $http, $location, $rootScope, $cookies, Session, Impersonate) {
  var deferred = $q.defer();
  $http.defaults.headers.common.Authorization = $cookies.get(cookieNames.SESSION_ID);
  const impersonationCookie = $cookies.get(cookieNames.IMPERSONATION_ID);
  if (impersonationCookie) {
    Impersonate.setHeaders(impersonationCookie);
  }
  $http({
    method: 'GET',
    url: __GATEWAY_CLIENT_ENDPOINT__ + 'api/authenticated',
    headers: {
      'Authorization': $cookies.get(cookieNames.SESSION_ID)
    }
  }).success(function (user) {
    // Authenticated
    if (user.userId !== undefined) {
      $timeout(deferred.resolve, 0);
      if (!impersonationCookie) {
        $rootScope.userid = user.userId;
        $rootScope.username = user.username;
      }
    } else {
      Session.clear();
      $rootScope.message = 'You need to sign in.';
      $timeout(function () {
        deferred.reject();
      }, 0);
      $location.url('/');
    }
  }).error(function (e) {
    $timeout(deferred.reject, 0);
    console.log(e);
    console.log('ERROR: trying to authenticate');
  });
  return deferred.promise;
};

/**
 * This is a specialty function that just checks if the cookie exists and is
 * not expired. This helps when we are in a flow that requires logging in and
 * quickly transitioning between states
 */
var optimisticallyCheckLoggedin = function ($q, $timeout, $http, $location, $rootScope, $cookies, Session) {
  if (Session.beOptimistic) {
    var deferred = $q.defer();
    var sessionId = $cookies.get(cookieNames.SESSION_ID);
    if (_.isEmpty(sessionId) ) {
      Session.clear();
      deferred.reject();
    } else {
      deferred.resolve();
    }

    return deferred.promise;
  }

  checkLoggedin($q, $timeout, $http, $location, $rootScope, $cookies, Session);
};

/**
 * Takes a javascript date and returns a
 * string formated MM/DD/YYYY
 * @param date - Javascript Date
 * @param days to add - How many days to add to the original date passed in
 * @return string formatted in the way we want to display
 */
function formatDate(date, days){
  if(days === undefined){
    days = 0;
  }
  var d = moment(date);
  d.add(days, 'd');
  return d.format('MM/DD/YYYY');
}

module.exports = {
  getCookie: getCookie,
  preventRouteTypeUrlEncoding: preventRouteTypeUrlEncoding,
  checkLoggedin: checkLoggedin,
  optimisticallyCheckLoggedin: optimisticallyCheckLoggedin,
  formatDate: formatDate,
  convertStringToDate: convertStringToDate,
};
