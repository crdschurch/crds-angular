// TODO: This is a deprecated file due to the Pushpay work and is no longer in use
(function() {
  'use strict()';

  var constants = require('crds-constants');

  require('./recurring/templates/recurring_giving_create_modal.html');
  require('./recurring/templates/recurring_giving_edit_modal.html');
  require('./recurring/templates/recurring_giving_remove_modal.html');
  require('./recurring/templates/recurring_giving_list.html');
  require('./recurring/templates/recurring_gift_template.html');
  require('./commitment/templates/commitment_list.html');
  require('./profileGiving.html');

  var app = angular.module(constants.MODULES.PROFILE);
  app.controller('RecurringGivingModals', require('./recurring/recurring_giving_modals.controller'));
  app.controller('ProfileGivingController', require('./profile_giving.controller'));
  app.directive('recurringGivingList', require('./recurring/recurring_giving_list.directive'));
  app.directive('commitmentList', require('./commitment/commitment_list.directive'));
  app.factory('CommitmentService', require('./commitment/commitment.service'));

})();
