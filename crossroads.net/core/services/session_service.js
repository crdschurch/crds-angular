
/* eslint-disable no-param-reassign */

(() => {
  function openStayLoggedInModal($injector, $state, $modal) {
    // Only open if on a protected page? and the modal is not already shown.
    if ($state.current.data.isProtected && !$('#stayLoggedInModal').is(':visible')) {
      const AuthService = $injector.get('AuthService');
      const modal = $modal.open({
        templateUrl: 'stayLoggedInModal/stayLoggedInModal.html',
        controller: 'StayLoggedInController as StayLoggedIn',
        backdrop: 'static',
        keyboard: false,
        show: false,
        openedClass: 'crds-legacy-styles'
      });

      modal.result.then(
        () => {
          // login success
        },
        () => {
          // TODO:Once we stop using rootScope we can remove this and the depenedency on Injector
          AuthService.logout();
          $state.go('content', {
            link: '/'
          });
        });
    }
  }

  let timeoutPromise;
  const cookieNames = require('crds-constants').COOKIES;

  function SessionService(
    $log,
    $http,
    $state,
    $location,
    $window,
    $interval,
    $timeout,
    $cookies,
    $modal,
    $injector,
    $rootScope,
    $q,
    Impersonate
  ) {
    const vm = this;

    vm.create = (refreshToken, sessionId, userTokenExp, userId, username) => {
      $log.debug('creating cookies!');
      const expDate = new Date();
      expDate.setTime(expDate.getTime() + (userTokenExp * 1000));
      $cookies.put(cookieNames.SESSION_ID, sessionId, {
        expires: expDate
      });
      $cookies.put('userId', userId);
      $cookies.put('username', username);
      $cookies.put(cookieNames.REFRESH_TOKEN, refreshToken, {
        expires: expDate
      });
      $http.defaults.headers.common.Authorization = sessionId;
      $http.defaults.headers.common.RefreshToken = refreshToken;
    };

    vm.refresh = (response) => {
      $log.debug('updating cookies!');
      const expDate = new Date();
      // TODO: Consider how we could make this less hard coded,
      // put the timeout in the header also?
      const sessionLength = 1800000;

      // We must ensure the timer for the stayLoggedInModal fires before the SESSION_ID cookie
      // has expired.  The reactiveSso timer that fires every 3 seconds will automatically set
      // $rootScope.email = null if the SESSION_ID cookie has expired.  If the reactiveSso timer
      // fires AFTER the cookie has expired but BEFORE the stayLoggedInModal timer has fired,
      // the stayLoggedInModal will end up with a null username (email) and any subsequent
      // login attempts via that dialog will fail [reference DE2957]
      // TODO: Can/should reactiveSso and stayLoggedInModal share a single timer to make the
      // order of operations more predictable?
      const stayLoggedInTimeout = sessionLength - 5000;   // 5 seconds before cookie expiration

      expDate.setTime(expDate.getTime() + sessionLength);
      if (timeoutPromise) {
        $interval.cancel(timeoutPromise);
      }

      timeoutPromise = $interval(
        () => {
          openStayLoggedInModal($injector, $state, $modal);
        },
        stayLoggedInTimeout
      );

      $cookies.put(cookieNames.SESSION_ID, response.headers('sessionId'), {
        expires: expDate
      });
      $cookies.put(cookieNames.REFRESH_TOKEN, response.headers('refreshToken'), {
        expires: expDate
      });
      $http.defaults.headers.common.RefreshToken = response.headers('refreshToken');
      $http.defaults.headers.common.Authorization = response.headers('sessionId');
    };

    /*
     * This formats the family as a comma seperated string before storing in the
     * cookie called 'family'
     *
     * @param family - an array of participant ids
     */
    vm.addFamilyMembers = (family) => {
      $log.debug(`Adding ${family} to family cookie`);
      $cookies.put('family', family.join(','));
    };

    /*
     * @returns an array of participant ids
     */
    vm.getFamilyMembers = () => {
      if (this.exists('family')) {
        return _.map($cookies.get('family').split(','), strFam => Number(strFam));
      }
      return [];
    };

    vm.isActive = () => {
      const ex = this.exists(cookieNames.SESSION_ID);
      if (ex === undefined || ex === null) {
        return false;
      }
      return true;
    };

    vm.exists = cookieId => $cookies.get(cookieId);

    vm.clear = () => {
      $cookies.remove(cookieNames.SESSION_ID);
      $cookies.remove(cookieNames.REFRESH_TOKEN);
      $cookies.remove(cookieNames.IMPERSONATION_ID);
      $cookies.remove('userId');
      $cookies.remove('username');
      $cookies.remove('family');
      $cookies.remove('age');
      $http.defaults.headers.common.Authorization = undefined;
      $http.defaults.headers.common.RefreshToken = undefined;
      $http.defaults.headers.common.ImpersonateUserId = undefined;
      return true;
    };

    vm.getUserRole = () => '';

    vm.redirectIfNeeded = () => {
      $timeout(() => {
        if (vm.hasRedirectionInfo()) {
          const url = vm.exists('redirectUrl');
          const params = vm.exists('params');
          vm.removeRedirectRoute();

          // check if absolute url
          var absoluteUrlRegex = /^https?:\/\//i;
          if (absoluteUrlRegex.test(url))
          {
            $window.location.href = url;
          } else {
            const foundState = $state.get().filter(state => state.name === url);
            if (foundState.length > 0) {
              if (params === undefined) {
                $state.go(url);
              } else {
                $state.go(url, JSON.parse(params));
              }
            } else if (params === undefined) {
              $location.url(url);
            } else {
              $location.url(url).search(JSON.parse(params));
            }
          }
        }
      });
    };

    vm.addRedirectRoute = (redirectUrl, params) => {
      $cookies.put('redirectUrl', redirectUrl);
      $cookies.put('params', JSON.stringify(params));
    };

    vm.removeRedirectRoute = () => {
      $cookies.remove('redirectUrl');
      $cookies.remove('params');
    };

    vm.hasRedirectionInfo = () => {
      if (this.exists('redirectUrl') !== undefined) {
        return true;
      }
      return false;
    };

    vm.verifyAuthentication = (event, stateName, stateData, stateToParams) => {
      if (vm.isActive()) {
        const promise = $http({
          method: 'GET',
          url: `${__GATEWAY_CLIENT_ENDPOINT__}api/authenticated`,
          withCredentials: true,
          headers: {
            Authorization: $cookies.get(cookieNames.SESSION_ID),
            RefreshToken: $cookies.get(cookieNames.REFRESH_TOKEN)
          }
        }).then((response) => {
          var user = response.data;
          $rootScope.userid = user.userId;
          $rootScope.username = user.username;
          $rootScope.email = user.userEmail;
          $rootScope.phone = user.userPhone;
          $rootScope.roles = user.roles;
          if ($rootScope.impersonation.active !== true) {
            $rootScope.canImpersonate = user.canImpersonate;
          }
          $cookies.put('userId', user.userId);
          $cookies.put('username', user.username);
          vm.enableReactiveSso(event, stateName, stateData, stateToParams);
          vm.restoreImpersonation();
        }, (response) => {
          if (response.status !== -1) {
            vm.clearAndRedirect(event, stateName, stateToParams);
            vm.enableReactiveSso(event, stateName, stateData, stateToParams);
          }
        });
        return promise;
      } else if (stateData !== undefined && stateData.isProtected) {
        vm.clearAndRedirect(event, stateName, stateToParams);
        vm.enableReactiveSso(event, stateName, stateData, stateToParams);
      } else {
        vm.clear();
        vm.resetCredentials();
        vm.enableReactiveSso(event, stateName, stateData, stateToParams);
      }
      const deferred = $q.defer();
      deferred.reject('User is not logged in.');
      return deferred.promise;
    };

    vm.restoreImpersonation = () => {
      const impersonationCookie = $cookies.get(cookieNames.IMPERSONATION_ID);
      if (impersonationCookie && !$rootScope.impersonation.active) {
        Impersonate.start(impersonationCookie)
          .success((response) => {
            Impersonate.storeCurrentUser();
            Impersonate.storeDetails(true, response, impersonationCookie);
            Impersonate.setCurrentUser(response);
          });
      }
    };

    // flag to determine if the current user was already
    // logged in or not. Used in performReactiveSso()
    vm.wasLoggedIn = vm.isActive();

    /**
    * the local property on session to bind the setInterval()
    * to. Can be accessed on SessionService.
    */
    vm.reactiveSsoInterval = null;

    /**
    * Clears out the setInterval created by enableReactiveSso
    */
    vm.disableReactiveSso = () => {
      $interval.cancel(vm.reactiveSsoInterval);
    };

    /**
    * Enables the reactive sso setInterval and clears out any pre-existing
    * intervals that may exist. Also passes the state info from core.run.js
    * for route changes.
    *
    * Parameters are passed down for use in the verifyAuthentication()
    * through performReactiveSso().
    */
    vm.enableReactiveSso = (event, stateName, stateData, stateToParams) => {
      vm.disableReactiveSso();
      vm.reactiveSsoInterval = $interval(() => {
        vm.performReactiveSso(event, stateName, stateData, stateToParams);
      }, 3000);
    };

    /**
    * Handles the logic of deciding whether the cross-domain cookies
    * have changed since the app instantiated.
    *
    * If changes are detected, it resets the flags and then performs
    * auth or logout depending on the scenario. In the case of logout
    * and a protected page, the user is redirected to the log in page.
    */
    vm.performReactiveSso = (event, stateName, stateData, stateToParams) => {
      if ($rootScope.resolving || $rootScope.processing) {
        return;
      }
      if (vm.isActive() && vm.wasLoggedIn === false) {
        vm.verifyAuthentication(event, stateName, stateData, stateToParams);
        vm.wasLoggedIn = true;
      } else if (!vm.isActive() && vm.wasLoggedIn === true) {
        vm.wasLoggedIn = false;
        if (stateData !== undefined && stateData.isProtected) {
          vm.clear();
          vm.resetCredentials();
          openStayLoggedInModal($injector, $state, $modal);
        } else {
          vm.clear();
          vm.resetCredentials();
          if (!$rootScope.$$phase) {
            $rootScope.$apply();
          }
        }
      }
    };

    vm.clearAndRedirect = (event, toState, toParams) => {
      vm.clear();
      vm.resetCredentials();
      vm.addRedirectRoute(toState, toParams);
      if (event) {
        event.preventDefault();
      }
      $state.go('login', {}, { location: 'replace' });
    };

    vm.resetCredentials = () => {
      $rootScope.userid = null;
      $rootScope.username = null;
      $rootScope.email = null;
      $rootScope.phone = null;
      $rootScope.roles = null;
      $rootScope.canImpersonate = false;
    };

    vm.beOptimistic = false;
    return this;
  }

  SessionService.$inject = [
    '$log',
    '$http',
    '$state',
    '$location',
    '$window',
    '$interval',
    '$timeout',
    '$cookies',
    '$modal',
    '$injector',
    '$rootScope',
    '$q',
    'Impersonate'
  ];

  angular.module('crossroads.core').service('Session', SessionService);
})();
