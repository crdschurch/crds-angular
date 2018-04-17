require('../../app/app');

describe('MyServeController', function() {

  var mockSession;

  var retArray = [
    { "day": "5/16/2015", "serveTimes" : [ { "time": "8:30am", "name" : "Kids Club Nusery", "members" : [ { "name": "John", "roles" : [ {"name": "NuseryA"}, {"name": "NuseryB"}, {"name": "NuseryC"}, {"name": "NuseryD"} ] }, { "name":  "Jane", "roles" : [ {"name": "NuseryA"}, {"name": "NuseryB"}, {"name": "NuseryC"}, {"name": "NuseryD"} ], "signedup" : "yes" }, ] }, { "time": "8:30am", "name": "First Impressions", "members" : [ { "name": "John"}, {"name": "Jane" } ] } ] }];

  var more = [ { "day":"5/17/2015", "serveTimes" : [ { "time": "8:30am", "name" : "Kids Club Nusery", "members" : [ { "name": "John", "roles" : [ {"name": "NuseryA"}, {"name": "NuseryB"}, {"name": "NuseryC"}, {"name": "NuseryD"} ] }, { "name":  "Jane", "roles" : [ {"name": "NuseryA"}, {"name": "NuseryB"}, {"name": "NuseryC"}, {"name": "NuseryD"} ], "signedup" : "yes" }, ] }, { "time": "8:30am", "name": "First Impressions", "members" : [ { "name": "John"}, {"name": "Jane" } ] } ] }];


  beforeEach(angular.mock.module('crossroads'));

  beforeEach(angular.mock.module(function($provide){
    $provide.value('$state', { get: function() {} });
    $provide.value('Groups', retArray);
    $provide.value('leader', {isLeader: 'true'});
    mockSession= jasmine.createSpyObj('Session', ['exists', 'isActive']);
    mockSession.exists.and.callFake(function(something){
      return '12345678';
    });
    $provide.value('Session', mockSession);
  }));

  var $controller, $log, mockServeResource, $httpBackend, Session;
  beforeEach(inject(function(_$controller_, _$log_, $injector){
    $controller = _$controller_;
    $log = _$log_;
    $httpBackend = $injector.get('$httpBackend');
    mockServeResource = $injector.get('ServeOpportunities');
    Session = $injector.get('Session');
  }));

  describe('Serve Controller', function(){

    var $scope, controller;

    beforeEach(inject(function($log, $httpBackend){
      $scope = {};
      $scope.serveForm = {
        $dirty: false,
        $setPristine : function(){
          return true;
        }
      };
      controller = $controller('MyServeController', { $scope: $scope});
    }));

    it('should show the opportunities message', function(){
      controller.groups = [];
      expect(controller.showNoOpportunitiesMsg()).toBe(true);
    });

    it('should load more opportunities', function(){
      var lastDate = retArray[0].day
      var date = new Date(lastDate);
      date.setDate(date.getDate() + 1);
      var newDate = new Date(lastDate);
      newDate.setDate(newDate.getDate() + 8);
      expect(controller.groups.length).toBe(1);
      $httpBackend.expect('GET', window.__env__['CRDS_GATEWAY_CLIENT_ENDPOINT'] + 'api/serve/family-serve-days/12345678?from='+ date/1000+ '&to=' + newDate/1000).respond(200, more);
      controller.loadNextMonth();

      $httpBackend.flush();
      expect(controller.groups.length).toBe(2);
    });

  });

})
