using AutoMapper;
using DNBase.Common.Constants;
using DDTH.DataLayer.MongoProvider;
using DNBase.DataLayer.Dapper;
using DNBase.DataLayer.EF;
using DNBase.DataLayer.EF.Entities;
using DNBase.Services;
using DNBase.Services.Interfaces;
using DNBase.ViewModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentValidation.AspNetCore;
using System.Security.Claims;
using Elasticsearch.Net;
using Nest;
using DNBase.Common;

namespace DNBaseNetCore.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            services.AddMemoryCache();
            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString(AppsettingConstants.MAIN_CONNECT_STRING)));
            services.AddIdentity<AppUser, AppRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 4;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
            })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders()
                .AddUserManager<UserManager<AppUser>>()
                .AddRoleManager<RoleManager<AppRole>>();

            var key = Configuration.GetSection("JwtOptionsModel:Secret").Value;
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    Description = "JWT Authorization header using the Bearer scheme."
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new List<string>()
                    }
                });
            });

            services.AddAuthentication(options =>
            {
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddControllers();

            services.AddSwaggerGen();

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });
            services.AddCors(options =>
            {
                options.AddPolicy("KspSpecificOrigins",
                builder =>
                {
                    builder.WithOrigins(Configuration["AllowOrigins"])
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });
            var jwtOptions = Configuration.GetSection(nameof(JwtOptionsModel));
            services.Configure<JwtOptionsModel>(jwtOptions);
            var authConfig = Configuration.GetSection(nameof(AuthConfigModel));
            services.Configure<AuthConfigModel>(authConfig);
            var mongoDBConfig = Configuration.GetSection(nameof(MongoDBSetting));
            services.Configure<MongoDBSetting>(mongoDBConfig);

            ////Add Elastichsearch
            //var nodes = new Uri[] { new Uri(Configuration.GetSection("Elastichsearch")["URL"]) };
            //StaticConnectionPool connectionPool = new StaticConnectionPool(nodes);
            //ConnectionSettings connectionSettings = new ConnectionSettings(connectionPool).RequestTimeout(TimeSpan.FromSeconds(5));
            //ElasticClient elasticClient = new ElasticClient(connectionSettings);
            //services.AddSingleton(elasticClient);

            services.AddControllers();
            services.RegisterServices();

            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(ValidatorActionFilter));
            }).AddFluentValidation(fvc => fvc.RegisterValidatorsFromAssemblyContaining<Startup>());
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRequestResponseLogging();

            app.UseCors(x => x
                .SetIsOriginAllowed(origin => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
            });

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseDefaultFiles();
            app.UseStaticFiles();
            using var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope();
            var context = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Database.Migrate();

            try
            {
                var dbInitializer = serviceScope.ServiceProvider.GetRequiredService<QueryMigrationInitilize>();
                dbInitializer.Seed();
            }
            catch (Exception ex)
            {
                var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }
    }

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services)
        {
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new RequestMappingProfile());
                mc.AddProfile(new ResponeMappingProfile());
            });

            IMapper mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);

            services.AddSingleton<IElasticSearchClient, ElasticSearchClient>();
            services.AddSingleton<IRedisClient, RedisClient>();

            services.AddSingleton<IDapper, DapperProvider>();
            services.AddSingleton<IMongo, MongoProvider>();
            services.AddTransient<IGenericRepository, GenericRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork<AppDbContext>>();
            services.AddScoped<ICurrentPrincipal, CurrentPrincipal>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddTransient<QueryMigrationInitilize>();
            services.AddTransient<UserManager<AppUser>, UserManager<AppUser>>();
            services.AddTransient<SignInManager<AppUser>, SignInManager<AppUser>>();
            services.AddTransient<RoleManager<AppRole>, RoleManager<AppRole>>();

            services.AddScoped<IJwtHandler, JwtHandler>();
            services.AddScoped<IAuthLDValidator, AuthLDValidator>();
            services.AddScoped<IAuthService, AuthService<AppUser>>();
            services.AddScoped<ISystemService, SystemService>();
            services.AddScoped<IQT_UserService, QT_UserService<AppUser>>();
            services.AddScoped<IQT_RoleService, QT_RoleService<AppRole>>();
            services.AddScoped<IQT_QuyenService, QT_QuyenService>();
            services.AddScoped<IQT_ChucNangService, QT_ChucNangService>();

            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IFilesService, FilesService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ICRUDService, CRUDService>();
            services.AddScoped<ICRUD_MGService, CRUD_MGService>();

            return services;
        }
    }

    public class QueryMigrationInitilize
    {
        private readonly AppDbContext _context;

        public QueryMigrationInitilize(AppDbContext context)
        {
            _context = context;
        }

        public void Seed()
        {
            var queryIndb = _context.QueryMigrations.Select(x => x.Name);
            DirectoryInfo d = new DirectoryInfo(System.IO.Directory.GetCurrentDirectory() + @"\Queries"); //Assuming Test is your Folder

            FileInfo[] files = d.GetFiles("*.sql"); //Getting sql files
            if (queryIndb.Any())
            {
                files = files.Where(f => !queryIndb.Contains(f.Name.Replace(".sql", string.Empty))).ToArray();
            }

            foreach (FileInfo file in files)
            {
                var query = System.IO.File.ReadAllText(file.FullName);
                var cnn = (SqlConnection)_context.Database.GetDbConnection();
                if (cnn.State == ConnectionState.Closed)
                    cnn.Open();
                using (var cmd = new SqlCommand(query, cnn))
                {
                    cmd.ExecuteNonQuery();
                    _context.QueryMigrations.Add(new QueryMigration
                    {
                        Name = file.Name.Replace(".sql", string.Empty)
                    });
                    _context.SaveChanges();
                }
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class APIAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity.IsAuthenticated)
            {
                // it isn't needed to set unauthorized result 
                // as the base class already requires the user to be authenticated
                // this also makes redirect to a login page work properly
                // context.Result = new UnauthorizedResult();
                return;
            }


            if (user == null)
            {
                context.Result = new StatusCodeResult((int)System.Net.HttpStatusCode.Forbidden);
                return;
            }
        }
    }

    //public class ClaimRequirementFilter : IAuthorizationFilter
    //{
    //    public ClaimRequirementFilter()
    //    {
    //    }

    //    public void OnAuthorization(AuthorizationFilterContext context)
    //    {
    //        var user = context.HttpContext.User;
    //        if (user == null)
    //        {
    //            context.Result = new ForbidResult();
    //        }
    //    }
    //}

    //public class ClaimRequirementAttribute : TypeFilterAttribute
    //{
    //    public ClaimRequirementAttribute() : base(typeof(ClaimRequirementFilter))
    //    {

    //    }
    //}

    public class ValidatorActionFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.ModelState.IsValid)
            {
                filterContext.Result = new BadRequestObjectResult(filterContext.ModelState);
            }
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
        }
    }

    //Middleware ghi log request

    public static class RequestResponseLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestResponseLoggingMiddleware>();
        }
    }

    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestResponseLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, AppDbContext db)
        {
            var requestPath = context.Request.Host + context.Request.Path;
            if (requestPath.Contains("/api/") && !requestPath.Contains("api/systems"))
            {
                Stream originalBody = context.Response.Body;
                var requestBody = "";
                var responseBody = "";

                context.Request.EnableBuffering();

                // Leave the body open so the next middleware can read it.
                using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, false, leaveOpen: true))
                {
                    requestBody = await reader.ReadToEndAsync();
                    // Do some processing with body…
                    // Reset the request body stream position so the next middleware can read it
                    context.Request.Body.Position = 0;
                }

                try
                {
                    await using var memStream = new MemoryStream();
                    context.Response.Body = memStream;

                    await _next(context);
                    memStream.Position = 0;
                    responseBody = new StreamReader(memStream).ReadToEnd();

                    memStream.Position = 0;
                    await memStream.CopyToAsync(originalBody);
                }
                finally
                {
                    context.Response.Body = originalBody;
                }

                var queryPath2 = context.Request.QueryString.ToString();
                if (requestBody.IsNullOrEmpty())
                {
                    requestBody = queryPath2;
                }

                if (!responseBody.StartsWith("{"))
                {
                    responseBody = "";
                }
                var userId = GetUserIdId(context);
                var userName = context.User?.Identity?.Name;
                db.AppLog.Add(new AppLog() { Id = Guid.NewGuid(), RequestBody = requestBody, ResponseBody = responseBody, UserId = userId, UserName = userName, RequestPath = requestPath });
                await db.SaveChangesAsync();
            }
            else
            {
                await _next(context);
            }
        }

        private Guid GetUserIdId(HttpContext httpContext)
        {
            var identifier = httpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return identifier.IsNullOrEmpty() ? Guid.Empty : new Guid(identifier);
        }
    }
}
