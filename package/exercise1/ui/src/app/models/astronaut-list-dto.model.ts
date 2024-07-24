import { Astronaut } from "./astronaut.model";

export class AstronautListDto {
    people: Astronaut[] = [];
    success: boolean = true;
    message?: string;
    responseCode: number = 200;
}
