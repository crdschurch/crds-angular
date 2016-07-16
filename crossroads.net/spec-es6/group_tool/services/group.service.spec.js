
import CONSTANTS from 'crds-constants';
import GroupService from '../../../app/group_tool/services/group.service'
import SmallGroup from '../../../app/group_tool/model/smallGroup';
import GroupInquiry from '../../../app/group_tool/model/groupInquiry';
import GroupInvitation from '../../../app/group_tool/model/groupInvitation';

describe('Group Tool Group Service', () => {
  let fixture,
    log,
    resource,
    deferred,
    AuthService,
    authenticated,
    httpBackend,
    ImageService;

  const endpoint = `${window.__env__['CRDS_API_ENDPOINT']}api`;

  beforeEach(angular.mock.module(CONSTANTS.MODULES.GROUP_TOOL));

  beforeEach(inject(function($injector) {
    log = $injector.get('$log');
    resource = $injector.get('$resource');
    deferred = $injector.get('$q');
    AuthService = $injector.get('AuthService');
    httpBackend = $injector.get('$httpBackend');
    ImageService = $injector.get('ImageService');

    fixture = new GroupService(log, resource, deferred, AuthService, ImageService);
  }));

  afterEach(() => {
    httpBackend.verifyNoOutstandingExpectation();
    httpBackend.verifyNoOutstandingRequest();
  });

  describe('getMyGroups() groups', () => {
    it('should return groups for authenticated user', () => {
        let groups = [{
          'groupName': 'Learning and Growing In Life',
          'groupDescription': 'Learn about Jesus and Life Managment',
          'groupId': 172272,
          'groupTypeId': 1,
          'ministryId': 0,
          'congregationId': 0,
          'contactId': 0,
          'contactName': null,
          'primaryContactEmail': null,
          'startDate': '0001-01-01T00:00:00',
          'endDate': null,
          'availableOnline': false,
          'remainingCapacity': 0,
          'groupFullInd': false,
          'waitListInd': false,
          'waitListGroupId': 0,
          'childCareInd': false,
          'minAge': 0,
          'SignUpFamilyMembers': null,
          'events': null,
          'meetingDayId': null,
          'meetingDay': 'Friday',
          'meetingTime': '12:30:00',
          'meetingFrequency': 'Every Week',
          'meetingTimeFrequency': 'Friday\'s at 12:30 PM, Every Week',
          'groupRoleId': 0,
          'address': {
            'addressId': null,
            'addressLine1': 'Fake Street 98th',
            'addressLine2': null,
            'city': 'Madison',
            'state': 'IN',
            'zip': '47250',
            'foreignCountry': null,
            'county': null
          },
          'attributeTypes': {},
          'singleAttributes': {},
          'maximumAge': 0,
          'minimumParticipants': 0,
          'maximumParticipants': 0,
          'Participants': [
            {
              'participantId': 7537153,
              'contactId': 2562378,
              'groupParticipantId': 14581869,
              'nickName': 'Dustin',
              'lastName': 'Kocher',
              'groupRoleId': 22,
              'groupRoleTitle': 'Leader',
              'email': 'dtkocher@callibrity.com',
              'attributeTypes': null,
              'singleAttributes': null
            },
            {
              'participantId': 7547422,
              'contactId': 7654100,
              'groupParticipantId': 14581873,
              'nickName': 'Jim',
              'lastName': 'Kriz',
              'groupRoleId': 21,
              'groupRoleTitle': 'Leader',
              'email': 'jim.kriz@ingagepartners.com',
              'attributeTypes': null,
              'singleAttributes': null
            }
          ]
        },{
          'groupName': 'Bible Study',
          'groupDescription': 'Learn',
          'groupId': 172,
          'groupTypeId': 1,
          'ministryId': 0,
          'congregationId': 0,
          'contactId': 0,
        }];

        let groupsObj = groups.map((group) => {
          return new SmallGroup(group);
        });
        console.log('hi');
        httpBackend.expectGET(`${endpoint}/group/mine/${CONSTANTS.GROUP.GROUP_TYPE_ID.SMALL_GROUPS}`).
                    respond(200, groups);

        var promise = fixture.getMyGroups();
        httpBackend.flush();
        expect(promise.$$state.status).toEqual(1);
        promise.then(function(data) {
        	expect(data[0].groupName).toEqual(groupsObj[0].groupName);
        	expect(data[0].address.length).toEqual(groupsObj[0].address.length);
        	expect(data[0].participants.length).toEqual(groupsObj[0].groupName.participants.length);
        });
    });
  });
  
  describe('getInvities() inquires', () => {
    it('should return all invities assocated to the group', () => {
      let mockInvities = [
        {
          "sourceId": 172286,
          "groupRoleId": 16,
          "emailAddress": "for@me.com",
          "recipientName": "Knowledge Man",
          "requestDate": "2016-07-14T11:00:00",
          "invitationType": 1,
          "invitationId": 0,
          "invitationGuid": null
        },
        {
          "sourceId": 172286,
          "groupRoleId": 16,
          "emailAddress": "really@fast.com",
          "recipientName": "Buffer Dude",
          "requestDate": "2016-07-14T11:00:00",
          "invitationType": 1,
          "invitationId": 0,
          "invitationGuid": null
        }
      ];

      let groupId = 172286;

      let invities = mockInvities.map((invitation) => {
        return new GroupInvitation(invitation);
      });
      
      httpBackend.expectGET(`${endpoint}/grouptool/invitations/${groupId}/1`).
                  respond(200, mockInvities);

      var promise = fixture.getInvities(groupId);
      httpBackend.flush();
      expect(promise.$$state.status).toEqual(1);
      promise.then(function(data) {
        expect(data[1].sourceId).toEqual(invities[1].sourceId);
        expect(data[1].groupRoleId).toEqual(invities[1].groupRoleId);
        expect(data[1].emailAddress).toEqual(invities[1].emailAddress);
        expect(data[1].recipientName).toEqual(invities[1].recipientName);
        expect(data[1].requestDate).toEqual(invities[1].requestDate);
        expect(data[1].invitationType).toEqual(invities[1].invitationType);
        expect(data[1].invitationId).toEqual(invities[1].invitationId);
        expect(data[1].invitationGuid).toEqual(invities[1].invitationGuid);
      });
    });
  });

  describe('getInquires() inquires', () => {
    it('should return all inquiries assocated to the group', () => {
      let mockInquires = [
        {
          "groupId": 172286,
          "emailAddress": "jim.kriz@ingagepartners.com",
          "phoneNumber": "513-432-1973",
          "firstName": "Dustin",
          "lastName": "Kocher",
          "requestDate": "2016-07-14T10:00:00",
          "placed": false,
          "inquiryId": 19,
          "contactId": 123
        },
        {
          "groupId": 172286,
          "emailAddress": "jkerstanoff@callibrity.com",
          "phoneNumber": "513-987-1983",
          "firstName": "Joe",
          "lastName": "Kerstanoff",
          "requestDate": "2016-07-14T10:00:00",
          "placed": false,
          "inquiryId": 20,
          "contactId": 124
        },
        {
          "groupId": 172286,
          "emailAddress": "kim.farrow@thrivecincinnati.com",
          "phoneNumber": "513-874-6947",
          "firstName": "Kim",
          "lastName": "Farrow",
          "requestDate": "2016-07-14T10:00:00",
          "placed": true,
          "inquiryId": 21,
          "contactId": 124
        }
      ];

      let groupId = 172286;

      let inquires = mockInquires.map((inquiry) => {
        return new GroupInquiry(inquiry);
      });
      
      httpBackend.expectGET(`${endpoint}/grouptool/inquiries/${groupId}`).
                  respond(200, mockInquires);

      var promise = fixture.getInquiries(groupId);
      httpBackend.flush();
      expect(promise.$$state.status).toEqual(1);
      promise.then(function(data) {
        expect(data[0].groupId).toEqual(inquires[0].groupId);
        expect(data[0].emailAddress).toEqual(inquires[0].emailAddress);
        expect(data[0].phoneNumber).toEqual(groupsObj[0].phoneNumber);
        expect(data[0].firstName).toEqual(inquires[0].firstName);
        expect(data[0].lastName).toEqual(inquires[0].lastName);
        expect(data[0].requestDate).toEqual(inquires[0].requestDate);
        expect(data[0].placed).toEqual(inquires[0].placed);
        expect(data[0].inquiryId).toEqual(inquires[0].inquiryId);
      });
    });
  });
});