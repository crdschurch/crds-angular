// TODO: This is a deprecated file due to the Pushpay work and is no longer in use
(function () {
  'use strict()';

  module.exports = DonationList;

  DonationList.$inject = ['$log', 'PaymentDisplayDetailService'];

  function DonationList($log, PaymentDisplayDetailService) {
    return {
      restrict: 'EA',
      transclude: true,
      templateUrl: 'templates/donation_list.html',
      scope: {
        donationsInput: '=',
        donationTotalAmount: '=',
        donationStatementTotalAmount: '=',
        donationDoNotShowLabels: '='
      },
      link: link
    };

    function link(scope) {
      scope.$watch('donationsInput', function (donations) {
        scope.donations = PaymentDisplayDetailService.postProcess(donations);
      });
    }
  }
})();
