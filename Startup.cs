using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using VSign.DbContexts;
using VSign.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Newtonsoft.Json;

namespace VSign
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
            services.AddCors();

            IConfiguration JWTSecurityConfig = Configuration.GetSection("JWTSecurity");
            string securityKey = JWTSecurityConfig.GetValue<string>("securityKey");
            string issuer = JWTSecurityConfig.GetValue<string>("issuer");
            string audience = JWTSecurityConfig.GetValue<string>("audience");

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        //what to validate
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                        //setup validate data
                        ValidIssuer = issuer,
                        ValidAudience = audience,
                        IssuerSigningKey = symmetricSecurityKey
                    };
                });
            services.AddSingleton<IConfiguration>(Configuration);
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });
            services.AddDbContext<AuthContext>(opts => opts.UseSqlServer(Configuration.GetConnectionString("AuthContext")));
            services.AddTransient<IMasterRepository, MasterRepository>();
            services.AddTransient<IAuthRepository, AuthRepository>();
            services.AddControllers();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseCors(a => a.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
           // app.UseMvc();

        }
    }

}
