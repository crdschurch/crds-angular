import moment from 'moment';

export default class CMSService {
  /* @ngInject */
  constructor($http) {
    this.http = $http;
    this.url = `${__CMS_CLIENT_ENDPOINT__}api`;
    this.todaysDate = moment().format('YYYY-MM-DD');
  }

  getCurrentSeries() {
    const currentSeriesUrl = `${this.url}/series?endDate__GreaterThanOrEqual=${this.todaysDate}&endDate__sort=ASC`;
    return this.http.get(encodeURI(currentSeriesUrl))
      .then(({ data }) => {
        let currentSeries;
        let allActiveSeries = data.series;

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

  getNearestSeries() {
    const nearestSeriesUrl = `${this.url}/series?startDate__GreaterThanOrEqual=${this.todaysDate}&startDate__sort=ASC&__limit=1`;
    return this.http.get(nearestSeriesUrl)
      .then(({ data }) => _.first(data.series));
  }

  getLastSeries() {
    const lastSeriesUrl = `${this.url}/series?endDate__LessThanOrEqual=${this.todaysDate}&endDate__sort=DESC&__limit=1`;
    return this.http.get(lastSeriesUrl)
      .then(({ data }) => _.first(data.series));
  }

  getSeries(query) {
    return this.http.get(`${this.url}/series?${query}`)
      .then(({ data }) => data.series);
  }

  getRecentMessages(limit) {
    return this.http.get(`${this.url}/messages?date__LessThanOrEqual=${this.todaysDate}&date__sort=DESC&ID__sort=DESC&SeriesID__GreaterThan=0&__limit=${limit}`)
      .then(({ data }) => data.messages.slice(0, limit));
  }

  getMessages(query) {
    return this.http.get(`${this.url}/messages?${query}`)
      .then(({ data }) => data.messages);
  }

  getDigitalProgram() {
    return this.http.get(`${this.url}/features`)
      .then(({ data }) => data.features);
  }

  getSections() {
    return this.http.get(`${this.url}/sections`)
      .then(({ data }) => data.sections);
  }

  getSectionsById(idArr) {
    const query = _.reduce(idArr, (acc, id) => (acc === '' ? `id[]=${id}` : `${acc}&id[]=${id}`), '');

    return this.http.get(`${this.url}/sections?${query}`)
      .then(({ data }) => data.sections);
  }

  getContentBlock(query) {
    return this.http.get(`${this.url}/contentblock?${query}`)
      .then(({ data }) => _.first(data.contentblocks));
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
