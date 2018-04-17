import CONSTANTS from 'crds-constants';
import ServeTeamLeaderController from '../../app/my_serve/serve_team_leader/serveTeamLeader.controller';

describe('ServeTeamLeaderController', () => {
	let fixture,
		qApi,
		serveTeamService,
		rootScope,
		growl,
		serveOpportunities;

	beforeEach(angular.mock.module(CONSTANTS.MODULES.MY_SERVE));

	beforeEach(inject(function ($injector) {
		serveTeamService = $injector.get('ServeTeamService');
		serveOpportunities = $injector.get('ServeOpportunities');
		rootScope = $injector.get('$rootScope');
		growl = $injector.get('growl');
		qApi = $injector.get('$q');
		fixture = new ServeTeamLeaderController(serveTeamService, serveOpportunities, rootScope, growl, qApi);
	}));



	it('$onInit should call getAllTeamMembersForLoggedInLeader and getCapacity', () => {
		spyOn(serveTeamService, 'getAllTeamMembersForLoggedInLeader').and.callFake(function () {
			var deferred = qApi.defer();
			deferred.resolve(true);
			return deferred.promise;
		});

		spyOn(serveTeamService, 'getCapacity').and.callFake(function () {
			var deferred = qApi.defer();
			deferred.resolve();
			return deferred.promise;
		});

		fixture.team = team;
		fixture.oppServeDate = '10/10/2016';
		fixture.oppServeTime = '15:30:00';
		fixture.$onInit();

		expect(serveTeamService.getAllTeamMembersForLoggedInLeader).toHaveBeenCalled();
		expect(serveTeamService.getCapacity).toHaveBeenCalled();
		expect(serveTeamService.getCapacity.calls.count()).toBe(2);
		expect(fixture.model.selectedFrequency.value).toBe(0);
		expect(fixture.model.fromDt = fixture.oppserveDate);
	});

	it('populate dates should set model objects if selectedFrequency is null', () =>{
		fixture.model.selectedFrequency = {value: null};
		fixture.populateDates();
		expect(fixture.model.fromDt).toBeNull();
		expect(fixture.model.toDt).toBeNull();
		expect(fixture.datesDisabled).toBeTruthy();
	});

	it('populate dates should set model objects if selectedFrequency is 0', () =>{
		fixture.model.selectedFrequency = {value: 0};
		fixture.oppServeDate = '10/10/2016';
		fixture.populateDates();
		expect(fixture.model.fromDt).toBe('10/10/2016');
		expect(fixture.model.toDt).toBe('10/10/2016');
		expect(fixture.datesDisabled).toBeTruthy();
	});

	it('should construct date strings for once everyWeek and everyOtherWeek', () => {
		fixture.oppServeDate = '10/10/2016';
		fixture.oppServeTime = '15:00';
		let data = fixture.getFrequency();

		expect(data[0].value).toBe(0);
		expect(data[1].value).toBe(1);
		expect(data[2].value).toBe(2);
		expect(data[0].text).toBe('Once 10/10/2016 3:00pm');
		expect(data[1].text).toBe('Every Week Mondays 3:00pm');
		expect(data[2].text).toBe('Every Other Week Mondays 3:00pm');
		
	});

	it('should filter list of ppl for typeahead', () => {
		fixture.teamMembers = [{
			displayName: 'Silbernagel, MaTT',
			email: 'MaTTS@compuserve.net'
		}, {
			displayName: 'Kerstanoff, Joe',
			email: 'JoeKer@gmail.com'
		},{
			displayName: 'CinnamonBagel, Matty',
			email:'ilovebagels@aol.com'
		},{
			displayName: 'DirtyBaum, DJ',
			email:'illestbeats@yahoo.com'
		}]

		let data = fixture.loadTeamMembersSearch('MATT');
		expect(data.length).toBe(2);
	});

	it('should save rsvp', () => {
		spyOn(serveTeamService, 'getAllTeamMembersForLoggedInLeader').and.callFake(function () {
			var deferred = qApi.defer();
			deferred.resolve(true);
			return deferred.promise;
		});

		spyOn(serveTeamService, 'getCapacity').and.callFake(function () {
			var deferred = qApi.defer();
			deferred.resolve();
			return deferred.promise;
		});

		const onUpdateTeamSpy = jasmine.createSpy('onTeamUpdate');
		fixture.onUpdateTeam = onUpdateTeamSpy;
		fixture.team = team;
		fixture.team.serveOpportunities.push(notAvailableOpp);		
		fixture.oppServeDate = '10/10/2016';
		fixture.oppServeTime = '15:30:00';
	   
		fixture.$onInit();
		fixture.team = team;
		fixture.model.selectedOpp = 2217865;
		fixture.individuals.push({displayName: 'Kerstanoff, Joe', 
		email:'joeker@ip.com', 
		participantId:123,
		contactId: 1,
		nickName: 'Joe',
		lastName: 'Kerstanoff'
		},{
			displayName:'Nukem, Duke',
			email: 'theduke@compuserve.net',
			participantId: 43215,
			contactId: 2,
			nickName: 'Duke',
			lastName: 'Nukem'
		});

		spyOn(serveOpportunities.SaveRsvp, 'save').and.callFake(function (promises) {
			var deferred = qApi.defer();
			deferred.resolve();
			return deferred.promise;
		});

		serveOpportunities.SaveRsvp.save.calls.reset();
		expect(fixture.team.serveOpportunities[0].rsvpMembers.length).toBe(2);
		expect(fixture.individuals.length).toBe(2);
		let returnVal = fixture.saveRsvp();
		rootScope.$digest(); //makes qApi.all resolve

		expect(serveOpportunities.SaveRsvp.save.calls.count()).toBe(2);
		expect(onUpdateTeamSpy.calls.count()).toBe(2);		

	});

	var team = {
		"groupId": 166949,
		"eventId": 4510385,
		"eventType": "Service - Oakley Saturday at 4:30pm",
		"eventTypeId": 94,
		"index": 1,
		"name": "KC Oakley Welcome Center",
		"primaryContact": "vlam@crossroads.net",
		"pastDeadline": false,
		"pastDeadlineMessage": 58,
		"rsvpYesCount": 4,
		"members": [{
			"name": "Joe",
			"lastName": "Kerstanoff",
			"contactId": 2562378,
			"emailAddress": "jkerstanoff@callibrity.com",
			"index": 1,
			"roles": [{
				"name": "KC Welcome Center Sat 4:30 Leader",
				"room": null,
				"roleId": 2217865,
				"maximum": 1,
				"minimum": 1,
				"shiftEndTime": "05:50 PM",
				"shiftStartTime": "04:05 PM"
			}, {
				"name": "KC Welcome Center Sat 4:30 Member",
				"room": "OAK KC - Welcome Area",
				"roleId": 2217635,
				"maximum": 8,
				"minimum": 5,
				"shiftEndTime": "05:55 PM",
				"shiftStartTime": "04:05 PM"
			}
			],
			"serveRsvp": {
				"roleId": 0,
				"attending": false
			}
		}
		],
		"serveOpportunities": [{
			"Opportunity_ID": 2217865,
			"Opportunity_Title": "KC Welcome Center Sat 4:30",
			"rsvpMembers": [{
				"Event_ID": 4510385,
				"Participant_ID": 7482112,
				"Opportunity_ID": 2217865,
				"Group_Role_ID": 22,
				"NickName": "Jason",
				"Last_Name": "Close",
				"Response_Result_ID": 1,
				"age": 43
			}, {
				"Event_ID": 4510385,
				"Participant_ID": 7537153,
				"Opportunity_ID": 2217865,
				"Group_Role_ID": 22,
				"NickName": "Dude",
				"Last_Name": "Man",
				"Response_Result_ID": 1,
				"age": 32
			}
			],
			"Group_Role_ID": 22,
			"roleTitle": "Leader",
			"shiftStartTime": "16:05:00",
			"shiftEndTime": "17:50:00",
			"room": null,
			"minimum": 1,
			"maximum": 1
		}, {
			"Opportunity_ID": 2217635,
			"Opportunity_Title": "KC Welcome Center Sat 4:30",
			"rsvpMembers": [{
				"Event_ID": 4510385,
				"Participant_ID": 7537153,
				"Opportunity_ID": 2217635,
				"Group_Role_ID": 16,
				"NickName": "Joe",
				"Last_Name": "Kerstanoff",
				"Response_Result_ID": 1,
				"age": 32
			}, {
				"Event_ID": 4510385,
				"Participant_ID": 7478392,
				"Opportunity_ID": 2217635,
				"Group_Role_ID": 16,
				"NickName": "Jackie",
				"Last_Name": "Seddon",
				"Response_Result_ID": 1,
				"age": 66
			}, {
				"Event_ID": 4510385,
				"Participant_ID": 7482731,
				"Opportunity_ID": 2217635,
				"Group_Role_ID": 16,
				"NickName": "Tom",
				"Last_Name": "Seddon",
				"Response_Result_ID": 1,
				"age": 67
			}, {
				"Event_ID": 4510385,
				"Participant_ID": 7438866,
				"Opportunity_ID": 2217635,
				"Group_Role_ID": 16,
				"NickName": "Doc",
				"Last_Name": "Bryant",
				"Response_Result_ID": 1,
				"age": 47
			}
			],
			"Group_Role_ID": 16,
			"roleTitle": "Member",
			"shiftStartTime": "16:05:00",
			"shiftEndTime": "17:55:00",
			"room": "OAK KC - Welcome Area",
			"minimum": 5,
			"maximum": 8
		}
		]
	};

	var notAvailableOpp = {
		Opportunity_ID: 0,
		Opportunity_Title: "Not Available",
		rsvpMembers: [],
		roleTitle: ""
	}

});
