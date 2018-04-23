import constants from '../constants';
//Removing routes to prevent users from accessing Go Local 2016/2017
//All of the Go Volunteer code should really be deleted as it is dead
//import routes from './goVolunteer.routes';
import formly from './goVolunteer.formly';
import goVolunteerService from './goVolunteer.service';
import goVolunteerOrganizations from './organizations.service';
import goVolunteerDataService from './goVolunteerData.service';
import skillsService from './skills.service';
import groupConnectors from './groupConnectors.service';

export default angular.module(constants.MODULES.GO_VOLUNTEER, ['crossroads.core', 'crossroads.common', 'ngFileSaver'])
 // .config(routes)
  .config(formly)
  .service('GoVolunteerService', goVolunteerService)
  .service('Organizations', goVolunteerOrganizations)
  .service('GoVolunteerDataService', goVolunteerDataService)
  .factory('SkillsService', skillsService)
  .factory('GroupConnectors', groupConnectors)
  .name
  ;

require('./anywhereLeader');
require('./cms');
require('./city');
require('./organizations');
require('./page');

require('./projectCard');
