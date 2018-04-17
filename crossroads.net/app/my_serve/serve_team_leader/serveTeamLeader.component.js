import ServeTeamLeaderController from './serveTeamLeader.controller';
import template from './serveTeamLeader.html';

export default function serveTeamLeaderComponent() {
  return {
    bindings: {
      team: '<',
      oppServeDate: '<',
      oppServeTime: '<',
      onCancel: '&',
      onUpdateTeam: '&'
    },
    template,
    controller: ServeTeamLeaderController,
    controllerAs: 'serveTeamLeader'
  };
}