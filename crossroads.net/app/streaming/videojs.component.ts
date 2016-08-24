import { Component, AfterViewInit, OnDestroy, Inject } from '@angular/core';
import { StreamspotService } from './streamspot.service';

declare var window: any;
declare var chrome: any;

window.videojs = require('video.js/dist/video');

require('./vendor/streamspotAnalytics');
require('./vendor/videojs5-hlsjs-source-handler.min');
require('videojs-chromecast/dist/videojs-chromecast');

@Component({
  selector: 'videojs',
  templateUrl: './videojs.ng2component.html'
})

export class VideoJSComponent implements AfterViewInit, OnDestroy {

  id: string = "videojs-player";
  player: any;
  visible: boolean = false;
  debug: boolean = false;
  angulartics: any;

  constructor(
    private streamspot: StreamspotService, 
    @Inject('$analytics') angularticsService) {
    this.angulartics = angularticsService;
  }

  ngOnDestroy() {
    this.player.dispose();
  }

  ngAfterViewInit() {

    this.streamspot.getBroadcaster().subscribe( response => {

      if ( response.success === true && response.data.broadcaster !== undefined ) {

        var broadcaster = response.data.broadcaster;

        if ( broadcaster.players === undefined || broadcaster.players.length === 0 ) {
          console.log('Error getting player from broadcast.');
          return;
        }

        var defaultPlayer;
        broadcaster.players.forEach(element => {
          if ( element.default === true ) {
            defaultPlayer = element;
            return false;
          }
        });

        if ( defaultPlayer === undefined ) {
          defaultPlayer = broadcaster.players[0];
        }

        this.player = window.videojs(this.id, {
          "techOrder": ["html5"],
          "fluid": true,
          "poster" : defaultPlayer.bgLink,
          "preload": 'auto',
          "controls": true,
          "html5": {
            "hlsjsConfig": {
              "debug": false
            }
          }
        });

        // create play handler (analytics)
        this.player.on('play', () => {
          window.SSTracker = window.SSTracker ? window.SSTracker : new window.Tracker(this.streamspot.ssid);
          window.SSTracker.start(broadcaster.live_src.cdn_hls, true, this.streamspot.ssid);
          if ( this.angulartics !== undefined ) {
            this.angulartics.eventTrack('Play', {
              category: 'Streaming',
              label: 'Live Streaming Play'
            });
            console.log('Video played.');
          }
        });

        // create stop handler (analytics)
        this.player.on('pause', () => {
          if(window.SSTracker){
            window.SSTracker.stop();
            window.SSTracker = null;
          }
          if ( this.angulartics !== undefined ) {
            this.angulartics.eventTrack('Pause', {
              category: 'Streaming',
              label: 'Live Streaming Pause'
            });
            console.log('Video paused.');
          }
        });
            
        if ( broadcaster.isBroadcasting === true || this.debug ) {
          this.playerInit(broadcaster);
        }
        else {
          console.log('No broadcast available.');
          window.location.href = '/live';
        }

      }
      else {
        console.log('StreamSpot API Failure!');
      }

    });

  }

  playerInit(broadcaster) {
    let src = this.debug ? "http://vjs.zencdn.net/v/oceans.mp4" : broadcaster.live_src.cdn_hls;
    let type = this.debug ? "video/mp4" : "application/x-mpegURL";

    this.player.src([
      {
        "type": type,
        "src": src
      }
    ]);
    this.visible = true;
    this.player.ready(() => this.player.play());
  }

}
