
import constants from 'crds-constants';
import LinkedContentController from '../../core/linked_content/linkedContent.controller';

describe('Linked Content Controller', () => {
  let fixture,
      element,
      sce;

  // beforeEach(angular.mock.module(constants.MODULES.GROUP_TOOL));

  beforeEach(inject(function ($injector) {
    element = angular.element('<div></div>');
    sce     = $injector.get('$sce');

    fixture = new LinkedContentController(element, sce);
  }));

  it('should evaluate href value', () => {
    expect(fixture.isLinked()).not.toBeTruthy();

    fixture.href = 'http://crossroads.net';
    expect(fixture.isLinked()).toBeTruthy();

    fixture.href = '/live/stream';
    expect(fixture.isLinked()).toBeTruthy();

    fixture.href = '#';
    expect(fixture.isLinked()).not.toBeTruthy();

    fixture.href = 'javascript:;';
    expect(fixture.isLinked()).not.toBeTruthy();

    fixture.href = '';
    expect(fixture.isLinked()).not.toBeTruthy();

    fixture.href = undefined;
    expect(fixture.isLinked()).not.toBeTruthy();
  });

})
