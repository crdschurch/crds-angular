import CONSTANTS from '../../../../constants';

export default class ChangeParticipantRoleController {
  constructor(GroupService, $anchorScroll, $rootScope, GroupDetailService) {
    this.groupService = GroupService;
    this.groupRoles = CONSTANTS.GROUP.ROLES;
    this.processing = false;
    this.anchorScroll = $anchorScroll;
    this.rootScope = $rootScope;
    this.groupDetailService = GroupDetailService;
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
    return !this.participant.isApprovedLeader;
  }

  warningLeaderMax() {
    let countLeaders = 0;
    if (!this.groupDetailService.participants) {
      countLeaders = 0;
    }
    else {
      countLeaders = this.groupDetailService.participants.filter(function (val) {
        return val.groupRoleId === CONSTANTS.GROUP.ROLES.LEADER;
      }).length;
    }

    if (countLeaders >= CONSTANTS.GROUP.MAX_LEADERS){
      return true;
    }
    return false;
  }

  warningApprenticeMax() {
    let countApprentices = 0;
    if (!this.groupDetailService.participants) {
      countApprentices = 0;
    }
    else {
      countApprentices = this.groupDetailService.participants.filter(function (val) {
        return val.groupRoleId === CONSTANTS.GROUP.ROLES.APPRENTICE;
      }).length;
    }
    if (countApprentices >= CONSTANTS.GROUP.MAX_APPRENTICE){
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
