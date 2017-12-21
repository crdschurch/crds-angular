require('../../../../app/common/common.module');
require('../../../../app/trips/trips.module');

describe('Trip Deposit Controller', () => {

  let rootScope,
      scope,
      state,
      timeout,
      session,
      donationService,
      giveTransferService,
      giveFlow,
      auth_events,
      log,
      controller,
      componentController,
      tripDeposit,
      tripsSignup,
      stateParams,
      httpBackend;

  const pledgeCampaignId = 898989;
  const pledgeId = 9909090;
  const depositAmount = 300;
  const pledgeCampaignName = 'GO India';
  const programName = 'Go India';
  const programId = 98763;
  const donorId = 23489734;
  const contactId = 12345679945945;
  const email = 'email@email.com';

  const endpoint = window.__env__['CRDS_GATEWAY_CLIENT_ENDPOINT'] + 'api';

  beforeEach(() => angular.mock.module('crossroads.trips'));

  // injections
  beforeEach(inject((_$componentController_, _$rootScope_, $injector) => {
    rootScope = _$rootScope_;
    scope = _$rootScope_.$new();
    log = $injector.get('$log');
    state = $injector.get('$state');
    timeout = $injector.get('$timeout');
    session = $injector.get('Session');
    donationService = $injector.get('DonationService');
    giveTransferService = $injector.get('GiveTransferService');
    auth_events = $injector.get('AUTH_EVENTS');
    giveFlow = $injector.get('GiveFlow');
    componentController = _$componentController_;
    tripDeposit = $injector.get('TripDeposit');
    tripsSignup = $injector.get('TripsSignupService');
    stateParams = $injector.get('$stateParams');
    tripsSignup.person = {contactId};
    stateParams.contactId = contactId;
    stateParams.campaignId = pledgeCampaignId;
    rootScope.email = email;
    httpBackend = $injector.get('$httpBackend');
  }));

  // setup component
  beforeEach( () => {
    // setup the signup service
    tripsSignup.donorId = donorId;
    tripsSignup.programId = programId;
    tripsSignup.programName = programName;
    tripsSignup.pledgeId = pledgeId;
    tripsSignup.depositAmount = depositAmount;
    tripsSignup.campaign = {name: pledgeCampaignName, id: pledgeCampaignId};
    controller = componentController('tripDeposit', { $scope: scope }, {});
  });

  // setup spies
  beforeEach( () => {
    spyOn(scope, '$on').and.callThrough();
    spyOn(tripDeposit, 'initDefaultState');
    spyOn(state, 'go');
    spyOn(session, 'isActive').and.returnValue(true);
    spyOn(donationService, 'confirmDonation');
  });

  describe('application is valid', () => {

    beforeEach( () => {
      tripsSignup.applicationValid = true;
      controller.$onInit();
    });

    it('should set the components with the correct start values', () => {
      commonExpectations();
    });

    it('should submit the application if deposit was successful', () => {
      commonExpectations();
      controller.saveDeposit();
      expect(donationService.confirmDonation).toHaveBeenCalled();
    });


  });

  describe('applicaion not valid', () => {

    beforeEach( () => {
      controller.$onInit();
    });

    it('should redirect if application was not filled out before reaching deposit page', () => {
      expect(state.go).toHaveBeenCalledWith('tripsignup', {campaignId: pledgeCampaignId});
    });

  });

  function commonExpectations() {
    expect(tripDeposit.initDefaultState).toHaveBeenCalledWith(
      {
        ProgramId: programId,
        Name: programName
      },
      {
        campaignId: pledgeCampaignId,
        campaignName: pledgeCampaignName,
        pledgeDonorId: donorId
      },
      depositAmount
    );
    expect(scope.$on).toHaveBeenCalled();
    expect(state.go).toHaveBeenCalledWith('tripdeposit.account', {contactId, campaignId: pledgeCampaignId });
  }

});
