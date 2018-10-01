// TODO: This is a deprecated file due to the Pushpay work and is no longer in use
(function() {
  'use strict';

  module.exports = RecurringGivingModals;

  RecurringGivingModals.$inject = ['$modalInstance',
      '$rootScope',
      'DonationService',
      'GiveTransferService',
      'RecurringGiving',
      'donation',
      'programList'];

  function RecurringGivingModals($modalInstance,
                                 $rootScope,
                                 DonationService,
                                 GiveTransferService,
                                 RecurringGiving,
                                 donation,
                                 programList) {
    var vm = this;
    vm.dto = GiveTransferService;
    vm.programsInput = programList;
    vm.donation = donation;
    vm.cancel = cancel;
    vm.remove = remove;
    vm.edit = edit;
    vm.create = create;
    vm.impersonateDonorId = undefined;

    activate();

    function activate() {
      vm.impersonateDonorId = GiveTransferService.impersonateDonorId;
      RecurringGiving.loadDonationInformation(vm.programsInput, vm.donation, vm.impersonateDonorId);
    }

    function cancel() {
      $modalInstance.dismiss('cancel');
    }

    function remove() {
      vm.dto.processing = true;

      DonationService.deleteRecurringGift(vm.impersonateDonorId).then(function() {
        $modalInstance.close(true);
      }, function(/*error*/) {

        $modalInstance.close(false);
      });
    }

    function edit(recurringGiveForm) {
      RecurringGiving.updateGift(recurringGiveForm, successful, failure, vm.impersonateDonorId);
    }

    function create(recurringGiveForm) {
      RecurringGiving.createGift(recurringGiveForm, successful, failure, vm.impersonateDonorId);
    }

    function successful() {
      $modalInstance.close(true);
    }

    function failure() {
      $modalInstance.close(false);
    }

  };

})();
