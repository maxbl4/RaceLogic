import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import {OptionsViewComponent} from "./options-view/options-view.component";
import {MonitorViewComponent} from "./monitor-view/monitor-view.component";
import {LogsViewComponent} from "./logs-view/logs-view.component";
import {LowRpsViewComponent} from "./low-rps-view/low-rps-view.component";
import {DashboardViewComponent} from "./dashboard-view/dashboard-view.component";
import {TagViewComponent} from "./tag-view/tag-view.component";
import {CleanupViewComponent} from "./cleanup-view/cleanup-view.component";


const routes: Routes = [
  {path: '', pathMatch: 'prefix', redirectTo: 'dash'},
  {path: 'dash', component: DashboardViewComponent},
  {path: 'low-rps', component: LowRpsViewComponent},
  {path: 'options', component: OptionsViewComponent},
  {path: 'monitor', component: MonitorViewComponent},
  {path: 'tags', component: TagViewComponent},
  {path: 'logs', component: LogsViewComponent},
  {path: 'cleanup', component: CleanupViewComponent},
];

@NgModule({
  imports: [RouterModule.forRoot(routes, {useHash: true})],
  exports: [RouterModule]
})
export class AppRoutingModule { }
