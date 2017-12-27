require('../../app/app');

describe('Volunteer Application Factory', function() {

  var mockPageInfo = setupPageInfo();
  var mockAdult = setupPerson(32);
  var mockStudent = setupPerson(15);
  var mockUnderAge = setupPerson(9);
  var mockProfile = setupProfile();

  var $rootScope, scope, Page, Opportunity, VolunteerApplication, $httpBackend;

  beforeEach(angular.mock.module('crossroads'));

  beforeEach(angular.mock.module(function($provide) {
    $provide.value('$state', { get: function() {} });
  }));

  beforeEach(inject(function(_Page_, _Opportunity_, _VolunteerApplication_, $injector) {
    Page = _Page_;
    Opportunity = _Opportunity_;
    VolunteerApplication = _VolunteerApplication_;
    $httpBackend = $injector.get('$httpBackend');
  }));

  it('should fetch the page object', function(){
    VolunteerApplication.getPageInfo();
    let url = window.__env__['CRDS_CMS_CLIENT_ENDPOINT'] +
        'api/Page?link=%2Fvolunteer-application%2Fkids-club%2F';
    $httpBackend.expectGET(url).respond(200, mockPageInfo );
    $httpBackend.flush();
  });

  it('should show the adult application', function(){
    var showAdult = VolunteerApplication.show('adult', mockAdult );
    expect(showAdult).toBe(true);
  });

  it('should show the student application', function(){
    var showStudent = VolunteerApplication.show('student', mockStudent);
    expect(showStudent).toBe(true);
  });

  it('should show an error is the person is under 10', function(){
    var showError = VolunteerApplication.show('student', mockUnderAge);
    expect(showError).toBe(false);
  });

  it('should get the middle intial of the person', function(){
    expect(VolunteerApplication.middleInitial(mockProfile)).toBe('M');
  });

  it('should get the response for the volunteer', function(){
    VolunteerApplication.getResponse(115, 12345);
    $httpBackend.expectGET( window.__env__['CRDS_GATEWAY_CLIENT_ENDPOINT'] +
          'api/opportunity/getResponseForOpportunity/'+
          115 + '/' + 12345 ).respond(200);
    $httpBackend.flush();

  });


  /* MOCK DATA SETUP */

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

  function setupPerson(age){
    return {
      age: age,
      contactId: 2186211,
      email: 'matt.silbernagel@ingagepartners.com',
      lastName: null,
      loggedInUser: true,
      participantId: 2213526,
      preferredName: 'Matt',
      relationshipId: 0
    };
  }

  function setupProfile(){
    return {
      addressId: 1687838,
      addressLine1: '2322 Raeburn Ter',
      addressLine2: '',
      age: 15,
      anniversaryDate: '',
      city: 'Cincinnati',
      congregationId: 5,
      contactId: 2186211,
      dateOfBirth: '02/21/2000',
      emailAddress: 'matt.silbernagel@ingagepartners.com',
      employerName: null,
      firstName: 'Matt',
      foreignCountry: 'United States',
      genderId: 1,
      homePhone: '513-555-5555',
      householdId: 1709940,
      lastName: 'Silbernagel',
      maidenName: null,
      maritalStatusId: 2,
      middleName: 'Micheal',
      mobileCarrierId: null,
      mobilePhone: null,
      nickName: 'Matt',
      postalCode: '45223-1231',
      state: 'OH'
    };
  }

});
