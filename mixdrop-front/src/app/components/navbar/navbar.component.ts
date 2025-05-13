import { Component, ElementRef, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { environment } from '../../../environments/environment';
import { User } from '../../models/user';
import { Router } from '@angular/router';
import { WebsocketService } from '../../services/websocket.service';
import { TranslocoModule, TranslocoService } from '@jsverse/transloco';
import { TranslatorService } from '../../services/translator.service';
import { Language } from '../../models/language';
import { MatTooltipModule } from '@angular/material/tooltip';
import Swal, { SweetAlertIcon } from 'sweetalert2';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [TranslocoModule, MatTooltipModule],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css'
})
export class NavbarComponent implements OnInit{
  @Input() frameName = "/images/speaker/speaker1.png"
  @Output() musicEvent = new EventEmitter();

  constructor(public authService: AuthService, public router: Router, private webSocketService : WebsocketService, private translocoService: TranslocoService,
    private translatorService: TranslatorService){}

  // Índice del idioma actualmente seleccionado.
  languageSelected: number = 0;
  languageSelect: string = "";
  languages: Language[] = [];
  selectedLangIndex: number = 0;
  isMenuOpen = false;

  user: User | null = null;

  public readonly IMG_URL = environment.apiImg;

  startMusic() {
    this.musicEvent.emit();
  }

  ngOnInit() {
    this.languages = this.translatorService.LANGUAGES;

    const activeLang = this.translocoService.getActiveLang();
    this.languageSelected =
      this.translatorService.findLanguageIndex(activeLang);

    // usuario logueado
    this.user = this.authService.getUser();
    console.log("ruta: ", this.router.url)
  }

  authClick() {
    // Cerrar sesión
    if (this.authService.isAuthenticated()) {

      this.authService.logout()
      this.webSocketService.disconnectNative()
      this.navigateToUrl("/")
      this.showAlert("Éxito", "Sesión cerrada correctamente", 'success')
      // Iniciar sesión
    } else {
      this.navigateToUrl("login");
    }
  }

  navigateToUrl(url: string)
  {
    this.router.navigateByUrl(url);
  }

  toggleMenu() {
    this.isMenuOpen = !this.isMenuOpen;
  }

  closeMenu() {
    this.isMenuOpen = false;
  }

  getFlagUrl(langCode: string): string {
    const map: Record<string, string> = {
      en: 'gb',
      es: 'es',
    };
    const countryCode = map[langCode] || langCode;
    return `https://flagcdn.com/w40/${countryCode}.png`;
  }

  @ViewChild('langSelector', { static: false })
  langSelector!: ElementRef<HTMLElement>;

  // Se llama cuando el usuario cambia el idioma desde la interfaz
  onLanguageChanged() {
    // Obtiene el idioma que el usuario ha seleccionado.
    const language = this.languages[this.languageSelected];
    // Cambia el idioma activo en Transloco al seleccionado.
    this.translocoService.setActiveLang(language.code);

    this.langSelector.nativeElement.removeAttribute('open');
  }

  private showAlert(title: string, message: string, icon: SweetAlertIcon) {
        Swal.fire({
          title: title,
          text: message,
          showConfirmButton: false,
          icon: icon,
          timer: 2000
        })
      }
}
