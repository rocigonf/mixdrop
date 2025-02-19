import { Component, ElementRef, ViewChild } from '@angular/core';

@Component({
  selector: 'app-roulette',
  templateUrl: './roulette.component.html',
  styleUrls: ['./roulette.component.css']
})
export class RouletteComponent {
  @ViewChild('wheel', { static: false }) wheel!: ElementRef<HTMLDivElement>;
  @ViewChild('spinButton', { static: false }) spinButton!: ElementRef<HTMLButtonElement>;

  private currentRotation = 0;

  spinRoulette() {
    
    const extraRotations = 720; // Mínimo de giros completos
    const randomAngle = Math.floor(Math.random() * 360); // Ángulo aleatorio de detención
    const finalAngle = this.currentRotation + extraRotations + randomAngle;

    // Cambiar la propiedad de transform para girar la rueda
    this.wheel.nativeElement.style.transition = "transform 2s ease-out";
    this.wheel.nativeElement.style.transform = `rotate(${finalAngle}deg)`;

    this.currentRotation = finalAngle; // Guardar el estado actual de la rotación

    // Deshabilitar el botón mientras la ruleta gira
    this.spinButton.nativeElement.disabled = true;

    // Reactivar el botón una vez que el giro termine
    this.wheel.nativeElement.addEventListener("transitionend", () => {
      this.spinButton.nativeElement.disabled = false;
    }, { once: true });
  }
}
