require('../../app/app');

describe('GivingYears Directive', function() {
  var $compile;
  var $rootScope;
  var $httpBackend;
  var $timeout;
  var scope;
  var callback;
  var templateString;

  beforeEach(angular.mock.module('crossroads'));

  beforeEach(angular.mock.module(function($provide) {
    $provide.value('$state', { get: function() {} });
  }));

  beforeEach(
      inject(function(_$compile_, _$rootScope_, _$httpBackend_, _$timeout_) {
        $compile = _$compile_;
        $rootScope = _$rootScope_;
        $httpBackend = _$httpBackend_;
        $timeout = _$timeout_;

        callback = jasmine.createSpy('onChange');

        scope = $rootScope.$new();
        scope.selectedGivingYear = {};
        scope.allYears = [{key: '2015', value: '2015'}, {key: '2014', value: '2014'}];
        scope.onChange = callback;

        templateString =
            '<div class="col-sm-6" giving-years'
          + ' selected-year="selectedGivingYear"'
          + ' all-years="allYears"'
          + ' on-change="onChange()"></div>';
      })
  );

});
