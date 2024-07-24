import { Component, inject, Input, ViewChild, OnInit, OnDestroy, AfterViewInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule, DatePipe } from '@angular/common';
import { Router, ActivatedRoute, NavigationEnd } from '@angular/router';
import { DataTablesModule } from 'angular-datatables';
import { DataTableDirective } from 'angular-datatables';
import { Subject } from 'rxjs';
import { Astronaut } from '../models/astronaut.model';
import { AstronautService } from '../services/astronaut-service/astronaut.service';
import { AstronautDutyDto } from '../models/astronaut-duty-dto.model';
import { AstronautDetailsComponent } from '../astronaut-details/astronaut-details.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, FormsModule, DataTablesModule, AstronautDetailsComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements AfterViewInit, OnDestroy, OnInit {
  router: Router = inject(Router);
  activatedRoute: ActivatedRoute = inject(ActivatedRoute);
  astronautService: AstronautService = inject(AstronautService);
  @Input() newAstronaut: Astronaut = new Astronaut();
  @Input() newAstronautDuty: AstronautDutyDto = new AstronautDutyDto();

  routeSubscription: any;
  datePipe: DatePipe = new DatePipe('en-US');

  @ViewChild(DataTableDirective, { static: false })
  dtElement!: DataTableDirective;
  dtTrigger: Subject<any> = new Subject();
  dtOptions = {};

  newAstronautDutyFormVisible: boolean = false;
  astronautResultList: Astronaut[] = [];
  selectedAstronaut?: Astronaut;

  isAddingNewAstronaut: boolean = false;
  isInAddingStep1: boolean = false;
  isInAddingStep2: boolean = false;
  skipStep2: boolean = false;

  constructor() {
    // this.router.routeReuseStrategy.shouldReuseRoute = () => false;
    this.routeSubscription = this.router.events.subscribe((event) => {
      if (event instanceof NavigationEnd) {
         // Trick the Router into believing it's last link wasn't previously loaded
         this.router.navigated = false;
      }
    });
  }

  ngOnInit(): void {
    this.createAndLoadTable();
  }

  ngAfterViewInit(): void {
    this.dtTrigger.next(null);
  }

  ngOnDestroy(): void {
    if (this.dtTrigger) {
    this.dtTrigger.unsubscribe();
    }
    if (this.routeSubscription) {
      this.routeSubscription.unsubscribe();
    }
  }

  createAndLoadTable() {
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

  addNewAstronautThenDuty(): void {
    this.astronautService.createAstronaut(this.newAstronaut)
      .subscribe({
        next: (res) => {
          if (this.skipStep2) {
            this.hideNewAstronautForm();
            this.reloadAstronautTable();
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
    this.skipStep2 = true;
    this.addNewAstronautThenDuty();
  }

  addAstronautDuty_NewAstronautProcess(): void {
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
          this.hideNewAstronautForm();
          this.reloadAstronautTable();
        },
        error: (e) => console.error(e)
      });
  }

  reloadAstronautTable(): void {
    if (this.dtElement?.dtInstance === undefined) {
      this.router.navigate([this.router.url]);
      return;
    };

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

    this.astronautService.createAstronautDutyRecord(this.newAstronautDuty)
      .subscribe({
        next: (res) => {
          // this.hideNewAstronautDutyForm();
          const astronautName: string = this.newAstronautDuty.name!;
          this.resetAstronautDutyForm();

          this.reloadAstronautTable();
          this.loadDetails(astronautName);
        },
        error: (e) => console.error(e),
      });
  }

  loadDetails(name?: string): void {

    if (name === null || name === undefined) return;
    this.astronautService.getAstronautByName(encodeURIComponent(name))
      .subscribe({
        next: (res) => {
          console.log(res);
          this.selectedAstronaut = res.person;
        },
        error: (e) => console.error(e),
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

  showNewAstronautForm(): void {
    this.clearAstronautForm();
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
