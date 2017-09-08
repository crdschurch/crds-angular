describe('Login Controller', function () {

  var $controller, $q, LoginController, $rootScope, MESSAGES, $state, $log, AuthService, Session, $timeout, AnalyticsService;

  beforeEach(function () {
    angular.mock.module('crossroads.core', function ($provide) {

      // mocked service
      $provide.value('AuthService', {
        login: function () {
          return {
            then: function (callback) {
              return callback('username');
            }
          };
        },
      });

      return null;
    });
  });

  beforeEach(inject(function (_$controller_, _$rootScope_, _MESSAGES_, _$state_, _$log_, _AuthService_, _$timeout_, _Session_, _AnalyticsService_) {
    $controller = _$controller_;
    $rootScope = _$rootScope_;
    MESSAGES = _MESSAGES_;
    $state = _$state_;
    $log = _$log_;
    $scope = $rootScope.$new();
    AuthService = _AuthService_;
    $timeout = _$timeout_;
    Session = _Session_;
    AnalyticsService = _AnalyticsService_;
    LoginController = $controller('LoginController', {
      $scope: $scope,
      $rootScope: $rootScope,
      MESSAGES: MESSAGES,
      AuthService: AuthService,
      Session: Session,
      AnalyticsService: AnalyticsService
    });

  }));

  it('should route to homepage when navigateToHome is called', function() {
    spyOn($state, 'go');
    LoginController.navigateToHome();
    expect($state.go).toHaveBeenCalledWith('content', { link: '/' });
  });

  it('should route to homepage after signing in and has no redirect parameters', function() {

    $scope.navlogin = {
      $setPristine: function () {
        return null;
      }
    };

    $scope.credentials = {
      username: 'username',
      password: 'password'
    };

    $state.current.name = 'login';

    spyOn($state, 'go');
    spyOn($rootScope, '$emit');
    spyOn(AnalyticsService, 'identify');
    $rootScope.userid = 12345;

    spyOn(Session, 'hasRedirectionInfo').and.returnValue(false);

    $scope.login();
    $timeout.flush();

    expect($rootScope.$emit).toHaveBeenCalledTimes(0);
    expect($state.go).toHaveBeenCalledWith('content', { link: '/' });
    expect(AnalyticsService.identify).toHaveBeenCalledWith(12345);
  });

});