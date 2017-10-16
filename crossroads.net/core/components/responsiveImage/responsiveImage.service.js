(function() {
  'use strict';

  module.exports = ResponsiveImageService;

  ResponsiveImageService.$inject = ['$timeout'];

  function ResponsiveImageService($timeout) {
    return {
      updateResponsiveImages: function() {
        $timeout(replaceImages);
      }
    }

    function replaceImages() {
      updateImages('imgix-fluid');
      updateImages('imgix-fluid-bg');
    }

    function updateImages(className) {
      var fluidSet = imgix.fluid({
        fluidClass: className,
        updateOnResizeDown: true,
        updateOnPinchZoom: true,
        pixelStep: 10,
      });

      // This makes sure imgix is called on images that may have
      // been hidden when imgix.fluid was first invoked.
      var images = $('.' + className);
      _.forEach(images, function(i) {
        var image = angular.element(i);
        if(!image.is(':visible') && !image[0].imgix_watched) {
          var watcher = image.scope().$watch(
            function() {
              return(image.is(':visible'));
            },
            function() {
              if(image.is(':visible')) {
                $timeout(function() {
                  fluidSet.updateSrc(image[0]);
                  // Remove the watch
                  watcher();
                });
              }
            }
          );
          image[0].imgix_watched = true;
        }
      });
    }
  }
})();
