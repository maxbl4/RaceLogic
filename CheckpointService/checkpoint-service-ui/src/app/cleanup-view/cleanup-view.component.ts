import { Component, OnInit } from '@angular/core';
import {HttpClient} from "@angular/common/http";

@Component({
  selector: 'app-cleanup-view',
  template: `
      <div class="row">
          <div class="col-md mb-2">
              <div class="card h-100">
                  <div class="card-body">
                      <h6 class="card-title">Remove Tags</h6>                      
                      <button mat-raised-button (click)="removeTags()">Remove</button>
                  </div>
              </div>
          </div>
          <div class="col-md mb-2">
              <div class="card h-100">
                  <div class="card-body">
                      <h6 class="card-title">Remove Checkpoints</h6>
                      <button mat-raised-button (click)="removeCheckpoints()">Remove</button>
                  </div>
              </div>
          </div>
          <div class="col-md mb-2">
              <div class="card h-100">
                  <div class="card-body">
                      <h6 class="card-title">Remove Logs</h6>
                      <button mat-raised-button (click)="removeLogs()">Remove</button>
                  </div>
              </div>
          </div>
          <div class="col-md mb-2">
              <div class="card h-100">
                  <div class="card-body">
                      <h6 class="card-title">Remove All</h6>
                      <button mat-raised-button (click)="removeAll()">Remove</button>
                  </div>
              </div>
          </div>
      </div>
  `,
  host: {'class': 'flex-container'},
  styles: []
})
export class CleanupViewComponent implements OnInit {

  constructor(private http: HttpClient) { }

  ngOnInit() {
  }

  removeTags(ask = true) {
    if (!ask || confirm('Remove tags?')) {
      this.http.delete('tag').subscribe();
    }
  }

  removeCheckpoints(ask = true) {
    if (!ask || confirm('Remove checkpoints?')) {
      this.http.delete('cp').subscribe();
      location.reload();
    }
  }

  removeLogs(ask = true) {
    if (!ask || confirm('Remove logs?')) {
      this.http.delete('log').subscribe();
      this.http.delete('log?errors=true').subscribe();
    }
  }

  removeAll() {
    if (confirm('Remove all data?')) {
      this.removeTags(false);
      this.removeLogs(false);
      this.removeCheckpoints(false);
    }
  }
}
