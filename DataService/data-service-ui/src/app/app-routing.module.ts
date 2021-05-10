import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import {MainViewComponent} from "./main-view/main-view.component";

const routes: Routes = [
  {path: '', pathMatch: 'prefix', redirectTo: 'main'},
  {path: 'main', component: MainViewComponent},
];

@NgModule({
  imports: [RouterModule.forRoot(routes, { useHash: true, relativeLinkResolution: 'legacy' })],
  exports: [RouterModule]
})
export class AppRoutingModule { }
