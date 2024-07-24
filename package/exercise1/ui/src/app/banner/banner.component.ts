import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-banner',
  standalone: true,
  imports: [RouterModule],
  templateUrl: './banner.component.html',
  styleUrl: './banner.component.css'
})
export class BannerComponent {
  nasaNameLine1: string = "National Aeronautics and";
  nasaNameLine2: string = "Space Administration";
  appName: string = "Astronaut Career Tracking System (ACTS)";
  codeName: string = "Stargate";
}
