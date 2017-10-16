

/* @ngInject */
export default class WaiversService {
  constructor($resource) {
    this.waiverResource = $resource(`${__GATEWAY_CLIENT_ENDPOINT__}api/v1.0.0/waivers/:waiverId`);
    this.sendInviteResource = $resource(`${__GATEWAY_CLIENT_ENDPOINT__}api/v1.0.0/waivers/:waiverId/send/:eventParticipantId`);
    this.acceptResource = $resource(`${__GATEWAY_CLIENT_ENDPOINT__}api/v1.0.0/waivers/accept/:guid`);
  }

  getWaiver(waiverId) {
    return this.waiverResource.get({ waiverId }).$promise;
  }

  sendAcceptEmail(waiverId, eventParticipantId) {
    return this.sendInviteResource.save({ waiverId, eventParticipantId }, {}).$promise;
  }

  acceptWaiver(guid) {
    return this.acceptResource.save({ guid }, {}).$promise;
  }
}
