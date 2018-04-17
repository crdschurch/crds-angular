ServeTeamMessageComponent.$inject = [];

import ServeTeamMessageController from './serveTeamMessage.controller';

export default function ServeTeamMessageComponent() {
  let serveTeamMessageComponent = {
    restrict: 'E',

    templateUrl: 'serve_team_message/serveTeamMessage.html',
    controller: ServeTeamMessageController,
    controllerAs: 'serveTeamMessage',
    bindToController: true
  };

  return serveTeamMessageComponent;
}
