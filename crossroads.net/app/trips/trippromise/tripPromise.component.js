import TripPromiseController from './tripPromise.controller';
import template from './tripPromise.html';

const TripPromiseComponent = {
  bindings: {
    myTripPromise: '<'
  },
  template,
  controller: TripPromiseController,
  resolve: []
};

export default TripPromiseComponent;