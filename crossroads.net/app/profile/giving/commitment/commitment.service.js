// TODO: This is a deprecated file due to the Pushpay work and is no longer in use
(function() {
  'use strict';
  module.exports = CommitmentService;

  CommitmentService.$inject = ['$resource'];

  function CommitmentService($resource) {

    return {
      getPledgeCommitments: $resource(__GATEWAY_CLIENT_ENDPOINT__ + 'api/donor/pledge'),
    };

  }

})();
