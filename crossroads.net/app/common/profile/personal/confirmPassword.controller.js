(function() {
  'use strict';

  module.exports = ConfirmPasswordController;

  ConfirmPasswordController.$inject = [
      '$rootScope',
      '$modalInstance',
      'modalTypeItem',
      'PasswordService'
  ];

  function ConfirmPasswordController(
      $rootScope,
      $modalInstance,
      modalTypeItem,
      PasswordService) {

    var vm = this;
    vm.ok = ok;
    vm.cancel = cancel;
    vm.passwd = '';
    vm.modalTypeItem = modalTypeItem;
    vm.saving = false;

    function ok() {

      vm.saving = true;
      var currentPassword = vm.passwd;
      var encodedPassword = JSON.stringify(currentPassword);

      PasswordService.VerifyPassword.save(encodedPassword).$promise.then(function(response) {
        vm.passwd = '';
        $modalInstance.close(currentPassword);
      }, function(error) {

        $rootScope.$emit('notify', $rootScope.MESSAGES.passwordNotVerified);
        vm.saving = false;
      });
    }

    function cancel() {
      vm.passwd = '';
      $modalInstance.close();
    }

  }
})();
