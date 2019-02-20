
(function() {
  'use strict';

  var moment = require('moment');
  module.exports = VolunteerApplicationController;

  VolunteerApplicationController.$inject = ['$log', 'MESSAGES',
                                            'VolunteerApplication', 'Session',
                                            '$stateParams', 'Opportunity',
                                            'VolunteerService', 'Family', 'PageInfo'];

  function VolunteerApplicationController($log, MESSAGES,
      VolunteerApplication,
      Session,
      $stateParams,
      Opportunity,
      VolunteerService, Family, PageInfo) {

    var vm = this;
    vm.contactId = $stateParams.id;
    vm.family = Family;
    vm.pageInfo = pageInfo(PageInfo.cmsInfo);
    vm.person = PageInfo.contact;
    vm.phoneFormat = /^\(?(\d{3})\)?[\s.-]?(\d{3})[\s.-]?(\d{4})$/;
    vm.responseCheck = false;
    vm.showAccessDenied = false;
    vm.showAdult = false;
    vm.showContent = true;
    vm.showInvalidResponse = false;
    vm.showStudent = false;
    vm.showSuccess = false;
    vm.showBlock = showBlock;
    vm.viewReady = false;

    activate();

    ///////////////////////////////////////////

    function activate() {
      if (checkFamily()) {
        vm.showAccessDenied = false;
        vm.person.middleInitial = VolunteerApplication.middleInitial(vm.person);
        opportunityResponse();
        applicationVersion();
      } else {
        vm.showAccessDenied = true;
      }
      vm.viewReady = true;
    }

    function applicationVersion() {
      //var block = VolunteerApplication.applicationVersion(vm.person);
      if (vm.person.age >= 16) {
        vm.showAdult = true;
      } else if ((vm.person.age >= 10) && (vm.person.age <= 15)) {
        vm.showStudent = true;
      } else {
        vm.showError = true;
      }
    }

    function checkFamily() {
      for (var i = 0; i < vm.family.length; i++) {
        if (vm.family[i].contactId === Number(vm.contactId)) {
          return true;
        }
      }
      return false;
    }

    function opportunityResponse() {
      Opportunity.GetResponse.get({
          id: vm.pageInfo.opportunity,
          contactId: vm.contactId
        }).$promise
        .then(function(response) {
          vm.responseCheck = true;
          if ((response !== null) && (response.responseId !== undefined)) {
            vm.responseId = response.responseId;
          } else {
            vm.showInvalidResponse = ((response === null) || ((response.responseId === undefined)));
          }
        });
    }

    function pageInfo(cmsInfo) {
      return cmsInfo.pages[0].fields;
    }

    function showBlock(blockName) {
      switch (blockName) {
        case 'adult':
          vm.showContent = true;
          return vm.showAdult && !vm.showInvalidResponse && vm.responseCheck;
        case 'student':
          vm.showContent = true;
          return vm.showStudent && !vm.showInvalidResponse && vm.responseCheck;
        case 'no-response':
          vm.showContent = false;
          return vm.showInvalidResponse;
        case 'denied':
          vm.showContent = false;
          return vm.showAccessDenied;
        case 'age-error':
          vm.showContent = false;
          return vm.showError;
        default:
          $log.debug('show block undefined: ' + blockName);
          return false;
      }
    }

  }
})();
