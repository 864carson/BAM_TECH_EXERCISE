import { Component, inject, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule, DatePipe } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { AstronautResultsComponent } from "../astronaut-results/astronaut-results.component";
import { Astronaut } from '../models/astronaut.model';
import { AstronautService } from '../services/astronaut-service/astronaut.service';
import { AstronautDutyDto } from '../models/astronaut-duty-dto.model';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterOutlet, AstronautResultsComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent {
  astronautService: AstronautService = inject(AstronautService);
  @Input() newAstronaut: Astronaut = new Astronaut();
  @Input() newAstronautDuty: AstronautDutyDto = new AstronautDutyDto();
  datePipe: DatePipe = new DatePipe('en-US');

  _isChildBusy: boolean = false;
  reloadTable: boolean = false;
  isAddingNewAstronaut: boolean = false;
  isInAddingStep1: boolean = false;
  isInAddingStep2: boolean = false;
  skipStep2: boolean = false;

  constructor() { }

  isChildBusy(busy: boolean): void {
    this._isChildBusy = busy;
    if (!busy) {
      this.reloadTable = false;
    }
  }

  addNewAstronautThenDuty(): void {
    console.log("Add and continue");
    this.astronautService.createAstronaut(this.newAstronaut)
      .subscribe({
        next: (res) => {
          console.log(res);

          if (this.skipStep2) {
            this.hideNewAstronautForm();
            this.reloadTable = true;
          } else {
            this.newAstronautDuty.name = this.newAstronaut.name;
            this.newAstronautDuty.dutyStartDate = this.datePipe.transform(new Date(), "MM/dd/yyyy")!;
            this.isInAddingStep1 = false;
            this.isInAddingStep2 = true;
          }
        },
        error: (e) => console.error(e)
      });
  }

  addNewAstronaut(): void {
    console.log("Add and stop");
    this.skipStep2 = true;
    this.addNewAstronautThenDuty();
  }

  addAstronautDuty(): void {
    try {
      const newDutyDate: string = new Date(this.newAstronautDuty.dutyStartDate!).toISOString();
      this.newAstronautDuty.dutyStartDate = newDutyDate;
    }
    catch (ex) {
      console.error(ex);
      return;
    }

    this.astronautService.createAstronautDutyRecord(this.newAstronautDuty)
      .subscribe({
        next: (res) => {
          console.log(res);
          this.hideNewAstronautForm();
          this.reloadTable = true;
        },
        error: (e) => console.error(e)
      });
  }

  showNewAstronautForm(): void {
    this.clearAstronautForm();
    this.reloadTable = false;
    this.isAddingNewAstronaut = true;
    this.isInAddingStep1 = true;
    this.isInAddingStep2 = false;
  }
  hideNewAstronautForm(): void {
    this.isAddingNewAstronaut = false;
    this.isInAddingStep1 = false;
    this.skipStep2 = false;
  }
  clearAstronautForm(): void {
    this.newAstronaut = new Astronaut();
    this.newAstronautDuty = new AstronautDutyDto();
  }
}
