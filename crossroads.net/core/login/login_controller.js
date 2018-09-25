(function () {
  'use strict';

  module.exports = LoginController;

  LoginController.$inject = [
    '$q',
    '$http',
    '$location',
    '$cookies',
    '$scope',
    '$rootScope',
    'AUTH_EVENTS',
    'MESSAGES',
    'AuthService',
    '$state',
    '$log',
    'Session',
    '$timeout',
    'User',
    'ImageService',
    'Impersonate',
    'AnalyticsService'
  ];

  function LoginController(
    $q,
    $http,
    $location,
    $cookies,
    $scope,
    $rootScope,
    AUTH_EVENTS,
    MESSAGES,
    AuthService,
    $state,
    $log,
    Session,
    $timeout,
    User,
    ImageService,
    Impersonate,
    AnalyticsService) {

    var vm = this;
    vm.path = ImageService.ProfileImageBaseURL + vm.contactId;
    vm.defaultImage = ImageService.DefaultProfileImage;
    vm.navigateToHome = navigateToHome;

    $scope.alreadyLoggedInCheckProcessing = true;
    redirectUserToDestinationOrHomepageIfAlreadyLoggedIn();

    $scope.loginShow = false;
    $scope.newuser = User;
    $scope.credentials = {};
    $scope.credentials.username = $scope.newuser.email;
    $scope.passwordPrefix = 'login-page';
    $scope.checkEmail = function () {
      //This logic is crazy, needs some attention
      return ($scope.navlogin.username.$error.required &&
        $scope.navlogin.$submitted &&
        $scope.navlogin.username.$dirty ||
        $scope.navlogin.username.$error.required &&
        $scope.navlogin.$submitted &&
        !$scope.navlogin.username.$touched ||
        $scope.navlogin.username.$error.required &&
        $scope.navlogin.$submitted &&
        $scope.navlogin.username.$touched ||
        !$scope.navlogin.username.$error.required &&
        $scope.navlogin.username.$dirty &&
        !$scope.navlogin.username.$valid);
    };

    $scope.toggleDesktopLogin = function () {
      $scope.loginShow = !$scope.loginShow;
      if ($scope.registerShow) {
        $scope.registerShow = !$scope.registerShow;
        $scope.credentials.username = $scope.newuser.email;
        $scope.credentials.password = $scope.newuser.password;
      }
    };

    $scope.logout = function () {
      $state.go('logout');
      return;
    };

    function navigateToHome() {
      $state.go('content', {
        link: '/'
      });
    }

    function redirectUserToDestinationOrHomepageIfAlreadyLoggedIn() {
      crds_utilities.checkLoggedin($q, $timeout, $http, $location, $rootScope, $cookies, Session, Impersonate)
        .then(
          () => {
            redirectToSpecifiedPageOrToHomepage(Session, $timeout);
          },
          () => {
            $scope.alreadyLoggedInCheckProcessing = false;
          }
        );
    }

    function redirectToSpecifiedPageOrToHomepage(Session, $timeout) {
      $timeout(function () {
        if (Session.hasRedirectionInfo()) {
          console.log('HAS redirect info');
          Session.redirectIfNeeded();
        } else {
          console.log('Has NO redirect info');
          navigateToHome();
        }
      }, 500);
    }

    $scope.login = function () {

      if ($scope.navlogin && $scope.navlogin.username) {
        $scope.navlogin.username.$setTouched();
        $scope.navlogin['passwd.passwordForm'].password.$setTouched();
      }

      if (($scope.credentials === undefined) ||
        ($scope.credentials.username === undefined ||
          $scope.credentials.password === undefined)) {
        $scope.pending = true;
        $scope.loginFailed = false;
        $rootScope.$emit('notify', $rootScope.MESSAGES.generalError);
      } else {
        $scope.processing = true;
        AuthService.login($scope.credentials).then(function (user) {

            $scope.loginShow = false;
            if ($scope.modal) {
              $scope.modal.close();
            }

            clearCredentials();
            AnalyticsService.identifyLoggedInUser($rootScope.userid, $scope.credentials.username);
            // If the state name ends with login or register (like 'login' or 'give.one_time_login'),
            // either redirect to specified URL, or redirect to profile if URL is not specified.
            if (_.endsWith($state.current.name, 'login') || _.endsWith($state.current.name, 'register')) {
              redirectToSpecifiedPageOrToHomepage(Session, $timeout);
            } else if ($scope.loginCallback) {
              $scope.processing = false;
              $scope.loginCallback();
            }

            $scope.loginFailed = false;
            $rootScope.showLoginButton = false;
            $scope.navlogin.$setPristine();
          },

          function () {
            $scope.pending = false;
            $scope.processing = false;
            $scope.loginFailed = true;
            $rootScope.$emit('notify', $rootScope.MESSAGES.generalError);
          });
      }
    };

    function clearCredentials() {
      if ($scope.credentials !== undefined) {
        $scope.credentials.username = undefined;
        $scope.credentials.password = undefined;
      }
    }
  };
})();
