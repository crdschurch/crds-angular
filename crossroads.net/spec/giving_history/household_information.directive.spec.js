require('../../app/app');

describe('HouseholdInformation Directive', function() {
  var $compile;
  var $rootScope;
  var $httpBackend;
  var scope;
  var templateString;

  beforeEach(angular.mock.module('crossroads'));

  beforeEach(angular.mock.module(function($provide) {
    $provide.value('$state', { get: function() {} });
  }));

  beforeEach(
      inject(function(_$compile_, _$rootScope_, _$httpBackend_) {
        $compile = _$compile_;
        $rootScope = _$rootScope_;
        $httpBackend = _$httpBackend_;

        scope = $rootScope.$new();
        scope.profile = {
          nickName: 'Eddie',
          lastName: 'Van Halen',
          contactId: 123
        };

        templateString =
            '<household-information ' +
            ' profile="profile"></household-information>';
      })
  );

});
