require('../../app/common/common.module');
require('../../app/signup');

describe('Signup Component', function() {
  var CONSTANTS = require('crds-constants');
  var MODULE = CONSTANTS.MODULES.SIGNUP;
  var helpers = require('./signup.helpers');

  var $compile;
  var $rootScope;
  var element;
  var scope;
  var $httpBackend;
  var SignupService;
  var isolated;

  beforeEach(function() {
    angular.mock.module(MODULE);
  });

  beforeEach(angular.mock.module(function($provide) {
    $provide.value('$state', { get: function() {} });
  }));

  beforeEach(inject(function(_$compile_, _$rootScope_, $injector) {
    $compile = _$compile_;
    $rootScope = _$rootScope_;

    SignupService = $injector.get('SignupService');
    SignupService.cmsInfo = helpers.cmsInfo;

    scope = $rootScope.$new();
    element = '<crds-signup></crds-signup>';
    element = $compile(element)(scope);
    scope.$digest();
    isolated = element.isolateScope();
  }));

  it('should show the onetimesignup component', function() {
    expect(isolated.signup.showOnetimeEvent()).toBe(true);
  });

  it('should show the community group signup', function() {
    SignupService.cmsInfo.items[0].fields.className = CONSTANTS.CMS.PAGENAMES.COMMUNITYGROUPS;
    expect(isolated.signup.showCommunityGroups()).toBe(true);
  });

});
