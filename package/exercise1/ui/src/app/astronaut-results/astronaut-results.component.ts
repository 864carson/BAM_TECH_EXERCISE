import { Component, inject, Input, Output, EventEmitter, SimpleChanges, OnChanges, ViewChild, OnInit, OnDestroy, AfterViewInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule, DatePipe } from '@angular/common';
import { RouterOutlet, RouterModule } from '@angular/router';
import { Astronaut } from '../models/astronaut.model';
import { AstronautDetailsComponent } from '../astronaut-details/astronaut-details.component';
import { AstronautService } from '../services/astronaut-service/astronaut.service';
import { AstronautDutyDto } from '../models/astronaut-duty-dto.model';
import { DataTablesModule } from 'angular-datatables';
import { DataTableDirective } from 'angular-datatables';
import { Subject } from 'rxjs';

@Component({
  selector: 'app-astronaut-results',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterOutlet, RouterModule, AstronautDetailsComponent, DataTablesModule],
  templateUrl: './astronaut-results.component.html',
  styleUrl: './astronaut-results.component.css'
})
export class AstronautResultsComponent implements AfterViewInit, OnDestroy, OnInit, OnChanges {
  astronautService: AstronautService = inject(AstronautService);
  @Input() newAstronautDuty: AstronautDutyDto = new AstronautDutyDto();
  @Input() reloadTable: boolean = false;
  @Input() searchName?: string;
  @Output() isBusy = new EventEmitter<boolean>();
  @Output() astronautDutyAdded = new EventEmitter<number>();

  @ViewChild(DataTableDirective, { static: false })
  dtElement!: DataTableDirective;
  dtTrigger: Subject<any> = new Subject();

  dtOptions = {};
  datePipe: DatePipe = new DatePipe('en-US');

  newAstronautDutyFormVisible: boolean = false;
  astronautResultList: Astronaut[] = [];
  selectedAstronaut?: Astronaut;

  constructor() {
  }

  ngOnInit(): void {
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
          this.showNewAstronautDutyForm(<Astronaut>data, !row.firstChild?.parentElement?.classList.contains('selected'));
        });
        return row;
      },
      select: {
        style: 'single',
        info: false,
        toggleable: false
      },
      language: {
        entries: {
          _: 'astronauts',
          1: 'astronaut'
        }
      }
    }
  }

  ngAfterViewInit(): void {
    this.dtTrigger.next(null);
  }

  ngOnDestroy(): void {
    // Do not forget to unsubscribe the event
    this.dtTrigger.unsubscribe();
  }

  ngOnChanges(changes: SimpleChanges) {
    for (let propName in changes) {
      if (propName !== "reloadTable") {
        break;
      }

      if (JSON.stringify(changes[propName].currentValue) === "true") {
        this.reloadAstronautTable();
      }
      break;
    }
  }

  reloadAstronautTable(): void {
    this.dtElement.dtInstance.then(dtInstance => {
      // Destroy the table first
      dtInstance.destroy();
      // Call the dtTrigger to rerender again
      this.dtTrigger.next(null);
    });
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

    this.isBusy.emit(true);
    this.astronautService.createAstronautDutyRecord(this.newAstronautDuty)
      .subscribe({
        next: (res) => {
          this.hideNewAstronautDutyForm();

          this.reloadAstronautTable();
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

  showNewAstronautDutyForm(astronaut: Astronaut, isSelected: boolean): void {
    if (!isSelected) return;

    this.loadDetails(astronaut.name);
    this.resetAstronautDutyForm();

    this.newAstronautDuty.name = astronaut.name;
    this.newAstronautDutyFormVisible = true;
  }
  hideNewAstronautDutyForm(): void {
    this.newAstronautDutyFormVisible = false;
  }
  resetAstronautDutyForm(): void {
    this.newAstronautDuty = new AstronautDutyDto();
    this.newAstronautDuty.dutyStartDate = this.datePipe.transform(new Date(), "MM/dd/yyyy")!;
  }
}
