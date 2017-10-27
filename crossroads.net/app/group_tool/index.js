import CONSTANTS from 'crds-constants';
import ParticipantService from './services/participant.service';
import GroupService from './services/group.service';
import groupToolRouter from './groupTool.routes';

export default angular.
  module(CONSTANTS.MODULES.GROUP_TOOL, [ CONSTANTS.MODULES.CORE, CONSTANTS.MODULES.COMMON,
                                          CONSTANTS.MODULES.FORMLY_BUILDER ]).
  config(groupToolRouter).
  service('ParticipantService', ParticipantService).
  service('GroupService', GroupService)
  ;

import cms from './cms';