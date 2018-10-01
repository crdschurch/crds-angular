require('../../app/app');

describe('GivingHistoryController', function() {

  var httpBackend;
  var scope;
  var controllerConstructor;
  var GivingHistoryService;
  var sut;

  beforeEach(angular.mock.module('crossroads'));

  beforeEach(angular.mock.module(function($provide) {
    $provide.value('$state', { get: function() {} });
  }));

  var mockDonationYearsResponse =
  {
    years: ['2015', '2014'],
    most_recent_giving_year: '2015'
  };

  var mockDonationResponse =
  {
    donations: [
      {
        amount: 78900,
        status: 'Succeeded',
        date: '2015-05-14T09:52:44.873',
        distributions: [
          {
            program_name: 'Crossroads',
            amount: 78900
          }
        ],
        source:
        {
          type: 'CreditCard',
          last4: '1234',
          brand: 'Visa',
          payment_processor_id: 'ch_16jvzDEldv5NE53smr7Mwi4x'
        }
      },
      {
        amount: 1200,
        status: 'Succeeded',
        date: '2015-05-21T10:25:32.923',
        distributions: [
          {
            program_name: 'Game Change',
            amount: 1200
          }
        ],
        source:
        {
          type: 'Bank',
          last4: '4321',
          payment_processor_id: 'py_16YS8wEldv5NE53s770upiHw'
        },
      },
      {
        amount: 100,
        status: 'Deposited',
        date: '2015-05-28T10:25:32.923',
        distributions: [
          {
            program_name: 'Crossroads',
            amount: 100
          }
        ],
        source:
        {
          type: 'Cash',
          name: 'Cash'
        }
      }
    ],
    donation_total_amount: 80000,
    beginning_donation_date: '123',
    ending_donation_date: '456'
  };

  var mockSoftCreditDonationResponse =
  {
    donations: [
      {
        amount: 2000,
        status: 'Succeeded',
        date: '2015-03-31T09:52:44.873',
        distributions: [
          {
            program_name: 'Crossroads',
            amount: 2000
          }
        ],
        source:
        {
          type: 'SoftCredit',
          name: 'Fidelity Charity Account'
        }
      },
      {
        amount: 2000,
        status: 'Succeeded',
        date: '2015-06-30T10:25:32.923',
        distributions: [
          {
            program_name: 'Crossroads',
            amount: 2000
          }
        ],
        source:
        {
          type: 'SoftCredit',
          name: 'Fidelity Charity Account'
        }
      }
    ],
    donation_total_amount: 4000
  };

  beforeEach(inject(function(_$injector_, $httpBackend, _$controller_, $rootScope, _GivingHistoryService_) {
    var $injector = _$injector_;

    httpBackend = $httpBackend;

    controllerConstructor = _$controller_;

    scope = $rootScope.$new();

    GivingHistoryService = _GivingHistoryService_;
  })
  );

  afterEach(function() {
    httpBackend.verifyNoOutstandingExpectation();
    httpBackend.verifyNoOutstandingRequest();
  });

});
