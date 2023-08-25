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
using MySql.Data.MySqlClient;
using System.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(opt => opt.AddDefaultPolicy(x => x.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
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
IDbConnection connection = new MySqlConnection(builder.Configuration.GetConnectionString("golden"));

builder.Services.AddScoped<IBusinessApplication>(x => new BusinessApplication(new PersistentApplication(connection)));
builder.Services.AddScoped<IBusinessRole>(x => new BusinessRole(new PersistentRole(connection)));
builder.Services.AddScoped<IBusinessUser>(x => new BusinessUser(new PersistentUser(connection)));
builder.Services.AddScoped<IBusiness<City>>(x => new BusinessCity(new PersistentCity(connection)));
builder.Services.AddScoped<IBusiness<Country>>(x => new BusinessCountry(new PersistentCountry(connection)));
builder.Services.AddScoped<IBusiness<IdentificationType>>(x => new BusinessIdentificationType(new PersistentIdentificationType(connection)));
builder.Services.AddScoped<IBusiness<IncomeType>>(x => new BusinessIncomeType(new PersistentIncomeType(connection)));
builder.Services.AddScoped<IBusiness<Office>>(x => new BusinessOffice(new PersistentOffice(connection)));
builder.Services.AddScoped<IBusiness<Parameter>>(x => new BusinessParameter(new PersistentParameter(connection)));
builder.Services.AddScoped<IBusiness<Plan>>(x => new BusinessPlan(new PersistentPlan(connection)));
builder.Services.AddScoped<IBusiness<Notification>>(x => new BusinessNotification(new PersistentNotification(connection)));
builder.Services.AddScoped<IBusiness<Template>>(x => new BusinessTemplate(new PersistentTemplate(connection)));
builder.Services.AddScoped<IPersistentBase<LogComponent>>(x => new PersistentLogComponent(connection));
builder.Services.AddSingleton<IAuthorizationHandler, DbAuthorizationHandler>(x => new DbAuthorizationHandler(new BusinessApplication(new PersistentApplication(connection))));

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseCors(x => x.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();