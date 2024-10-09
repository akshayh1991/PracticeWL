using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SecMan.BL;
using SecMan.Data.Config;
using SecMan.Interfaces.BL;
using SecMan.Interfaces.DAL;
using SecMan.Model;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;
using System.Text;
using UserAccessManagement.Filters;
using UserAccessManagement.Middleware;
using SecMan.Data.Config;
using Microsoft.EntityFrameworkCore;
using SecMan.Data.SQLCipher;
using SecMan.Data.DAL;
using SecMan.Data.Repository;
using SecMan.Data;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration) // Read settings from appsettings.json
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

// Add Serilog to the logging pipeline
builder.Host.UseSerilog();


builder.Services.AddDbContext<Db>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));

#if DEBUG
    options.UseLoggerFactory(LoggerFactory.Create(builder => builder.AddDebug()));
    options.EnableSensitiveDataLogging(); // only for debugging builds
#endif
});

_ = new SecManDb();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

builder.Services.ConfigureDALServices(builder.Configuration);

builder.Services.Configure<JwtTokenOptions>(builder.Configuration.GetSection(JwtTokenOptions.JWTTokenValue));

builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers(options =>
{
    options.Filters.Add(new AddRequestUrlToResponseFilter());
    options.Filters.Add(new ModelValidationActionFilter());
});
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("1.0.0", new OpenApiInfo
    {
        Title = "User Access Manager API",
        Version = "1.0.0",
        Description = "Watlow NextGen EPM Suite will onboard its first default application User Access Manager (UAM). UAM will manage Zones, Users, Roles, Devices and System Policies."
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please insert JWT with Bearer into field",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
});


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration.GetValue<string>("JWT:ValidIssuer"),
        ValidAudience = builder.Configuration.GetValue<string>("JWT:ValidAudience"),
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration?.GetValue<string>("JWT:SecretKey") ?? string.Empty)),
    };
    options.Events = new JwtBearerEvents
    {
        OnChallenge = async context =>
        {
            JwtBearerEventsMiddleware middleware = context.HttpContext.RequestServices.GetRequiredService<JwtBearerEventsMiddleware>();
            await middleware.OnChallenge(context);
        }
    };
});



builder.Services.AddScoped<IRoleBL, RoleBL>();
builder.Services.AddScoped<IRoleDal, SecMan.Data.Role>();

builder.Services.AddScoped<IUserBL, UserBL>();

builder.Services.AddScoped<IPasswordBl, PasswordBL>();
builder.Services.AddScoped<IPasswordRepository, PasswordRepository>();

builder.Services.AddScoped<IAuthBL, UserBL>();
builder.Services.AddScoped<ICurrentUserServices, CurrentUserServices>();
builder.Services.AddScoped<AddRequestUrlToResponseFilter>();
builder.Services.AddScoped<ModelValidationActionFilter>();
builder.Services.AddScoped<JwtBearerEventsMiddleware>();
builder.Services.AddScoped<IEncryptionDecryption, EncryptionDecryption>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddSwaggerGen(c =>
{
    string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});
WebApplication app = builder.Build();
app.UseMiddleware<ExceptionHandlingMiddleware>();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/1.0.0/swagger.json", "User Access Manager API 1.0.0");
        c.DocExpansion(DocExpansion.List);
        c.DefaultModelsExpandDepth(0);
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.UseCors("AllowAll");

await app.RunAsync();
