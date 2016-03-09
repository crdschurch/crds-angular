(function() {
  'use strict';

  var MODULE = require('crds-constants').MODULES.GO_VOLUNTEER;

  require('./goVolunteerPage.template.html');

  angular.module(MODULE)
    .directive('goVolunteerPage', require('./goVolunteerPage.component'));

  require('./signin');
  require('./profilePage');
  require('./spouse');
  require('./orgName');
  require('./spouseName');
  require('./children');
  require('./childrenCount');
  require('./groupConnector');
  require('./groupFindConnector');
  require('./connectorInfo');  

})();
