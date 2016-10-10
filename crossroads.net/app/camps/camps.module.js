import CampRoutes from './camps.routes';
import constants from '../constants';
import CampComponent from './camp.component';
import CamperInfoComponent from './camper_info/camper_info.component';

import CampService from './camp.service';
import CamperInfoForm from './camper_info/camper_info_form.service';

var campsModule = angular.module(constants.MODULES.CAMPS, ['crossroads.core', 'crossroads.common'])
  .config(CampRoutes)
  .component('crossroadsCamp', CampComponent)
  .component('camperInfo', CamperInfoComponent)

  .service('CampService', CampService)
  .service('CamperInfoForm', CamperInfoForm)
  ;

export default campsModule;
