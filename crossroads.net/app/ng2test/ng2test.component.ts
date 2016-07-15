import { Component } from '@angular/core';
declare var moment: any;

@Component({
  selector: 'ng2-test',
  template: `
    <h2>Welcome from our first angular2 component</h2>
    <h2>{{today}}</h2>
  `
})
export class Ng2TestComponent {
  today: string = moment().format('D MMM YYYY');
}

