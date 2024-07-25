import { Component, inject, Input, ViewChild, OnInit, OnDestroy, AfterViewInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule, DatePipe } from '@angular/common';
import { DataTablesModule } from 'angular-datatables';
import { DataTableDirective } from 'angular-datatables';
import { Subject } from 'rxjs';
import { Astronaut } from '../models/astronaut.model';
import { AstronautService } from '../services/astronaut-service/astronaut.service';
import { AstronautDutyDto } from '../models/astronaut-duty-dto.model';
import { AstronautDuty } from '../models/astronaut-duty.model';
import { ErrorService } from '../services/error-service/error.service';
import { DateUtils } from '../../utils/dateUtils';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, FormsModule, DataTablesModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
/**
 * Manages astronaut data and duties, including creating new astronauts and assigning duties,
 * with DataTable integration for displaying astronaut information.
 */
export class HomeComponent implements AfterViewInit, OnDestroy, OnInit {
  private componentDescriptor: string = "app-home";

  astronautService: AstronautService = inject(AstronautService);
  errorService: ErrorService = inject(ErrorService);
  @Input() newAstronaut: Astronaut = new Astronaut();
  @Input() newAstronautDuty: AstronautDutyDto = new AstronautDutyDto();

  datePipe: DatePipe = new DatePipe('en-US');
  dateUtils: DateUtils = new DateUtils();

  @ViewChild(DataTableDirective, { static: false })
  dtElement!: DataTableDirective;
  dtTrigger: Subject<any> = new Subject();
  dtOptions = {};

  newAstronautDutyFormVisible: boolean = false;
  astronautResultList: Astronaut[] = [];
  selectedAstronaut?: Astronaut;
  astronautDuties?: AstronautDuty[];

  isAddingNewAstronaut: boolean = false;
  isInAddingStep1: boolean = false;
  isInAddingStep2: boolean = false;
  skipStep2: boolean = false;

  constructor() { }

  //#region [ Method Overrides ]

  /**
   * Calls the createAndLoadTable method when the component is initialized.
   */
  ngOnInit(): void {
    this.createAndLoadTable();
  }

  /**
   * Triggers the DataTables plugin after the view has been initialized.
   */
  ngAfterViewInit(): void {
    this.dtTrigger.next(null);
  }

  /**
   * Unsubscribes from a data trigger if it exists.
   */
  ngOnDestroy(): void {
    if (this.dtTrigger) {
      this.dtTrigger.unsubscribe();
    }
  }

  //#endregion

  //#region [ Datatable Creation/Management ]
  /**
   * Sets up a DataTable with specific options and loads astronaut data into the table.
   */
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
            error: (e) => {
              alert(e);
              console.error(e);
            }
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

  /**
   * Checks if a DataTable instance exists, and if so, it refreshes the table by destroying and rerendering it.
   * Otherwise, the page will be reloaded to accomplish the same end goal of showing the new data in the table.
   */
  reloadAstronautTable(): void {
    // If dtInstance is undefined, just reload the page
    if (this.dtElement?.dtInstance === undefined) {
      window.location.reload();
      return;
    };

    this.dtElement.dtInstance.then(dtInstance => {
      // Destroy the table first
      dtInstance.destroy();

      // Call the dtTrigger to rerender again
      this.dtTrigger.next(null);
    });
  }

  //#endregion

  //#region [ New Astronaut Workflow ]

  /**
   * Adds a new astronaut and proceeds to assign duty based on certain conditions.
   */
  addAstronautAndContinue(): void {
    this.astronautService.createAstronaut(this.newAstronaut)
      .subscribe({
        next: (res) => {
          if (this.skipStep2) {
            this.cancelNewAstronautWorkflow();
          } else {
            this.newAstronautDuty.name = this.newAstronaut.name;
            this.newAstronautDuty.dutyStartDate = this.dateUtils.formatDateToShortDateFormat(new Date());
            this.isInAddingStep1 = false;
            this.isInAddingStep2 = true;
          }
        },
        error: (e) => {
          this.errorService.handleError(e, this.componentDescriptor);
        }
      });
  }

  /**
   * Sets `skipStep2` to true and then calls `addAstronautAndContinue`.
   */
  addAstronautAndFinish(): void {
    this.skipStep2 = true;
    this.addAstronautAndContinue();
  }

  /**
   * Hides the new astronaut form and reloads the astronaut table.
   */
  cancelNewAstronautWorkflow(): void {
    this.hideNewAstronautForm();
    this.reloadAstronautTable();
  }

  /**
   * Processes a new astronaut duty by converting the duty start date to ISO string and creating a new
   * astronaut duty record.
   * @returns If an error occurs during the date conversion attempt an exception by displaying an alert
   * with the error message and logging the error to the console. The function will then return.
   */
  addAstronautDutyDuringNewAstronautWorkflow(): void {
    this.addDutyRecord(
      (res: any) => {
        this.cancelNewAstronautWorkflow();
      });
  }

  //#endregion

  //#region [ Duty History and Creation ]

  /**
   * Retrieves astronaut duty details by name and sorts them in descending order based on ID.
   * @param {string} [name] - [optional] Parameter `name` of type string. This function is responsible
   * for loading duty details for an astronaut based on the provided name. If the `name` parameter is
   * null or undefined, the function will return early without making any requests. Otherwise, it
   * @returns If the `name` parameter is null or undefined, the function will return early without
   * making the API call. If a valid `name` is provided, the function will call the
   * `getAstronautHistoryByName` method from the `astronautService` to fetch astronaut duty details.
   * The response data is then sorted by `id` in descending order and stored in the `astronautDuties` variable.
   */
  loadDutyDetails(name?: string) {
    if (name === null || name === undefined) return;
    this.astronautService.getAstronautHistoryByName(encodeURIComponent(name))
      .subscribe({
        next: (res) => {
          this.astronautDuties = res.astronautDuties.sort((a: AstronautDuty, b: AstronautDuty) => a.id! - b.id!).reverse();
        },
        error: (e) => {
          this.errorService.handleError(e, this.componentDescriptor);
        }
      });
  }

  /**
   * Adds a new astronaut duty record, converting the duty start date to ISO format, and making API
   * calls to create the record.
   * @returns If an error occurs during the date conversion attempt an exception by displaying an alert
   * with the error message and logging the error to the console. The function will then return.
   */
  addAstronautDuty(): void {
    this.addDutyRecord(
      (res: any) => {
        const astronautName: string = this.newAstronautDuty.name!;
        this.resetAstronautDutyForm();
        this.selectedAstronaut = undefined;

        this.reloadAstronautTable();
        this.loadDutyDetails(astronautName);
      });
  }

  //#endregion

  //#region [ Form State Management ]

  /**
   * Displays a form for assigning a new duty to a selected astronaut.
   * @param {Astronaut} astronaut - The `astronaut` parameter is an object of type `Astronaut`, which
   * contains information about an astronaut such as their name. In the `showNewAstronautDutyForm`
   * function, this parameter is used to set the selected astronaut variable.
   * @param {boolean} isSelected - The `isSelected` parameter is a boolean value that indicates whether
   * the astronaut has been selected or not. If `isSelected` is `true`, the function proceeds to show a
   * new astronaut duty form for the selected astronaut.
   * @returns If the `isSelected` parameter is `false`, the function will return early and not execute
   * the rest of the code block.
   */
  showNewAstronautDutyForm(astronaut: Astronaut, isSelected: boolean): void {
    if (!isSelected) return;

    this.selectedAstronaut = astronaut;
    this.loadDutyDetails(astronaut.name);
    this.resetAstronautDutyForm();

    this.newAstronautDuty.name = astronaut.name;
    this.newAstronautDutyFormVisible = true;
  }

  /**
   * Sets the visibility of a form for adding a new astronaut duty to false.
   */
  hideNewAstronautDutyForm(): void {
    this.newAstronautDutyFormVisible = false;
  }

  /**
   * Resets the `newAstronautDuty` object and sets the `dutyStartDate` to the current date in MM/dd/yyyy format.
   */
  resetAstronautDutyForm(): void {
    this.newAstronautDuty = new AstronautDutyDto();
    this.newAstronautDuty.dutyStartDate = this.dateUtils.formatDateToShortDateFormat(new Date());
  }

  /**
   * Clears the astronaut form and sets flags to indicate that a new astronaut is being added.
   */
  showNewAstronautForm(): void {
    this.clearAstronautForm();
    this.isAddingNewAstronaut = true;
    this.isInAddingStep1 = true;
    this.isInAddingStep2 = false;
  }

  /**
   * Hides the new astronaut form and resets related flags.
   */
  hideNewAstronautForm(): void {
    this.isAddingNewAstronaut = false;
    this.isInAddingStep1 = false;
    this.skipStep2 = false;
  }

  /**
   * Resets the newAstronaut and newAstronautDuty objects to their default values.
   */
  clearAstronautForm(): void {
    this.newAstronaut = new Astronaut();
    this.newAstronautDuty = new AstronautDutyDto();
  }

  //#endregion

  //#region [ Helpers ]

  private addDutyRecord(successCallback: any, errorCallback?: any): void {
    try {
      // Convert the duty start date from short date format to ISO format and handle exceptions
      // caused by invalid date values.
      const newDutyDate: string = this.dateUtils.convertShortDateStringToISOString(this.newAstronautDuty.dutyStartDate!);
      this.newAstronautDuty.dutyStartDate = newDutyDate;
    }
    catch (ex) {
      this.errorService.handleError(ex, this.componentDescriptor);
      return;
    }

    this.astronautService.createAstronautDutyRecord(this.newAstronautDuty)
      .subscribe({
        next: (res) => {
          if (successCallback) {
            successCallback(res);
          }
        },
        error: (e) => {
          this.errorService.handleError(e, this.componentDescriptor);
          if (errorCallback) {
            errorCallback(e);
          }
        }
      });
  }

  //#endregion
}
