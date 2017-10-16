import constants from '../constants';

import WaiversRoutes from './waivers.routes';
import WaiversService from './waivers.service';

import SignWaiverComponent from './signWaiver/sign-waiver.component';

const { MODULES: modules } = constants;

export default angular.module(modules.WAIVERS, [modules.CORE, modules.COMMON])
  .config(WaiversRoutes)
  .component('signWaiver', SignWaiverComponent)
  .service('WaiversService', WaiversService)
  .name;
