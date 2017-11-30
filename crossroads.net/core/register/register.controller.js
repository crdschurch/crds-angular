(function() {
  'use strict';

  module.exports = RegisterController;

  RegisterController.$inject = [
    '$rootScope',
    'AuthService',
    'User',
    'Session',
    '$log',
    '$timeout',
    '$state',
    'Validation',
    'AnalyticsService'
  ];

  function RegisterController(
    $rootScope,
    AuthService,
    User,
    Session,
    $log,
    $timeout,
    $state,
    Validation,
    AnalyticsService) {

    var vm = this;
    vm.newuser = User;
    vm.passwordPrefix = 'registration';
    vm.register = register;
    vm.registerShow = false;
    vm.showRegisterButton = true;
    vm.validation = Validation;
    vm.redirectToGetStarted = redirectToGetStarted;

    function register() {
      vm.showRegisterButton = false;

      form.firstname.$setTouched();
      form.lastname.$setTouched();
      form['email_field'].email.$setTouched();
      form['passwd.passwordForm'].password.$setTouched();

      if (!vm.registerForm.$valid) {
        $rootScope.$emit('notify', $rootScope.MESSAGES.generalError);
        vm.showRegisterButton = true;
        return;
      }

      vm.credentials = {};
      vm.credentials.username = vm.newuser.email;
      vm.credentials.password = vm.newuser.password;

      User.$save().then(successfulRegistration).catch(failedRegistration);
    }

    function failedRegistration(error) {
      vm.processing = false;
      vm.showRegisterButton = true;
      if (error.data && error.data.message === 'Duplicate User') {
        $rootScope.$emit('notify', $rootScope.MESSAGES.emailInUse);
      } else if (error.status === 409) {
        var message = $rootScope.MESSAGES.registrationContactError;
        message.content = message.content.replace('[contactId]', error.data.message);
        $rootScope.$emit('notify', message);
      } else {
        $rootScope.$emit('notify', $rootScope.MESSAGES.failedResponse);
      }
    }

    function failedLogin() {
      vm.pending = false;
      vm.loginFailed = true;
      vm.showRegisterButton = true;
    }

    function successfulRegistration() {
      AuthService.login(vm.credentials).then(successfulLogin, failedLogin).then(function() {
        vm.processing = false;
        vm.showRegisterButton = true;
      });
    }

    function successfulLogin(user) {
      AnalyticsService.newUserRegistered($rootScope.userid, vm.credentials.username, vm.registerForm.firstname.$modelValue, vm.registerForm.lastname.$modelValue);
      // TODO Refactor this to a shared location for use here and in login_controller
      vm.registerShow = !vm.registerShow;
      $rootScope.showLoginButton = false; //TODO use a service here, avoid using rootscope
      $rootScope.$emit('notify', $rootScope.MESSAGES.successfullRegistration);
      if (vm.registerForm) {
        vm.registerForm.$setPristine();
      }

      vm.newuser = {};
      $timeout(function() {
        if (vm.registerCallback) {
          vm.registerCallback();
        } else if (Session.hasRedirectionInfo()) {
          Session.redirectIfNeeded();
        } else {
          redirectToGetStarted();
        }
      }, 500);
    }

    function redirectToGetStarted() {
      $state.go('content', {link:'/getstarted'});
    }
  }
})();
