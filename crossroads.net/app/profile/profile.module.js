(function() {
  'use strict';

  var MODULES = require('crds-constants').MODULES;

  angular.module(MODULES.PROFILE, [MODULES.CORE, MODULES.COMMON])
    .config(require('./profile.routes'))
    .controller('ProfileController', require('./profile.controller'))
    .controller('ProfileModalController', require('./profileModalController'))
    ;

  require('./communications');
  require('./services');
  require('./skills');

  require('./profile.html');
  require('./editProfile.html');
  require('./personal/profilePersonal.html');
})();
