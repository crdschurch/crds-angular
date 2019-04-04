(function () {
  'use strict';
  var CONSTANTS = require('../../constants');
  var moment = require('moment');
  var formatDate = crds_utilities.formatDate;

  module.exports = CommunityGroupsController;

  CommunityGroupsController.$inject = [
    '$scope',
    '$rootScope',
    'Profile',
    'Group',
    '$log',
    '$stateParams',
    'SignUpForm',
    '$modal',
    'ChildCare',
    'Session'
  ];

  function CommunityGroupsController(
    $scope,
    $rootScope,
    Profile,
    Group,
    $log,
    $stateParams,
    SignUpForm,
    $modal,
    ChildCare,
    Session) {

    var vm = this;
    var now = moment();

    vm.allSignedUp = allSignedUp;
    vm.alreadySignedUp = false;
    vm.atLeastOneParticipant = false;
    vm.childCareEvents = undefined;
    vm.childCareAvailable = false;
    vm.childCareNeeded = false;
    vm.childCareChange = childCareChange;
    vm.contactId = Session.exists('userId') !== undefined ? Session.exists('userId') : 0;
    vm.editProfile = editProfile;
    vm.formValid = true;
    vm.hasParticipantID = hasParticipantID;
    vm.modalInstance = {};
    vm.person = {};
    vm.profileButtonText = 'Save Profile';
    vm.response = {};
    vm.saving = false;
    vm.showContent = true;
    vm.showFull = false;
    vm.showSuccess = false;
    vm.showWaitList = false;
    vm.showWaitSuccess = false;
    vm.signup = signup;
    vm.signupPage = $rootScope.signupPage;
    vm.viewReady = false;
    vm.waitListCase = false;

    activate();

    ///////////////////////////

    function activate() {
      var link = (_.endsWith($stateParams.link, '/') === false) ? $stateParams.link + '/' : $stateParams.link;

      SignUpForm.get({ url: link }).then(function (pageRequest) {
        if (pageRequest.items.length > 0) {
          vm.signupPage = pageRequest.items[0].fields;
          vm.groupId = vm.signupPage.group;
          vm.groupDetails = Group.Detail.get({
            groupId: vm.groupId
          }).$promise.then(function (response) {
            vm.response = response.SignUpFamilyMembers;
            vm.groupEvents = response.events;
            vm.childCareEvents = _.find(vm.groupEvents, function (i) {
              return (i.eventType == CONSTANTS.EVENTS.EVENT_TYPES.CHILDCARE && moment(i.startDate).isAfter(now));
            });

            if (vm.childCareEvents !== undefined) {
              vm.childCareAvailable = true;
              vm.childCareNeeded = true;
            }

            if (!response.waitListInd) {
              vm.viewReady = true;
            }

            if (allSignedUp(response)) {
              vm.alreadySignedUp = true;
            }

            if (response.groupFullInd && response.waitListInd) {
              vm.waitListCase = true;
              vm.showWaitList = true;
              vm.signupPage.title = vm.signupPage.title + ' - Waitlist';
              vm.groupId = response.waitListGroupId;

              // I now need to get the group-detail again for the wait list there are are two new possible cases
              // 1. the user is a already a member
              // 2. the user is not yet a member
              // 3. DE616 the user is already member of non-wait list group
              var originalMembers = vm.response;
              vm.groupDetails = Group.Detail.get({
                groupId: vm.groupId
              }).$promise.then(function (response) {
                var familyMembers = response.SignUpFamilyMembers;
                _.forEach(familyMembers, function (member) {
                  if (!member.userInGroup) {
                    var m = _.find(originalMembers, function (i) {
                      return i.participantId === member.participantId;
                    });

                    member.userInGroup = m.userInGroup;
                  }
                });

                vm.response = familyMembers;
                vm.childCareAvailable = response.childCareInd;
                if (allSignedUp(response)) {
                  vm.alreadySignedUp = true;
                }

                vm.viewReady = true;
              });

              // this is the case where the group is full and
              // there is NO waitlist and at least one of your family IS a participant
            } else if (response.groupFullInd &&
              !response.waitListInd &&
              vm.atLeastOneParticipant) {
              vm.waitListCase = false;
              vm.showFull = true;
              vm.showContent = true;
              vm.showWaitList = false;
              vm.viewReady = true;

              //this is the case where the group is NOT full and there IS waitlist
            } else if (!response.groupFullInd && response.waitListInd) {
              vm.waitListCase = false;
              vm.showFull = false;
              vm.showContent = true;
              vm.showWaitList = false;
              vm.viewReady = true;

              //this is the case where the group is full and there is NO waitlist
            } else if (response.groupFullInd && !response.waitListInd && !vm.alreadySignedUp) {
              vm.showFull = true;
              vm.waitListCase = false;
              vm.showContent = false;
              vm.showWaitList = false;
              vm.viewReady = true;

            }
          });
        } else {
          const queryParams = $location.search();
          link = removeTrailingSlashIfNecessary($stateParams.link);
          const queryParamsString = angular.equals(queryParams, {}) ? '' : `?${$httpParamSerializer(queryParams)}`;
          $window.location.replace(`${link}${queryParamsString}`);
        }
      });
    }

    function allSignedUp(array) {
      var result = false;
      if (array.SignUpFamilyMembers.length > 1) {
        for (var i = 0; i < array.SignUpFamilyMembers.length; i++) {
          if (array.SignUpFamilyMembers[i].userInGroup === false) {
            result = false;
            break;
          } else {
            result = true;
            vm.atLeastOneParticipant = true;
          }
        }
      } else {
        if (array.SignUpFamilyMembers[0].userInGroup === false) {
          result = false;
        } else {
          result = true;
        }
      }

      return result;
    }

    function childCareChange(changedRecord) {
      _.forEach(vm.response, function (found) {
        if (found.participantId === changedRecord.participantId) {
          found.childCareNeeded = changedRecord.value;
        }
      });
    }

    function editProfile() {
      vm.modalInstance = $modal.open({
        templateUrl: 'editProfile.html',
        openedClass: 'crds-legacy-styles',
        backdrop: true,
        scope: $scope
      });
    }

    function hasParticipantID(array) {
      var result = {};
      result.partId = [];
      if (array.length > 1) {
        for (var i = 0; i < array.length; i++) {
          if (array[i].newAdd !== undefined && array[i].newAdd !== '') {
            result.partId[result.partId.length] = {
              participantId: array[i].newAdd,
              childCareNeeded: vm.childCareNeeded,
              capacityNeeded: 0,
              sendConfirmationEmail: true
            };
          }
        }
      } else if (array.length === 1) {
        result.partId[0] = {
          participantId: array[0].participantId,
          childCareNeeded: vm.childCareNeeded,
          capacityNeeded: 0,
          sendConfirmationEmail: true
        };
      }

      return result;
    }

    function signup() {
      vm.saving = true;
      var participantArray = hasParticipantID(vm.response);
      var flag = false;
      for (var i = 0; i < vm.response.length; i++) {
        if (!vm.response[i].userInGroup &&
          vm.response[i].newAdd !== undefined &&
          vm.response[i].newAdd !== '') {
          flag = true;
          break;
        }
      }

      if (vm.response.length === 1) {
        flag = true;
      }

      vm.formValid = flag;
      if (!vm.formValid) {
        $rootScope.$emit('notify', $rootScope.MESSAGES.noPeopleSelectedError);
        vm.saving = false;
        return;
      }

      //Add Person to group
      Group.Participant.save({
        groupId: vm.groupId
      }, participantArray.partId).$promise.then(function () {
        if (vm.waitListCase) {
          $rootScope.$emit('notify', $rootScope.MESSAGES.successfullWaitlistSignup);
        } else {
          $rootScope.$emit('notify', $rootScope.MESSAGES.successfullRegistration);
        }

        vm.showContent = false;
        vm.showSuccess = true;
        vm.showWaitList = false;
        vm.showWaitSuccess = true;
        vm.saving = false;

      }, function (error) {
        // 422 indicates an HTTP "Unprocessable Entity", in this case meaning Group is Full
        // http://tools.ietf.org/html/rfc4918#section-11.2
        if (error.status === 422) {
          vm.groupDetails = Group.Detail.get({
            groupId: vm.groupId
          }).$promise.then(function (res) {
            if (res.waitListInd) {
              $rootScope.$emit('notify', $rootScope.MESSAGES.notEnoughRoomError);
              vm.waitListCase = true;
              vm.showWaitList = true;
              vm.signupPage.title = vm.signupPage.title + ' - Waitlist';
              vm.groupId = res.waitListGroupId;
            }
            else {
              $rootScope.$emit('notify', $rootScope.MESSAGES.fullGroupError);
              vm.showFull = true;
              vm.showContent = false;
            }
          });

        } else {
          $rootScope.$emit('notify', $rootScope.MESSAGES.generalError);
          vm.showFull = false;
          vm.showContent = true;
        }

        vm.saving = false;

      });
    }

  }
})();
