import TravelInformationComponent from './travelInformation.component';
import TravelInformationService from './travelInformation.service';

const MODULE = 'crossroads.trips';
const app = angular.module(MODULE);

app.component('travelInformation', TravelInformationComponent);
app.service('TravelInformationService', TravelInformationService);
