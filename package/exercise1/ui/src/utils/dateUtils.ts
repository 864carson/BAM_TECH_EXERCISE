import { DatePipe } from '@angular/common';

/**
 * The DateUtils class provides methods for formatting dates and converting short date strings to ISO strings.
 */
export class DateUtils {
    datePipe: DatePipe = new DatePipe('en-US');

    /**
     * Takes a Date object and a format string, then uses the datePipe service to transform the
     * date into the specified format.
     * @param {Date} date - The `date` parameter is a Date object representing the date that you want
     * to format.
     * @param {string} format - The `format` parameter is a string that specifies the desired format
     * for the date to be transformed into. This format string can include placeholders for various
     * date and time components, such as year, month, day, hour, minute, second, etc.
     * @returns Returns a formatted date string based on the input `date` and `format` provided.
     * It uses Angular's `DatePipe` to transform the date into the specified format.
     */
    formatDateToCustomFormat(date: Date, format: string): string {
        return this.datePipe.transform(date, format)!;
    }

    /**
     * Takes a Date object and returns a string in the format "MM/dd/yyyy".
     * @param {Date} date - Date object that represents a specific date and time.
     * @returns A formatted date string in the "MM/dd/yyyy" format.
     */
    formatDateToShortDateFormat(date: Date): string {
        return this.datePipe.transform(date, "MM/dd/yyyy")!;
    }

    /**
     * Converts a short date string to an ISO string representation.
     * @param {string} shortDate - A short date string in the format "MM/DD/YYYY".
     * @returns An ISO string format using the `toISOString` method of the Date object.
     */
    convertShortDateStringToISOString(shortDate: string): string {
        return new Date(shortDate).toISOString();
    }
}