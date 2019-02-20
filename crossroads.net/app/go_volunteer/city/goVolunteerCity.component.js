(function() {
  'use strict';

  module.exports = GoCity;

  GoCity.$inject = ['GoVolunteerService', '$stateParams'];

  function GoCity(GoVolunteerService, $stateParams) {
    return {
      restrict: 'E',
      scope: {},
      bindToController: true,
      controller: GoCityController,
      controllerAs: 'goCity',
      templateUrl: 'city/goVolunteerCity.template.html'
    };

    function GoCityController() {
      var vm = this;

      // this has to be set first
      vm.cmsInfo = GoVolunteerService.cmsInfo.pages[0].fields;

      vm.city = $stateParams.city;
      vm.content = content;
      vm.title = title;

      function content() {
        return vm.cmsInfo.content;
      }

      function title() {
        //TODO: needs proper case
        return 'GO ' + vm.cmsInfo.title;
      }

    }

  }
})();
