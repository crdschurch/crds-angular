import CONSTANTS from 'crds-constants';
import ServeTeamContainerController from '../../app/my_serve/serve_team_container/serveTeamContainer.controller';

describe('ServeTeamContainerController', () => {
  let fixture;
  let qApi;
  let serveTeamService;
  let growl;
  let rootScope;
  let serveOpportunities;

  beforeEach(angular.mock.module(CONSTANTS.MODULES.MY_SERVE));

  beforeEach(inject(($injector) => {
    serveTeamService = $injector.get('ServeTeamService');
    qApi = $injector.get('$q');
    growl = $injector.get('growl');
    rootScope = $injector.get('$rootScope');
    serveOpportunities = $injector.get('ServeOpportunities');
    fixture = new ServeTeamContainerController(serveTeamService, qApi, serveOpportunities, growl, rootScope);
	fixture.team = team;
	fixture.team.serveOpportunities.push(notAvailableOpp);
  }));


  it('$onInit should call getIsLeader and getTeamRsvps', () => {
    spyOn(serveTeamService, 'getIsLeader').and.callFake(() => {
      const deferred = qApi.defer();
      deferred.resolve(true);
      return deferred.promise;
    });

    spyOn(serveTeamService, 'getTeamRsvps').and.callFake(() => {
      const deferred = qApi.defer();
      deferred.resolve({ opportunityId: 1234 });
      return deferred.promise;
    });

    fixture.team = { groupId: '123' };

    fixture.$onInit();

    expect(serveTeamService.getIsLeader).toHaveBeenCalled();
    expect(serveTeamService.getTeamRsvps).toHaveBeenCalled();
  });

  it('Update Team should add a new rsvp member', () => {
    const person = {
      displayName: 'Bourne, Jason',
      email: 'jasonbource@ip.com',
      participantId: 123,
      contactId: 1,
      nickName: 'Jason',
      lastName: 'Bourne'
    };
    const updatedOpp = _.find(fixture.team.serveOpportunities, { opportunityId: 2217865 });
    expect(updatedOpp.rsvpMembers.length).toBe(2);
    fixture.updateTeam(person, 2217865);
    expect(updatedOpp.rsvpMembers.length).toBe(3);
  });

  it('Update Team should move an rsvpMember from one opportunity to another', () => { 
    const person = {
      displayName: 'Man, Dude',
      email: 'dudeman@ip.com',
      participantId: 76453,
      contactId: 8,
      nickName: 'Dude',
      lastName: 'Man'
    };

    expect(fixture.team.serveOpportunities[0].rsvpMembers.length).toBe(3);
	expect(fixture.team.serveOpportunities[1].rsvpMembers.length).toBe(4);
    fixture.updateTeam(person, 2217635);
    expect(fixture.team.serveOpportunities[0].rsvpMembers.length).toBe(2);
	expect(fixture.team.serveOpportunities[1].rsvpMembers.length).toBe(5);
  });

  let team = {
    groupId: 166949,
    eventId: 4510385,
    eventType: 'Service - Oakley Saturday at 4:30pm',
    eventTypeId: 94,
    index: 1,
    name: 'KC Oakley Welcome Center',
    primaryContact: 'vlam@crossroads.net',
    pastDeadline: false,
    pastDeadlineMessage: 58,
    rsvpYesCount: 4,
    members: [{
      name: 'Joe',
      lastName: 'Kerstanoff',
      contactId: 2562378,
      emailAddress: 'jkerstanoff@callibrity.com',
      index: 1,
      roles: [{
        name: 'KC Welcome Center Sat 4:30 Leader',
        room: null,
        roleId: 2217865,
        maximum: 1,
        minimum: 1,
        shiftEndTime: '05:50 PM',
        shiftStartTime: '04:05 PM'
      }, {
        name: 'KC Welcome Center Sat 4:30 Member',
        room: 'OAK KC - Welcome Area',
        roleId: 2217635,
        maximum: 8,
        minimum: 5,
        shiftEndTime: '05:55 PM',
        shiftStartTime: '04:05 PM'
      }],
      serveRsvp: {
        roleId: 0,
        attending: false
      }
    }],
    serveOpportunities: [{
      opportunityId: 2217865,
      opportunityTitle: 'KC Welcome Center Sat 4:30',
      rsvpMembers: [{
        eventId: 4510385,
        participantId: 7482112,
        opportunityId: 2217865,
        groupRoleId: 22,
        nickName: 'Jason',
        lastName: 'Close',
        responseResultId: 1,
        age: 43
      }, {
        eventId: 4510385,
        participantId: 76453,
        opportunityId: 2217865,
        groupRoleId: 22,
        nickName: 'Dude',
        lastName: 'Man',
        responseResultId: 1,
        age: 32
      }],
      groupRoleId: 22,
      roleTitle: 'Leader',
      shiftStartTime: '16:05:00',
      shiftEndTime: '17:50:00',
      room: null,
      minimum: 1,
      maximum: 1
    }, {
      opportunityId: 2217635,
      opportunityTitle: 'KC Welcome Center Sat 4:30',
      rsvpMembers: [{
        eventId: 4510385,
        participantId: 7537153,
        opportunityId: 2217635,
        groupRoleId: 16,
        nickName: 'Joe',
        lastName: 'Kerstanoff',
        responseResultId: 1,
        age: 32
      }, {
        eventId: 4510385,
        participantId: 7478392,
        opportunityId: 2217635,
        groupRoleId: 16,
        nickName: 'Jackie',
        lastName: 'Seddon',
        responseResultId: 1,
        age: 66
      }, {
        eventId: 4510385,
        participantId: 7482731,
        opportunityId: 2217635,
        groupRoleId: 16,
        nickName: 'Tom',
        lastName: 'Seddon',
        responseResultId: 1,
        age: 67
      }, {
        eventId: 4510385,
        participantId: 7438866,
        opportunityId: 2217635,
        groupRoleId: 16,
        nickName: 'Doc',
        lastName: 'Bryant',
        responseResultId: 1,
        age: 47
      }],
      groupRoleId: 16,
      roleTitle: 'Member',
      shiftStartTime: '16:05:00',
      shiftEndTime: '17:55:00',
      room: 'OAK KC - Welcome Area',
      minimum: 5,
      maximum: 8
    }]
  };

  let notAvailableOpp = {
    opportunityId: 0,
    opportunityTitle: 'Not Available',
    rsvpMembers: [],
    roleTitle: ''
  };
});