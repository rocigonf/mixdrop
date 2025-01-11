import { Component, OnInit } from '@angular/core';
import { NavbarComponent } from "../../components/navbar/navbar.component";

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [NavbarComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit {
  private bpm: number = 90;
  private intervalTime: number = 0;
  private isReproducing : boolean = false
  private interval : any
  private audio = new Audio('home_song.mp3');

  private readonly RUNA_FRAMES: string[] = [
    '/images/runa-speaker/runa-speaker1.png',
    '/images/runa-speaker/runa-speaker2.png',
    '/images/runa-speaker/runa-speaker3.png',
    '/images/runa-speaker/runa-speaker4.png',
    '/images/runa-speaker/runa-speaker5.png',
    '/images/runa-speaker/runa-speaker6.png',
  ];
  public currentFrameNumber : number = 0
  public currentFrameName : string = this.RUNA_FRAMES[0]

  private readonly SONGS: { [index: string]: number; } = {
    "/songs/pressure.mp3" : 114,
    "/songs/break_free.mp3" : 109,
    "/songs/enemy.mp3" : 77,
    "/songs/hip_shop.mp3" : 97.50884434,
    "/songs/home_song.mp3" : 90
  }


  ngOnInit(): void {
    // Escoge aleatoriamente la canción y luego la selecciona
    const keys = Object.keys(this.SONGS)
    const songName = keys[Math.floor(keys.length * Math.random())]
    this.audio = new Audio(songName)
    this.bpm = this.SONGS[songName]

    this.intervalTime = (60 / this.bpm) * 1000; // Calcula la cantidad de veces que lo tiene que hacer en segundos y lo pasa a milisegundos
    this.audio.load()
    this.audio.addEventListener('ended', () => { this.reproduce(); this.reproduce()}) // Lo para y luego lo vuelve a activar, por eso llamarlo dos veces
    this.reproduce()
  }

  // CRÉDITOS POR EL "bind": https://stackoverflow.com/questions/70634283/react-typescript-uncaught-typeerror-this-is-undefined
  // Básicamente, al decirle en el "accurateInterval" que se le pasa la función "animate", parece que le da la paja y pierde el contexto, y con el "bind" se le pasa
  reproduce()
  {
    console.log("SE HA PULSADO EL BOTÓN DE REPRODUCIR")
    if(this.isReproducing)
    {
      this.interval.cancel()
      this.audio.pause()
      this.audio.currentTime = 0
      this.isReproducing = false
    }
    else
    {
      this.interval = this.accurateInterval(this.intervalTime, this.animate.bind(this));
      this.audio.play()
      this.isReproducing = true
    }
  }

  animate(this : any) {
    console.log('ola') 
    if (this.currentFrameNumber < this.RUNA_FRAMES.length - 1)
    {
      this.currentFrameNumber++
    } 
    else
    {
      this.currentFrameNumber = 0
    }
  
    this.currentFrameName = this.RUNA_FRAMES[this.currentFrameNumber]
  }

  // CRÉDITOS: https://gist.github.com/AlexJWayne/1d99b3cd81d610ac7351
  private accurateInterval(time: any, fn: Function) {
    let cancel : any, nextAt : any, timeout : any, wrapper : any, _ref: any
    nextAt = new Date().getTime() + time
    timeout = null

    if (typeof time === 'function') _ref = [time, fn], fn = _ref[0], time = _ref[1]

    wrapper = function() {
      nextAt += time
      timeout = setTimeout(wrapper, nextAt - new Date().getTime())
      return fn()
    }

    cancel = function() {
      return clearTimeout(timeout)
    }

    timeout = setTimeout(wrapper, nextAt - new Date().getTime())

    return {
      cancel: cancel
    }
  }
}
