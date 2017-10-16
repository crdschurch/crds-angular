(function() {
  'use strict';

  var constants = require('../../../constants.js');

  angular.module(constants.MODULES.COMMON)
    .directive('profilePicture', require('./profilePicture.directive'))
    .controller('ProfilePictureController', require('./profilePicture.controller'))
    .controller('ChangeProfileImageController', require('./changeProfileImage.controller'))
    ;

  require('./profilePicture.html');
  require('./profileImageUpload.html');
  require('script!./../../../../lib/load-image.all.min.js');

})();
