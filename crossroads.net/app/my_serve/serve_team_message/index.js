import CONSTANTS from 'crds-constants';

import serveTeamMessageComponent from './serveTeamMessage.component';
import './serveTeamMessage.html';
import './serveTeamMessageAutocomplete.html';

export default angular.
module(CONSTANTS.MODULES.MY_SERVE).
component('serveTeamMessage', serveTeamMessageComponent())
;
