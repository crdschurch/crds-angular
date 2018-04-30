require('./group_tool');
require('./childcare');
require('./mp_tools');
require('./live_stream');
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
    constants.MODULES.GROUP_TOOL,
    constants.MODULES.MEDIA,
    constants.MODULES.LIVE_STREAM,
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
  /// #if INCLUDE_STYLEGUIDE
  require('./styleguide');
  /// #endif
  require('./thedaily');
  require('./volunteer_signup');
  require('./volunteer_application');
  require('./giving_history');
  require('./leaveyourmark');
})();
