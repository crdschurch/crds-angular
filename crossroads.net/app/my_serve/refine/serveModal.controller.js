'use strict()';
(function() {

  var moment = require('moment');
  var formatDate = crds_utilities.formatDate;

  module.exports = ServeModalController;

  ServeModalController.$inject = ['$rootScope', '$modalInstance', 'dates'];

  function ServeModalController($rootScope, $modalInstance, dates) {

    var vm = this;

    vm.apply = apply;
    vm.buttonText = 'Save';
    vm.dateOptions = {
      formatYear: 'yy',
      startingDay: 1,
      showWeeks: 'false'
    };
    vm.datePickers = { fromOpened: false, toOpened: false };
    vm.format = 'MM/dd/yyyy';
    vm.fromDate = formatDate(dates.fromDate);
    vm.fromDateError = false;
    vm.isFromError = isFromError;
    vm.isToError = isToError;
    vm.toDate = formatDate(dates.toDate);
    vm.toDateError = false;
    vm.openFromDate = openFromDate;
    vm.openToDate = openToDate;
    vm.readyFilterByDate = readyFilterByDate;
    vm.saving = false;

    ///////////////////////////////////////////

    function apply() {
      $modalInstance.close(vm.fromDate, vm.toDate);
    }

    function isFromError() {
      return vm.filterdates.fromdate.$dirty && (
        vm.filterdates.fromdate.$error.fromDateToLarge ||
        vm.filterdates.fromdate.$error.date ||
        vm.filterdates.fromdate.$error.required);
    }

    function isToError() {
      return vm.filterdates.todate.$dirty && (
        vm.filterdates.todate.$error.fromDate ||
        vm.filterdates.todate.$error.required ||
        vm.filterdates.todate.$error.date
      );
    }

    function parseDate(date) {
      if (date instanceof Date) {
        var iso = date.toISOString();
        return moment(iso);
      } else {
        return moment(date, 'MM/DD/YYYY');

      }
    }

    function readyFilterByDate() {
      // set the button loading state
      vm.saving = true;
      vm.buttonText = 'Saving...';

      var now = moment();
      now.hour(0);
      var toDate = parseDate(vm.toDate);
      toDate.hour(23);

      if (now.unix() > toDate.unix()) {
        vm.filterdates.todate.$error.fromDate = true;
        $rootScope.$emit('notify', $rootScope.MESSAGES.generalError);
        vm.saving = false;
        return false;
      } else {
        vm.filterdates.todate.$error.fromDate = false;
      }

      if (vm.toDate !== undefined && toDate.isValid()) {
        if (vm.fromDate === undefined) {
          vm.filterdates.fromdate.$error.date = true;
          $rootScope.$emit('notify', $rootScope.MESSAGES.generalError);
          vm.saving = false;
          return false;
        }

        var fromDate = parseDate(vm.fromDate);
        if (fromDate.isBefore(now, 'days')) {
          fromDate = now;
        }

        if (!fromDate.isValid()) {
          vm.filterdates.fromdate.$error.date = true;
          $rootScope.$emit('notify', $rootScope.MESSAGES.generalError);
          vm.saving = false;
          return false;
        }

        if (fromDate.isAfter(toDate, 'days')) {
          vm.filterdates.fromdate.$error.fromDateToLarge = true;
          $rootScope.$emit('notify', $rootScope.MESSAGES.generalError);
          vm.saving = false;
          return false;
        } else {
          vm.filterdates.fromdate.$error.fromDateToLarge = false;
        }

        $rootScope.$emit('filterByDates', {fromDate: fromDate, toDate: toDate});
        $rootScope.$on('filterByDatesDone', function(event, data) {
          $modalInstance.close({ fromDate:vm.fromDate, toDate: vm.toDate });
        },

        function(err) {
          $rootScope.$emit('notify', $rootScope.MESSAGES.generalError);
          vm.saving = false;
          return false;
        });
      } else {
        vm.filterdates.todate.$error.date = true;
        $rootScope.$emit('notify', $rootScope.MESSAGES.generalError);
        vm.saving = false;
        return false;
      }
    }

    function openFromDate($event) {
      $event.preventDefault();
      $event.stopPropagation();
      vm.datePickers.fromOpened = true;
    }

    function openToDate($event) {
      $event.preventDefault();
      $event.stopPropagation();
      vm.datePickers.toOpened = true;
    }

  }

})();
