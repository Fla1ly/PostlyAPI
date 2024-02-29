using Microsoft.OpenApi.Models;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace c_ApiLayout
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddHttpClient();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Swagger UI", Version = "v1" });
            });

            services.AddSingleton<IMongoClient>(serviceProvider =>
            {
                var settings = Configuration.GetSection("MDB_Settings");
                var connectionString = settings["apiUrl"];
                return new MongoClient(connectionString);
            });

            services.AddCors(options =>
            {
                options.AddPolicy("AllowLocalhost", builder =>
                {
                    builder.WithOrigins("http://localhost:5000") 
                           .AllowAnyHeader()
                           .AllowAnyMethod();
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:5173"));

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "C# API Layout"));

            app.UseForwardedHeaders();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}