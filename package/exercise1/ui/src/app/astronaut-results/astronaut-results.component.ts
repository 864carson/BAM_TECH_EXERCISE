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
  @Input() searchName?: string;
  @Output() isBusy = new EventEmitter<boolean>();
  @Output() astronautDutyAdded = new EventEmitter<number>();

  newAstronautDutyFormVisible: boolean = false;
  astronautResultList: Astronaut[] = [];
  selectedAstronaut?: Astronaut;

  constructor() {
    this.loadAllAstronauts();
  }

  ngOnChanges(changes: SimpleChanges) {
    for (let propName in changes) {
      if (propName !== "reloadTable") {
        break;
      }

      if (JSON.stringify(changes[propName].currentValue) === "true") {
        this.loadAllAstronauts();
      }
      break;
    }
  }

  loadAllAstronauts(): void {
    this.isBusy.emit(true);
    if (this.searchName === null || this.searchName === undefined) {
      this.astronautService.getAllAstronauts()
        .subscribe({
          next: (res) => {
            this.astronautResultList = res.people;
          },
          error: (e) => console.error(e),
          complete: () => this.isBusy.emit(false)
        });
    } else {
      console.log(`Searching for: ${this.searchName}`);
      this.astronautService.getAstronautByName(encodeURIComponent(this.searchName))
        .subscribe({
          next: (res) => {
            console.log(res);
            this.astronautResultList = [];
            this.astronautResultList.push(res.person!);
          },
          error: (e) => {
            if (e.status === 404) {
              alert(e.error.message);
            } else {
              console.error(e);
            }
          },
          complete: () => this.isBusy.emit(false)
        });
    }
  }

  addAstronautDuty(): void {
    this.isBusy.emit(true);
    this.astronautService.createAstronautDutyRecord(this.newAstronautDuty)
      .subscribe({
        next: (res) => {
          console.log(res);
          this.hideNewAstronautDutyForm();

          this.loadAllAstronauts();
          this.loadDetails(this.newAstronautDuty.name);
          this.astronautDutyAdded.emit(res.id);
        },
        error: (e) => console.error(e),
        complete: () => this.isBusy.emit(false)
      });
  }

  loadDetails(name?: string): void {
    if (name === null || name === undefined) return;
    this.isBusy.emit(true);
    this.astronautService.getAstronautByName(encodeURIComponent(name))
      .subscribe({
        next: (res) => {
          console.log(res);
          this.selectedAstronaut = res.person;
        },
        error: (e) => console.error(e),
        complete: () => this.isBusy.emit(false)
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
