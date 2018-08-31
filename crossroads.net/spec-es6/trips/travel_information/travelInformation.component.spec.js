const helpers = require('../trips.helpers');


describe('Travel Information Component', () => {
  beforeEach(angular.mock.module('crossroads.trips'));
  // beforeEach(angular.mock.module(($provide) => {
    // any setup from the route goes here
    // example: $provide.value('MyTrips', myTrips);
  // }));

  // eslint-disable-next-line no-underscore-dangle
  const endpoint = `${window.__env__.CRDS_GATEWAY_CLIENT_ENDPOINT}api`;

  let componentController;
  let rootScope;
  let travelInformation;
  let httpBackend;
  let travelInformationService;
  let preferredAirportService;
  let state;
  const contactId = 898989;

  beforeEach(inject((_$componentController_,
    _Validation_,
    _$rootScope_,
    _AttributeTypeService_,
    _PreferredAirportService_,
    _$httpBackend_,
    _$cookies_,
    _$state_,
    _TravelInformationService_) => {
    componentController = _$componentController_;
    preferredAirportService = _PreferredAirportService_;
    rootScope = _$rootScope_;
    httpBackend = _$httpBackend_;
    state = _$state_;
    travelInformationService = _TravelInformationService_;

    rootScope.MESSAGES = {
      generalError: 'generalError'
    };

    spyOn(travelInformationService, 'resetPerson').and.callThrough();
    spyOn(rootScope, '$emit').and.callThrough();
    spyOn(_$cookies_, 'get').and.returnValue(contactId);
    spyOn(state, 'go').and.callThrough();
    spyOn(preferredAirportService, 'GetCommonAirportNameOrOther')
      .and.callFake( (airport, commonAirports) => 'Random mock text' );
    spyOn(preferredAirportService, 'GetPreferredAirportName').and.callFake(p => 'Random mock text');
    spyOn(preferredAirportService, 'SetAttributeNotesAndStartDate')
      .and.callFake( (preferredAirportAttribute, preferredAirportName) => {preferredAirportAttribute});
    spyOn(preferredAirportService, 'DoesPersonHaveValidPreferredAirportNameAttributeSet')
      .and.callFake(p => false);
    spyOn(preferredAirportService, 'UpdateOrSetPreferredAirportAttributeType')
      .and.callFake( (person, preferredAirportAttributeType ) => person);


    const bindings = {};
    travelInformation = componentController('travelInformation', null, bindings);
  }));

  afterEach(() => {
    httpBackend.verifyNoOutstandingExpectation();
    httpBackend.verifyNoOutstandingRequest();
  });

  describe('Passport information already in MP', () => {
    beforeEach(() => {
      spyOn(travelInformationService, 'getPerson').and.returnValue(helpers.TripInfo.currentPerson);
      httpBackend.expectGET(`${endpoint}/AttributeType/63`).respond({ attributes: helpers.TripInfo.attributes });
      travelInformation.$onInit();
      httpBackend.flush();
    });

    it('should set validPassport to true', () => {
      expect(travelInformation.validPassport).toBeTruthy();
    });
  });

  describe('Passport information NOT already in MP', () => {
    beforeEach(() => {
      const person = helpers.TripInfo.currentPerson;
      person.passportNumber = null;
      spyOn(travelInformationService, 'getPerson').and.returnValue(person);
      httpBackend.expectGET(`${endpoint}/AttributeType/63`).respond({ attributes: helpers.TripInfo.attributes });
      travelInformation.$onInit();
      httpBackend.flush();
    });

    it('should set validPassport to null', () => {
      expect(travelInformation.validPassport).toBeNull();
    });
  });

  describe('Regardless of result of getting frequent flyer attributes', () => {
    beforeEach(() => {
      spyOn(travelInformationService, 'getPerson').and.returnValue(helpers.TripInfo.currentPerson);
      httpBackend.expectGET(`${endpoint}/AttributeType/63`).respond({ attributes: helpers.TripInfo.attributes });
      travelInformation.$onInit();
      httpBackend.flush();
    });

    it('should get the frequent flyer value', () => {
      helpers.TripInfo.currentPerson.attributeTypes['63'].attributes.forEach((ff) => {
        const flyerNotes = travelInformation.frequentFlyerValue(ff.attributeId);
        expect(flyerNotes).toBe(ff.notes);
      });
    });

    it('should get the frequent flyer value for american airlines', () => {
      const flyerNotes = travelInformation.frequentFlyerValue(9049);
      expect(flyerNotes).toBe('090909890234j23jklhsdfasf');
    });

    it('should get null notes for united airlines', () => {
      const flyerNotes = travelInformation.frequentFlyerValue(3960);
      expect(flyerNotes).toBeNull();
    });

    it('should display error message when trying to save an invalid form', () => {
      travelInformation.submit();
      expect(rootScope.$emit).toHaveBeenCalledWith('notify', rootScope.MESSAGES.generalError);
    });

    it('should save the profile', () => {
      travelInformation.travelInfoForm.$valid = true;
      const ff = travelInformation.buildFrequentFlyers();
      const person = helpers.TripInfo.currentPerson;
      person.attributeTypes['63'].attributes = ff;
      httpBackend.expectPOST(`${endpoint}/profile`, person).respond(200);
      travelInformation.submit();
      expect(rootScope.$emit).not.toHaveBeenCalledWith('notify', rootScope.MESSAGES.generalError);
      httpBackend.flush();
    });

    it('should go to the mytrips page after saving the profile', () => {
      travelInformation.travelInfoForm.$valid = true;
      const ff = travelInformation.buildFrequentFlyers();
      const person = helpers.TripInfo.currentPerson;
      person.attributeTypes['63'].attributes = ff;
      httpBackend.expectPOST(`${endpoint}/profile`, person).respond(200, {});
      travelInformation.submit();
      httpBackend.flush();
      expect(state.go).toHaveBeenCalledWith('mytrips');
    });

    it('should reset the current user after saving the profile', () => {
      travelInformation.travelInfoForm.$valid = true;
      const ff = travelInformation.buildFrequentFlyers();
      const person = helpers.TripInfo.currentPerson;
      person.attributeTypes['63'].attributes = ff;
      httpBackend.expectPOST(`${endpoint}/profile`, person).respond(200, {});
      travelInformation.submit();
      httpBackend.flush();
      expect(travelInformationService.resetPerson).toHaveBeenCalled();
    });
  });

  describe('Errors getting frequent flyer attributes', () => {
    beforeEach(() => {
      spyOn(travelInformationService, 'getPerson').and.returnValue(helpers.TripInfo.currentPerson);
      httpBackend.expectGET(`${endpoint}/AttributeType/63`).respond(500);
      travelInformation.$onInit();
      httpBackend.flush();
    });

    it('should set an error message', () => {
      expect(travelInformation.error).not.toBeNull();
    });
  });
});
