

describe('Email Field Directive', function () {
  var mockSession, mockUser;

  var templateString, scope, callback;

  var $compile, $rootScope, $templateCache, $httpBackend, $timeout;

  beforeEach(function () {
    angular.mock.module('crossroads.core', function ($provide) {
      mockSession = jasmine.createSpyObj('Session', ['exists', 'isActive', 'updateSessionExpiration']);
      mockSession.exists.and.callFake(function (something) {
        return undefined;
      });
      mockSession.isActive.and.callFake(function () {
        return (false);
      });
      $provide.value('Session', mockSession);

      mockUser = jasmine.createSpy('User');
      $provide.value('User', mockUser);
    });
  });

  beforeEach(
    inject(function (_$compile_, _$rootScope_, _$templateCache_, _$httpBackend_, _$timeout_) {
      $compile = _$compile_;
      $rootScope = _$rootScope_;
      $templateCache = _$templateCache_;
      $httpBackend = _$httpBackend_;
      $timeout = _$timeout_;

      $templateCache.put('on-submit-messages', '<span ng-message="required">Required</span>');
      $templateCache.put('on-blur-messages',
        '<span ng-message="unique">Not a Unique Email</span>'
        + '<span ng-message="email">Invalid Email</span>');

      callback = jasmine.createSpyObj('callback', ['onEmailFound', 'onEmailNotFound']);

      scope = $rootScope.$new();
      scope.model = {
        emailModel: 'me@here.com',
        emailPrefix: 'myPrefix',
        formSubmitted: false,
        onEmailNotFound: callback.onEmailNotFound,
        onEmailFound: callback.onEmailFound,
        validateUnique: true,
        checkUnique: true,
        focused: true,
      };

      templateString =
        '<email-field '
        + 'email="model.emailModel" '
        + 'prefix="model.emailPrefix" '
        + 'submitted="model.formSubmitted" '
        + 'on-email-not-found="model.onEmailNotFound" '
        + 'on-email-found="model.onEmailFound" '
        + 'validate-unique="model.validateUnique" '
        + 'check-unique="model.checkUnique" '
        + 'focused="model.focused"/>';
    })
  );

  describe('markup', function () {
    var element;
    beforeEach(function () {
      $httpBackend.whenGET(window.__env__['CRDS_GATEWAY_CLIENT_ENDPOINT'] + 'api/lookup/0/find/?email=me@here.com').respond(404, 'not found');

      element = $compile(templateString)(scope);
      scope.$digest();
    });

    it('should have an id set with the prefix value', function () {
      expect(element.html()).toContain('id=\"' + scope.model.emailPrefix + '-email\"');
    });

    it('should have focused the email element', function () {
      var emailInput = element.find('input')[0];
      spyOn(emailInput, 'focus');
      $timeout.flush();
      expect(emailInput.focus).toHaveBeenCalled();
    });
  });

  describe('checkUniqueEmail', function () {
    var element, form, isolateScope;
    beforeEach(function () {
      scope.model.emailModel = 'me+you@here.com';
      $httpBackend.whenGET(window.__env__['CRDS_GATEWAY_CLIENT_ENDPOINT'] + 'api/lookup/0/find/?email=me%2Byou@here.com').respond(404, 'not found');
      element = $compile(templateString)(scope);
      scope.$digest();
      isolateScope = element.isolateScope();
      form = isolateScope.email_field;

      // This forces the directive to update on default - allows us to easily test
      // the email field that normally only updates on blur
      form.email.$options.updateOnDefault = true;
      form.email.$options.updateOn = 'default blur';
    });

    afterEach(function () {
      //$httpBackend.verifyNoOutstandingExpectation();
      //$httpBackend.verifyNoOutstandingRequest();
    });

    it('should lookup an email with an encoded plus sign', function () {
      $httpBackend.expectGET(window.__env__['CRDS_GATEWAY_CLIENT_ENDPOINT'] + 'api/lookup/0/find/?email=me%2Byou%2Bus@there.com').respond(404, 'existing email');

      form.email.$setViewValue('me+you+us@there.com');
      isolateScope.$digest();
      $httpBackend.flush();
    });

    it('should call onEmailNotFound callback if API returns a 200', function () {
      $httpBackend.expectGET(window.__env__['CRDS_GATEWAY_CLIENT_ENDPOINT'] + 'api/lookup/0/find/?email=me%2Byou@there.com').respond(200, 'email ok');

      form.email.$setViewValue('me+you@there.com');
      isolateScope.$digest();
      $httpBackend.flush();
      expect(isolateScope.email).toBe('me+you@there.com');
      expect(form.email.$valid).toBeTruthy();
      expect(callback.onEmailNotFound).toHaveBeenCalledWith('me+you@there.com');
      expect(callback.onEmailFound).toHaveBeenCalledWith('me+you@here.com');
      expect(callback.onEmailFound).not.toHaveBeenCalledWith('me+you@there.com');
    });

    it('should call onEmailFound callback if API returns a 404', function () {
      $httpBackend.expectGET(window.__env__['CRDS_GATEWAY_CLIENT_ENDPOINT'] + 'api/lookup/0/find/?email=me%2Byou@there.com').respond(404, 'existing email');

      form.email.$setViewValue('me+you@there.com');
      isolateScope.$digest();
      $httpBackend.flush();
      expect(isolateScope.email).toBeUndefined();
      expect(form.email.$valid).toBeFalsy();
      expect(callback.onEmailFound).toHaveBeenCalledWith('me+you@there.com');
      expect(callback.onEmailNotFound).not.toHaveBeenCalled();
    });


  });
});
