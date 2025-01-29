<app-navbar></app-navbar>

<div class="cabecera">
    <img [src]="IMG_URL + user?.avatarPath" alt="avatar de usuario">
    <span style="font-size: 2rem;">{{user?.nickname}}</span> <!-- nickname del usuario-->
</div>

<!--mostrar tamb avatar de invitado / anfitrion-->

<p>si es anitrion</p>
<button (click)="gameWithBot()">Jugar contra un bot</button>
<button (click)="gameRandom()">Jugar con oponente aleatorio</button>

<!-- q te abra ventana model con amigos conectados ? -->
<p>Invitar amigo a jugar</p>
<div class="listaUsuarios"></div>
@if (conenctedFriends.length < 1) { <span> No hay amigos conectados </span>
    } @else {
    @for (friend of conenctedFriends; track $index) {


        <div class="tarjetaUsuario">
            <div class="contenedorAvatar">
                <img class="avatar" [src]="IMG_URL + friend.avatarPath" alt="usuario avatar" width="50px">
            </div>
    
            <span class="nickname">
                {{ friend.nickname }}
            </span>

            <div class="contenedorIcono">
                <img class="icono" (click)="gameWithFriend(friend)" src="/images/addFriend.webp"
                    alt="invitar a jugar a amigo">
            </div>

        </div>

        <span class="nickname">
            {{ friend.nickname }}
        </span>

        <div class="contenedorIcono">
            <img class="icono" (click)="gameWithFriend(friend)" src="/images/battle.png" alt="invitar a jugar a amigo">
        </div>
    </div>
    }
    }
    console.log("Respuesta del socket en JSON: ", jsonResponse)
  }

  askForInfo(messageType: MessageType) {
    console.log("Mensaje pedido: ", messageType)
    this.webSocketService.sendRxjs(messageType.toString())
  }


  gameWithBot() {
    // el user 2 es nulo y el false de que no es random
    this.battleService.createBattle(null, false)
  }

  gameWithFriend(friend: User) {
    this.battleService.createBattle(friend, false)
  }

  gameRandom() {
    this.battleService.randomBattle()
    console.log("mortadela");
  }

  // comprueba q el usuario ya tiene una solicitud de batalla pendiente con otro
  hasBattle(user: User | null): boolean {
    const has: boolean = this.pendingBattles.some(battle =>
      (battle.user.id === user?.id) || (battle.user.id === this.user?.id)
      || (battle.battleUsers[0].id === this.user?.id)  || (battle.battleUsers[1].id === this.user?.id) 
    );
    return has;
  }


  processFriends() {

    this.myFriends = []
    this.conenctedFriends = []

    console.log(this.friendsRaw)

    for (const friend of this.friendsRaw) {
      if (friend.accepted) {
        this.myFriends.push(friend)

        if (friend.senderUser?.stateId == 2 || friend.receiverUser?.stateId == 2) {

          if (friend.senderUser) this.conenctedFriends.push(friend.senderUser)
          else if (friend.receiverUser) this.conenctedFriends.push(friend.receiverUser)
        }
      }
    }
    console.log("amigos: ", this.myFriends)
  }
}
