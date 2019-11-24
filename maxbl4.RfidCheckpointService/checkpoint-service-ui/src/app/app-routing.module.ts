import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import {OptionsViewComponent} from "./options-view/options-view.component";
import {MonitorViewComponent} from "./monitor-view/monitor-view.component";
import {LogsViewComponent} from "./logs-view/logs-view.component";
import {DashboardViewComponent} from "./dashboard-view/dashboard-view.component";


const routes: Routes = [
  {path: '', pathMatch: 'prefix', redirectTo: 'dashboard'},
  {path: 'dashboard', component: DashboardViewComponent},
  {path: 'options', component: OptionsViewComponent},
  {path: 'monitor', component: MonitorViewComponent},
  {path: 'logs', component: LogsViewComponent},
];

@NgModule({
  imports: [RouterModule.forRoot(routes, {useHash: true})],
  exports: [RouterModule]
})
export class AppRoutingModule { }
