import ServeTeamContainerController from './serveTeamContainer.controller';
import template from './serveTeamContainer.html';

export default function serveTeamContainerComponent() {
  return {
    bindings: {
      team: '=',
      oppServeDate: '<',
      oppServeTime: '<',
      onCancel: '&'
    },
    template,
    controller: ServeTeamContainerController,
    controllerAs: 'serveTeamContainer'
  };
}