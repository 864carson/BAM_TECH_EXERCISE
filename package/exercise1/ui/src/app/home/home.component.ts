import { Component, inject, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { AstronautResultsComponent } from "../astronaut-results/astronaut-results.component";
import { Astronaut } from '../models/astronaut.model';
import { AstronautService } from '../services/astronaut-service/astronaut.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterOutlet, AstronautResultsComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent {
  @Input() newAstronaut: Astronaut = new Astronaut();
  reloadTable: boolean = false;
  astronautService: AstronautService = inject(AstronautService);

  newAstronautFormVisible: boolean = false;

  constructor() { }

  addNewAstronaut(): void {
    console.log(this.newAstronaut.name);
    this.astronautService.createAstronaut(this.newAstronaut)
      .subscribe({
        next: (res) => {
          console.log(res);
          this.reloadTable = true;
          this.hideNewAstronautForm();
        },
        error: (e) => console.error(e)
      });
  }

  showNewAstronautForm(): void {
    this.clearAstronautForm();
    this.reloadTable = false;
    this.newAstronautFormVisible = true;
  }
  hideNewAstronautForm(): void {
    this.newAstronautFormVisible = false;
  }

  clearAstronautForm(): void {
    this.newAstronaut = new Astronaut();
  }
}
