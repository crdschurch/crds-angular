require('../../app/app');

describe('Serve Tabs Directive', function() {

  var $compile, $rootScope, element, scope, mockSession, $httpBackend;

  var mockOpportunity = { "time": "8:30am", "name" : "Kids Club Nusery", "members" : [ { "name": "John", "contactId" : 12345678, "roles" : [ {"name": "NuseryA"}, {"name": "NuseryB"}, {"name": "NuseryC"}, {"name": "NuseryD"} ] }, { "name":  "Jane", "contactId": 1234567890, "roles" : [ {"name": "NuseryA"}, {"name": "NuseryB"}, {"name": "NuseryC"}, {"name": "NuseryD"} ], "signedup" : "yes" }, ] };


  beforeEach(function(){
    angular.mock.module('crossroads', function($provide){
      mockSession= jasmine.createSpyObj('Session', ['exists']);
      mockSession.exists.and.callFake(function(something){
        return '12345678';
      });
      $provide.value('Session', mockSession);
    });
  });

  beforeEach(inject(function(_$compile_, _$rootScope_, _$httpBackend_){
    $compile = _$compile_;
    $rootScope = _$rootScope_;
    $httpBackend = _$httpBackend_;
    scope = $rootScope.$new();
    element = '<serve-tabs opportunity="opp"> </serve-tabs>';
    scope.opp = mockOpportunity;
    element = $compile(element)(scope);
    scope.$digest();
  }));

  describe("markup to be correct", function(){
    it("should have a highlighted tab", function(){
      expect(element.html()).toContain("<div class=\"serve-day-time row push-top\">")
    });
  });

});
