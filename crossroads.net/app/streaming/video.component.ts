// angular imports
import { Component, EventEmitter, Input, Output, AfterViewInit } from '@angular/core';

// streaming imports
import { StreamspotIframeComponent } from './streamspot-iframe.component';
import { StreamspotService } from './streamspot.service';
import { ContentCardComponent } from './content-card.component'
import { VideoJSComponent } from './videojs.component';
import { LinkedContentNg2Component } from '../../core/linked_content/linked-content-ng2.component';

// core imports
import { CMSDataService } from '../../core/services/CMSData.service'

// pipes
import { TruncatePipe } from '../../core/pipes/truncate.pipe';

var WOW = require('wow.js/dist/wow.min.js');

@Component({
  selector: 'live-stream',
  templateUrl: './video.ng2component.html',
  providers: [CMSDataService],
  directives: [StreamspotIframeComponent, ContentCardComponent, VideoJSComponent, LinkedContentNg2Component],
  pipes: [TruncatePipe]
})

export class VideoComponent {
  @Input() inModal: boolean = false;
  @Output('close') _close = new EventEmitter();

  inProgress:       boolean    = false;
  number_of_people: number     = 2;
  displayCounter:   boolean    = true;
  countSubmit:      boolean    = false;
  dontMiss:         Array<any> = [];
  beTheChurch:      Array<any> = [];
  redirectText:     string     = 'Go Back';

  closeModal:       EventEmitter<any> = new EventEmitter();

  constructor(private cmsDataService: CMSDataService,
              private streamspotService: StreamspotService) {

    this.streamspotService.isBroadcasting.subscribe((inProgress: boolean) => {
      this.inProgress = inProgress;
      this.redirect();
    });
    
    this.cmsDataService
        .getDigitalProgram()
        .subscribe((data) => {
          data.forEach((feature, i, data) => {
            // null status indicates a published feature
            if (feature.status === null || feature.status.toLowerCase() !== 'draft') {
              feature.delay = i * 100
              feature.url = 'javascript:;';

              if (feature.link !== null) {
                feature.url = feature.link;
              }

              feature.target = '_blank';

              if (typeof feature.image !== 'undefined' && typeof feature.image.filename !== 'undefined') {
                feature.image = feature.image.filename;
              } else {
                feature.image = 'https://crds-cms-uploads.imgix.net/content/images/register-bg.jpg'
              }
              if (feature.section == 1 ) {
                this.dontMiss.push(feature)
              } else if (feature.section == 2 ) {
                this.beTheChurch.push(feature);
              }
            }
          })
        });
    
    new WOW({
      mobile: false
    }).init();
  }

  ngAfterViewInit() {
    // Trigger a window.resize() event so imgix will
    // reevaluate background images in modal context
    setTimeout(function() {
      var event;
      if ("createEvent" in document) {
        // initUIEvent() is deprecated but IE11 doesn't support UIEvent()
        event = document.createEvent('UIEvents');
        event.initUIEvent('resize', true, false, window, 0);
      } else {
        event = new UIEvent('resize');
      }
      window.dispatchEvent(event);
    }, 1000);

  }

  redirect() {
    if (this.inProgress === false) {
      if (this.inModal) {
        this._close.emit({});
      } else {
        window.location.href = '/live';
      }
    }
  }

  increaseCount() {
    this.number_of_people++;
  }

  decreaseCount() {
    if(this.number_of_people > 1) {
      this.number_of_people--;
    }
  }

  submitCount() {
    this.countSubmit = true;
  }

  goBack() {
    if (!this.inModal) {
      window.location.href = '/live';
    } else {
      this._close.emit({});
    }
  }
}
