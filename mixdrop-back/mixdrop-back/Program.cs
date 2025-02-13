
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using mixdrop_back.Models.Mappers;
using mixdrop_back.Services;
using mixdrop_back.Sockets;
using System.Text;
using System.Text.Json.Serialization;

namespace mixdrop_back;

public class Program
{
    public static async Task Main(string[] args)
    {
        /* EL PROCESO SERÍA EL SIGUIENTE:
         * 1) Se obtiene el BPM de las canciones de la BBDD
         * 2) Se calcula cuánto tiene que subir y bajar (usando los cálculos de Rocío)
         * 3) Se obtiene la tonalidad de las canciones de la BBDD (que tienen que ser un valor del diccionario de arriba)
         * 4) Se calcula cuántos semitonos tiene que bajar
         * 5) Se pasan los nuevos valores de BPM y semitonos arriba o abajo
         * 6) Se mixea */
         
        Console.WriteLine("Procesando...");

        //HellIsForever.ChangeBPM("wwwroot/songs/input/vocal/vocal.wav", "output.wav", 1, 1.28f);

        // HellIsForever.ChangeBPM("songs/input/insane_music_loop.wav", "songs/input/insane.wav", 1.0428f);
        // HellIsForever.ChangeBPM("songs/input/rickroll_full_loop.mp3", "songs/input/rickroll.wav", 0.9606f);

        // HellIsForever.MixFiles("songs/input/insane.wav", "songs/input/rickroll.wav", "songs/output/mixed.wav");

        Console.WriteLine("Procesado");

        // AQUÍ EMPIEZA EL SERVIDOR COMO TAL
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.Configure<Settings>(builder.Configuration.GetSection("Settings"));
        builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<Settings>>().Value);

        // Inyectamos el DbContext
        builder.Services.AddScoped<MixDropContext>();
        builder.Services.AddScoped<UnitOfWork>();

        // Inyección de Mappers
        builder.Services.AddScoped<UserMapper>();

        // Inyección de Servicios
        builder.Services.AddScoped<UserService>();
        builder.Services.AddScoped<FriendshipService>();
        builder.Services.AddScoped<BattleService>();

        builder.Services.AddScoped<HellIsForever>(); // Habrá que cambiarle el nombre xD

        // Inyección del servicio de WebSocket como Singleton
        builder.Services.AddSingleton<WebSocketHandler>();
        builder.Services.AddSingleton<GayNetwork>();

        // Inyección de Middleware
        builder.Services.AddTransient<PreAuthMiddleware>();

        // Add services to the container.

        builder.Services.AddControllers();
        builder.Services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        });

        // CONFIGURANDO JWT
        builder.Services.AddAuthentication()
            .AddJwtBearer(options =>
            {
                string key = Environment.GetEnvironmentVariable("JwtKey");
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,

                    // INDICAMOS LA CLAVE
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
                };
            });

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Configuración de CORS
        /*builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAllOrigins", builder =>
            {
                builder.AllowAnyOrigin() // Permitir cualquier origen
                       .AllowAnyHeader()
                       .AllowAnyMethod();
            });
        });*/
        builder.Services.AddCors(
                options =>
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                        ;
                    })
                );


        builder.Services.AddScoped<MixDropContext>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Permite CORS
        app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

        // Para permitir web sockets
        // El protocolo de websockets no permite cabeceras, de manera que cuando en el front se pide abrir el websocket, se hace una petición que recibe el controlador pero a través de ese protocolo, de manera que no se puede incluir el JWT
        // Por tanto, tiene que haber un middleware que coja de la URL este JWT y lo ponga en la cabecera para que Authorize no bloquee el acceso
        app.UseWebSockets();

        // Se usa nuestro middleware :D (DEBE IR AQUÍ)
        app.UseMiddleware<PreAuthMiddleware>();

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.UseStaticFiles();

        await SeedDataBaseAsync(app.Services);

        app.Run();

        // metodo para el seeder
        static async Task SeedDataBaseAsync(IServiceProvider serviceProvider)
        {
            using IServiceScope scope = serviceProvider.CreateScope();
            using MixDropContext dbContext = scope.ServiceProvider.GetService<MixDropContext>();

            // Si no existe la base de datos, la creamos y ejecutamos el seeder
            if (dbContext.Database.EnsureCreated())
            {
                Seeder seeder = new Seeder(dbContext);
                await seeder.SeedAsync();
            }
        }
    }
}
