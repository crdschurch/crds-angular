(function () {
  'use strict';

  var MODULE = require('crds-constants').MODULES;

  var app = angular.module(MODULE.CORKBOARD, [
    MODULE.CORE
  ]).constant('CORKBOARD_EVENTS', {
    postAdded: 'corkboard-post-added',
    postCanceled: 'corkboard-post-canceled'
  }).constant('CORKBOARD_TEMPLATES', {
    replyToTemplateId: 11419
  }).constant('CORKBOARD_ADMIN_ROLE_ID', 67);

  app.config(require('./corkboard.routes'));

  require('./templates/corkboard-listings.html');
  require('./templates/corkboard-listing-detail.html');
  require('./templates/post-item.html');
  require('./templates/post-event.html');
  require('./templates/post-job.html');
  require('./templates/post-need.html');
  require('./templates/corkboard.card.html');

  app.controller('CorkboardController', require('./controllers/corkboard.controller'));
  app.controller('CorkboardEditController', require('./controllers/corkboard.edit.controller'));

  app.directive('corkboardCard', require('./directives/corkboard.card.directive'));

  app.factory('CorkboardPostTypes', require('./services/corkboard.post-types.service'));
  app.factory('CorkboardListings', require('./services/corkboard.service'));
  app.factory('CorkboardSession', require('./services/corkboard.session.service'));
  app.factory('ContactAboutPost', require('./services/corkboard.contact.service'));

})();
