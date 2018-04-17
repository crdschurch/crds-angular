import ServeTeamMembersController from './serveTeamMembers.controller';
import template from './serveTeamMembers.html';

export default function serveTeamMembersComponent() {
  return {
    bindings: {
      opportunities: '<',
      onMemberClick: '&',
      onMemberRemove: '&',
      isLeader: '='
    },
    template,
    controller: ServeTeamMembersController,
    controllerAs: 'serveTeamMembers'
  }
}