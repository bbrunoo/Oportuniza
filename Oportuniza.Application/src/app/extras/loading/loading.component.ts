import { Component, Inject, ViewEncapsulation } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';

@Component({
  selector: 'app-loading',
  imports: [MatDialogModule],
  templateUrl: './loading.component.html',
  styleUrl: './loading.component.css',
  encapsulation: ViewEncapsulation.None
})
export class LoadingComponent {
  constructor(
    public dialogRef: MatDialogRef<LoadingComponent>,
    @Inject(MAT_DIALOG_DATA) public data: LoadingComponent) { }
}
