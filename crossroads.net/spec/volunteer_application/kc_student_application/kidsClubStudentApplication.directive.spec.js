describe("KidsClub Student Application Directive", function() {

  var $compile, $rootScope, $log, element, scope, controller, isolateScope;
  
  var mockVolunteer= {
    addressId: 99999,
    addressLine1: "9000 Observatory Lane",
    addressLine2: "",
    age: 35,
    anniversaryDate: "",
    city: "Cincinnati",
    congregationId: 5,
    contactId: 2186211,
    dateOfBirth: "04/03/2005",
    emailAddress: "matt.silbernagel@ingagepartners.com",
    employerName: null,
    firstName: "Miles",
    foreignCountry: "United States",
    genderId: 1,
    homePhone: "513-555-5555",
    householdId: 1709940,
    lastName: "Silbernagel",
    maidenName: null,
    maritalStatusId: 2,
    middleName: null,
    mobileCarrierId: null,
    mobilePhone: null,
    nickName: "Miles",
    postalCode: "45223-1231",
    state: "OH" 
  };

  var mockContactId = 12345;
  var mockResponseId = 34567;
  var mockPageInfo = {
    accessDenied: "<p>Oops! Looks like you are not authorized to access this information. If you think this is a mistake, please contact the system administrator.</p>",
    canEditType: "Inherit",
    canViewType: "Inherit",
    content: "<p>Please complete this application.</p>",
    extraMeta: null,
    group: "27705",
    hasBrokenFile: "0",
    hasBrokenLink: "0",
    id: 83,
    link: "/volunteer-application/kids-club/",
    menuTitle: null,
    metaDescription: null,
    noExistingResponse: "<p>Oops! Please contact the group leader of the team you are looking to serve. Looks like we don't have a request from you to join this team.</p>",
    opportunity: "115",
    pageType: "VolunteerApplicationPage",
    parent: 82,
    renderedContent: "<p>Please complete this application.</p>",
    reportClass: null,
    showInMenus: "1",
    showInSearch: "1",
    sort: "1",
    success: "<p>Default SUCCESS text for this page, see ApplicationPage.php to change</p>",
    title: "Kids Club",
    uRLSegment: "kids-club",
    version: "15"
  };

  beforeEach(function() {
    module('crossroads');
  });

  beforeEach(inject(function(_$compile_, _$rootScope_, _$q_, _$log_){
    $compile = _$compile_;
    $rootScope = _$rootScope_;
    $q = _$q_;
    $log = _$log_;
    scope = $rootScope.$new();
    element = "<kids-club-student-application volunteer='volunteer' contact-id='contactId' page-info='pageInfo' response-id='responseId'></kids-club-student-application>";
    scope.volunteer = mockVolunteer;
    scope.contactId = mockContactId;
    scope.pageInfo = mockPageInfo;
    scope.responseId = mockResponseId;
    element = $compile(element)(scope);
    scope.$digest();
    isolateScope = element.isolateScope();
  }));



});
