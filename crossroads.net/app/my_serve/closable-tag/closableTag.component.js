import controller from './closableTag.controller';

export default function closableTagComponent() {
  return {
    bindings: {
      name: '<',
      age: '<',
      onClick: '&',
      onClose: '&',
      isLeader: '=',
      opportunityId: '<'
    },
    templateUrl: 'closable-tag/closableTag.html',
    controller: controller,
    controllerAs: 'closableTag'
  }
}