require('../../../app/common/common.module');
require('../../../app/profile/profile.module');
require('../../../app/app');

describe('ProfileGivingController', function() {

  var httpBackend;
  var scope;
  var controllerConstructor;
  var sut;

  beforeEach(angular.mock.module('crossroads'));

  beforeEach(angular.mock.module(function($provide) {
    $provide.value('$state', { get: function() {} });
  }));

  var mockRecurringGiftsResponse = [
    {
      amount: 1000,
      recurrence: 'Fridays Weekly',
      program: 'Crossroads',
      source:
      {
        type: 'CreditCard',
        brand: 'Visa',
        last4: '1000',
        expectedViewBox: '0 0 160 100',
        expectedName: 'ending in 1000',
        expectedIcon: 'cc_visa'
      }
    },
    {
      amount: 2000,
      recurrence: '8th Monthly',
      program: 'Crossroads',
      source: {
        type: 'CreditCard',
        brand: 'MasterCard',
        last4: '2000',
        expectedViewBox: '0 0 160 100',
        expectedName: 'ending in 2000',
        expectedIcon: 'cc_mastercard'
      }
    },
    {
      amount: 3000,
      recurrence: '30th Monthly',
      program: 'Crossroads',
      source: {
        type: 'CreditCard',
        brand: 'AmericanExpress',
        last4: '3000',
        expectedViewBox: '0 0 160 100',
        expectedName: 'ending in 3000',
        expectedIcon: 'cc_american_express'
      }
    },
    {
      amount: 4000,
      recurrence: '21st Monthly',
      program: 'Crossroads',
      source: {
        type: 'CreditCard',
        brand: 'Discover',
        last4: '4000',
        expectedViewBox: '0 0 160 100',
        expectedName: 'ending in 4000',
        expectedIcon: 'cc_discover'
      }
    }
  ];

  var mockPledgeCommitmentsResponse = [
    {
      pledge_id: 330508,
      pledge_campaign_id: 852,
      donor_display_name: 'Pledge, Donor',
      pledge_campaign: 'Super Campaign',
      pledge_status: 'Active',
      campaign_start_date: '"10/1/2015',
      campaign_end_date: '"6/1/2019',
      total_pledge: 1500,
      pledge_donations: 155
    },
    {
      pledge_id: 330509,
      pledge_campaign_id: 528,
      donor_display_name: 'Commitment, Pledge',
      pledge_campaign: 'Long Campaign',
      pledge_status: 'Active',
      campaign_start_date: '10/1/2015',
      campaign_end_date: '6/1/2019',
      total_pledge: 150,
      pledge_donations: 65
    }
  ];

  beforeEach(inject(function(_$injector_, $httpBackend, _$controller_, $rootScope) {
      var $injector = _$injector_;

      httpBackend = $httpBackend;

      controllerConstructor = _$controller_;

      scope = $rootScope.$new();
    })
  );

  afterEach(function() {
    httpBackend.verifyNoOutstandingExpectation();
    httpBackend.verifyNoOutstandingRequest();
  });

});
