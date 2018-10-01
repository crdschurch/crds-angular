// TODO: This is a deprecated file due to the Pushpay work and is no longer in use
(function() {
  'use strict';

  module.exports = ProfileGivingController;

  ProfileGivingController.$inject = ['$log', '$filter', '$state', 'DonationService', 'CommitmentService'];

  function ProfileGivingController($log, $filter, $state, DonationService, CommitmentService) {
    var vm = this;
    vm.pledge_commitments = [];
    vm.pledge_commitments_data = false
    vm.pledge_commitments_view_ready = false;
    vm.recurring_gifts = [];
    vm.recurring_giving = false;
    vm.recurring_giving_view_ready = false;

    activate();

    function activate() {
      vm.recurring_giving_view_ready = false;
      vm.pledge_commitments_view_ready = false;

      DonationService.queryRecurringGifts().then(function(data) {
        vm.recurring_gifts = data;
        vm.recurring_giving_view_ready = true;
        vm.recurring_giving = true;
      }, function(/*error*/) {

        vm.recurring_giving = false;
        vm.recurring_giving_view_ready = true;
      });

      CommitmentService.getPledgeCommitments.query().$promise.then(function(data){
        vm.pledge_commitments = data;
        vm.pledge_commitments_data = true;
        vm.pledge_commitments_view_ready = true;
      }, function(/*error*/) {

        vm.pledge_commitments_data = false;
        vm.pledge_commitments_view_ready = true;
      });
    }
  }
})();
