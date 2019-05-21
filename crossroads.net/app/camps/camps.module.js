import CampRoutes from './camps.routes';
import constants from '../constants';

import FormlyConfig from './camps.formly';
import './formly_wrappers/bootstrap_row.html';

import CampComponent from './camp.component';
import CampsDashboardComponent from './dashboard/camps_dashboard.component';
import CampCardComponent from './dashboard/camp_card/camp_card.component';
import CampHouseholdMembersComponent from './camps_family/camp_household_members/camp_household_members.component';
import CampsFamilyComponent from './camps_family/camps_family.component';
import CampThankYouComponent from './thank_you/camp_thank_you.component';

import CampsService from './camps.service';

import ApplicationPage from './application_page/application_page.module';

export default angular.module(constants.MODULES.CAMPS, [
  constants.MODULES.CORE,
  constants.MODULES.COMMON,
  constants.MODULES.FORMLY_BUILDER,
  ApplicationPage])
  .config(CampRoutes)
  .config(FormlyConfig)
  .component('crossroadsCamp', CampComponent)
  .component('campsDashboard', CampsDashboardComponent)
  .component('campCard', CampCardComponent)
  .component('campHouseholdMembers', CampHouseholdMembersComponent)
  .component('campsFamily', CampsFamilyComponent)
  .component('campThankYou', CampThankYouComponent)
  .service('CampsService', CampsService)
  .name;
