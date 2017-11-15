(function() {
  'use strict';

  module.exports = RecurringGiving;

  var moment = require('moment');

  RecurringGiving.$inject = ['GiveTransferService',
    'DonationService',
    'GiveFlow',
    'Session',
    'PaymentService',
    '$state',
    '$filter',
    '$rootScope'];

  function RecurringGiving(GiveTransferService,
                           DonationService,
                           GiveFlow,
                           Session,
                           PaymentService,
                           $state,
                           $filter,
                           $rootScope) {
    var service = {
      name: 'RecurringGiving',
      initDefaultState: initDefaultState,
      resetGiveFlow: resetGiveFlow,
      goToAccount: goToAccount,
      stateName: stateName,
      goToChange: goToChange,
      goToLogin: goToLogin,
      submitBankInfo: submitBankInfo,
      processChange: processChange,
      getLoggedInUserDonorPaymentInfo: getLoggedInUserDonorPaymentInfo,
      resetGiveTransferServiceGiveType: resetGiveTransferServiceGiveType,
      loadDonationInformation: loadDonationInformation,
      updateGift: updateGift,
      createGift: createGift,
      frequencyCalculation: frequencyCalculation,
    };

    function initDefaultState() {
      GiveTransferService.reset();
      GiveTransferService.processing = false;

      resetGiveFlow();
      GiveTransferService.initialized = true;
      GiveTransferService.givingType = 'month';

      Session.removeRedirectRoute();
      $state.go(GiveFlow.amount);
    }

    function resetGiveFlow() {
      // Setup the give flow service
      GiveFlow.reset({
        amount: 'give.amount',
        account: 'give.recurring_account',
        login: 'give.recurring_login',
        register: 'give.register',
        confirm: 'give.recurring_account',
        change: 'give.recurring_change',
        thankYou: 'give.recurring_thank-you'
      });
    }

    function resetGiveTransferServiceGiveType() {
      GiveTransferService.givingType = 'month';
    }

    function stateName(state) {
      return GiveFlow[state];
    }

    function goToAccount(giveForm) {
      GiveFlow.goToAccount(giveForm);
    }

    function goToChange() {
      GiveFlow.goToChange();
    }

    function goToLogin() {
      GiveFlow.goToLogin();
    }

    function submitBankInfo(giveForm, programsInput) {
      GiveTransferService.bankinfoSubmitted = true;
      if (giveForm.accountForm.$valid) {
        DonationService.createRecurringGift();
      } else {
        $rootScope.$emit('notify', $rootScope.MESSAGES.generalError);
      }
    }

    function processChange() {
      DonationService.processChange();
    }

    function getLoggedInUserDonorPaymentInfo(event, toState) {
    }

    function loadDonationInformation(programsInput, donation = null, impersonateDonorId = null) {
      GiveTransferService.reset();

      GiveTransferService.impersonateDonorId = impersonateDonorId;
      GiveTransferService.amountSubmitted = false;
      GiveTransferService.bankinfoSubmitted = false;
      GiveTransferService.changeAccountInfo = true;
      GiveTransferService.initialized = true;
      setupDonor(donation, impersonateDonorId);

      if (donation !== null) {
        GiveTransferService.recurringGiftId = donation.recurring_gift_id;
        GiveTransferService.amount = donation.amount;
        GiveTransferService.brand = '#' + donation.source.icon;
        GiveTransferService.ccNumberClass = donation.source.icon;
        GiveTransferService.givingType = donation.interval;
        GiveTransferService.last4 = donation.source.last4;
        GiveTransferService.program = $filter('filter')(programsInput, {ProgramId: donation.program})[0];
        GiveTransferService.recurringStartDate = donation.start_date;
        GiveTransferService.view = donation.source.type === 'CreditCard' ? 'cc' : 'bank';
        GiveTransferService.donor.default_source.bank_account.accountHolderType = donation.source.account_holder_type;
        GiveTransferService.donor.default_source.bank_account.accountHolderName = donation.source.account_holder_name;
        setupInterval(donation);
      }
    }

    function setupDonor(donation, impersonateDonorId = null) {
      GiveTransferService.donor = {
        id: (impersonateDonorId === null ? donation.donor_id : impersonateDonorId),
        default_source: {
          credit_card: {
            last4: null,
            brand: null,
            address_zip: null,
            exp_date: null,
          },
          bank_account: {
            routing: null,
            last4: null,
            accountHolderName: null,
            accountHolderType: null
          },
        },
      };

      if (donation !== null && donation.source.type === 'CreditCard') {
        GiveTransferService.donor.default_source.credit_card.last4 = donation.source.last4;
        GiveTransferService.donor.default_source.credit_card.brand = donation.source.brand;
        GiveTransferService.donor.default_source.credit_card.address_zip = donation.source.address_zip;
        GiveTransferService.donor.default_source.credit_card.exp_date = moment(donation.source.exp_date).format('MMYY');
      } else if (donation !== null) {
        GiveTransferService.donor.default_source.bank_account.last4 = donation.source.last4;
        GiveTransferService.donor.default_source.bank_account.routing = donation.source.routing;
        GiveTransferService.donor.default_source.bank_account.accountHolderName = donation.source.account_holder_name;
        GiveTransferService.donor.default_source.bank_account.accountHolderType = donation.source.account_holder_type;
      }
    }

    function setupInterval(donation) {
      if (donation.interval !== null) {
        GiveTransferService.interval = _.capitalize(donation.interval.toLowerCase()) + 'ly';
      }
    }

    function createGift(recurringGiveForm, success, failure, impersonateDonorId = null) {
      GiveTransferService.processing = true;
      GiveTransferService.amountSubmitted = true;

      if (!validForm(recurringGiveForm, false)) {
        return;
      }

      // Credit card or bank account info is touched so update token from strip
      DonationService.adminCreateRecurringGift(impersonateDonorId).then(function() {
        success();
      }, function(/*error*/) {

        failure();
      });
    }

    function updateGift(recurringGiveForm, success, failure, impersonateDonorId = null) {
      GiveTransferService.processing = true;
      GiveTransferService.amountSubmitted = true;

      if (!validForm(recurringGiveForm, true)) {
        return;
      }

      // Form is valid so update
      if ((recurringGiveForm.creditCardForm !== undefined && recurringGiveForm.creditCardForm.$dirty) ||
          (recurringGiveForm.bankAccountForm !== undefined && recurringGiveForm.bankAccountForm.$dirty)) {
        // Credit card or bank account info is touched so update token from strip
        DonationService.updateRecurringGift(true, impersonateDonorId).then(function() {
          success();
        }, function(/*error*/) {

          failure();
        });
      } else if (recurringGiveForm.donationDetailsForm && recurringGiveForm.donationDetailsForm.$dirty) {
        // Credit card or bank account info was not touched so do not update token from strip
        DonationService.updateRecurringGift(false, impersonateDonorId).then(function() {
          success();
        }, function(/*error*/) {

          failure();
        });
      } else {
        // Nothing touched so just close
        success();
      }
    }

    function validForm(recurringGiveForm, allowPristine) {
      // Amount is not valid
      if (recurringGiveForm.donationDetailsForm !== undefined && !recurringGiveForm.donationDetailsForm.amount.$valid) {
        $rootScope.$emit('notify', $rootScope.MESSAGES.generalError);
        GiveTransferService.processing = false;
        return false;
      }

      // Recurring Start Date was touched and is not valid - We don't want to validate if they are not updating this field
      if (recurringGiveForm.donationDetailsForm !== undefined &&
          (recurringGiveForm.donationDetailsForm.recurringStartDate === undefined ||
            (recurringGiveForm.donationDetailsForm.recurringStartDate.$dirty &&
            !recurringGiveForm.donationDetailsForm.recurringStartDate.$valid))) {
        $rootScope.$emit('notify', $rootScope.MESSAGES.generalError);
        GiveTransferService.processing = false;
        return false;
      }

      // Validate the credit card or bank account form
      var accountForm = recurringGiveForm.creditCardForm !== undefined ?
          recurringGiveForm.creditCardForm : recurringGiveForm.bankAccountForm;
      if (accountForm !== undefined && !accountForm.$valid && !accountForm.$dirty && !allowPristine) {
        $rootScope.$emit('notify', $rootScope.MESSAGES.generalError);
        GiveTransferService.processing = false;
        return false;
      }

      return true;
    }

    function frequencyCalculation() {
      var startDate = moment(GiveTransferService.recurringStartDate);

      if (GiveTransferService.givingType == 'month') {
        return  'the ' + startDate.format('Do') + ' of the Month';
      }

      return 'Every ' + startDate.format('dddd');
    }

    return service;
  }

})();

