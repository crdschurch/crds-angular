import controller from './goVolunteerProjectCard.controller';
import './goVolunteerProjectCard.html';

export default function goVolunteerProjectCardComponent() {
  let component = {
    templateUrl: 'projectCard/goVolunteerProjectCard.html',
    controller,
    controllerAs: 'card',
    bindToController: true
  };

  return component;
}
