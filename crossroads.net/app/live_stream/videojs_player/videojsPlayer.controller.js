window.videojs = require('video.js/dist/video');

// require('./vendor/streamspotAnalytics');
require('../vendor/videojs5-hlsjs-source-handler.min');
require('videojs-chromecast/dist/videojs-chromecast');

export default class VideojsPlayerController {

  constructor(StreamspotService, $rootScope, AnalyticsService) {
    this.streamspotService = StreamspotService;
    this.rootScope = $rootScope;
    this.analyticsService = AnalyticsService;

    this.id = 'videojs-player';
    this.visibile = false;
    this.debug = false;
    this.player = null;
  }

  $onDestroy() {
    this.player.dispose();
  }

  $onInit() {
    this.streamspotService.getBroadcaster().then((response) => {
      if (response.success === true && response.data.broadcaster !== undefined) {
        const broadcaster = response.data.broadcaster;

        if (broadcaster.players === undefined || broadcaster.players.length === 0) {
          return false;
        }

        let defaultPlayer;
        broadcaster.players.forEach((element) => {
          if (element.default === true) {
            defaultPlayer = element;
          }
          return false;
        });

        if (defaultPlayer === undefined) {
          defaultPlayer = broadcaster.players[0];
        }

        this.player = window.videojs(this.id, {
          techOrder: ['html5'],
          fluid: true,
          poster: defaultPlayer.bgLink,
          controls: true,
          html5: {
            hlsjsConfig: {
              debug: false,
              abrBandWidthFactor: 0.7,
              abrBandWidthUpFactor: 0.6
            }
          }
        });

        // create play handler (analytics)
        this.player.on('play', () => {
          if (window.Tracker !== undefined) {
            window.SSTracker = window.SSTracker ? window.SSTracker : new window.Tracker(this.streamspot.ssid);
            window.SSTracker.start(broadcaster.live_src.cdn_hls, true, this.streamspot.ssid);
          }
          if (this.angulartics !== undefined) {
            this.analyticsService.trackStream('Play');
          }
        });

        // create stop handler (analytics)
        this.player.on('pause', () => {
          if (window.SSTracker) {
            window.SSTracker.stop();
            window.SSTracker = null;
          }
          if (this.angulartics !== undefined) {
            this.analyticsService.trackStream('Pause');
          }
        });

        if (broadcaster.isBroadcasting === true || this.debug) {
          this.playerInit(broadcaster);
        }
      }
    });
  }

  playerInit(broadcaster) {
    const videoType = this.debug ? 'video/mp4' : 'application/x-mpegURL';
    const videoSrc = this.debug ? 'http://vjs.zencdn.net/v/oceans.mp4' : broadcaster.live_src.cdn_hls;

    this.player.src([{
      type: videoType,
      src: videoSrc
    }]);
    this.player.qualityPickerPlugin();
    this.visible = true;
    this.player.ready(() => {
      this.player.play();
    });
  }
}