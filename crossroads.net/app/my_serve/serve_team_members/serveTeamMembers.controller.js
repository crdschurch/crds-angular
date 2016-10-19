import CONSTANTS from 'crds-constants';

export default class ServeTeamMembersController {
  /*@ngInject*/
  constructor(ServeTeamService) {
    console.debug('Construct ServeTeamMembersController');
    this.servingOpportunities = {};
    this.rsvpNoMembers = [];
    this.rsvpYesLeaders = [];
    this.allMembers = [];
    this.serveTeamService = ServeTeamService;
    this.selectedRole = undefined;
    this.ready = false;
  }

  $onInit()
  {
    this.serveTeamService.getTeamRsvps(this.team).then((team) =>{
      this.loadTeamMembers(team);
      this.ready = true;
    });
  }

  loadTeamMembersSearch() {
        console.debug('Query team members');
        // TODO UI!!! IMPLEMENT THIS
        return [
          {
            id: 1001,
            name: 'Genie Simmons',
            email: 'gsimmons@gmail.com',
            phone: '513-313-5984',
            role: 'Leader'
          },
          {
            id: 1002,
            name: 'Holly Gennaro',
            email: 'hgennaro@excite.com',
            phone: '513-857-9587',
            role: null
          },
        ]
      }


  loadTeamMembers(team) {
      this.servingOpportunities = team.serveOppertunities; // gets passed in from component attribute.

      this.servingOpportunities = this.splitMembers(this.servingOpportunities);
      this.allMembers = [];

      this.addTeam('Leaders', this.rsvpYesLeaders);

      _.forEach(this.servingOpportunities, (opportunity) => {
        if(opportunity.Group_Role_ID !== CONSTANTS.GROUP.ROLES.LEADER)
        this.addTeam(opportunity.Opportunity_Title, opportunity.rsvpMembers);
      });

      this.addTeam('Not Available', _.uniq(this.rsvpNoMembers, 'Participant_ID'));
  }

  splitMembers(opportunities) {
    _.forEach(opportunities, (opportunity) => {
      let partitionedArray = _.partition(opportunity.rsvpMembers, (member) => {return member.Response_Result_ID === CONSTANTS.SERVING_RESPONSES.NOT_AVAILABLE});
      this.rsvpNoMembers = this.rsvpNoMembers.concat(partitionedArray[0]);
      partitionedArray = _.partition(partitionedArray[1], (member) => {return member.Group_Role_ID === CONSTANTS.GROUP.ROLES.LEADER});
      this.rsvpYesLeaders = this.rsvpYesLeaders.concat(partitionedArray[0]);
      opportunity.rsvpMembers = partitionedArray[1];
    })
    return opportunities;
  }

  addTeam(teamName, members)
  {
    let team = {
      name: teamName,
      members: null
    };
    team.members = (members !== null && members.length > 0) ? members : undefined;
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