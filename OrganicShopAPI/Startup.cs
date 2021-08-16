using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OpenApi.Models;

using OrganicShopAPI.DataAccess;
using OrganicShopAPI.Models;

namespace OrganicShopAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<OrganicShopDbContext>(options => 
            options.UseSqlServer(Configuration["ConnectionStrings:DefaultConnection"]));

            //to make sure all dependency injection get same context value
            services.AddScoped<OrganicShopDbContext>();

            //add dependency injection for repository interface
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            //enable CORS(Cross-Origin requests)
            services.AddCors();

            //Configure Odtata options
            services.AddControllers().AddOData(opt => opt.Filter().Expand().Select().OrderBy()
                                     .AddRouteComponents("odata",GetEdmModel()));

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "OrganicShopAPI", Version = "v1" });

                //for ambiguity exception related to Odata
                c.DocInclusionPredicate((name, api) => api.HttpMethod != null);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "OrganicShopAPI v1"));

            }

            //enable CORS(Cross-Origin requests)
            app.UseCors(builder =>builder.AllowAnyOrigin()
                                         .AllowAnyMethod()
                                         .AllowAnyHeader());
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        //configure controllers adnd Entity sets for Odata
        private static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Category>("Categories");
            builder.EntitySet<Product>("Products");
            builder.EntitySet<AppUser>("Users");
            return builder.GetEdmModel();
        }
    }
}
