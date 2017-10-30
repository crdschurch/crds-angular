
import participantService from './participant.service';
import groupService from './group.service';
import CONSTANTS from 'crds-constants';

export default angular.
  module(CONSTANTS.MODULES.GROUP_TOOL).
  service('ParticipantService', participantService).
  service('GroupService', groupService);