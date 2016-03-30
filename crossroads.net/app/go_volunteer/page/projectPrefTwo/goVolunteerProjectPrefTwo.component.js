(function() {
  'use strict';

  module.exports = GoVolunteerProjectPrefTwo;

  GoVolunteerProjectPrefTwo.$inject = ['GoVolunteerService'];

  function GoVolunteerProjectPrefTwo(GoVolunteerService) {
    return {
      restrict: 'E',
      scope: {
        onSubmit: '&'
      },
      bindToController: true,
      controller: GoVolunteerProjectPrefTwoController,
      controllerAs: 'goProjectPrefTwo',
      templateUrl: 'projectPrefTwo/goVolunteerProjectPrefTwo.template.html'
    };

    function GoVolunteerProjectPrefTwoController() {
      var vm = this;
      vm.projectTypes = GoVolunteerService.projectTypes;
      vm.alreadySelected = alreadySelected;
      vm.submit = submit;

      function alreadySelected(projectTypeId) {
        if (GoVolunteerService.projectPrefOne === projectTypeId) {
          return ['disabled', 'checked'];
        }

        return [];
      }

      function submit(projectTypeId) {
        if (GoVolunteerService.projectPrefOne == projectTypeId) {
          return;
        }

        GoVolunteerService.projectPrefTwo = projectTypeId;
        vm.onSubmit({nextState: 'project-preference-three'});
      }

    }
  }

})();
