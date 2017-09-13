import moment from 'moment';

/* @ngInject */
export default class CMSService {
  constructor($http) {
    this.http = $http;
    this.url = `${__CMS_CLIENT_ENDPOINT__}api`;
    this.todaysDate = moment().format('YYYY-MM-DD');
  }

  getCurrentSeries() {
    const currentSeriesAPIAddress = `${this.url}/series?endDate__GreaterThanOrEqual=${this.todaysDate}&endDate__sort=ASC`;
    return this.http.get(encodeURI(currentSeriesAPIAddress))
      .then((resp) => {
        let currentSeries;
        let allActiveSeries = resp.data.series;

        allActiveSeries.some((series) => {
          const seriesStart = moment(series.startDate, 'YYYY-MM-DD');
          if (seriesStart.isBefore(this.todaysDate) || seriesStart.isSame(this.todaysDate)) {
            currentSeries = series;
            return true;
          }

          return undefined;
        });

        if (currentSeries === undefined) {
          allActiveSeries = allActiveSeries.sort(this.dateSortMethod);
          currentSeries = allActiveSeries[0];
        }

        return currentSeries;
      });
  }

  // eslint-disable-next-line class-methods-use-this
  dateSortMethod(a, b) {
    if (moment(a.startDate, 'YYYY-MM-DD').isBefore(moment(b.startDate, 'YYYY-MM-DD'))) {
      return -1;
    }

    if (moment(a.startDate, 'YYYY-MM-DD').isAfter(moment(b.startDate, 'YYYY-MM-DD'))) {
      return 1;
    }

    return 0;
  }
}
