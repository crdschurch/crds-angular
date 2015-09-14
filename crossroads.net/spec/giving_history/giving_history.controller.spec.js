require('crds-core');
require('../../app/app');
require('../../app/giving_history');

describe('GivingHistoryController', function() {

  var httpBackend;
  var scope;
  var controllerConstructor;
  var sut;

  beforeEach(angular.mock.module('crossroads'));

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
    donation_total_amount: 80000
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
          type: 'NonCashAsset',
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
          type: 'NonCashAsset',
          name: 'Fidelity Charity Account'
        }
      }
    ],
    donation_total_amount: 4000
  };

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

  describe('On initialization', function() {
    beforeEach(function() {
      sut = controllerConstructor('GivingHistoryController', {$scope: scope});

      httpBackend.whenGET(/SiteConfig*/).respond('');

    });

    it('should retrieve most recent giving year donations for current user', function() {
      httpBackend.whenGET(/SiteConfig*/).respond('');
      httpBackend.expectGET(window.__env__['CRDS_API_ENDPOINT'] + 'api/profile').respond({});
      httpBackend.expectGET(window.__env__['CRDS_API_ENDPOINT'] + 'api/donations/years')
                             .respond(mockDonationYearsResponse);
      httpBackend.expectGET(window.__env__['CRDS_API_ENDPOINT'] + 'api/donations/2015')
                             .respond(mockDonationResponse);
      httpBackend.flush();

      expect(sut.donation_years.length).toBe(3);
      expect(sut.donation_years[0]).toEqual({key: '2015', value: '2015'});
      expect(sut.donation_years[2]).toEqual({key: '', value: 'All'});
      expect(sut.most_recent_giving_year).toEqual('2015');

      expect(sut.donations.length).toBe(3);
      expect(sut.donations[0].distributions[0].program_name).toEqual('Crossroads');
      expect(sut.donation_total_amount).toEqual(80000);
    });
  });
});
