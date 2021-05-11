import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import {MainViewComponent} from "./main-view/main-view.component";
import {EventViewComponent} from "./event-view/event-view.component";
import {SessionViewComponent} from "./session-view/session-view.component";
import {TimingSessionViewComponent} from "./timing-session-view/timing-session-view.component";

const routes: Routes = [
  {path: '', pathMatch: 'prefix', redirectTo: 'main'},
  {path: 'main', component: MainViewComponent},
  {path: 'event/:eventId/session/:sessionId/timing/:timingSessionId', component: TimingSessionViewComponent},
  {path: 'event/:eventId/session/:sessionId', component: SessionViewComponent},
  {path: 'event/:eventId', component: EventViewComponent},
];

@NgModule({
  imports: [RouterModule.forRoot(routes, { useHash: true, relativeLinkResolution: 'legacy' })],
  exports: [RouterModule]
})
export class AppRoutingModule { }
