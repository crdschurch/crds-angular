import CONSTANTS from 'crds-constants';
import ServeTeamMembersController from '../../app/my_serve/serve_team_members/serveTeamMembers.controller';

describe('ServeTeamMembersController', () => {
  let fixture,
    log,
    qApi,
    serveTeamService;

  beforeEach(angular.mock.module(CONSTANTS.MODULES.MY_SERVE));

  beforeEach(inject(($injector) => {
    fixture = new ServeTeamMembersController();
  }));


  it('should build out allMembers data object', () => {
    fixture.opportunities = opportunitiesData;
    expect(fixture.opportunities.length).toBe(3, 'because this is before Not Available opp has been added');

    fixture.loadTeamMembers();

    expect(fixture.allMembers.length).toBe(4);
    expect(fixture.allMembers[3].members.length).toBe(3);
    expect(_.uniq(fixture.allMembers[3].members, 'participantId').length).toBe(2);
    expect(fixture.opportunities.length).toBe(4, 'because a Not available opportunity is added');
  });

  let opportunitiesData = [{
    opportunityId: 2218723,
    opportunityTitle: '(t) Coffee Setup Sat 2:00',
    groupRoleId: 16,
    rsvpMembers: [{
    Event_ID: 4510383,
    participantId: 7554224,
    opportunityId: 2218723,
    groupRoleId: 16,
    nickName: 'Heather',
    lastName: 'Kerstanoff',
    responseResultId: 2,
    age: 18
  }, {
  Event_ID: 4510383,
  participantId: 7537153,
  opportunityId: 2218723,
  groupRoleId: 16,
  nickName: 'Joe',
  lastName: 'Kerstanoff',
  responseResultId: 1,
  age: 31
}
		]
  }, {
  opportunityId: 2218724,
  opportunityTitle: '(t) Coffee Team Sat 4:00',
  groupRoleId: 16,
  rsvpMembers: [{
    Event_ID: 4510383,
    participantId: 7554224,
    opportunityId: 2218724,
    groupRoleId: 16,
    nickName: 'Heather',
    lastName: 'Kerstanoff',
    responseResultId: 2,
    age: 18
  }
		]
}, {
  opportunityId: 2218725,
  opportunityTitle: '(t) Coffee Leader Sat 4:00',
  groupRoleId: 22,
  rsvpMembers: [{
    Event_ID: 4510383,
    participantId: 7554225,
    opportunityId: 2218723,
    groupRoleId: 22,
    nickName: 'Paige',
    lastName: 'Kerstanoff',
    responseResultId: 2,
    age: 13
  }, {
  Event_ID: 4510383,
  participantId: 7537155,
  opportunityId: 2218725,
  groupRoleId: 22,
  nickName: 'James',
  lastName: 'Kerstanoff',
  responseResultId: 1,
  age: 15
}
		]
}];
});