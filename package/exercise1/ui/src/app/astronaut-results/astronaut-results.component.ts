import { Component, inject, Input, Output, EventEmitter, SimpleChanges, OnChanges } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterModule } from '@angular/router';
import { Astronaut } from '../models/astronaut.model';
import { AstronautDetailsComponent } from '../astronaut-details/astronaut-details.component';
import { AstronautService } from '../services/astronaut-service/astronaut.service';
import { AstronautDutyDto } from '../models/astronaut-duty-dto.model';

@Component({
  selector: 'app-astronaut-results',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterOutlet, RouterModule, AstronautDetailsComponent],
  templateUrl: './astronaut-results.component.html',
  styleUrl: './astronaut-results.component.css'
})
export class AstronautResultsComponent implements OnChanges {
  astronautService: AstronautService = inject(AstronautService);
  @Input() newAstronautDuty: AstronautDutyDto = new AstronautDutyDto();
  @Input() reloadTable: boolean = false;
  @Output() astronautDutyAdded = new EventEmitter<number>();

  newAstronautDutyFormVisible: boolean = false;
  astronautResultList: Astronaut[] = [];
  selectedAstronaut?: Astronaut;

  constructor() {
    this.loadAllAstronauts();
  }

  ngOnChanges(changes: SimpleChanges) {
    for (let propName in changes) {
      if (propName === "reloadTable" && JSON.stringify(changes[propName].currentValue) === "true") {
        this.loadAllAstronauts();
        break;
      }
    }
  }

  loadAllAstronauts(): void {
    let self = this;
    this.astronautService.getAllAstronauts()
      .subscribe({
        next: (res) => {
          console.log(res);
          self.astronautResultList = res.people;
        },
        error: (e) => console.error(e)
      });
  }

  addAstronautDuty(): void {
    let self = this;
    console.log(this.newAstronautDuty.name);
    this.astronautService.createAstronautDutyRecord(this.newAstronautDuty)
      .subscribe({
        next: (res) => {
          console.log(res);
          this.hideNewAstronautDutyForm();

          self.loadAllAstronauts();
          self.loadDetails(self.newAstronautDuty.name);
          this.astronautDutyAdded.emit(res.id);
        },
        error: (e) => console.error(e)
      });
  }

  loadDetails(name?: string): void {
    let self = this;
    console.log("loadDetails(" + name + ")");
    if (name === null || name === undefined) return;
    this.astronautService.getAstronautByName(encodeURIComponent(name))
      .subscribe({
        next: (res) => {
          console.log(res);
          self.selectedAstronaut = res.person;
        },
        error: (e) => console.error(e)
      });
  }

  showNewAstronautDutyForm(astronaut: Astronaut): void {
    this.loadDetails(astronaut.name);
    this.clearAstronautDutyForm();

    this.newAstronautDuty.name = astronaut.name;
    this.newAstronautDuty.dutyStartDate = new Date().toISOString();
    this.newAstronautDutyFormVisible = true;
  }
  hideNewAstronautDutyForm(): void {
    this.newAstronautDutyFormVisible = false;
  }

  clearAstronautDutyForm(): void {
    this.newAstronautDuty = new AstronautDutyDto();
  }
}
