
import CONSTANTS from '../../constants';

export default class GroupService {
  /*@ngInject*/
  constructor($resource, $q) {
    this.resource = $resource;
    this.qApi = $q;
  }

  groupLeaderUrl() {
    return this.resource(__GATEWAY_CLIENT_ENDPOINT__ + 'api/v1.0.0/group-leader/url-segment')
      .get().$promise.then((result) => {
        return result.url;
    });
  }

}
