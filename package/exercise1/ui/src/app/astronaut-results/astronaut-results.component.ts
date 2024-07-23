import { Component, inject, Input, Output, EventEmitter, SimpleChanges, OnChanges } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule, DatePipe } from '@angular/common';
import { RouterOutlet, RouterModule } from '@angular/router';
import { Astronaut } from '../models/astronaut.model';
import { AstronautDetailsComponent } from '../astronaut-details/astronaut-details.component';
import { AstronautService } from '../services/astronaut-service/astronaut.service';
import { AstronautDutyDto } from '../models/astronaut-duty-dto.model';
import { DataTablesModule } from 'angular-datatables';

@Component({
  selector: 'app-astronaut-results',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterOutlet, RouterModule, AstronautDetailsComponent, DataTablesModule],
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

  dtOptions = {};
  datePipe: DatePipe = new DatePipe('en-US');

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
      this.dtOptions = {
        processing: true,
        lengthMenu: [5, 10, 25],
        ajax: (dataTableParameters: any, callback: any) => {
          this.astronautService.getAllAstronauts()
            .subscribe({
              next: (res) => {
                this.astronautResultList = res.people;
                callback({
                  recordsTotal: res.people.length,
                  recordsFiltered: res.people.length,
                  data: res.people
                })
              },
              error: (e) => console.error(e),
              complete: () => this.isBusy.emit(false)
            });
        },
        columns: [
          { title: "Astronaut", data: "name" },
          { title: "Rank", data: "currentRank" },
          {
            title: "Duty Title",
            data: "currentDutyTitle",
            render: (data: any, type: string, row: any, meta: object) => {
              return row.currentDutyTitle;
            }
          },
          { title: "Career Start Date", data: "careerStartDate", searchable: false, ngPipeInstance: this.datePipe, ngPipeArgs: ['MM/dd/yyyy'] },
          { title: "Career End Date", data: "careerEndDate", searchable: false, ngPipeInstance: this.datePipe, ngPipeArgs: ['MM/dd/yyyy'] }
        ],
        rowCallback: (row: Node, data: any[] | Object, index: number) => {
          // Unbind first in order to avoid any duplicate handler
          $('td', row).off('click');
          $('td', row).on('click', () => {
            this.showNewAstronautDutyForm(<Astronaut>data);
          });
          return row;
        },
        select: {
          style: 'single',
          info: false
        },
        language: {
          entries: {
            _: 'astronauts',
            1: 'astronaut'
        }
        }
      }
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
