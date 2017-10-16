import CONSTANTS from '../../constants';

export default class MessageService {
    /*@ngInject*/
    constructor($log, $resource, $q) {
        this.log = $log;
        this.resource = $resource;
        this.deferred = $q;
    }

    sendGroupMessage(groupId, message) {
        return this.resource(__GATEWAY_CLIENT_ENDPOINT__ + 'api/grouptool/:groupId/:groupTypeId/groupmessage').save({
            groupId: groupId,
            groupTypeId: CONSTANTS.GROUP.GROUP_TYPE_ID.SMALL_GROUPS
        }, message).$promise;
    }

    sendLeaderMessage(message) {
        return this.resource(__GATEWAY_CLIENT_ENDPOINT__ + 'api/grouptool/:groupId/leadermessage').
        save({
            groupId: message.groupId
        }, message).$promise;
    }
}