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
/**
 * The `AstronautService` class in TypeScript defines methods to interact with an API for managing
 * astronauts and their duty records.
 */
export class AstronautService {
  protected httpClient: HttpClient = inject(HttpClient);

  // This should be an environment variable and not hard-coded.
  baseUrl: string = "http://localhost:5204";

  constructor() { }

  /**
   * Returns an Observable of type AstronautListDto by making an HTTP GET request to the specified URL.
   * @returns An Observable of type AstronautListDto is being returned.
   */
  getAllAstronauts(): Observable<AstronautListDto> {
    return this.httpClient.get<AstronautListDto>(`${this.baseUrl}/Person`);
  }

  /**
   * Retrieves an astronaut by name using an HTTP GET request.
   * @param {string} name - The `name` parameter is a string that represents the name of the astronaut
   * you want to retrieve information about.
   * @returns An Observable of type AstronautDto is being returned.
   */
  getAstronautByName(name: string): Observable<AstronautDto> {
    return this.httpClient.get<AstronautDto>(`${this.baseUrl}/Person/${name}`);
  }

  /**
   * Retrieves the astronaut history by name using an HTTP GET request.
   * @param {string} name - The `name` parameter is a string that represents the name of the astronaut
   * whose history you want to retrieve.
   * @returns An Observable of type AstronautHistoryDto is being returned.
   */
  getAstronautHistoryByName(name: string): Observable<AstronautHistoryDto> {
    return this.httpClient.get<AstronautHistoryDto>(`${this.baseUrl}/AstronautDuty/${name}`);
  }

  /**
   * Sends a POST request to create an astronaut with the specified name.
   * @param {Astronaut} astronaut - An astronaut object containing the details of the astronaut to be
   * created, such as their name.
   * @returns An Observable of type CreationResultDto is being returned.
   */
  createAstronaut(astronaut: Astronaut): Observable<CreationResultDto> {
    return this.httpClient.post<CreationResultDto>(
      `${this.baseUrl}/Person`,
      `"${astronaut.name}"`,
      { headers: { "Content-Type": "application/json" }});
  }

  /**
   * Sends a PUT request to update an astronaut's name using the HttpClient in Angular.
   * @param {Astronaut} astronaut - The `astronaut` parameter is an object of type `Astronaut`, which
   * contains information about an astronaut such as their name.
   * @param {string} newName - The `newName` parameter is a string that represents the new name that
   * you want to update for the astronaut.
   * @returns An Observable of type CreationResultDto is being returned.
   */
  updateAstronaut(astronaut: Astronaut, newName: string): Observable<CreationResultDto> {
    return this.httpClient.put<CreationResultDto>(
      `${this.baseUrl}/Person`,
      `"${astronaut.name}"`,
      { headers: { "Content-Type": "application/json" }});
  }

  /**
   * Sends a POST request to create a new astronaut duty record with the provided data.
   * @param {AstronautDutyDto} duty - The `duty` parameter is an objet of type `AstronautDutyDto`, which
   * contains information about an astronaut's duty assignment.
   * @returns An Observable of type CreationResultDto is being returned.
   */
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
