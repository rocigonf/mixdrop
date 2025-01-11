namespace mixdrop_back
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Estas 3 variables habrá que ponerlas en otro lado
            var semitone = Math.Pow(2, 1.0 / 12);
            var upOneTone = semitone * semitone;
            var downOneTone = 1.0 / upOneTone;

            Console.WriteLine("Procesando...");
            HellIsForever.ChangeBPM("music.wav", "output.wav", 2.0f);
            Console.WriteLine("Procesado");

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();

            builder.Services.AddScoped<MixDropContext>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapRazorPages();

            using (IServiceScope scope = app.Services.CreateScope())
            {
                MixDropContext dbContext = scope.ServiceProvider.GetService<MixDropContext>();
                dbContext.Database.EnsureCreated();
            }

            app.Run();
        }
    }
}
