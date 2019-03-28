(function() {
  'use strict';
  
  module.exports = GoVolunteerCms;

  GoVolunteerCms.$inject = ['GoVolunteerService'];

  function GoVolunteerCms(GoVolunteerService) {
    return {
      restrict: 'E',
      scope: {},
      bindToController: true,
      controller: function() {
        var vm = this;
        if (GoVolunteerService.cmsInfo && GoVolunteerService.cmsInfo.pages.length > 0) {
          vm.content = GoVolunteerService.cmsInfo.items[0].fields.content;
        } 
      },
      controllerAs: 'cms',
      template: '<div dynamic-content=\'cms.content | html\'> </div>'
    };
  }

})();
