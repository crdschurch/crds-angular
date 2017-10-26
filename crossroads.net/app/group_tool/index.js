import groupToolRouter from './groupTool.routes';
import CONSTANTS from 'crds-constants';

export default angular.
  module(CONSTANTS.MODULES.GROUP_TOOL, [ CONSTANTS.MODULES.CORE, CONSTANTS.MODULES.COMMON,
                                          CONSTANTS.MODULES.FORMLY_BUILDER ]).
  config(groupToolRouter)
  ;

import cms from './cms';