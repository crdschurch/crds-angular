require('../../app/routes.content');

(() => {
  'use strict';
  describe('RoutesContent', () => {
    it('#removeTrailingSlashIfNecessary(link) should remove slash if necessary', () => {
      let link = "https://www.crossroads.net/care/";
      let result = removeTrailingSlashIfNecessary(link);

      expect(result).toEqual("https://www.crossroads.net/foobar");
    }); 

    it('#getPersonalizedContentPath(link) should return the link with "/personalized" prepended to it', () => {
      let link = "/foobar/";
      let result = getPersonalizedContentPath(link);

      expect(result).toEqual("/personalized/foobar/");
    });
  });
});

