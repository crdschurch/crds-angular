import campsModule from '../../../app/camps/camps.module';
import campHelpers from '../campHelpers';

describe('Camps Family Select Tool', () => {
  let $componentController;
  let familySelectController;
  let campsService;
  let log;
  let rootScope;
  let state;

  beforeEach(angular.mock.module(campsModule));

  beforeEach(inject((_$componentController_, _CampsService_, _$log_, _$rootScope_, _$state_) => {
    $componentController = _$componentController_;
    campsService = _CampsService_;
    campsService.family = campHelpers.family; // TODO: create the family helper
    campsService.campTitle = 'Summer Camp';
    log = _$log_;
    rootScope = _$rootScope_;
    state = _$state_;

    state.toParams = {
      campId: 123
    };

    spyOn(log, 'debug').and.callThrough();

    rootScope.MESSAGES = {
      campIntro_123: { content: 'success' },
      summercampIntro: {
        content: 'summer camp intro text'
      }
    };

    const bindings = { };
    familySelectController = $componentController('campsFamily', null, bindings);
  }));

  it('should initialize the component', () => {
    familySelectController.$onInit();
    expect(log.debug).toHaveBeenCalled();
  });
});
