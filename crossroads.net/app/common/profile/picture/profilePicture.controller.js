(function() {
  'use strict';

  module.exports = ProfilePictureController;

  ProfilePictureController.$inject = ['$rootScope', '$scope', 'ImageService', '$modal'];

  /**
   * Controller for the ProfilePictureDirective
   * Variables passed into the directive and available to
   * this controller include:
   *    ...
   *    ...
   */
  function ProfilePictureController($rootScope, $scope, ImageService, $modal) {
    var vm = this;
    vm.path = ImageService.ProfileImageBaseURL + vm.contactId;
    vm.defaultImage = ImageService.DefaultProfileImage;
    vm.disallowChange = vm.disallowChange || false;
    vm.openModal = openModal;
    // Manage directive style and text defaults
    vm.wrapperClass = vm.wrapperClass || 'col-xs-3 col-sm-2';
    vm.imageClass = vm.imageClass || '';
    vm.buttonText = vm.buttonText || 'Change Photo';
    vm.useDefault = vm.useDefault || false;

    activate();

    function activate() {
      if (vm.useDefault) {
        // debugger;
        vm.path = vm.defaultImage;
      }
    }

    function openModal() {
      var changeProfileImage = $modal.open({
        templateUrl: 'picture/profileImageUpload.html',
        controller: 'ChangeProfileImageController as modal',
        openedClass: 'crds-legacy-styles',
        backdrop: true,
        show: false
      });

      changeProfileImage.result.then(function(croppedImage) {
        vm.path = croppedImage;

        var saveResult = ImageService.ProfileImage.save(croppedImage);
        saveResult.$promise.then(function() {
          // broadcast a javascript event (not angular) so that non-angular
          // components (e.g., shared header) can monitor
          document.dispatchEvent(new Event('profilePhotoChanged'));
        });
      });

    }
  }

})();
