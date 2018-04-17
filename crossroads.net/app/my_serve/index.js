import CONSTANTS from 'crds-constants';

import './myserve.html';
import './myserve_icon_key.html';
import './one_time_serve_mockup.html';
import './event_registration_mockup.html';
import './event_registration_mockup_desired.html';
import './serveTabs.html';
import './serveTeam.html';
import './refine/refineList.html';
import './refine/serveModalContent.html';

import OpportunityCapacityService from './capacity.service';
import FilterStateService from  './filterState.service.js';
import ServeTeamService from './serve_team.service.js';

import MyServeController from './myserve.controller';
import ServeModalController from './refine/serveModal.controller';

import ServeTabsDirective from './serveTabs.directive';
import ServeTeamDirective from './serveTeam.directive';
import RefineListDirective from './refine/refineList.directive';

import myServeRouter from './my_serve.routes';

export default angular
  .module(CONSTANTS.MODULES.MY_SERVE, [
    'ngTagsInput',

    CONSTANTS.MODULES.CORE,
    CONSTANTS.MODULES.COMMON
  ])

  .config(myServeRouter)

  .factory('ServeTeamFilterState', FilterStateService)
  .factory('OpportunityCapacityService', OpportunityCapacityService)
  .service('ServeTeamService', ServeTeamService)

  .service('ServeTeamService', ServeTeamService)

  .controller('MyServeController', MyServeController)
  .controller('ServeModalController', ServeModalController)

  .directive('serveTabs', ServeTabsDirective)
  .directive('serveTeam', ServeTeamDirective)
  .directive('serveTeamRefineList', RefineListDirective)
 // .filter('unique', UniqueFilter)
;

//
// Module sub-components
//

import './closable-tag';
import './serve_team_members';
import './serve_team_message';
import './serve_team_container';
import './serve_team_leader';
