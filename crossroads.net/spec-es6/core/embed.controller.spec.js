import constants from 'crds-constants';
import EmbedController from '../../core/embed/embed.controller';

describe('Embed Controller', () => {
  let fixture, element,
      sce,
      attrs;

  describe('just href no map', () => {
    beforeEach(inject(function ($injector) {
      element = angular.element('<crds-embed></crds-embed>');
      sce = $injector.get('$sce');
      attrs = {
        href: '/?type=something&stuff=123'
      };
      fixture = new EmbedController(element, sce, attrs);
    }));

    it('should return URL for inline giving', () => {
      const url = fixture.buildUrl().$$unwrapTrustedValue();
      expect(url).toBe('https://embed.crossroads.net/?type=something&stuff=123');
    });
  });

  describe('just map no href', () => {
    beforeEach(inject(function ($injector) {
      element = angular.element('<crds-embed></crds-embed>');
      sce = $injector.get('$sce');
      attrs = {
        map: 'true'
      };
      fixture = new EmbedController(element, sce, attrs);
    }));

    it('should return URL for add me to the map', () => {
      const url = fixture.buildUrl().$$unwrapTrustedValue();
      expect(url).toBe('https://embed.crossroads.net/add-me-to-the-map?type=donation');
    });
  });

})
