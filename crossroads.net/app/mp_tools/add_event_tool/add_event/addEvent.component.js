(function() {
  'use strict';

  module.exports = AddEventComponent;

  AddEventComponent.$inject = [
    '$log',
    '$rootScope',
    'AddEvent',
    'Lookup',
    'Programs',
    'StaffContact',
    'Validation'
  ];

  function AddEventComponent() {
    return {
      restrict: 'E',
      scope: {
        eventData: '='
      },
      templateUrl: 'add_event/add_event.html',
      controller: AddEventController,
      controllerAs: 'evt',
      bindToController: true
    };
  }

  function AddEventController($log, $rootScope, AddEvent, Lookup, Programs, StaffContact, Validation) {
    var vm = this;

    vm.crossroadsLocations = [];
    vm.addEvent = AddEvent;
    vm.endDateOpen = endDateOpen;
    vm.endDateOpened = false;
    vm.eventTypes = Lookup.query({ table: 'eventtypes' });
    vm.formatContact = formatContact;
    vm.programs = Programs.AllPrograms.query();
    vm.reminderDays = Lookup.query({ table: 'reminderdays' });
    vm.resetRooms = resetRooms;
    vm.staffContacts = StaffContact.query();
    vm.startDateOpen = startDateOpen;
    vm.startDateOpened = false;
    vm.validation = Validation;
    vm.validDateRange = validDateRange;
    vm.childcareSelectedFlag = false;
    vm.childcareSelected = childcareSelected;
    vm.eventTypeChanged = eventTypeChanged;
    vm.checkMinMax = checkMinMax;
    activate();

    ///////
    function activate() {

      // Get the congregations
      Lookup.query({ table: 'crossroadslocations' }, function(locations) {
        vm.crossroadsLocations = locations;

        // does the current location need to be updated with the name?
        // if (AddEvent.editMode) {
        //   vm.eventData.event.congregation = _.find(locations, function(l) {
        //     return l.dp_RecordID === vm.eventData.event.congregation.dp_RecordID;
        //   });
        // }
      });

      if (_.isEmpty(vm.eventData)) {
        var startDate = new Date();
        startDate.setMinutes(0);
        startDate.setSeconds(0);
        var endDate = new Date(startDate);
        endDate.setHours(startDate.getHours() + 1);
        vm.eventData = {
          donationBatch: 0,
          sendReminder: 0,
          minutesSetup: 0,
          minutesCleanup: 0,
          startDate: new Date(),
          endDate: new Date(),
          startTime: startDate,
          endTime: endDate
        };
      }
      else {
          vm.eventTypeChanged();
      }
    }

    function dateTime(dateForDate, dateForTime) {

      if (dateForDate === undefined) {
        return null;
      }

      if (dateForTime === undefined) {
        return null;
      }

      return new Date(
          dateForDate.getFullYear(),
          dateForDate.getMonth(),
          dateForDate.getDate(),
          dateForTime.getHours(),
          dateForTime.getMinutes(),
          dateForTime.getSeconds(),
          dateForTime.getMilliseconds());
    }

    function endDateOpen($event) {
      $event.preventDefault();
      $event.stopPropagation();
      vm.endDateOpened = true;
    }

    function formatContact(contact) {
      var displayName = contact.displayName;
      var email = contact.email;
      return displayName + ' - ' + email;
    }

    function resetRooms() {
      vm.addEvent.eventData.rooms.length = 0;
    }

    function startDateOpen($event) {
      $event.preventDefault();
      $event.stopPropagation();
      vm.startDateOpened = true;
    }

    function childcareSelected() {
      return vm.childcareSelectedFlag;
    }

    function checkMinMax(form) {
      if (vm.eventData.minimumChildren === undefined || vm.eventData.maximumChildren === undefined) {
        return false;
      }

      //set the proper error state
      if (vm.eventData.minimumChildren > vm.eventData.maximumChildren) {
        form.maximumChildren.$error.minmax = true;
        form.maximumChildren.$valid = false;
        form.maximumChildren.$invalid = true;
        form.maximumChildren.$dirty = true;
        form.$valid = false;
        form.$invalid = true;
        return true;
      }
      else {
        form.maximumChildren.$error.minmax = false;
        form.maximumChildren.$error.endDate = false;
        form.maximumChildren.$valid = true;
        form.maximumChildren.$invalid = false;
        return false;
      }
    }

    function eventTypeChanged() {
      // if childcare is selected then show additional fields
      // constrain congregations
      if (vm.eventData.eventType.dp_RecordName === 'Childcare') {
        vm.childcareSelectedFlag = true;
        Lookup.query({ table: 'childcarelocations' }, function(locations) {vm.crossroadsLocations = locations;});
      }
      else {
        vm.childcareSelectedFlag = false;
        Lookup.query({ table: 'crossroadslocations' }, function(locations) {vm.crossroadsLocations = locations;});
      }
      
    }



    function validDateRange(form) {
      if (form === undefined) {
        return false;
      }

      //verify that dates are valid;
      var start;
      var end;
      try {
        start =  dateTime(vm.eventData.startDate, vm.eventData.startTime);
        end = dateTime(vm.eventData.endDate, vm.eventData.endTime);
      } catch (err) {
        form.endDate.$error.endDate = true;
        form.endDate.$valid = false;
        form.endDate.$invalid = true;
        form.endDate.$dirty = true;
        form.$valid = false;
        form.$invalid = true;
        return true;
      }

      if (moment(start) <= moment(end)) {
        form.endDate.$error.endDate = false;
        form.endDate.$valid = true;
        form.endDate.$invalid = false;
        return false;
      }

      // set the endDate Invalid...
      form.endDate.$error.endDate = true;
      form.endDate.$valid = false;
      form.endDate.$invalid = true;
      form.endDate.$dirty = true;
      form.$valid = false;
      form.$invalid = true;
      return true;
    }

  }

})();
