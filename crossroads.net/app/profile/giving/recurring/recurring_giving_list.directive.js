// TODO: This is a deprecated file due to the Pushpay work and is no longer in use
(function() {
  'use strict()';

  module.exports = RecurringGivingList;

  RecurringGivingList.$inject = ['$rootScope', '$log', '$modal', 'PaymentDisplayDetailService', 'DonationService', 'GiveTransferService'];

  function RecurringGivingList($rootScope, $log, $modal, PaymentDisplayDetailService, DonationService, GiveTransferService) {
    return {
      restrict: 'EA',
      transclude: true,
      templateUrl: 'templates/recurring_giving_list.html',
      scope: {
        recurringGiftsInput: '=',
      },
      link: link
    };

    function link(scope) {
      scope.openRemoveGiftModal = openRemoveGiftModal;
      scope.openEditGiftModal = openEditGiftModal;
      scope.impersonateDonorId = GiveTransferService.impersonateDonorId;

      scope.$watch('recurringGiftsInput', function(recurringGifts) {
        scope.recurringGifts = PaymentDisplayDetailService.postProcess(recurringGifts);
      });

      function openRemoveGiftModal(selectedDonation, index) {
        scope.modalInstance = $modal.open({
          parent: 'noSideBar',
          templateUrl: 'recurring_giving_remove_modal',
          controller: 'RecurringGivingModals as recurringGift',
          openedClass: 'crds-legacy-styles',
          resolve: {
            donation: function() {
              return selectedDonation;
            },

            programList: function() {
              return [
                {
                  ProgramId: selectedDonation.program,
                  Name: selectedDonation.program_name,
                }
              ];
            },
          }
        });

        scope.modalInstance.result.then(function(success) {
          if (success) {
            scope.recurringGifts.splice(index, 1);
            $rootScope.$emit('notify', $rootScope.MESSAGES.giveRecurringRemovedSuccess);
          } else {
            $rootScope.$emit('notify', $rootScope.MESSAGES.failedResponse);
          }
        }, function() {

          $log.info('Modal dismissed at: ' + new Date());
        });
      };

      function openEditGiftModal(selectedDonation) {
        scope.modalInstance = $modal.open({
          parent: 'noSideBar',
          templateUrl: 'recurring_giving_edit_modal',
          controller: 'RecurringGivingModals as recurringGift',
          openedClass: 'crds-legacy-styles',
          resolve: {
            donation: function() {
              return selectedDonation;
            },

            Programs: 'Programs',
            programList: function(Programs) {
              // TODO The number one relates to the programType in MP. At some point we should fetch
              // that number from MP based in human readable input here.
              return Programs.Programs.query({
                programType: 1
              }).$promise;
            },
          }
        });

        scope.modalInstance.result.then(function(success) {
          if (success) {
            DonationService.queryRecurringGifts(scope.impersonateDonorId).then(function(data) {
              scope.recurringGiftsInput = data;
            }, function(/*error*/) {

              $rootScope.$emit('notify', $rootScope.MESSAGES.failedResponse);
            });

            $rootScope.$emit('notify', $rootScope.MESSAGES.giveRecurringSetupSuccess);
          } else {
            $rootScope.$emit('notify', $rootScope.MESSAGES.giveRecurringSetupWarning);
          }
        }, function() {

          $log.info('Modal dismissed at: ' + new Date());
        });
      };
    }
  }
})();
