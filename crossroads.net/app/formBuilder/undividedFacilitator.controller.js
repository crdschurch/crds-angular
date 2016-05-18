(function() {
  'use strict';

  module.exports = UndividedFacilitatorCtrl;

  UndividedFacilitatorCtrl.$inject = ['$rootScope', 'Group', 'Session'];

  function UndividedFacilitatorCtrl($rootScope, Group, Session) {
    var vm = this;

    var constants = require('crds-constants');

    vm.saving = false;
    vm.save = save;

    vm.responses = {};

    function save() {
      vm.saving = true;

      try {
        var singleAttributes = _.cloneDeep(vm.responses.singleAttributes);
        var coFacilitator = vm.responses[constants.CMS.FORM_BUILDER.FIELD_NAME.COFACILITATOR];

        if (coFacilitator && coFacilitator !== '') {

          var item = {
            attribute: {
              attributeId: constants.ATTRIBUTE_IDS.COFACILITATOR
            },
            notes: coFacilitator,
          };

          singleAttributes[constants.ATTRIBUTE_TYPE_IDS.COFACILITATOR] = item;
        }

        var participant = [{
          capacity: 1,
          contactId: parseInt(Session.exists('userId')),
          groupRoleId: constants.GROUP.ROLES.LEADER,
          childCareNeeded: vm.responses.Childcare,
          sendConfirmationEmail: false,
          singleAttributes: singleAttributes,
          attributeTypes: {},
        }];

        Group.Participant.save({
          groupId: constants.GROUP.GROUP_ID.UNDIVIDED_FACILITATOR,
        }, participant).$promise.then(function(response) {
          $rootScope.$emit('notify', $rootScope.MESSAGES.successfullRegistration);
          vm.saving = false;
        }, function(error) {
          $rootScope.$emit('notify', $rootScope.MESSAGES.generalError);
          vm.saving = false;
        });
      }
      catch (error) {
        vm.saving = false;
        throw (error);
      }
    }
  }

})();
