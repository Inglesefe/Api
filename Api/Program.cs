using Api.Authorization;
using Business;
using Business.Auth;
using Business.Config;
using Business.Noti;
using Dal;
using Dal.Auth;
using Dal.Config;
using Dal.Log;
using Dal.Noti;
using Entities.Config;
using Entities.Log;
using Entities.Noti;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(opt => opt.AddDefaultPolicy(x => x
    .WithOrigins("https://localhost", "https://192.168.1.37", "https://192.168.1.4", "https://192.168.1.7")
    .AllowAnyHeader()
    .AllowAnyMethod()));
builder.Services.AddControllers();
builder.Services
    .AddEndpointsApiExplorer()
    .AddAuthorization()
    .AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey
        });
    })
    .AddAuthorization(x => x.AddPolicy("db", p => { p.RequireAuthenticatedUser(); p.AddRequirements(new DbAuthorizationRequirement()); }))
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? ""))
        };
    }
    );
string connString = builder.Configuration.GetConnectionString("golden") ?? "";
builder.Services.AddScoped<IBusinessApplication>(x => new BusinessApplication(new PersistentApplication(connString)));
builder.Services.AddScoped<IBusinessRole>(x => new BusinessRole(new PersistentRole(connString)));
builder.Services.AddScoped<IBusinessUser>(x => new BusinessUser(new PersistentUser(connString)));
builder.Services.AddScoped<IBusiness<City>>(x => new BusinessCity(new PersistentCity(connString)));
builder.Services.AddScoped<IBusiness<Country>>(x => new BusinessCountry(new PersistentCountry(connString)));
builder.Services.AddScoped<IBusiness<IdentificationType>>(x => new BusinessIdentificationType(new PersistentIdentificationType(connString)));
builder.Services.AddScoped<IBusiness<IncomeType>>(x => new BusinessIncomeType(new PersistentIncomeType(connString)));
builder.Services.AddScoped<IBusiness<Office>>(x => new BusinessOffice(new PersistentOffice(connString)));
builder.Services.AddScoped<IBusiness<Parameter>>(x => new BusinessParameter(new PersistentParameter(connString)));
builder.Services.AddScoped<IBusiness<Plan>>(x => new BusinessPlan(new PersistentPlan(connString)));
builder.Services.AddScoped<IBusiness<Notification>>(x => new BusinessNotification(new PersistentNotification(connString)));
builder.Services.AddScoped<IBusiness<Template>>(x => new BusinessTemplate(new PersistentTemplate(connString)));
builder.Services.AddScoped<IPersistent<LogComponent>>(x => new PersistentLogComponent(connString));
builder.Services.AddSingleton<IAuthorizationHandler, DbAuthorizationHandler>(x => new DbAuthorizationHandler(new BusinessApplication(new PersistentApplication(connString))));

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();