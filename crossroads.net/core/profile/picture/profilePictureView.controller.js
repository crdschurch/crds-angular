
(() => {
  function ProfilePictureViewController($rootScope, $scope, $timeout, ImageService, $cookies) {
    const vm = this;
    if (!vm.contactId) {
      if (!$rootScope.userid) {
        vm.contactId = $cookies.get('userId');
      } else {
        vm.contactId = $rootScope.userid;
      }
    }

    vm.path = ImageService.ProfileImageBaseURL + vm.contactId;
    vm.defaultImage = ImageService.DefaultProfileImage;
  }

  module.exports = ProfilePictureViewController;

  ProfilePictureViewController.$inject = ['$rootScope', '$scope', '$timeout', 'ImageService', '$cookies'];
})();
