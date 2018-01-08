'use strict';
var MODULE = 'crossroads.media';

require('plangular');

var app = angular.module(MODULE, ['crossroads.core', 'plangular'])
  .config(require('./media.routes'))
  .config(configureAudioPlayer)
  .factory('Media', require('./services/media.service'))
  .factory('YouTubePlayerFactory', require('./factories/youTubePlayer.factory.js'));

require('./templates/viewAll.html');
require('./templates/viewAllMusic.html');
require('./templates/viewAllSeries.html');
require('./templates/viewAllVideos.html');
require('./templates/mediaSingle.html');
require('./templates/subscribeButtonMessages.html');
require('./templates/subscribeButtonMusic.html');
require('./templates/subscribeButtonVideos.html');
require('./templates/subscribeButtonDropdown.html');
require('./templates/subscribeButtonMusicDropdown.html');
require('./templates/subscribeButtonVideoDropdown.html');
require('./templates/messageActionButtons.html');
require('./templates/mediaDetails.html');
require('./templates/mediaListCard.html');
require('./templates/seriesListCard.html');
require('./templates/messagesListCard.html');
require('./templates/rssFeed.html');

app.controller('MediaController', require('./media.controller'));
app.controller('SingleMediaController', require('./singleMedia.controller'));
app.filter('replaceNonAlphaNumeric', require('./filters/replaceNonAlphaNumeric.filter.js'));

app.directive('mediaListCard', require('./directives/mediaListCard.directive'));
app.directive('youTubePlayer', require('./directives/youTubePlayer.directive'));
app.directive('rssFeed', require('./directives/rssFeed.directive.js'));

app.constant('YT_EVENT', {
  STOP:            0,
  PLAY:            1,
  PAUSE:           2,
  STATUS_CHANGE:   3
});

function configureAudioPlayer(plangularConfigProvider) {
  plangularConfigProvider.clientId = __SOUNDCLOUD_API_KEY__;
}
