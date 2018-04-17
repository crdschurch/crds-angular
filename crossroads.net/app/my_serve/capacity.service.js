OpportunityCapacityService.$inject = ['$resource'];

export default function OpportunityCapacityService($resource) {
  return $resource(__GATEWAY_CLIENT_ENDPOINT__ + 'api/serve/opp-capacity');
}
