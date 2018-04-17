import CONSTANTS from '../constants';

export default class ServeTeamService {
    /*@ngInject*/
    constructor($log, $resource, $q) {
        this.log = $log;
        this.resource = $resource;
        this.qApi = $q;

        //isleader gets set by my_serve.routes
    }

    getAllTeamMembersForLoggedInLeader(groupId) {
        return this.resource(`${__GATEWAY_CLIENT_ENDPOINT__}api/serve/GetLoggedInLeadersGroupsParticipants`).query({groupId: groupId}).$promise;
    }



    getIsLeader(groupId) {
        return this.resource(`${__GATEWAY_CLIENT_ENDPOINT__}api/serve/GetIsLeader`).get({groupId: groupId}).$promise;
    }

    getTeamDetailsByLeader() {
        return this.resource(`${__GATEWAY_CLIENT_ENDPOINT__}api/serve/GetLoggedInLeadersGroups`).query().$promise;
    }

//TODO: THIS METHOD IS BASICALLY THE SAME METHOD IN GROUP TOOL SERVICES/MESSAGE SERVICE.  THAT SERVICE SHOULD BE REFACTORED
//AND PULLED UP TO A HIGHER LEVEL TO BE USED MORE BROADLY
    sendGroupMessage(groupId, message) {
        return this.resource(`${__GATEWAY_CLIENT_ENDPOINT__}api/grouptool/:groupId/:groupTypeId/groupmessage`).save({
            groupId: groupId,
            groupTypeId: CONSTANTS.GROUP.GROUP_TYPE_ID.MY_SERVE
        }, message).$promise;
    }

    sendParticipantsMessage(message) {
        return this.resource(`${__GATEWAY_CLIENT_ENDPOINT__}api/group/messageselectparticipants`).save(message).$promise;
    }

    getTeamRsvps(team) {
        return this.resource(`${__GATEWAY_CLIENT_ENDPOINT__}api/serve/getTeamRsvps`)
        .save(team).$promise;
    }

    getCapacity(opp, eventId) {
          return this.resource(`${__GATEWAY_CLIENT_ENDPOINT__}api/serve/opp-capacity`).get({
            id: (opp.Opportunity_ID != null) ? opp.Opportunity_ID : opp.opportunityId,
            eventId: eventId,
            min: opp.minimum,
            max: opp.maximum
          });
      }
}