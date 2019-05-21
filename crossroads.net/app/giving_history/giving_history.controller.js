// TODO: This is a deprecated file due to the Pushpay work and is no longer in use
(function () {
  'use strict';
  module.exports = GivingHistoryController;

  GivingHistoryController.$inject = ['$log',
                                     'GivingHistoryService',
                                     'Profile',
                                     'AuthService',
                                     'CRDS_TOOLS_CONSTANTS'];

  function GivingHistoryController($log,
                                   GivingHistoryService,
                                   Profile,
                                   AuthService,
                                   CRDS_TOOLS_CONSTANTS) {
    var vm = this;

    vm.overall_view_ready = false;
    vm.beginning_donation_date = undefined;
    vm.currentDate = new Date();
    vm.donation_statement_total_amount = undefined;
    vm.donation_total_amount = undefined;
    vm.donation_years = [];
    vm.donations = [];
    vm.donations_all = false;
    vm.donation_history = false;
    vm.non_soft_credit_donation_history = false;
    vm.donation_view_ready = false;
    vm.ending_donation_date = undefined;
    vm.impersonate_donor_id = GivingHistoryService.impersonateDonorId;
    vm.impersonation_error = false;
    vm.impersonation_error_message = undefined;
    vm.impersonation_not_allowed = false;
    vm.impersonation_user_not_found = false;
    vm.profile = {};
    vm.selected_giving_year = undefined;
    vm.soft_credit_donations = [];
    vm.soft_credit_donation_statement_total_amount = undefined;
    vm.soft_credit_donation_total_amount = undefined;
    vm.soft_credit_donation_history = false;
    vm.soft_credit_donation_view_ready = false;
    vm.server_error = false;

    vm.getDonations = getDonations;
    vm.getSoftCreditDonations = getSoftCreditDonations;

    activate();

    function activate() {
      Profile.Personal.get({ impersonateDonorId: vm.impersonate_donor_id }, function (data) {
        vm.profile = data;
        GivingHistoryService.donationYears.get({ impersonateDonorId: vm.impersonate_donor_id },
          function (data) {
          var most_recent_giving_year = data.most_recent_giving_year;

          // Create a map out of the array of donation years, so we can add an 'All' option easily,
          // and to facilitate ng-options on the frontend select
          vm.donation_years = _.transform(data.years, function (result, year) {
            result.push({ key: year, value: year });
          });

          vm.donation_years.push({ key: '', value: 'All' });

          // Set the default selected year based on the most recent giving year
          vm.selected_giving_year = _.find(vm.donation_years, { key: most_recent_giving_year });

          // Now get the donations for the selected year
          vm.overall_view_ready = true;
          vm.donation_history = true;
          vm.getDonations();
          vm.getSoftCreditDonations();
        },

        function (error /*GivingHistoryService.donationYears.get error*/) {
          vm.overall_view_ready = true;
          vm.donation_view_ready = true;
          vm.donation_history = false;
          vm.non_soft_credit_donation_history = false;
          vm.soft_credit_donation_history = false;
          setErrorState(error);
        });
      },

      function (error /*Profile.Personal.get error*/) {
        vm.overall_view_ready = true;
        vm.donation_view_ready = true;
        vm.donation_history = false;
        vm.non_soft_credit_donation_history = false;
        vm.soft_credit_donation_history = false;
        setErrorState(error);
      });
    }

    vm.allowAdminAccess = function () {
      return (AuthService.isAuthenticated() &&
        AuthService.isAuthorized(CRDS_TOOLS_CONSTANTS.SECURITY_ROLES.FinanceTools));
    };

    function setErrorState(error) {
      // server error happens
      if (error && error.status >= 500) {
        vm.server_error = true;
      }

      if (vm.impersonate_donor_id === undefined ||
            error === undefined ||
            error.status === undefined) {
        return;
      }

      switch (error.status) {
        case 403: // Forbidden - not allowed to impersonate
          vm.impersonation_error = true;
          vm.impersonation_not_allowed = true;
          vm.impersonation_error_message = error.data === undefined ||
                                           error.data.message === undefined ?
                                          'User is not allowed to impersonate' : error.data.message;
          break;
        case 409: // Conflict - tried to impersonate, but user could not be found
          vm.impersonation_error = true;
          vm.impersonation_user_not_found = true;
          vm.impersonation_error_message = error.data === undefined ||
                                           error.data.message === undefined ?
                                          'Could not find user to impersonate' : error.data.message;
          break;
        default:
          break;
      }
    }

    function getDonations() {
      vm.donation_view_ready = false;
      GivingHistoryService.donations.get({ donationYear: vm.selected_giving_year.key,
                                           softCredit: false,
                                           impersonateDonorId: vm.impersonate_donor_id },
          function (data) {
            vm.donations = data.donations;
            vm.donation_total_amount = data.donation_total_amount;
            vm.donation_statement_total_amount = data.donation_statement_total_amount;
            vm.donation_view_ready = true;
            // vm.donation_history = true;
            vm.non_soft_credit_donation_history = true;
            vm.donations_all = vm.selected_giving_year.key === '' ? true : false;
            vm.beginning_donation_date = data.beginning_donation_date;
            vm.ending_donation_date = data.ending_donation_date;
          },

          function (error) {
            // vm.donation_history = false;
            vm.non_soft_credit_donation_history = false;
            vm.donation_view_ready = true;
            setErrorState(error);
          });
    }

    function getSoftCreditDonations() {
      vm.soft_credit_donation_view_ready = false;
      GivingHistoryService.donations.get({ donationYear: vm.selected_giving_year.key,
                                           softCredit: true,
                                           impersonateDonorId: vm.impersonate_donor_id },
          function (data) {
            vm.soft_credit_donations = data.donations;
            vm.soft_credit_donation_total_amount = data.donation_total_amount;
            vm.soft_credit_donation_statement_total_amount = data.donation_statement_total_amount;
            vm.soft_credit_donation_view_ready = true;
            vm.soft_credit_donation_history = true;
          },

          function (error) {
            vm.soft_credit_donation_history = false;
            vm.soft_credit_donation_view_ready = true;
            setErrorState(error);
          });
    }
  }
})();
