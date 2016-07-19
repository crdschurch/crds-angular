import controller from './groupDetail.participant.card.controller';

GroupDetailParticipantCardComponent.$inject = [];

export default function GroupDetailParticipantCardComponent() {

  let groupDetailParticipantCardComponent = {
    bindings: {
      participant: '<',
      edit: '<',
      deleteAction: '&'
    },
    restrict: 'E',
    templateUrl: 'participant_card/groupDetail.participant.card.html',
    controller: controller,
    controllerAs: 'groupDetailParticipantCard',
    bindToController: true
  };

  return groupDetailParticipantCardComponent;

}