describe('Session Service', function () {

  var $cookies;
  var Backend;
  var Session;
  var Injector;
  var rootScope;
  var modal;
  var state;
  var location;
  var cookieNames = require('crds-constants').COOKIES;
  var family = [0, 1, 2, 3, 4];
  var timeout;

  beforeEach(angular.mock.module('crossroads.core'));
  

  beforeEach(inject(function ($injector, _$cookies_, _$rootScope_, _Session_, _$modal_, _$timeout_) {
    $cookies = _$cookies_;
    state = $injector.get('$state');
    location = $injector.get('$location');
    spyOn(state, 'go');
    Backend = $injector.get('$httpBackend');
    Session = _Session_;
    Injector = $injector;
    rootScope = _$rootScope_;
    modal = _$modal_;
    timeout = _$timeout_;
  }));

  afterEach(function () {
    Backend.verifyNoOutstandingExpectation();
    Backend.verifyNoOutstandingRequest();
  });

  it('should save an array of family members', function () {
    Session.addFamilyMembers(family);
    expect($cookies.get('family')).toBe(family.join(','));
  });

  it('should return an array of family members', function () {
    Session.addFamilyMembers(family);
    expect(Session.getFamilyMembers()[0]).toBe(family[0]);
    expect(Session.getFamilyMembers()[1]).toBe(family[1]);
    expect(Session.getFamilyMembers()[4]).toBe(family[4]);
  });

  it('should add a family member to an existing family', function () {
    Session.addFamilyMembers(family);
    expect(Session.getFamilyMembers()[0]).toBe(family[0]);
    expect(Session.getFamilyMembers()[1]).toBe(family[1]);
    expect(Session.getFamilyMembers()[4]).toBe(family[4]);
    family.push(5);
    Session.addFamilyMembers(family);
    expect(Session.getFamilyMembers()[5]).toBe(family[5]);
  });

  describe('function enableReactiveSso', function () {

    var mockResponse = {
      age: 0,
      canImpersonate: true,
      refreshToken: null,
      roles: [],
      userEmail: 'test@tester.com',
      userId: 1234567,
      userPhone: null,
      userToken: 'Aadfwedrwererre',
      userTokenExp: null,
      username: 'Test'
    };

    it('should create a setInterval to monitor cookie changes', function () {
      Session.enableReactiveSso();
      expect(Session.reactiveSsoInterval).toBeDefined();
    });

    it('should set credentials when login is detected', function () {
      headersResp = {}
      headersResp[cookieNames.SESSION_ID] = 'abcdef';
      $cookies.put(cookieNames.SESSION_ID, mockResponse.userToken);
      // Backend.expectGET(window.__env__['CRDS_GATEWAY_CLIENT_ENDPOINT'] + 'api/authenticated').respond(200, mockResponse);
      Backend.expectGET(window.__env__['CRDS_GATEWAY_CLIENT_ENDPOINT'] + 'api/authenticated').respond(function (){
        return [200, mockResponse];
      });
      spyOn(Session, 'setupLoggedOutModal');
      Session.performReactiveSso();
      Backend.flush();
      expect(rootScope.userid).toBe(mockResponse.userId);
      expect(rootScope.username).toBe(mockResponse.username);
      expect(rootScope.email).toBe(mockResponse.userEmail);
      expect(rootScope.phone).toBe(mockResponse.userPhone);
    });

    it('should clear credentials when logout is detected', function () {
      Session.wasLoggedIn = true;
      $cookies.remove(cookieNames.SESSION_ID);
      Session.performReactiveSso();
      expect(rootScope.userid).toBe(null);
      expect(rootScope.username).toBe(null);
      expect(rootScope.email).toBe(null);
      expect(rootScope.phone).toBe(null);
    });

    it('should open login model when logout is detected on protected route', function () {
      Session.wasLoggedIn = true;
      spyOn(modal, 'open').and.callThrough();
      state.current.data = {
        isProtected: true
      };
      $cookies.remove(cookieNames.SESSION_ID);
      Session.performReactiveSso(undefined, state.current.name, state.current.data, state.current.params);
      expect(modal.open).toHaveBeenCalledWith({
        templateUrl: 'stayLoggedInModal/stayLoggedInModal.html',
        controller: 'StayLoggedInController as StayLoggedIn',
        backdrop: 'static',
        keyboard: false,
        show: false,
        openedClass: 'crds-legacy-styles'
      });
    });

  });

  describe('function addRedirectRoute', function () {
    it('should add redirectUrl and params cookies', function () {
      var params = {
        p1: 2,
        p3: '4'
      };
      Session.addRedirectRoute('new.state', params);
      expect($cookies.get('redirectUrl')).toBe('new.state');
      expect($cookies.get('params')).toBe(JSON.stringify(params));
    });
  });

  describe('function verifyAuthentication', function () {

    describe('if user session exists', function () {

      var mockResponse = {
        age: 0,
        canImpersonate: true,
        refreshToken: null,
        roles: [],
        userEmail: 'test@tester.com',
        userId: 1234567,
        userPhone: null,
        userToken: 'Aadfwedrwererre',
        userTokenExp: null,
        username: 'Test'
      };

      beforeEach(function () {
        $cookies.put(cookieNames.SESSION_ID, mockResponse.userToken);
      });

      it('should handle successful login', function () {
        Backend.expectGET(window.__env__['CRDS_GATEWAY_CLIENT_ENDPOINT'] + 'api/authenticated').respond(200, mockResponse);
        Session.verifyAuthentication(undefined, state.current.name, state.current.data, state.current.params);
        Backend.flush();
        expect(rootScope.userid).toBe(mockResponse.userId);
        expect(rootScope.username).toBe(mockResponse.username);
        expect(rootScope.email).toBe(mockResponse.userEmail);
        expect(rootScope.phone).toBe(mockResponse.userPhone);
      });

    });

    describe('if user session does not exist', function () {

      var mockResponse = {};

      beforeEach(function () {
        $cookies.remove(cookieNames.SESSION_ID);
      });

      it('should make you login when visiting protected pages', function () {
        state.current.data = {
          isProtected: true
        };
        Session.verifyAuthentication(undefined, state.current.name, state.current.data, state.current.params);
        expect(state.go).toHaveBeenCalledWith('login', {}, { location: 'replace' });
      });

      it('should stay on non protected pages when not logged in', function () {
        state.current.data = {
          isProtected: false
        };
        Session.verifyAuthentication(undefined, state.current.name, state.current.data, state.current.params);
        expect(state.go).not.toHaveBeenCalled();
      });
    });
  });

  describe('function redirectIfNeeded', function () {

    var redirectUrl, params;
    var MockSession = {
      exists: function (value) {
        if (value == 'redirectUrl') {
          return redirectUrl;
        } else if (value == 'params') {
          return params;
        }
      }
    };

    it('should not redirect', function () {
      Session.redirectIfNeeded();
      expect(state.go).toHaveBeenCalledTimes(0);
    });

    it('should route to known state after signing in and has redirectUrl', function () {
      state.current.name = 'login';
      redirectUrl = 'logout';

      spyOn(state, 'get').and.returnValue([{ url: '/signout', controller: 'LogoutController', data: { isProtected: false, meta: { title: 'Sign out', description: '' } }, name: 'logout' }]);

      spyOn(Session, 'hasRedirectionInfo').and.returnValue(true);
      spyOn(Session, 'exists').and.callFake(MockSession.exists);

      Session.redirectIfNeeded();
      timeout.flush();

      expect(state.go).toHaveBeenCalledWith(redirectUrl);
    });

    it('should route to known state after signing in and has redirectUrl and params', function () {

      state.current.name = 'login';
      redirectUrl = 'logout';
      const paramsObj = { bye: 'Felicia' };
      params = JSON.stringify(paramsObj);

      spyOn(state, 'get').and.returnValue([{ url: '/signout', controller: 'LogoutController', data: { isProtected: false, meta: { title: 'Sign out', description: '' } }, name: 'logout' }]);

      spyOn(Session, 'hasRedirectionInfo').and.returnValue(true);
      spyOn(Session, 'exists').and.callFake(MockSession.exists);

      Session.redirectIfNeeded();
      timeout.flush();

      expect(state.go).toHaveBeenCalledWith(redirectUrl, paramsObj);
    });

    it('should route to external url after signing in and has redirectUrl', function () {

      state.current.name = 'login';
      redirectUrl = '/connect';

      spyOn(location, 'url');
      spyOn(state, 'get').and.returnValue([{ url: '/signout', controller: 'LogoutController', data: { isProtected: false, meta: { title: 'Sign out', description: '' } }, name: 'logout' }]);

      spyOn(Session, 'hasRedirectionInfo').and.returnValue(true);
      spyOn(Session, 'exists').and.callFake(MockSession.exists);

      Session.redirectIfNeeded();
      timeout.flush();

      expect(location.url).toHaveBeenCalledWith(redirectUrl);
    });

    it('should route to external url after signing in and has redirectUrl and params', function () {

      state.current.name = 'login';
      redirectUrl = '/connect';
      const paramsObj = { resolve: true };
      params = JSON.stringify(paramsObj);

      spyOn(location, 'url').and.returnValue(location);
      spyOn(location, 'search');
      spyOn(state, 'get').and.returnValue([{ url: '/signout', controller: 'LogoutController', data: { isProtected: false, meta: { title: 'Sign out', description: '' } }, name: 'logout' }]);

      spyOn(Session, 'hasRedirectionInfo').and.returnValue(true);
      spyOn(Session, 'exists').and.callFake(MockSession.exists);

      Session.redirectIfNeeded();
      timeout.flush();

      expect(location.url).toHaveBeenCalledWith(redirectUrl);
      expect(location.search).toHaveBeenCalledWith(paramsObj);

    });

  })

  describe('function create(refreshToken, sessionId, userTokenExp, userId, username', function () {
    it('should set session cookies when called', function () {
      var refreshToken = 'abcdefg123456789';
      var sessionId = 'afoobarsessionid0987';
      var userTokenExp = 1800;
      var userId = '123456789';
      var username = 'Foobar User';
      Session.create(refreshToken, sessionId, userTokenExp, userId, username);
      expect($cookies.get('refreshToken')).toBe(refreshToken);
      expect($cookies.get('sessionId')).toBe(sessionId);
      expect($cookies.get('userId')).toBe(userId);
      expect($cookies.get('username')).toBe(username);
    });

    it('should set session cookie expirations when called', function () {
      var baseTime = new Date(2018, 01, 01);
      jasmine.clock().mockDate(baseTime);
      var refreshToken = 'abcdefg123456789';
      var sessionId = 'afoobarsessionid0987';
      var userTokenExp = 1800;
      var userId = '123456789';
      var username = 'Foobar User';
      var expiration_time = "Thu Feb 01 2018 00:30:00 GMT-0500 (EST)"
      spyOn($cookies, 'put');
      Session.create(refreshToken, sessionId, userTokenExp, userId, username);
      expect($cookies.put).toHaveBeenCalledWith('userId', '123456789', Object({ expires: Date(expiration_time) }) )
      expect($cookies.put).toHaveBeenCalledWith('username', 'Foobar User', Object({ expires: Date(expiration_time) }) )
    });
  });

});
