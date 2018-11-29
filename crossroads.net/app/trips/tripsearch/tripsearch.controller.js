
(function() {
  'use strict';

  module.exports = TripSearchController;

  TripSearchController.$inject = ['$rootScope', '$scope', '$log', 'Trip'];

  function TripSearchController($rootScope, $scope, $log, Trip) {
    var vm = this;
    vm.empty = false;
    vm.loading = false;
    vm.search = search;
    vm.searchString = '';
    vm.showError = showError;
    vm.tripParticipants = [];

    function search(form) {
      vm.loading = true;
      vm.tripParticipants = [];

      if (vm.query === undefined || vm.query === '') {
        form.searchForm.searchText.$setValidity('required', false);
      } else {
        form.searchForm.searchText.$setValidity('required', true);
      }

      if (form.searchForm.$invalid) {
        $log.error('please fill out all required fields correctly');
        $rootScope.$emit('notify', $rootScope.MESSAGES.generalError);
        vm.loading = false;
        return false;
      }

      vm.searchString = vm.query;
      Trip.Search.query({query: vm.query}).$promise.then(function(response) {
        vm.tripParticipants = response;
        if (vm.tripParticipants.length > 0) {
          vm.empty = false;
        } else {
          vm.empty = true;
        }

        vm.loading = false;
      });
    }

    function showError(form, field) {
      if (form[field] === undefined) {
        return false;
      }

      if (form.$submitted || form[field].$dirty) {
        return form[field].$invalid;
      }

      return false;
    }
  }
})();
