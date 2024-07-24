import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AstronautDto } from '../../models/astronaut-dto.model';
import { AstronautListDto } from '../../models/astronaut-list-dto.model';
import { AstronautHistoryDto } from '../../models/astronaut-history-dto.model';
import { Astronaut } from '../../models/astronaut.model';
import { CreationResultDto } from '../../models/creation-result-dto.model';
import { AstronautDutyDto } from '../../models/astronaut-duty-dto.model';

@Injectable({
  providedIn: 'root'
})
export class AstronautService {
  protected httpClient: HttpClient = inject(HttpClient);

  baseUrl: string = "http://localhost:5204";

  constructor() { }

  getAllAstronauts(): Observable<AstronautListDto> {
    return this.httpClient.get<AstronautListDto>(`${this.baseUrl}/Person`);
  }

  getAstronautByName(name: string): Observable<AstronautDto> {
    return this.httpClient.get<AstronautDto>(`${this.baseUrl}/Person/${name}`);
  }

  getAstronautHistoryByName(name: string): Observable<AstronautHistoryDto> {
    return this.httpClient.get<AstronautHistoryDto>(`${this.baseUrl}/AstronautDuty/${name}`);
  }

  createAstronaut(astronaut: Astronaut): Observable<CreationResultDto> {
    return this.httpClient.post<CreationResultDto>(
      `${this.baseUrl}/Person`,
      `"${astronaut.name}"`,
      { headers: { "Content-Type": "application/json" }});
  }

  updateAstronaut(astronaut: Astronaut, newName: string): Observable<CreationResultDto> {
    return this.httpClient.put<CreationResultDto>(
      `${this.baseUrl}/Person`,
      `"${astronaut.name}"`,
      { headers: { "Content-Type": "application/json" }});
  }

  createAstronautDutyRecord(duty: AstronautDutyDto): Observable<CreationResultDto> {
    return this.httpClient.post<CreationResultDto>(
      `${this.baseUrl}/AstronautDuty`,
      {
        "name": duty.name,
        "rank": duty.rank,
        "dutyTitle": duty.dutyTitle,
        "dutyStartDate": duty.dutyStartDate
      },
      { headers: { "Content-Type": "application/json" }});
  }
}
