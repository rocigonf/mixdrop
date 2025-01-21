import { Component, OnInit } from '@angular/core';
import { User } from '../../models/user';
import { ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs';
import { UserService } from '../../services/user.service';
import { environment } from '../../../environments/environment';
import { NavbarComponent } from "../../components/navbar/navbar.component";
import { PasswordValidatorService } from '../../services/password-validator.service';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [NavbarComponent, CommonModule, ReactiveFormsModule],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit {

  id : number = 0;
  userForm: FormGroup;
  passwordForm: FormGroup;
  isNewPasswordHidden = true // Mostrar div de cambiar contraseña

  user: User | null = null
  isItself = false
  routeParamMap$: Subscription | null = null;
  public readonly IMG_URL = environment.apiImg;

  image: File | null = null
  imagePreview!: string;

  isEditing = false; //modo edición
  deleteAvatar = false;
  
  constructor(
    private activatedRoute: ActivatedRoute, 
    private userService: UserService, 
    private passwordValidator : PasswordValidatorService,
    private formBuild: FormBuilder,
    private authService: AuthService
  )
  {
    this.userForm = this.formBuild.group({
      nickname: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]]
    });

    this.passwordForm = this.formBuild.group({
      newPassword: ['', [Validators.required, Validators.minLength(1)]],
      confirmPassword: ['', Validators.required]
    },
      { validators: this.passwordValidator.passwordMatchValidator });
  }

  async ngOnInit(): Promise<void> {
    this.routeParamMap$ = this.activatedRoute.paramMap.subscribe(async paramMap => {
      this.id = paramMap.get('id') as unknown as number;
      await this.getUser()
      this.userForm.reset(this.user)
    })
  }

  async getUser()
  {
    const result = await this.userService.getUserById(this.id)
      console.log("Resultado de pedir el perfil ", this.id, ": ", result)

      if (result != null) {
        this.user = result

        // Pillo el id del JWT como en el ECommerce y si coincide con el usuario que he pedido, intenta acceder a sí mismo
        const jwt = this.userService.api.jwt
        if(jwt)
        {
          const id = JSON.parse(window.atob(jwt.split('.')[1])).nameid;
          if(id == this.user?.id)
          {
            this.isItself = true
          }
          //console.log(id)
        }
      }
  }

  onFileSelected(event: any) {
    const image = event.target.files[0] as File;
    this.image = image

    if(event.target.files.length > 0){
      const reader = new FileReader();
      reader.onload = (event:any) => {
        this.imagePreview = event.target.result;
      }
      reader.readAsDataURL(event.target.files[0])
    }
  }
  
  // TODO: Agregar verificación
  async updateUser() : Promise<void>
  {
    const role = this.user?.role.toString();
    const formData = new FormData();
    formData.append( "Nickname" ,this.userForm.value.nickname )

    if(this.image)
    {
      formData.append( "Image" ,this.image, this.image.name )
      formData.append( "ChangeImage", "true")
    }
    else if(this.deleteAvatar)
    {
      formData.append("ChangeImage", "true")
    }
    
    formData.append( "Email" ,this.userForm.value.email )
    formData.append( "Password" , this.passwordForm.get('newPassword')?.value )
    
    if(role)formData.append( "Role" , role )

    const result = await this.userService.updateUser(formData, this.id)
    console.error(result)
    this.authService.saveUser(result.data)
    window.location.reload()
  }

  edit() {
    this.isEditing = !this.isEditing;
    if (!this.isEditing) { // restaura los datos
      this.userForm.reset(this.user);
    }
  }

  editPassword() {
    const newPassword = this.passwordForm.get('newPassword')?.value;

      if (!newPassword) {
        console.error("Error: El campo de la contraseña está vacío.");
        
        return;
      }
      
      this.showEditPassword()
    /*if (this.passwordForm.valid) {
      
    } else{
      console.error("Error: El formulario de contraseña no es válido.")
    }*/
  }

  showEditPassword() {
    let element = document.getElementById("newPassword");
    let hidden = element?.getAttribute("hidden");

    if (hidden) {
      element?.removeAttribute("hidden");
    } else {
      element?.setAttribute("hidden", "hidden");
    }
  }

  // envia cambios para mofidicar el usuario
  onSubmit(): void {
    if (this.userForm.valid) {
      this.isEditing = false
      // ACTUALIZAR

    } else{
      alert("Los datos introducidos no son válidos")
    }
  }
}
