require('../../../app/common/common.module');
require('../../../app/app');

describe('RecurringGiving Service', function() {
  var fixture;
  var GiveTransferService;
  var DonationService;
  var GiveFlow;
  var Session;
  var $state;
  var $filter;
  var $rootScope;

  beforeEach(angular.mock.module('crossroads'));

  beforeEach(angular.mock.module(function($provide) {
    DonationService = {
      success: true,
      setSuccess: function(b) {
        this.success = b;
      },

      getSuccess: function() {
        return (this.success);
      },

      adminCreateRecurringGift: function(donorId) {
        var vm = this;
        return ({
          then: function(success, failure) {
            if (vm.getSuccess()) {
              success();
            } else {
              failure();
            }
          }
        });
      }
    };

    $state = { get: function() {} };

    $provide.value('DonationService', DonationService);
    $provide.value('$state', $state);
  }));

  beforeEach(inject(function(_RecurringGiving_, _GiveTransferService_, _$rootScope_) {
    fixture = _RecurringGiving_;
    GiveTransferService = _GiveTransferService_;
    $rootScope = _$rootScope_;
  }));

  describe('Function createGift', function() {
    var callback;
    var recurringGiveForm;

    beforeEach(function() {
      GiveTransferService.processing = false;
      GiveTransferService.amountSubmitted = false;

      callback = jasmine.createSpyObj('callback', ['onSuccess', 'onFailure']);
      recurringGiveForm = {};
    });

    it('should set amountSubmitted to true', function() {
      fixture.createGift(recurringGiveForm, callback.onSuccess, callback.onFailure, null);
      expect(GiveTransferService.processing).toBeTruthy();
      expect(GiveTransferService.amountSubmitted).toBeTruthy();
      expect(callback.onSuccess).toHaveBeenCalled();
      expect(callback.onFailure).not.toHaveBeenCalled();
    });
  });

  describe('Function updateGift', function() {
    var callback;
    var recurringGiveForm;

    beforeEach(function() {
      GiveTransferService.processing = false;
      GiveTransferService.amountSubmitted = false;

      callback = jasmine.createSpyObj('callback', ['onSuccess', 'onFailure']);
      recurringGiveForm = {
        donationDetailsForm: {
          $dirty: false,
          amount: {
            $valid: true
          },
          recurringStartDate: {
            $dirty: true,
            $valid: true
          }
        }
      };
    });

    it('should set amountSubmitted to true', function() {
      fixture.updateGift(recurringGiveForm, callback.onSuccess, callback.onFailure, null);
      expect(GiveTransferService.processing).toBeTruthy();
      expect(GiveTransferService.amountSubmitted).toBeTruthy();
      expect(callback.onSuccess).toHaveBeenCalled();
      expect(callback.onFailure).not.toHaveBeenCalled();
    });
  });

  describe('Function submitBankInfo', function() {
    var giveForm;
    beforeEach(function() {
      DonationService.createRecurringGift = jasmine.createSpy('createRecurringGift');
      GiveTransferService.bankinfoSubmitted = false;

      giveForm = {
        accountForm: {
          $valid: undefined
        }
      };
    });

    it('should call DonationService.createRecurringGift if form is valid', function() {
      giveForm.accountForm.$valid = true;
      fixture.submitBankInfo(giveForm, []);

      expect(GiveTransferService.bankinfoSubmitted).toBeTruthy();
      expect(DonationService.createRecurringGift).toHaveBeenCalled();
    });

    it('should not call donation service if form is invalid', function() {
      $rootScope.$emit = jasmine.createSpy('$emit');
      $rootScope.MESSAGES = {
        generalError: 123
      };

      giveForm.accountForm.$valid = false;
      fixture.submitBankInfo(giveForm, []);

      expect(GiveTransferService.bankinfoSubmitted).toBeTruthy();
      expect($rootScope.$emit).toHaveBeenCalledWith('notify', $rootScope.MESSAGES.generalError);
      expect(DonationService.createRecurringGift).not.toHaveBeenCalled();
    });

  });

  describe('Function frequencyCalculation', function() {
    it('should show the right frequency period', function() {
      GiveTransferService.givingType = 'week'
      GiveTransferService.recurringStartDate = '1/5/2016'
      expect(fixture.frequencyCalculation()).toBe('Every Tuesday');
      GiveTransferService.recurringStartDate = '1/8/2016'
      expect(fixture.frequencyCalculation()).toBe('Every Friday');

      GiveTransferService.givingType = 'month'
      GiveTransferService.recurringStartDate = '1/5/2015'
      expect(fixture.frequencyCalculation()).toBe('the 5th of the Month');
      GiveTransferService.recurringStartDate = '1/3/2015'
      expect(fixture.frequencyCalculation()).toBe('the 3rd of the Month');
      GiveTransferService.recurringStartDate = '1/1/2015'
      expect(fixture.frequencyCalculation()).toBe('the 1st of the Month');
      GiveTransferService.recurringStartDate = '1/22/2015'
      expect(fixture.frequencyCalculation()).toBe('the 22nd of the Month');
    });
  });
});
