"use strict";

(function() {

  module.exports = VolunteerController;

  VolunteerController.$inject = ['$rootScope', '$scope', '$log', '$filter', 'MESSAGES', 'Session', '$state', 'Opportunity', 'ServeOpportunities', 'CmsInfo', '$modal'];

  function VolunteerController($rootScope, $scope, $log, $filter, MESSAGES, Session, $state, Opportunity, ServeOpportunities, CmsInfo, $modal) {
    $log.debug("Inside VolunteerController");
    var vm = this;

    vm.allSignedUp = allSignedUp;
    vm.cmsInfo = CmsInfo;
    vm.disableCheckbox = disableCheckbox;
    vm.displayEmail = displayEmail;
    vm.displayPendingFlag = displayPendingFlag;
    vm.editProfile = editProfile;
    vm.modalInstance = {};
    vm.pageInfo = pageInfo(CmsInfo);
    vm.participants = null;
    vm.save = save;
    vm.showAllSignedUp = false;
    vm.showContent = true;
    vm.showSuccess = false;
    vm.viewReady = false;

    activate();

    ///////////////////////////////////////////

    function activate() {
      if (CmsInfo.items.length > 0) {
        ServeOpportunities.QualifiedServers.query({
            groupId: vm.pageInfo.group,
            opportunityId: vm.pageInfo.opportunity
          }, function(response) {
            vm.participants = response;
            allSignedUp();
            vm.viewReady = true;
          }, function(err){
            $state.go('content', {link:'/server-error/'});
          });
      } else {
        $state.go('content', {link:'/server-error/'});
      }
    }

    function allSignedUp() {
      var signupCount = 0;
      _.each(vm.participants, function(p) {
        if (p.memberOfGroup || p.pending) {
          signupCount = signupCount + 1;
        }
      });
      if (signupCount === vm.participants.length) {
        vm.showAllSignedUp = true;
        vm.showContent = false;
      }
    }

    function disableCheckbox(participant) {
      if (participant.memberOfGroup || participant.pending) {
        return true;
      }
      return false;
    }

    function displayEmail(emailAddress) {
      if (emailAddress === null || emailAddress === undefined) {
        return false;
      }
      if (emailAddress.length > 0) {
        return true;
      }
      return false;
    }

    function displayPendingFlag(participant) {
      if (participant.memberOfGroup) {
        return false;
      }
      if (participant.pending) {
        return true;
      }
      return false;
    }

    function editProfile(personToEdit) {
      vm.modalInstance = $modal.open({
        templateUrl: 'profile/editProfile.html',
        backdrop: true,
        controller: "ProfileModalController as modal",
        openedClass: 'crds-legacy-styles',
        // This is needed in order to get our scope
        // into the modal - by default, it uses $rootScope
        scope: $scope,
        resolve: {
          person: function() {
            return personToEdit;
          }
        }
      });
      vm.modalInstance.result.then(function(person) {
        personToEdit.preferredName = person.nickName === null ? person.firstName : person.nickName;
        $rootScope.$emit("personUpdated", person);
      });
    }


    function pageInfo(cmsInfo) {
      return cmsInfo.items[0].fields;
    }

    function save(form) {
      var save = new ServeOpportunities.SaveQualifiedServers();
      save.opportunityId = vm.pageInfo.opportunity;
      //just get participants that have checkbox checkLoggedin
      save.participants = _.pluck(_.filter(vm.participants, {
        add: true
      }), 'participantId');

      $log.debug(save.participants.length);
      if (save.participants.length < 1) {
        $rootScope.$emit('notify', $rootScope.MESSAGES.noPeopleSelectedError);
        return;
      }

      save.$save().then(function() {
        vm.created = true;
        vm.showContent = false;
        vm.showSuccess = true;
      }, function() {
        vm.rejected = true;
      });
    }
  }
})();
