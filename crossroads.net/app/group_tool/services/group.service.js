
import CONSTANTS from '../../constants';

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
    return this.resource(__GATEWAY_CLIENT_ENDPOINT__ + 'api/v1.0.0/group-leader/url-segment')
      .get().$promise.then((result) => {
        return result.url;
    });
  }

}
