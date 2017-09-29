(function() {
  'use strict';

  module.exports = CommunicationModals;

  CommunicationModals.$inject = ['$modalInstance',
      '$rootScope', 'team'];

  function CommunicationModals($modalInstance, $rootScope, team) {
    var vm = this;
    vm.cancel = cancel;
    vm.successful = successful;
    vm.failure = failure;
    vm.team = team;
    activate();

    function activate() {
      // debugger;
      console.log('hi');
    }

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
