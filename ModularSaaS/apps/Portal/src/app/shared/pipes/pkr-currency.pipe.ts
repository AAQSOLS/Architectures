import { Pipe, PipeTransform } from '@angular/core';

/**
 * Formats integer or decimal PKR numbers into localized South Asian notation (Lacs and Crores).
 * Usage: {{ 15000000 | pkrCurrency }} => "1.50 Crore PKR"
 */
@Pipe({
  name: 'pkrCurrency',
  standalone: true
})
export class PkrCurrencyPipe implements PipeTransform {
  transform(value: number | string | null | undefined, showUnit = true): string {
    if (value === null || value === undefined || value === '') {
      return '-';
    }

    const num = typeof value === 'string' ? parseFloat(value) : value;
    if (isNaN(num)) {
      return '-';
    }

    const unitSuffix = showUnit ? ' PKR' : '';

    if (num >= 10000000) {
      // 1 Crore = 10,000,000
      const croreVal = num / 10000000;
      return `${croreVal.toFixed(2).replace(/\.00$/, '')} Crore${unitSuffix}`;
    }

    if (num >= 100000) {
      // 1 Lac = 100,000
      const lacVal = num / 100000;
      return `${lacVal.toFixed(2).replace(/\.00$/, '')} Lacs${unitSuffix}`;
    }

    return `${num.toLocaleString('en-PK')}${unitSuffix}`;
  }
}
