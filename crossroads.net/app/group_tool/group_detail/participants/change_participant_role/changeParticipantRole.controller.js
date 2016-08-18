import CONSTANTS from '../../../../constants';

export default class ChangeParticipantRoleController {
  constructor(GroupService, $anchorScroll, $rootScope) {
    this.groupService = GroupService;
    this.groupRoles = CONSTANTS.GROUP.ROLES;
    this.processing = false;
    this.anchorScroll = $anchorScroll;
    this.rootScope = $rootScope;
  }

  submit() {
    this.processing = true;

      var promise = this.groupService.updateParticipant(this.participant)
        .then((data) => {
          this.rootScope.$emit('notify', this.rootScope.MESSAGES.successfulSubmission);
        },
        (data) => {
          this.rootScope.$emit('notify', this.rootScope.MESSAGES.generalError);
        }).finally(() => {
          this.processing = false;
          this.cancel();
        });

  }

  isParticipant() {
    return (this.participant.groupRoleId === CONSTANTS.GROUP.ROLES.MEMBER);
  }

  isLeader() {
    return (this.participant.groupRoleId === CONSTANTS.GROUP.ROLES.LEADER);
  }

  isApprentice() {
    return (this.participant.groupRoleId === CONSTANTS.GROUP.ROLES.APPRENTICE);
  }

  leaderDisabled() {
    return false;
  }

  apprenticeDisabled() {
    return false;
  }

  warningLeaderMax() {
    //TODO remove hard coded 5
    if (this.rootScope.countLeaders >= 5){
      return true;
    }
    return false;
  }

  warningLeaderApproval() {
    return false;
  }

  warningApprenticeMax() {
    //TODO remove hard coded 5
    if (this.rootScope.countApprentice >= 5){
      return true;
    }
    return false;
  }

  cancel() {
    // Invoke the parent callback function
    this.cancelAction();
    this.anchorScroll();
  }
}
