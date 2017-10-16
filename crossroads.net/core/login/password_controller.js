require('../services/password_service');

(function() {
  'use strict';

  module.exports = PasswordController;

  PasswordController.$inject = [
    '$rootScope',
    '$log',
    '$state',
    'PasswordService',
    'Validation',
    'AnalyticsService'
  ];

  function PasswordController(
    $rootScope,
    $log,
    $state,
    PasswordService,
    Validation,
    AnalyticsService) {

    $log.debug('Inside Password Controller');
    var vm = this;
    vm.saving = false;
    vm.resetRequest = resetRequest;
    vm.submitPassword = submitPassword;
    vm.validation = Validation;

    function resetRequest() {
      vm.saving = true;
      if (vm.forgotpasswordform.$valid) {
        // don't send reset request if the email doesn't exist -- this is a little backwards
        // because we're using an existing api call
        PasswordService.EmailExists.get({ email: vm.emailAddress }, function(response) {
          $rootScope.$emit('notify', $rootScope.MESSAGES.generalError);
          vm.saving = false;
        },

        function(error) {
          var email = { email: vm.emailAddress };
          PasswordService.ResetRequest.save(email).$promise.then(function(response) {
            $rootScope.$emit('notify', $rootScope.MESSAGES.resetRequestSuccess);
            $state.go('content', {link: '/'});
          },

          function(error) {
            $rootScope.$emit('notify', $rootScope.MESSAGES.generalError);
            vm.saving = false;
          });
        });
      } else {
        vm.saving = false;
      }
    }

    function submitPassword(form) {
      $log.debug('submitPassword start');
      AnalyticsService.trackForgotPassword();

      if (form !== null) {
        form.$setSubmitted(true);
        if (form.$valid) {
          $log.debug('form valid');
          resetRequest();
        } else {
          $log.debug('form INVALID');
          $rootScope.$emit('notify', $rootScope.MESSAGES.generalError);
        }
      }
    }
  }
})();
