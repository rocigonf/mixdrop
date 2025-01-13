
using Microsoft.IdentityModel.Tokens;
using mixdrop_back.Mappers;
using mixdrop_back.Repositories;
using mixdrop_back.Services;
using System.Text;
using System.Text.Json.Serialization;

namespace mixdrop_back;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Estas 3 variables habría que ponerlas en otro lado
        var semitone = Math.Pow(2, 1.0 / 12);
        var upOneTone = semitone * semitone;
        var downOneTone = 1.0 / upOneTone;

        Console.WriteLine("Procesando...");
        HellIsForever.ChangeBPM("music.wav", "output.wav", 2.0f);
        Console.WriteLine("Procesado");

        var builder = WebApplication.CreateBuilder(args);

        // Inyectamos el DbContext
        builder.Services.AddScoped<MixDropContext>();
        builder.Services.AddScoped<UnitOfWork>();

        // Inyección de todos los repositorios
        builder.Services.AddScoped<UserRepository>();

        // Inyección de Mappers
        builder.Services.AddScoped<UserMapper>();

        // Inyección de Servicios
        builder.Services.AddScoped<UserService>();

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
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAllOrigins", builder =>
            {
                builder.AllowAnyOrigin() // Permitir cualquier origen
                       .AllowAnyHeader()
                       .AllowAnyMethod();
            });
        });


        builder.Services.AddScoped<MixDropContext>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        // Permite CORS
        app.UseCors("AllowAllOrigins");

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.UseStaticFiles();

        await SeedDataBaseAsync(app.Services);

        using (IServiceScope scope = app.Services.CreateScope())
        {
            MixDropContext dbContext = scope.ServiceProvider.GetService<MixDropContext>();
            dbContext.Database.EnsureCreated();
        }

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
