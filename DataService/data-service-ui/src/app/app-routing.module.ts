import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import {MainViewComponent} from "@app/main-view/main-view.component";
import {TimingSessionViewComponent} from "@app/timing-session-view/timing-session-view.component";
import {SessionViewComponent} from "@app/session-view/session-view.component";
import {EventViewComponent} from "@app/event-view/event-view.component";
import {OptionsViewComponent} from "@app/options-view/options-view.component";
import {
  ActiveTimingSessionsViewComponent
} from "@app/active-timing-sessions-view/active-timing-sessions-view.component";

const routes: Routes = [
  {path: '', pathMatch: 'prefix', redirectTo: 'main'},
  {path: 'main', component: MainViewComponent},
  {path: 'event/:eventId/session/:sessionId/timing/:timingSessionId', component: TimingSessionViewComponent},
  {path: 'event/:eventId/session/:sessionId', component: SessionViewComponent},
  {path: 'event/:eventId', component: EventViewComponent},
  {path: 'active-timings', component: ActiveTimingSessionsViewComponent},
  {path: 'options', component: OptionsViewComponent},
];

@NgModule({
  imports: [RouterModule.forRoot(routes, { useHash: true})],
  exports: [RouterModule]
})
export class AppRoutingModule { }
