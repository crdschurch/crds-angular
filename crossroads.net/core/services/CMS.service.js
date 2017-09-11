(function() {
  'use strict';

  module.exports = CMSService;

  CMSService.$inject = ['$http'];

  function CMSService($http) {

    var vm = this;

    vm.http = $http;

    vm.url = `${__CMS_CLIENT_ENDPOINT__}api`;

    vm.todaysDate = moment().format('YYYY-MM-DD');

    vm.getCurrentSeries = function() {

      let currentSeriesAPIAddress = `${this.url}/series?endDate__GreaterThanOrEqual=${vm.todaysDate}&endDate__sort=ASC`
      return vm.http.get(encodeURI(currentSeriesAPIAddress))
        .then((response) => {
          let currentSeries;
          let allActiveSeries = response.data.series;

          allActiveSeries.some(series => {
            let seriesStart = moment(series.startDate, 'YYYY-MM-DD');
            if (seriesStart.isBefore(vm.todaysDate) || seriesStart.isSame(vm.todaysDate)) {
              currentSeries = series;
              return true;
            }
          })

          if ( currentSeries === undefined ) {
            allActiveSeries.sort(this.dateSortMethod);
            currentSeries = allActiveSeries[0];
          }

          return currentSeries;

      });
    }

    vm.getNearestSeries = function() {
      let nearestSeriesAPIAddress = `${this.url}/series?startDate__GreaterThanOrEqual=${vm.todaysDate}&startDate__sort=ASC&__limit=1`
      return vm.http.get(nearestSeriesAPIAddress)
              .then(rsp => { return _.first(rsp.data.series) });
    }

    vm.getLastSeries = function() {
      let nearestSeriesAPIAddress = `${this.url}/series?endDate__LessThanOrEqual=${vm.todaysDate}&endDate__sort=DESC&__limit=1`
      return vm.http.get(nearestSeriesAPIAddress)
              .then(rsp => { return _.first(rsp.data.series) });
    }

    vm.getSeries = function(query) {
      return vm.http.get(`${this.url}/series?${query}`)
              .then(rsp => {return rsp.data.series})
    }

    vm.getRecentMessages = function(limit) {
      return vm.http.get(`${this.url}/messages?date__LessThanOrEqual=${vm.todaysDate}&date__sort=DESC&ID__sort=DESC&SeriesID__GreaterThan=0&__limit=${limit}`)
                      .then(rsp => {return rsp.data.messages.slice(0,limit)});
    }

    vm.getMessages = function(query) {
      return vm.http.get(`${this.url}/messages?${query}`)
                        .then(rsp => {return rsp.data.messages});
    }

    vm.getDigitalProgram = function() {
      return vm.http.get(`${this.url}/features`)
              .then(rsp => {return rsp.data.features});
    }

    vm.getSections = function() {
      return vm.http.get(`${this.url}/sections`)
              .then(rsp => {return rsp.data.sections});
    }

    vm.getContentBlock = function(query) {
      return vm.http.get(`${this.url}/contentblock?${query}`)
              .then(rsp => {return _.first(rsp.data.contentblocks)});
    }

    vm.dateSortMethod = function(a,b) {
      if (moment(a.startDate, 'YYYY-MM-DD').isBefore( moment(b.startDate, 'YYYY-MM-DD')) )
        return -1;
      if (moment(a.startDate, 'YYYY-MM-DD').isAfter( moment(b.startDate, 'YYYY-MM-DD')) )
        return 1;
      return 0;
    }


    return vm;
  }
})();
