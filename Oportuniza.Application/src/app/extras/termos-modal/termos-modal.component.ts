import { CommonModule } from '@angular/common';
import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from "@angular/material/dialog";

@Component({
  selector: 'app-termos-modal',
  imports: [MatDialogModule, CommonModule],
  templateUrl: './termos-modal.component.html',
  styleUrl: './termos-modal.component.css'
})
export class TermosModalComponent {
  botaoVisivel = false;

  constructor(
    public dialogRef: MatDialogRef<TermosModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) { }

  aceitarTermos() {
    this.dialogRef.close('aceito');
  }

  verificarRolagem(event: Event) {
    const elemento = event.target as HTMLElement;
    const chegouAoFim = elemento.scrollHeight - elemento.scrollTop <= elemento.clientHeight + 10;

    if (chegouAoFim && !this.botaoVisivel) {
      this.botaoVisivel = true;
    }
  }
}
