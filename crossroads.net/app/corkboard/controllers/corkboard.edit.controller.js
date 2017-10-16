(function () {
  'use strict';
  module.exports = CorkboardEditController;

  CorkboardEditController.$inject = ['$log', '$rootScope', '$scope', '$stateParams', '$window', 'CORKBOARD_EVENTS', 'CorkboardPostTypes', 'CorkboardListings'];

  function CorkboardEditController($log, $rootScope, $scope, $stateParams, $window, CORKBOARD_EVENTS, CorkboardPostTypes, CorkboardListings) {
    var vm = this;
    vm.currentDate = new Date(Date.now());
    vm.selectedItem = {};
    vm.saving = false;
    vm.postType = $stateParams.type.toUpperCase();
    vm.postTypeValues = CorkboardPostTypes[vm.postType];
    //Refactor for style and TEST
    vm.submitPost = function () {
      vm.saving = true;
      if ($scope[vm.postTypeValues.form].$invalid) {
        $rootScope.$emit('notify', $rootScope.MESSAGES.generalError);
        vm.saving = false;
      } else {
        vm.selectedItem.PostType = vm.postType;
        CorkboardListings.post().save(vm.selectedItem).$promise.then(function (post) {
          // Remove angular properties that are added by saving individual object, that are not needed by the CorkboardListings
          delete post.$promise;
          delete post.$resolved;

          $scope[vm.postTypeValues.form].$setPristine();
          $scope.$emit(CORKBOARD_EVENTS.postAdded, post);
          $rootScope.$emit('notify', $rootScope.MESSAGES.corkboardPostSuccess);
          vm.saving = false;
        }, function (error) {

          $rootScope.$emit('notify', $rootScope.MESSAGES.generalError);
          vm.saving = false;
        });
      }
    };

    //Refactor for style and TEST
    vm.cancelPost = function () {
      $scope.$emit(CORKBOARD_EVENTS.postCanceled);
    };

    $window.onbeforeunload = function () {
      if ($scope[vm.postTypeValues.form].$dirty) {
        return '';
      }
    };

    $scope.$on('$stateChangeStart', function (event, next, current) {
      if ($scope[vm.postTypeValues.form] !== undefined) {
        if ($scope[vm.postTypeValues.form].$dirty) {
          if (!confirm('Are you sure you want to leave this page?')) {
            event.preventDefault();
          }
        }
      }
    });
  }
})();
