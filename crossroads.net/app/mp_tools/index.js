(function(){
  'use strict()';
  require('ui-select');

  var MODULE = 'crossroads.mptools';

  angular.module(MODULE, ['crossroads.core', 'crossroads.common', 'ui.select'])
    .config(require('./mpTools.config'))
    .config(require('./mpTools.routes'))
    .constant('CRDS_TOOLS_CONSTANTS', require('./mpTools.constants'))
    .factory('MPTools', require('./mpTools.service'))
    .run(require('./mpTools.run'))
    ;

  // Require any needed html files
  require('./tools.html');

  // The SU2S Tool
  require('./signup_to_serve');

  // The KC Appliant Tool
  require('./kc_applicant');

  // The Check Batch Processor Tool
  require('./check_batch_processor');

  // The Trip Private Invitation Tool
  require('./trip_private_invite');

  // The GP Export Tool
  require('./gp_export');

  // The Administrator Tool Common
  require('./admin_tool_common');

  // The Administrator Giving History Tool
  require('./admin_giving_history');

  // The Administrator Recurring Gift Tool
  require('./admin_recurring_gift');

  // The Contact a volunteer group
  require('./volunteer_contact');

  // The Add Events Tool
  require('./add_event_tool');

  // The Add Events Tool
  require('./admin_checkin_dashboard');

  // Event Setup Tool
  require('./event_setup_tool');

  // Request Childcare Tool
  require('./request_childcare');

  // Childcare Decision Tool
  require('./childcare_decision');

})();
