import { Component, OnInit } from '@angular/core';
import { NavbarComponent } from "../../components/navbar/navbar.component";
import {MatIconModule} from '@angular/material/icon';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [NavbarComponent, MatIconModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit {
  private bpm: number = 90;
  private intervalTime: number = 0;
  private isReproducing : boolean = false
  private interval : any
  private audio = new Audio('/songs/home_song.mp3');

  private readonly RUNA_FRAMES: string[] = [
    '/images/runa-speaker/runa-speaker1.png',
    '/images/runa-speaker/runa-speaker2.png',
    '/images/runa-speaker/runa-speaker3.png',
    '/images/runa-speaker/runa-speaker4.png',
  ];
  public currentRunaFrameNumber : number = 0
  public currentRunaFrameName : string = this.RUNA_FRAMES[0]

  // Por ahora pongo esto aquí, si en el juego quisiésemos que el altavoz se mueva también al ritmo de lo que hayan jugado los usuarios, pues se movería
  private readonly SPEAKER_FRAMES: string[] = [
    '/images/speaker/speaker1.png',
    '/images/speaker/speaker2.png',
  ];
  public currentSpeakerFrameNumber : number = 0
  public currentSpeakerFrameName : string = this.SPEAKER_FRAMES[0]

  private readonly SONGS: { [index: string]: number; } = {
    "/songs/pressure.mp3" : 114,
    "/songs/break_free.mp3" : 109,
    "/songs/enemy.mp3" : 77,
    "/songs/hip_shop.mp3" : 97.50884434,
    "/songs/home_song.mp3" : 90,
    "/songs/fly_octo_fly.flac" : 163,
    "/songs/cat.flac" : 112,
    "/songs/hopes_and_dreams.mp3" : 171,
    "/songs/underground.mp3" : 120,
    "/songs/natures_crescendo.mp3" : 121,
    "/songs/temazo.mp3" : 129
  }


  ngOnInit(): void {
    try 
    {
      // Escoge aleatoriamente la canción y luego la selecciona
      const keys = Object.keys(this.SONGS)
      const songName = keys[Math.floor(keys.length * Math.random())]
      this.audio = new Audio(songName)
      this.bpm = this.SONGS[songName]
      this.audio.volume = 0.2; // El volumen es un float por lo q 1 es el máximo

      this.intervalTime = (60 / this.bpm) * 1000; // Calcula la cantidad de veces que lo tiene que hacer en segundos y lo pasa a milisegundos
      this.audio.load()
      this.audio.addEventListener('ended', () => { this.reproduce(); this.reproduce()}) // Lo para y luego lo vuelve a activar, por eso llamarlo dos veces
      //this.reproduce()
    }
    catch {}
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
    //console.log('ola') 
  
    // Runa
    if (this.currentRunaFrameNumber < this.RUNA_FRAMES.length - 1)
    {
      this.currentRunaFrameNumber++
    } 
    else
    {
      this.currentRunaFrameNumber = 0
    }
    this.currentRunaFrameName = this.RUNA_FRAMES[this.currentRunaFrameNumber]

    // Altavoz
    if(this.currentSpeakerFrameNumber < this.SPEAKER_FRAMES.length - 1)
    {
      this.currentSpeakerFrameNumber++
    }
    else
    {
      this.currentSpeakerFrameNumber = 0
    }
    this.currentSpeakerFrameName = this.SPEAKER_FRAMES[this.currentSpeakerFrameNumber]
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

  scrollToTop() {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }
}
