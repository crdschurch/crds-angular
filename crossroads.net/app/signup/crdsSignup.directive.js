(function() {
  'use strict';

  var constants = require('crds-constants');

  module.exports = crdsSignup;

  crdsSignup.$inject = ['SignupService'];

  function crdsSignup(SignupService) {
    return {
      restrict: 'E',
      scope: {},
      controller: crdsSignupController,
    controllerAs: 'signup',
      bindToController: true,
      templateUrl: 'signup/signup.html'
    };

    function crdsSignupController() {
      /*jshint validthis: true */
      var vm = this;
      vm.cmsInfo = SignupService.cmsInfo;
      vm.family = SignupService.family;
      vm.group = SignupService.group;
      vm.locations = SignupService.locations;
      vm.signupService = SignupService;
      vm.showCommunityGroups = showCommunityGroups;
      vm.showOnetimeEvent = showOnetimeEvent;

      function showCommunityGroups() {
        return vm.signupService.cmsInfo.items[0].fields.pageType === constants.CMS.PAGENAMES.COMMUNITYGROUPS;
      }

      function showOnetimeEvent() {
        return vm.signupService.cmsInfo.items[0].fields.pageType === constants.CMS.PAGENAMES.ONETIMEEVENTS;
      }

    }
  }
})();
