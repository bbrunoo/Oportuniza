import { CommonModule } from '@angular/common';
import { Component, Inject, ViewChild } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from "@angular/material/dialog";
import { ImageCroppedEvent, ImageCropperComponent, ImageTransform } from "ngx-image-cropper";
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import Swal from 'sweetalert2';

export interface CropperDialogData {
  imageBase64: string;
}
@Component({
  selector: 'app-cropper-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    ImageCropperComponent, MatIconModule, MatButtonModule, MatProgressSpinnerModule
  ], templateUrl: './cropper-dialog.component.html',
  styleUrl: './cropper-dialog.component.css'
})
export class CropperDialogComponent {
  @ViewChild('cropper', { static: false }) cropper!: ImageCropperComponent;

  imageBase64: string = '';
  croppedImage: string = '';
  selectedImageFile?: File;

  isImageLoaded = false;
  loadImageError = false;
  transform: ImageTransform = {};

  private rotation = 0;
  private scale = 1;
  private flipH = false;


  constructor(
    public dialogRef: MatDialogRef<CropperDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: CropperDialogData
  ) {
    this.imageBase64 = data.imageBase64;
  }

  rotateLeft() {
    this.rotation -= 90;
    this.transform = { ...this.transform, rotate: this.rotation };
  }

  rotateRight() {
    this.rotation += 90;
    this.transform = { ...this.transform, rotate: this.rotation };
  }

  flipHorizontal() {
    this.flipH = !this.flipH;
    this.transform = { ...this.transform, flipH: this.flipH };
  }

  zoomIn() {
    this.scale += 0.1;
    this.transform = { ...this.transform, scale: this.scale };
  }

  zoomOut() {
    this.scale -= 0.1;
    this.transform = { ...this.transform, scale: this.scale };
  }

  fileChangeEvent(event: any): void {
    this.isImageLoaded = false;
    this.loadImageError = false;

    const file = event.target.files[0];
    if (!file) return;

    const reader = new FileReader();
    reader.readAsDataURL(file);
    reader.onload = () => {
      this.imageBase64 = reader.result as string;
      this.rotation = 0;
      this.scale = 1;
      this.flipH = false;
      this.transform = {};
    };
    reader.onerror = () => {
      this.loadImageFailed();
    };
  }

  imageCropped(event: ImageCroppedEvent) {
    if (event.objectUrl && event.blob) {
      this.croppedImage = event.objectUrl;
      this.selectedImageFile = new File(
        [event.blob],
        'imagem-cortada.png',
        { type: 'image/png' }
      );
    }
  }

  imageLoaded() {
    this.isImageLoaded = true;
    this.loadImageError = false;
  }

  loadImageFailed() {
    this.isImageLoaded = false;
    this.loadImageError = true;
    Swal.fire('Erro', 'Falha ao carregar a imagem. Tente um arquivo diferente.', 'error');
  }

  onSave(): void {
    this.cropper.crop();

    if (this.selectedImageFile) {
      this.dialogRef.close(this.selectedImageFile);
    } else {
      setTimeout(() => {
        if (this.selectedImageFile) {
          this.dialogRef.close(this.selectedImageFile);
        } else {
          Swal.fire('Atenção', 'A imagem ainda não foi cortada. Tente salvar novamente.', 'warning');
        }
      }, 100);
    }
  }

  onCancel(): void {
    this.dialogRef.close();
  }
}
