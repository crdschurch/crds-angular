import CONSTANTS from 'crds-constants';

export default class ServeTeamMembersController {
  /*@ngInject*/
  constructor() {
    console.debug('Construct ServeTeamMembersController');
    //this.opportunities; from component binding
    this.allMembers = [];
    this.selectedRole = undefined;
    this.ready = false;
  }

  $onInit()
  {
    this.loadTeamMembers();
  }

  $onChanges(teamChanges){
    console.log("changes Happened");
  }

  loadTeamMembers() {
      this.opportunities = this.splitMembers(this.opportunities);
      this.allMembers = [];

      _.forEach(this.opportunities, (opportunity) => {
        this.addTeam((opportunity.Opportunity_Title + " " + opportunity.roleTitle), opportunity.rsvpMembers);
      });
  }

  splitMembers(opportunities) {
    let notAvailable ={
        Opportunity_ID: 0,
        Opportunity_Title: "Not Available",
        rsvpMembers: [],
        roleTitle: ""
    };
    _.forEach(opportunities, (opportunity) => {
      let partitionedArray = _.partition(opportunity.rsvpMembers, (member) => {return member.Response_Result_ID === CONSTANTS.SERVING_RESPONSES.NOT_AVAILABLE});
      notAvailable.rsvpMembers = notAvailable.rsvpMembers.concat(partitionedArray[0]);
      opportunity.rsvpMembers = partitionedArray[1];
    });
    opportunities.push(notAvailable);
    return opportunities;
  }

  addTeam(teamName, members)
  {
    let team = {
      name: teamName,
      members: null
    };
    team.members = (members !== null) ? members : undefined;
    this.allMembers.push(team);
  }

  memberClick(member) {
    console.debug('member click', member);
    this.onMemberClick({ $member: member });
  }

  memberRemove(member) {
    console.debug('member remove', member);
    this.onMemberRemove({ $member: member });
  }
}