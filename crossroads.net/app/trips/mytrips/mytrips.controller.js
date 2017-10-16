import moment from 'moment';

/* @ngInject */
export default class MyTripsController {
  constructor($log, MyTrips, TripsUrlService, Trip) {
    this.log = $log;
    this.myTrips = MyTrips.myTrips;
    this.urlService = TripsUrlService;
    this.tripService = Trip;

    this.activate();
  }

  activate() {
    _.each(this.myTrips, (trip) => {
      this.tripService.Waivers.get({ eventId: trip.eventId }).$promise.then((res) => {
        trip.waivers = res;
      }).catch(this.log.error);

      this.tripService.User.get().$promise.then((res) => {
        trip.contact = res;

        // Grabs the date portion of the Datetime
        const date = res.Date_of_Birth.split('T')[0];
        const birthdate = moment(date, 'YYYY-MM-DD');

        // Calculates the difference between now and `birthdate` in years
        trip.userAge = moment().diff(birthdate, 'year');
      });

      trip.shareUrl = this.urlService.ShareUrl(trip.eventParticipantId);
    });
  }
}
