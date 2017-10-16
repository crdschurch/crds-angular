
import GroupInvitation from '../model/groupInvitation';
import CONSTANTS from '../../constants';
import SmallGroup from '../model/smallGroup';
import Participant from '../model/participant';
import GroupInquiry from '../model/groupInquiry';

export default class GroupService {
  /*@ngInject*/
  constructor($log, $resource, $q, $location, AuthService, LookupService, Profile, ImageService) {
    this.log = $log;
    this.resource = $resource;
    this.profile = Profile;
    this.qApi = $q;
    this.location = $location;
    this.auth = AuthService;
    this.lookupService = LookupService;
    this.imgService = ImageService;
  }

  groupLeaderUrl() {
    return this.resource(__GATEWAY_CLIENT_ENDPOINT__ + 'api/v1.0.0/group-leader/url-segment').get().$promise.then((result) => {
      return result.url;
    });
  }

  getAgeRanges() {
    return this.resource(__GATEWAY_CLIENT_ENDPOINT__ + 'api/attributetype/:attributeTypeId').
      get({ attributeTypeId: CONSTANTS.ATTRIBUTE_TYPE_IDS.GROUP_AGE_RANGE }).$promise;
  }

  getProfileData() {
    return this.profile.Personal.get().$promise;
  }

  getGroupData(groupId) {
    return this.resource(__GATEWAY_CLIENT_ENDPOINT__ + 'api/group/:groupId').
      get({ groupId: groupId }).$promise;
  }

  getGroupGenderMixType() {
    return this.resource(__GATEWAY_CLIENT_ENDPOINT__ + 'api/attributetype/:attributeTypeId').
      get({ attributeTypeId: CONSTANTS.ATTRIBUTE_TYPE_IDS.GROUP_TYPE }).$promise;
  }

  getEndedReasons() {
    return this.resource(__GATEWAY_CLIENT_ENDPOINT__ + 'api/lookup/groupreasonended').query().$promise;
  }

  getStates() {
    return this.resource(__GATEWAY_CLIENT_ENDPOINT__ + 'api/lookup/states').query().$promise;
  }

  getCountries() {
    return this.resource(__GATEWAY_CLIENT_ENDPOINT__ + 'api/lookup/countries').query().$promise;
  }

  getSites() {
    return this.lookupService.Sites.query().$promise;
  }

  getDaysOfTheWeek() {
    return this.lookupService.DaysOfTheWeek.query().$promise;
  }

  getGenders() {
    return this.lookupService.Genders.query().$promise;
  }

  getGroupTypeCategories() {
    return this.resource(__GATEWAY_CLIENT_ENDPOINT__ + 'api/grouptool/categories').query().$promise;
  }

  sendGroupInvitation(invitation) {
    return this.resource(__GATEWAY_CLIENT_ENDPOINT__ + 'api/invitation').save(invitation).$promise;
  }

  getMyGroups() {
    let promised = this.resource(`${__GATEWAY_CLIENT_ENDPOINT__}api/group/mine`).query().$promise;

    return promised.then((data) => {
      let groups = data.map((group) => {
        return new SmallGroup(group);
      });

      return groups;
    },
      (err) => {
        throw err;
      });
  }

  getGroup(groupId) {
    let promise = this.resource(`${__GATEWAY_CLIENT_ENDPOINT__}api/group/mine/:groupId`).
      query({ groupId: groupId }).$promise;

    return promise.then((data) => {
      let groups = data.map((group) => {
        return new SmallGroup(group);
      });

      if (!groups || groups.length === 0) {
        var err = { 'status': 404, 'statusText': 'Group not found' };
        throw err;
      }

      return groups[0];
    },
      (err) => {
        throw err;
      });
  }

  getGroupParticipants(groupId) {
    let promise = this.resource(`${__GATEWAY_CLIENT_ENDPOINT__}api/group/mine/:groupId`).
      query({ groupId: groupId }).$promise;

    return promise.then((data) => {
      if (!data || data.length === 0 || !data[0].Participants || data[0].Participants.length === 0) {
        var err = { 'status': 404, 'statusText': 'Group participants not found' };
        throw err;
      }

      let primaryContactId = data[0].contactId;

      let participants = data[0].Participants.map((participant) => {
        let p = new Participant(participant);
        p.primary = p.contactId === primaryContactId;
        return p;
      });

      return participants;
    },
      (err) => {
        throw err;
      });
  }

  removeGroupParticipant(groupId, participant) {
    let promise = this.resource(`${__GATEWAY_CLIENT_ENDPOINT__}api/grouptool/group/:groupId/participant/:groupParticipantId`).
      delete({
        groupId: groupId,
        groupParticipantId: participant.groupParticipantId,
        removalMessage: participant.message
      }).$promise;

    return promise.then((data) => {
      return data;
    }, (err) => {
      throw err;
    });
  }

  endGroup(groupId, groupReasonEndedId) {
    let promise = this.resource(`${__GATEWAY_CLIENT_ENDPOINT__}api/grouptool/:groupId/endsmallgroup`)
      .save({ groupId: groupId, groupReasonEndedId: groupReasonEndedId }, {}).$promise;
    return promise.then((data) => {
      return data;
    }, (err) => {
      throw err;
    });
  }

  approveDenyInquiry(groupId, approve, inquiry) {
    let promise = this.resource(`${__GATEWAY_CLIENT_ENDPOINT__}api/grouptool/grouptype/:groupTypeId/group/:groupId/inquiry/approve/:approve`).
      save({ groupTypeId: CONSTANTS.GROUP.GROUP_TYPE_ID.SMALL_GROUPS, groupId: groupId, approve: approve }, inquiry).$promise;

    return promise.then((data) => {
      return data;
    }, (err) => {
      throw err;
    });
  }

  getInvities(groupId) {
    let promised = this.resource(`${__GATEWAY_CLIENT_ENDPOINT__}api/grouptool/invitations/:sourceId/:invitationTypeId`).
      query({ sourceId: groupId, invitationTypeId: CONSTANTS.INVITATION.TYPES.GROUP }).$promise;

    return promised.then((data) => {
      let invitations = data.map((invitation) => {
        invitation.imageUrl = this.imgService.DefaultProfileImage;
        return new GroupInvitation(invitation);
      });

      return invitations;
    },
      (err) => {
        throw err;
      });
  }

  getInquiries(groupId) {
    let promised = this.resource(`${__GATEWAY_CLIENT_ENDPOINT__}api/grouptool/inquiries/:groupId`).
      query({ groupId: groupId }).$promise;

    return promised.then((data) => {
      let inquiries = data.map((inquiry) => {
        inquiry.imageUrl = `${this.imgService.ProfileImageBaseURL}${inquiry.contactId}`;
        inquiry.defaultProfileImageUrl = this.imgService.DefaultProfileImage;
        return new GroupInquiry(inquiry);
      });

      return inquiries;
    },
      (err) => {
        throw err;
      });
  }

  saveCreateGroupForm(smallGroup) {
    let promise = this.resource(`${__GATEWAY_CLIENT_ENDPOINT__}api/group`)
      .save({}, smallGroup).$promise;
    return promise.then((data) => {
      return data;
    }, (err) => {
      throw err;
    });
  }

  saveEditGroupForm(smallGroup) {
    let promise = this.resource(`${__GATEWAY_CLIENT_ENDPOINT__}api/group/edit`)
      .save({}, smallGroup).$promise;
    return promise.then((data) => {
      this.saveProfile(smallGroup.profile);
    }, (err) => {
      throw err;
    });
  }

  saveParticipant(participants, groupId) {
    let promise = this.resource(`${__GATEWAY_CLIENT_ENDPOINT__}api/group/:groupId/participants`)
      .save({ groupId: groupId }, participants).$promise;

    return promise.then((data) => {
      return data;
    }, (err) => {
      throw err;
    });
  }

  saveProfile(profile) {
    let promise = this.resource(`${__GATEWAY_CLIENT_ENDPOINT__}api/profile`)
      .save({}, profile).$promise;

    return promise.then((data) => {
      return data;
    }, (err) => {
      throw err;
    });
  }

  getGroupByInvitationGUID(invitationGUID) {
    let promise = this.resource(`${__GATEWAY_CLIENT_ENDPOINT__}api/group/invitation/:invitationGUID`).
      get({ invitationGUID: invitationGUID }).$promise;

    return promise.then((data) => {
      let group = new SmallGroup(data);
      group.primaryContact = {
        imageUrl: `${this.imgService.ProfileImageBaseURL}${group.contactId}`,
        contactId: group.contactId
      };

      return group;
    },
      (err) => {
        throw err;
      });
  }

  getIsLeader(groupId) {
    let promise = this.resource(`${__GATEWAY_CLIENT_ENDPOINT__}api/grouptool/:groupId/isleader`).
      get({ groupId: groupId }).$promise;

    return promise.then((data) => {
      return !(data.Group === null || data.Group === undefined)
    },
      (err) => {
        throw err;
      });
  }

  getGroupType(groupId) { 
    const promise = this.resource(`${__GATEWAY_CLIENT_ENDPOINT__}api/group/:groupId/groupType`)
      .get({ groupId }).$promise;

    return promise.then((data) => {
      return data.groupTypeId;
    }, (err) => {
      throw err;
    });
  }

  search(searchString, locationString, groupId) {
    let promise = this.resource(`${__GATEWAY_CLIENT_ENDPOINT__}api/grouptool/group/search`)
      .query({ s: searchString, loc: locationString, groupTypeIds: [CONSTANTS.GROUP.GROUP_TYPE_ID.SMALL_GROUPS, CONSTANTS.GROUP.GROUP_TYPE_ID.ONSITE_GROUPS], id: groupId }).$promise;

    return promise.then((data) => {
      let groups = data.map((group) => {
        return new SmallGroup(group);
      });

      if (!groups || groups.length === 0) {
        var err = { 'status': 404, 'statusText': 'Group not found' };
        throw err;
      }

      return groups;
    },
      (err) => {
        throw err;
      });
  }

  updateParticipant(participant) {
    let promise = this.resource(`${__GATEWAY_CLIENT_ENDPOINT__}api/group/updateParticipantRole`)
      .save({}, participant).$promise;
    return promise.then((data) => {
      return data;
    }, (err) => {
      throw err;
    });
  }

  submitJoinRequest(groupId) {
    return this.resource(`${__GATEWAY_CLIENT_ENDPOINT__}api/grouptool/group/:groupId/submitinquiry`)
      .save({ groupId: groupId }, {}).$promise;
  }

  shareUrl(groupId) {
    return this.location.protocol() + '://' + this.location.host() + '/groups/search/results?id=' + groupId;
  }
}
