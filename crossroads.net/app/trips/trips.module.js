const MODULE = 'crossroads.trips';

angular.module(MODULE, ['crossroads.core', 'crossroads.common'])
  .config(require('./trips.routes'))
  .factory('TripsUrlService', require('./tripsUrl.service'))
  .factory('Trip', require('./trips.service'))
  .factory('PreferredAirportService', require('./preferred-airport.service'));
require('./mytrips');
require('./travelInformation');
require('./trippromise');
require('./tripsearch');
require('./tripgiving');
require('./signup');
