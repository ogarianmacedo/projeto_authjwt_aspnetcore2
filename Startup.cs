using AuthJWT_ASPNETCore2.Database;
using AuthJWT_ASPNETCore2.Helpers.Swagger;
using AuthJWT_ASPNETCore2.Models;
using AuthJWT_ASPNETCore2.V1.Interfaces;
using AuthJWT_ASPNETCore2.V1.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthJWT_ASPNETCore2
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
            // Configuração para solicitar autorização nas rotas
            var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
            
            services.AddMvc(options => { options.Filters.Add(new AuthorizeFilter(policy)); })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddJsonOptions(
                    options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                );

            // Configuração para conexão com o banco de dados
            services.AddDbContext<Context>(
                config => config.UseSqlite(Configuration.GetConnectionString("DefaultConnection"))
            );

            // Configura o Identity
            services.AddIdentity<ApplicationUser, IdentityRole>(options => {
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
            })
            .AddEntityFrameworkStores<Context>()
            .AddDefaultTokenProviders();

            // Configura autenticação com o JWT
            services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options => {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("web-api-jwt-tasks"))
                };
            });

            services.AddAuthorization(auth => {
                auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
                                            .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                                            .RequireAuthenticatedUser()
                                            .Build()
                );
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = 401;
                    return Task.CompletedTask;
                };
            });

            // Configura a validação do ModelState
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            // Configura serviço de versionamento
            services.AddApiVersioning(config => {
                config.ReportApiVersions = true;

                config.AssumeDefaultVersionWhenUnspecified = true;
                config.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
            });

            // Configura o Swagger
            services.AddSwaggerGen(config =>
            {
                config.AddSecurityDefinition("Bearer", new ApiKeyScheme()
                {
                    In = "header",
                    Type = "apiKey",
                    Description = "Adicione o JWT para autenticar.",
                    Name = "Authorization"
                });

                var security = new Dictionary<string, IEnumerable<string>>()
                {
                    { "Bearer", new string[] { } }
                };
                config.AddSecurityRequirement(security);

                // Cria versionamento para o Swagger
                config.SwaggerDoc("v1.0", new Swashbuckle.AspNetCore.Swagger.Info()
                {
                    Title = "AuthJWT - V1.0",
                    Version = "v1.0"
                });

                config.DocInclusionPredicate((docName, apiDesc) =>
                {
                    var actionApiVersionModel = apiDesc.ActionDescriptor?.GetApiVersion();
                    // would mean this action is unversioned and should be included everywhere
                    if (actionApiVersionModel == null)
                    {
                        return true;
                    }
                    if (actionApiVersionModel.DeclaredApiVersions.Any())
                    {
                        return actionApiVersionModel.DeclaredApiVersions.Any(v => $"v{v.ToString()}" == docName);
                    }
                    return actionApiVersionModel.ImplementedApiVersions.Any(v => $"v{v.ToString()}" == docName);
                });

                config.OperationFilter<ApiVersionOperationFilter>();

                // Se houver conflito de rotas
                config.ResolveConflictingActions(apiDescription => apiDescription.First());
            });

            // Configura Interface Repository
            services.AddScoped<IUsuario, UsuarioRepository>();
            services.AddScoped<IToken, TokenRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStatusCodePages();
            app.UseAuthentication();

            app.UseHttpsRedirection();
            app.UseMvc();


            // Configuração do Swagger
            app.UseSwagger();

            // Configura inteface gráfica do Swagger
            app.UseSwaggerUI(config => {
                config.SwaggerEndpoint("/swagger/v1.0/swagger.json", "AuthJWT - V1.0");

                config.RoutePrefix = String.Empty;
            });
        }
    }
}
