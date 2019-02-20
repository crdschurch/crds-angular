require('../../app/app');

describe('Volunteer Application Controller', function() {

  var controller,
      $rootScope,
      $scope,
      httpBackend,
      mockSession,
      Session,
      CmsInfo,
      Contact,
      Opportunity,
      Family,
      PageInfo;

  var mockFamily = [{
     contactId : 768379,
     participantId : 994377,
     'preferredName': 'Tony',
     'lastName': null,
     'loggedInUser': true,
     'email': 'tmaddox33mp1@gmail.com',
     'relationshipId': 0
  }, {
     contactId: 1519134,
     participantId: 1446324,
     'preferredName': 'Brady',
     'lastName': 'Queenan',
     'loggedInUser': false,
     'email': null,
     'relationshipId': 6
  }, {
     'contactId': 768386,
     'participantId': 1446320,
     'preferredName': 'Claire',
     'lastName': 'Maddox',
     'loggedInUser': false,
     'email': 'tmaddox33mp1@gmail.com',
     'relationshipId': 6
  }, {
     'contactId': 1519207,
     'participantId': 1446358,
     'preferredName': 'Jack',
     'lastName': 'Maddox',
     'loggedInUser': false,
     'email': 'lsangam@yahoo.com',
     'relationshipId': 6
  }, {
     'contactId': 2186211,
     'participantId': 2213526,
     'preferredName': 'Matt',
     'lastName': 'Silbernagel',
     'loggedInUser': false,
     'email': 'matt.silbernagel@ingagepartners.com',
     'relationshipId': 6
  }];

  var mockVolunteer= {
    addressId: 99999,
    addressLine1: '9000 Observatory Lane',
    addressLine2: '',
    age: 35,
    anniversaryDate: '',
    city: 'Cincinnati',
    congregationId: 5,
    contactId: '12345678',
    dateOfBirth: '04/03/2005',
    emailAddress: 'matt.silbernagel@ingagepartners.com',
    employerName: null,
    firstName: 'Miles',
    foreignCountry: 'United States',
    genderId: 1,
    homePhone: '513-555-5555',
    householdId: 1709940,
    lastName: 'Silbernagel',
    maidenName: null,
    maritalStatusId: 2,
    middleName: null,
    mobileCarrierId: null,
    mobilePhone: null,
    nickName: 'Miles',
    postalCode: '45223-1231',
    state: 'OH'
  };

  var mockContactId = '12345678';
  var mockResponseId = 34567;

  var mockPageInfo = {
    pages : [
    {
    accessDenied: `<p>Oops! Looks like you are not authorized to access this information. 
      If you think this is a mistake, please contact the system administrator.</p>`,
    canEditType: 'Inherit',
    canViewType: 'Inherit',
    content: '<p>Please complete this application.</p>',
    extraMeta: null,
    group: '27705',
    hasBrokenFile: '0',
    hasBrokenLink: '0',
    id: 83,
    link: '/volunteer-application/kids-club/',
    menuTitle: null,
    metaDescription: null,
    noExistingResponse: `<p>Oops! Please contact the group leader of the team you are looking to serve.
      Looks like we don't have a request from you to join this team.</p>`,
    opportunity: '115',
    pageType: 'VolunteerApplicationPage',
    parent: 82,
    reportClass: null,
    showInMenus: '1',
    showInSearch: '1',
    sort: '1',
    success: '<p>Default SUCCESS text for this page, see ApplicationPage.php to change</p>',
    title: 'Kids Club',
    uRLSegment: 'kids-club',
    version: '15'
    }]
  };

  beforeEach(angular.mock.module('crossroads'));

  beforeEach(angular.mock.module(function($provide) {
    $provide.value('$state', { get: function() {} });
  }));

  describe('Not in Family', function(){

    beforeEach(angular.mock.module(function($provide){
      $provide.value('$stateParams', { id: 12345678});
      mockSession= jasmine.createSpyObj('Session', ['exists', 'isActive']);
      mockSession.exists.and.callFake(function(something){
        return '12345678';
      });
      $provide.value('Session', mockSession);
      $provide.value('PageInfo', {contact: mockVolunteer, cmsInfo: mockPageInfo});
      $provide.value('Family', mockFamily);
    }));

    var controller;

    beforeEach(inject(function(_$controller_, _$log_, $injector ){
      controller = _$controller_;
      var log = _$log_;

      httpBackend = $injector.get('$httpBackend');

      Opportunity = $injector.get('Opportunity');
      PageInfo = $injector.get('PageInfo');
      Session = $injector.get('Session');
      Family = $injector.get('Family');

      $scope = {};
      controller = controller('VolunteerApplicationController', { $scope: $scope });

    }));

    it('should not allow access if not in family', function(){
      expect(controller.showAccessDenied).toBe(true);
    });
  });

  describe('In Family', function(){

    beforeEach(angular.mock.module(function($provide){
      $provide.value('$stateParams', { id: 2186211});
      mockSession= jasmine.createSpyObj('Session', ['exists', 'isActive']);
      mockSession.exists.and.callFake(function(something){
        return '2186211';
      });
      $provide.value('Session', mockSession);
      $provide.value('PageInfo', {contact: mockVolunteer, cmsInfo: mockPageInfo});
      $provide.value('Family', mockFamily);
    }));

    var controller;

    beforeEach(inject(function(_$controller_, _$log_, $injector ){
      controller = _$controller_;
      var log = _$log_;

      httpBackend = $injector.get('$httpBackend');
      Opportunity = $injector.get('Opportunity');
      PageInfo = $injector.get('PageInfo');
      Session = $injector.get('Session');
      Family = $injector.get('Family');

      $scope = {};
      controller = controller('VolunteerApplicationController', { $scope: $scope });

    }));

    it('should allow access if in family', function(){
      expect(controller.showAccessDenied).toBe(false);
    });

    it('should call the opportunity resonponse service', function(){
      httpBackend.expectGET( window.__env__['CRDS_GATEWAY_CLIENT_ENDPOINT'] +
          'api/opportunity/getResponseForOpportunity/'+
          mockPageInfo.items[0].fields.opportunity + '/' + controller.contactId ).respond(200);

      httpBackend.flush();
    });

    it('should show adult', function(){
      expect(controller.showAdult).toBe(true);
      expect(controller.showStudent).toBe(false);
    });

  });

});
