import CONSTANTS from 'crds-constants';

export default class ServeTeamMembersController {
  /* @ngInject*/
  constructor() {
    // this.opportunities; from component binding
    this.allMembers = [];
    this.ready = false;
  }

  $onInit() {
    this.loadTeamMembers();
  }

  loadTeamMembers() {
    this.opportunities = this.splitMembers(this.opportunities);
    this.allMembers = [];

    _.forEach(this.opportunities, (opportunity) => {
      this.addTeam((`${opportunity.opportunityTitle} ${opportunity.roleTitle}`), opportunity.rsvpMembers);
    });
  }

  splitMembers(opportunities) {
    const notAvailable = {
      opportunityId: 0,
      opportunityTitle: 'Not Available',
      rsvpMembers: [],
      roleTitle: ''
    };
    _.forEach(opportunities, (opportunity) => {
      const partitionedArray = _.partition(opportunity.rsvpMembers, (member) => { return member.responseResultId === CONSTANTS.SERVING_RESPONSES.NOT_AVAILABLE; });
      notAvailable.rsvpMembers = notAvailable.rsvpMembers.concat(partitionedArray[0]);
      opportunity.rsvpMembers = partitionedArray[1];
    });
    _.map(notAvailable.rsvpMembers, (member) => {
      member.opportunityId = 0;
    });
    opportunities.push(notAvailable);
    return opportunities;
  }

  addTeam(teamName, members) {
    const team = {
      name: teamName,
      members: null
    };
    team.members = (members !== null) ? members : undefined;
    this.allMembers.push(team);
  }

  memberClick(member) {
    this.onMemberClick({ $member: member });
  }

  memberRemove(member) {
    this.onMemberRemove({ $member: member });
  }
}