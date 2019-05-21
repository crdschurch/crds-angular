require('./childcare');
require('./mp_tools');
require('ui-select/dist/select.css');
require('./invoices/invoices.module');

(function () {
  'use strict()';

  var constants = require('./constants');
  angular.module(constants.MODULES.CROSSROADS, [
    constants.MODULES.CHILDCARE_DASHBOARD,
    constants.MODULES.CORE,
    constants.MODULES.COMMON,
    constants.MODULES.CORKBOARD,
    constants.MODULES.FORM_BUILDER,
    constants.MODULES.GO_VOLUNTEER,
    constants.MODULES.MPTOOLS,
    constants.MODULES.PROFILE,
    constants.MODULES.SEARCH,
    constants.MODULES.SIGNUP,
    constants.MODULES.TRIPS,
    constants.MODULES.CAMPS,
    constants.MODULES.INVOICES,
    constants.MODULES.WAIVERS
  ]);

  angular.module(constants.MODULES.CROSSROADS)
    .config(require('./routes'))
    .config(require('./routes.content'))
    .config(['$logProvider', function($logProvider) {
      // disable debug log in prod
      if (!__CRDS_ENV__) {
        $logProvider.debugEnabled(false);
      }
    }]);

  require('./corkboard');
  require('./signup');
  require('./volunteer_signup');
  require('./leaveyourmark');
})();
