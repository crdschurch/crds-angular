// TODO: This is a deprecated file due to the Pushpay work and is no longer in use
(function() {
  'use strict';
  module.exports = GivingHistoryService;

  GivingHistoryService.$inject = ['$resource'];

  function GivingHistoryService($resource) {

    return {
      // api/donations/?donationYear=YYYY&softCredit=true|false&limit=int
      donations: $resource(__GATEWAY_CLIENT_ENDPOINT__ + 'api/donations/:donationYear',
        {donationYear: '@donationYear', softCredit: '@includeSoftCredits', limit: '@limit'}),
      donationYears: $resource(__GATEWAY_CLIENT_ENDPOINT__ + 'api/donations/years'),
      impersonateDonorId: undefined
    };

  }

})();
