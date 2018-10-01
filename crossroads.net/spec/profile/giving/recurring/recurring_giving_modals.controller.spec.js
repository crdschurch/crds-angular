require('../../../../app/common/common.module');
require('../../../../app/app');

describe('RecurringGivingModals', function() {

  var vm;
  var rootScope;
  var scope;
  var modalInstance;
  var filter;
  var DonationService;
  var GiveTransferService;
  var RecurringGiving;
  var donation;
  var programList;
  var httpBackend;

  var mockProgramList = [
    {
      ProgramId: 1,
      Name: 'Crossroads'
    }
  ];

  var mockRecurringGift =
  {
    recurring_gift_id: 12,
    donor_id: 123,
    amount: 1000,
    recurrence: '8th Monthly',
    interval: 'month',
    program: 1,
    source:
    {
      type: 'CreditCard',
      brand: 'Visa',
      last4: '1000',
      icon: 'cc_visa',
      address_zip: '41983',
      exp_date: '2029-08-01T00:00:00',
      expectedViewBox: '0 0 160 100',
      expectedBrand: '#cc_visa',
      expectedCCNumberClass: 'cc_visa',
    },
  };

  beforeEach(angular.mock.module('crossroads'));

  beforeEach(angular.mock.module(function($provide) {
    $provide.value('$state', { get: function() {} });
  }));

  beforeEach(inject(function(_$controller_, $injector) {
    httpBackend = $injector.get('$httpBackend');
    rootScope = $injector.get('$rootScope');

    scope = rootScope.$new();
    filter = $injector.get('$filter');
    DonationService = $injector.get('DonationService');
    GiveTransferService = $injector.get('GiveTransferService');
    RecurringGiving = $injector.get('RecurringGiving');

    modalInstance = {                    // Create a mock object using spies
      close: jasmine.createSpy('modalInstance.close'),
      dismiss: jasmine.createSpy('modalInstance.dismiss'),
      result: {
        then: jasmine.createSpy('modalInstance.result.then')
      }
    };

    vm = _$controller_('RecurringGivingModals',
                           {$modalInstance: modalInstance,
                             $filter: filter,
                             DonationService: DonationService,
                             GiveTransferService: GiveTransferService,
                             donation: mockRecurringGift,
                             programList: mockProgramList});
  }));

});
