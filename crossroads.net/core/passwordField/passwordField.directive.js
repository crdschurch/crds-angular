(function() {

  'use strict';

  module.exports = PasswordField;

  PasswordField.$inject = ['$log'];

  function PasswordField($log) {
    return {
      restrict: 'EA',
      replace: true,
      scope: {
        passwd: '=',
        submitted: '=',
        prefix: '=',
        required: '@',
        minLength: '@'
      },
      templateUrl: 'passwordField/passwordField.html',
      controller: PasswordFieldController,
      controllerAs: 'passwd',
      bindToController: true
    };

    function PasswordFieldController() {
      var vm = this;

      vm.inputType = 'password';
      vm.pwprocess = pwprocess;
      vm.pwprocessing = 'SHOW';
      vm.passwordInvalid = passwordInvalid;
      vm.isCollapsed = true;
      vm.passwd = '';

      ////////////////////////

      function passwordInvalid() {
        //TODO Once global validation logic method has been created, use that shorter method here
        return vm.passwordForm.password.$error.required && vm.submitted &&
            vm.passwordForm.password.$dirty ||
            vm.passwordForm.password.$error.required && vm.submitted &&
            !vm.passwordForm.password.$touched ||
            vm.passwordForm.password.$error.required && vm.submitted &&
            vm.passwordForm.password.$touched ||
            !vm.passwordForm.password.$error.required &&
            vm.passwordForm.password.$dirty &&
            !vm.passwordForm.password.$valid;
      }

      function pwprocess() {
        if (vm.pwprocessing === 'SHOW') {
          vm.pwprocessing = 'HIDE';
          vm.inputType = 'text';
        } else {
          vm.pwprocessing = 'SHOW';
          vm.inputType = 'password';
        }
      }
    }
  }
})();
