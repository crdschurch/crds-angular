export default class TripPromiseController {
  /* @ngInject() */
  constructor($rootScope, Validation, $state, $log, Trip) {
    this.$rootScope = $rootScope;
    this.validation = Validation;
    this.$state = $state;
    this.$log = $log;
    this.Trip = Trip;

    this.promise = false;

    this.processing = false;
    this.tripPromiseForm = {};
  }

  submit() {
    if (this.tripPromiseForm.$valid) {
      this.processing = true;

      const signedDoc = Object.assign(this.myTripPromise, { documentReceived: true });

      this.Trip.MyTripsPromise.save(signedDoc).$promise.then(() => {
        this.$state.go('mytrips');
      }).catch((err) => {
        this.$log.error(err);
        this.$rootScope.$emit('notify', this.$rootScope.MESSAGES.generalError);
        this.processing = false;
      });
    }
  }

  cancel() {
    this.$state.go('mytrips');
  }
}
