import { Component, inject, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OnChanges, SimpleChanges } from '@angular/core';
import { Astronaut } from '../models/astronaut.model';
import { AstronautService } from '../services/astronaut-service/astronaut.service';
import { AstronautDuty } from '../models/astronaut-duty.model';

@Component({
  selector: 'app-astronaut-details',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './astronaut-details.component.html',
  styleUrl: './astronaut-details.component.css'
})
export class AstronautDetailsComponent implements OnChanges {
  @Input() astronaut?: Astronaut;
  astronautDuties?: AstronautDuty[];
  astronautService: AstronautService = inject(AstronautService);

  constructor() { }

  ngOnChanges(changes: SimpleChanges) {
    if (this.astronaut === undefined) return;

    let self = this;
    this.astronautService.getAstronautHistoryByName(this.astronaut.name!)
      .subscribe({
        next: (res) => {
          console.log(res);
          self.astronaut = res.person;
          self.astronautDuties = res.astronautDuties.sort((a: AstronautDuty, b: AstronautDuty) => a.id! - b.id!).reverse();
        },
        error: (e) => console.error(e)
      });
  }
}
