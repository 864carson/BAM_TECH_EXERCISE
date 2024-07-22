import { Astronaut } from "./astronaut.model";

export class AstronautDto {
    person?: Astronaut;
    success: boolean = true;
    message?: string;
    responseCode: number = 200;
}
