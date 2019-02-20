require('../../../app/app');

describe('KC Applicant Tool', function(){

  var mockPageInfo = setupPageInfo();
  var mockVolunteer = setupVolunteer();
  var pageParams = setupPageParams();
  var mockResponse = setupResponse();

  beforeEach(angular.mock.module('crossroads'));

  beforeEach(angular.mock.module(function($provide){
    $provide.value('CmsInfo', mockPageInfo);
    $provide.value('$state', { get: function() {} });
  }));

  beforeEach( inject(function(_$location_){
    var $location = _$location_;
    spyOn($location, 'search').and.returnValue(pageParams);
  }));

  var controller, $log, $httpBackend, MPTools, $window, $scope, Profile, CmsInfo;

  beforeEach(inject(function(_$controller_, _$log_, _MPTools_, _$window_, $injector){
    $scope = {};
    controller = _$controller_('KCApplicantController', { $scope: $scope});
    $log = _$log_;
    $window = _$window_;
    MPTools = _MPTools_;
    Profile = $injector.get('Profile');
    CmsInfo = $injector.get('CmsInfo');
    $httpBackend = $injector.get('$httpBackend');
    $httpBackend.expectGET(window.__env__['CRDS_GATEWAY_CLIENT_ENDPOINT'] + 'api/profile/' + controller.params.recordId + '/admin')
      .respond(200, mockVolunteer);
  }));

  it('should get the correct query parameters', function(){
    $httpBackend.flush();
    expect(controller.params.userGuid).toBe(pageParams.ug);
  });

  it('should have a null middle initial', function(){
    $httpBackend.flush();
    expect(controller.person.middleInitial).toBeNull();
  });

  it('should get the have the correct page info', function(){
    $httpBackend.flush();
    expect(controller.pageInfo).toBe(mockPageInfo.items[0].fields);
  });

  it('should query for a response', function(){
    $httpBackend.expectGET(window.__env__['CRDS_GATEWAY_CLIENT_ENDPOINT'] +
        'api/opportunity/getResponseForOpportunity/' + 115 + '/' + 12345678)
      .respond(200, mockResponse);
    $httpBackend.flush();
    expect(controller.responseId).toBe(mockResponse.responseId);
  });

  it('should show error when no response if available', function(){
    $httpBackend.expectGET(window.__env__['CRDS_GATEWAY_CLIENT_ENDPOINT'] +
        'api/opportunity/getResponseForOpportunity/' + 115 + '/' + 12345678)
      .respond(200, null);

    $httpBackend.flush();
    expect(controller.error).toBeTruthy();
  });


  // SETUPS FOR MOCK DATA //
  function setupPageInfo() {
    return {
      pages : [
      {
      accessDenied: '<p>Oops! Looks like you are not authorized to access this information.' +
        'If you think this is a mistake, please contact the system administrator.</p>',
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
      noExistingResponse: '<p>Oops! Please contact the group leader of the team you are looking to serve.' +
        'Looks like we don\'t have a request from you to join this team.</p>',
      opportunity: '115',
      pageType: 'VolunteerApplicationPage',
      parent: 82,
      content: '<p>Please complete this application.</p>',
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
  }

  function setupVolunteer() {
    return {
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
  }

  function setupPageParams(){
    return {
      dg:'8b6242c9-ea32-40f7-97a2-e2bb3524ced2',
      ug:'c29e64a5-820b-461f-a57c-5831d070d578',
      pageID:'292',
      recordID:mockVolunteer.contactId,
      recordDescription: mockVolunteer.lastName,
      s:'11467',
      sc:'1',
      p:0,
      v:387
    };
  }

  function setupResponse(){
    return {
      closedId: false,
      comments: null,
      eventId: 0,
      opportunityId: 115,
      participantId: 1446358,
      responseDate: '2015-06-19T09:55:34',
      responseId: 7646,
      responseResultId: null
    };
  }

});
