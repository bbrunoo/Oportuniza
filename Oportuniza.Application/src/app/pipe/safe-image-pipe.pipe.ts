import { Pipe, PipeTransform } from '@angular/core';

@Pipe({name: 'safeImagePipe'})
export class SafeImagePipePipe implements PipeTransform {

   transform(value: string): string {
    return value && value.trim() !== '' ? value : '../../assets/logo.png';
  }
}
