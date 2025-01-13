import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { NavbarComponent } from "../../components/navbar/navbar.component";


@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, RouterModule, NavbarComponent, ReactiveFormsModule],
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


  constructor(
    private formBuilder: FormBuilder,
    private router: Router,
    private authService: AuthService,
  ) {
    this.registerForm = this.formBuilder.group({
      nickname: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      image: ['', Validators.required],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required]
    },
      { validators: this.passwordMatchValidator });
  }

  // Validator de contraseña
  passwordMatchValidator(form: FormGroup) {
    const password = form.get('password')?.value;
    const confirmPasswordControl = form.get('confirmPassword');
    const confirmPassword = confirmPasswordControl?.value;

    if (password !== confirmPassword && confirmPasswordControl) {
      confirmPasswordControl.setErrors({ mismatch: true });
    } else if (confirmPasswordControl) {
      confirmPasswordControl.setErrors(null);
    }
  }


  async login() {
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

      alert("inicio sesion exitoso");

      this.router.navigateByUrl("/"); // redirige a inicio
    } else {
      alert("error al iniciar sesion")

    }
  }


  // Registro
  async register() {
    if (this.registerForm.valid) {
      const formData = this.registerForm.value;
      const registerResult = await this.authService.register(formData);

      if (registerResult.success) {

        const authData = { emailOrNickname: formData.email, password: formData.password };
        const loginResult = await this.authService.login(authData, false);

        if (loginResult.success) {

          const user = this.authService.getUser();
          const nickname = user ? user.nickname : null;

          alert("Te has registrado con éxito.")
          this.router.navigateByUrl("/"); // redirige a inicio

        } else {
          alert("Error en el inicio de sesión");
        }

      } else {
        alert("Error en el registro");
      }

    } else {
      alert("Formulario no válido");
    }
  }

  onFileSelected(event: any) {
    const image = event.target.files[0] as File;
    if (image) {
      console.log("NUEVA IMAGEN")
      this.image = image
    }
    else {
      console.log("NO HAY IMAGEN")
    }
  }

}

