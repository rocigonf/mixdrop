import { Component } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { NavbarComponent } from "../../components/navbar/navbar.component";
import { WebsocketService } from '../../services/websocket.service';
import { NgIf } from '@angular/common';
import { PasswordValidatorService } from '../../services/password-validator.service';
import Swal, { SweetAlertIcon } from 'sweetalert2';



@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, RouterModule, NavbarComponent, ReactiveFormsModule, NgIf],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {

  menuSeleccion: 'login' | 'register' = 'login';

  emailOrNickname: string = '';
  password: string = '';
  rememberMe: boolean = false;
  jwt: string = '';

  registerForm: FormGroup;

  image: File | null = null

  imgSeleccionada: Boolean = false;

  imagePreview!: string;

  pressedEnter : Boolean = false;

  constructor(
    private formBuilder: FormBuilder,
    private router: Router,
    private authService: AuthService,
    private webSocketService: WebsocketService,
    private passwordValidator: PasswordValidatorService
  ) {
    this.registerForm = this.formBuilder.group({
      nickname: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required]
    },
      { validators: this.passwordValidator.passwordMatchValidator });
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

  async login() {
    if(this.pressedEnter) return;
    this.pressedEnter = true;

    const authData = { emailOrNickname: this.emailOrNickname, password: this.password };
    const result = await this.authService.login(authData, this.rememberMe);

    if (result.success) {

      if (result.data) {
        this.jwt = result.data.accessToken;
      } else {
        console.error('No se encontró información en result.data');
      }

      if (this.rememberMe) {
        localStorage.setItem('jwtToken', this.jwt);
      }

      this.showAlert("Éxito", "Inicio de sesión exitoso", 'success')

      this.router.navigateByUrl("menu"); // redirige a inicio

      this.webSocketService.connectNative()
    } else {
      this.showAlert("Error", "Datos incorrectos o baneo existente", 'error')
      this.pressedEnter = false

    }
  }


  // Registro
  async register() {
    if(this.pressedEnter) return;
    console.log(this.registerForm.value)
    this.pressedEnter = true

    if (this.registerForm.valid) {

      const formData = new FormData();
      formData.append("Nickname", this.registerForm.value.nickname)
      if (this.image) {
        formData.append("Image", this.image, this.image.name)
      }
      formData.append("Email", this.registerForm.value.email)
      formData.append("Password", this.registerForm.value.newPassword)

      const registerResult = await this.authService.register(formData);

      if (registerResult.success) {

        const formUser = this.registerForm.value;

        const authData = { emailOrNickname: formUser.email, password: formUser.newPassword };
        const loginResult = await this.authService.login(authData, false);

        if (loginResult.success) {
          this.showAlert("Éxito", "Te has registrado con éxito", 'success')
          this.router.navigateByUrl("menu"); // redirige a inicio

          this.webSocketService.connectNative()

        } else {
          this.showAlert("Error", "Error en el registro", 'error')
          this.pressedEnter = false
        }

      } else {
        this.showAlert("Error", "Error en el registro", 'error')
        this.pressedEnter = false;
      }

    } else {
      this.showAlert("Error", "Formulario no válido", 'error')
      this.pressedEnter = false;
    }
  }

  onFileSelected(event: any) {
    const image = event.target.files[0] as File;
    this.image = image

    this.imgSeleccionada = true;

    if (event.target.files.length > 0) {
      const reader = new FileReader();
      reader.onload = (event: any) => {
        this.imagePreview = event.target.result;
      }
      reader.readAsDataURL(event.target.files[0])
    }
  }

}

