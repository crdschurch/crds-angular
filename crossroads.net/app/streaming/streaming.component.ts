import { Component } from '@angular/core';
import { ScheduleComponent } from './schedule.component';

var WOW = require('wow.js/dist/wow.min.js');

@Component({
  selector: 'streaming',
  directives: [ScheduleComponent],
  templateUrl: './streaming.ng2component.html'
})

export class StreamingComponent {
  displayStreamCTA: boolean = false;
  constructor() {
    new WOW({
      offset: 100,
      mobile: false
    }).init();
  }
}
