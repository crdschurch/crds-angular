(function() {
  'use strict';

  module.exports = CommunicationModals;

  CommunicationModals.$inject = ['$modalInstance',
      '$rootScope'];

  function CommunicationModals($modalInstance, $rootScope) {
    var vm = this;
    vm.cancel = cancel;
    vm.successful = successful;
    vm.failure = failure;
    activate();

    function activate() { }

    function cancel() {
      $modalInstance.dismiss('cancel');
    }

    function successful() {
      $modalInstance.close(true);
    }

    function failure() {
      $modalInstance.close(false);
    }

  };

})();
