(function() {
  'use strict()';

  var MODULE = 'crossroads.mptools';

  require('./checkScannerBatches.service');
  require('./checkBatchProcessor.html');
  require('./required.validator');

  angular.module(MODULE)
  .constant('GIVE_PROGRAM_TYPES', { Fuel: 1, Events: 2, Trips: 3, NonFinancial: 4 })
  .controller('CheckBatchProcessor', require('./checkBatchProcessor.controller'));

})();
