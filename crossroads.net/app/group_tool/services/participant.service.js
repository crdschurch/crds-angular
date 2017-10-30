
export default class ParticipantService {
    /*@ngInject*/
    constructor($log, $resource, $q, AuthService) {
      this.log = $log; 
      this.resource = $resource;
      this.deferred = $q;
      this.auth = AuthService;
    }
  
    get() {
      if(this.auth.isAuthenticated()) {
        return this.resource(__GATEWAY_CLIENT_ENDPOINT__ + 'api/participant').get().$promise; 
      } else {
        this.log.info('Unauthenticated, no participant');
        var promised = this.deferred.defer();
        promised.resolve({'ApprovedSmallGroupLeader': false});
        return promised.promise;
      }
    }
  }