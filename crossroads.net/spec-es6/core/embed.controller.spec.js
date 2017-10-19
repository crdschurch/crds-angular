import constants from 'crds-constants';
import EmbedController from '../../core/embed/embed.controller';

describe('Embed Controller', () => {
  let fixture, element,
      sce,
      attrs;

  beforeEach(angular.mock.module(constants.MODULES.GROUP_TOOL));

  describe('just href no map', () => {
    beforeEach(inject(function ($injector) {
      element = angular.element('<crds-embed></crds-embed>');
      sce = $injector.get('$sce');
      attrs = {
        href: '/give/?type=something&stuff=123'
      };
      fixture = new EmbedController(element, sce, attrs);
      fixture.$onInit();
    }));

    it('should return URL for inline giving', () => {
      const url = fixture.buildUrl().$$unwrapTrustedValue();
      expect(url).toBe('https://embed.crossroads.net/give/?type=something&stuff=123');
    });
  });
})
