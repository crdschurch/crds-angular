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

  // describe('scope.$watch(profile)', function() {
  //   var element;
  //   beforeEach(function() {
  //     element = $compile(angular.element(templateString))(scope);
  //     scope.$digest();
  //   });

  //   it('should set household name properly for individual', function() {
  //     var isolateScope = element.isolateScope();
  //     expect(isolateScope.profile.householdName).toBeDefined();
  //     expect(isolateScope.profile.householdName).toEqual('Eddie Van Halen');
  //   });

  //   it('should set household name properly for multiple in a family', function() {
  //     scope.profile = {
  //       nickName: 'Eddie',
  //       lastName: 'Van Halen',
  //       contactId: 123,
  //       householdMembers: [
  //         { ContactId: 678, LastName: 'Lee Roth', Nickname: 'David', StatementTypeId: 2 },
  //         { ContactId: 123, LastName: 'Van Halen', Nickname: 'Edward', StatementTypeId: 2 },
  //         { ContactId: 789, LastName: 'Hagar', Nickname: 'Samuel', StatementTypeId: 1 },
  //         { ContactId: 456, LastName: 'VaN HaLeN', Nickname: 'Alex', StatementTypeId: 2 },
  //         { ContactId: 345, LastName: 'Anthony', Nickname: 'Michael', StatementTypeId: 2 },
  //         { ContactId: 234, LastName: 'vAN HALeN', Nickname: 'Wolfgang', StatementTypeId: 2 }
  //       ]
  //     };

  //     var isolateScope = element.isolateScope();
  //     isolateScope.$apply();

  //     expect(isolateScope.profile.householdName).toBeDefined();
  //     expect(isolateScope.profile.householdName).toEqual('Edward & Alex & Wolfgang Van Halen & David Lee Roth & Michael Anthony');
  //   });
  // });

});
