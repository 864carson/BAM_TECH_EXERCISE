import { AstronautDuty } from "./astronaut-duty.model";
import { Astronaut } from "./astronaut.model";

export class AstronautHistoryDto {
    person?: Astronaut;
    astronautDuties: AstronautDuty[] = [];
    success: boolean = true;
    message?: string;
    responseCode: number = 200;
}
