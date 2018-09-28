(function() {
  'use strict';

  module.exports = ProfileController;

  ProfileController.$inject = [
    '$rootScope',
    '$state',
    '$location',
    '$window',
    'AttributeTypes',
    'Person',
    'Profile',
    'Lookup',
    'Locations',
    'PaymentService',
    'STATE_CHANGE_EVENTS'];

  function ProfileController(
      $rootScope,
      $state,
      $location,
      $window,
      AttributeTypes,
      Person,
      Profile,
      Lookup,
      Locations,
      PaymentService,
      STATE_CHANGE_EVENTS) {

    var vm = this;
    vm.attributeTypes = AttributeTypes;
    vm.buttonText = 'Save';
    vm.displayLocation = displayLocation;
    vm.showLocationButton = showLocationButton;
    vm.enforceAgeRestriction = enforceAgeRestriction;
    vm.goToTab = goToTab;
    vm.locations = Locations;
    vm.locationFocus = locationFocus;
    vm.profileData = { person: Person };
    vm.tabs = [
      { title:'Personal', active: false, route: 'profile.personal' },
      { title:'Communications', active: false, route: 'profile.communications', class:'hidden-xs'},
      { title:'Skills', active: false, route: 'profile.skills', class:'hidden-xs' }
    ];

    $rootScope.$on('$stateChangeStart', stateChangeStart);
    $window.onbeforeunload = onBeforeUnload;

    activate();

    ////////////
    function activate() {
      _.forEach(vm.tabs, function(tab) {
        tab.active = $state.current.name === tab.route;
      });

    }

    function showLocationButton() {
     if (vm.profileData.person.congregationId === 2) {
       return 'btn btn-standard';
     }

     return '';
   }

    function displayLocation() {
      var locationName;
      if (vm.profileData.person.congregationId !== 2) {
        locationName = _.result(
          _.find(
            vm.locations,
            {'dp_RecordID': vm.profileData.person.congregationId}),
            'dp_RecordName');
      }

      if (!locationName) {
        return 'Select Crossroads Site';
      }

      return locationName;
    }

    function enforceAgeRestriction() {
      return 13;
    }

    function locationFocus() {
      $rootScope.$emit('locationFocus');
    }


    function goToTab(tab) {
      if (tab.title === 'Personal') {
        vm.profileParentForm.$setPristine();

        //TODO: If the bug is fixed on tabs we need to clean this up
        //      This is here because dirty state on forms inside of tabs
        //      and changing values on these other tabs make things dirty.
      }

      $state.go(tab.route);
    }

    function stateChangeStart(event, toState, toParams, fromState, fromParams) {
      if (fromState.name === 'profile.personal' && vm.profileParentForm.$dirty) {
        if (!$window.confirm('Are you sure you want to leave this page?')) {
          event.preventDefault();
          $rootScope.$emit(STATE_CHANGE_EVENTS.clearResolving);

          vm.tabs[0].active = true;
          return;
        }
      }
    }

    function onBeforeUnload() {
      if (vm.profileParentForm.$dirty) {
        return '';
      }
    }
  }
})();
