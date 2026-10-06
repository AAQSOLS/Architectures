import { Pipe, PipeTransform } from '@angular/core';

export type AreaTargetUnit = 'marla' | 'kanal' | 'sqft' | 'auto';

/**
 * Converts canonical Square Feet to regional real estate units (Marla / Kanal).
 * Uses standard Islamabad/Rawalpindi base (1 Marla = 225 sq ft; 1 Kanal = 4,500 sq ft).
 */
@Pipe({
  name: 'areaUnit',
  standalone: true
})
export class AreaUnitPipe implements PipeTransform {
  private readonly SQ_FT_PER_MARLA = 225;
  private readonly SQ_FT_PER_KANAL = 4500;

  transform(sqFtValue: number | null | undefined, target: AreaTargetUnit = 'auto'): string {
    if (sqFtValue === null || sqFtValue === undefined || isNaN(sqFtValue)) {
      return '-';
    }

    if (target === 'auto') {
      if (sqFtValue >= this.SQ_FT_PER_KANAL) {
        const kanals = sqFtValue / this.SQ_FT_PER_KANAL;
        return `${kanals.toFixed(2).replace(/\.00$/, '')} Kanal`;
      }
      if (sqFtValue >= this.SQ_FT_PER_MARLA) {
        const marlas = sqFtValue / this.SQ_FT_PER_MARLA;
        return `${marlas.toFixed(1).replace(/\.0$/, '')} Marla`;
      }
      return `${sqFtValue.toLocaleString()} Sq Ft`;
    }

    if (target === 'kanal') {
      return `${(sqFtValue / this.SQ_FT_PER_KANAL).toFixed(2)} Kanal`;
    }

    if (target === 'marla') {
      return `${(sqFtValue / this.SQ_FT_PER_MARLA).toFixed(1)} Marla`;
    }

    return `${sqFtValue.toLocaleString()} Sq Ft`;
  }
}
