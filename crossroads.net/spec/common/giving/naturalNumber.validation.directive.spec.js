require('../../../app/common/common.module');
require('../../../app/app');

describe('Natural Number Validation Directive', function() {
  var scope, form, httpBackend;

  beforeEach(function() {
    angular.mock.module('crossroads');
  });

  beforeEach(angular.mock.module(function($provide) {
    $provide.value('$state', { get: function() {} });
  }));

  beforeEach(inject(function($injector,_$compile_, _$rootScope_) {
    var $compile = _$compile_;
    var $rootScope = _$rootScope_;
    httpBackend = $injector.get('$httpBackend');

    var template = angular.element("<form name='form'><input type='text' name='amount' ng-model='model.amount' natural-number max-value='999'></input></form>");
    scope = $rootScope.$new();
    scope.model = { amount: null };
    var element = $compile(template)(scope);
    form = scope.form;
  }));

  it("should reject non-numeric", function() {

    form.amount.$setViewValue('abc');
    scope.$digest();
    expect(form.amount.$valid).toBeFalsy();
  });

  it("should reject value greater than max", function() {

    form.amount.$setViewValue(1000);
    scope.$digest();
    expect(form.amount.$valid).toBeFalsy();
  });

  it("should accept valid numeric value", function() {

    form.amount.$setViewValue(999);
    scope.$digest();
    expect(form.amount.$valid).toBeTruthy();
  });
});
