import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { AstronautDetailsComponent } from './astronaut-details/astronaut-details.component';

export const routes: Routes = [
    {
        path: '',
        component: HomeComponent,
        pathMatch: 'full',
    },
    {
        path: 'details/:name',
        component: AstronautDetailsComponent
    },
// { path: '**', component: PageNotFoundComponent }
];
