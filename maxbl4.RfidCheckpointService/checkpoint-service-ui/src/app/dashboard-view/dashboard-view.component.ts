import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-dashboard-view',
  template: `
    <p>
      dashboard-view works!
    </p>
  `,
  host: {'class': 'flex-container'},
  styles: []
})
export class DashboardViewComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}
