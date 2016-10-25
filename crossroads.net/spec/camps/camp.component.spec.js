import constants from '../../app/constants';

// eslint-disable-next-line no-unused-vars
import campsModule from '../../app/camps/camps.module';
import campHelpers from './campHelpers';

describe('Camp Component', () => {
  let $componentController;
  let campController;
  let campsService;
  let rootScope;

  beforeEach(angular.mock.module(constants.MODULES.CAMPS));

  beforeEach(inject((_$componentController_, _$httpBackend_, _CampsService_, _$rootScope_) => {
    $componentController = _$componentController_;
    rootScope = _$rootScope_;

    rootScope.MESSAGES = {
      summercampIntro: {
        content: 'summer camp intro text'
      }
    };

    campsService = _CampsService_;
    const bindings = {
    };
    campsService.campInfo = campHelpers().campInfo;
    campController = $componentController('crossroadsCamp', null, bindings);
    campController.$onInit();
  }));

  it('should set the view as ready', () => {
    expect(campController.viewReady).toBe(true);
  });

  it('should set the title correctly', () => {
    expect(campController.campsService.campTitle).toBe(campHelpers().campInfo.eventTitle);
  });
});
