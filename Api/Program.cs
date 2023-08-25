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
builder.Services.AddScoped<IDbConnection>(x => new MySqlConnection(builder.Configuration.GetConnectionString("golden")));
builder.Services.AddScoped<IBusinessApplication>(x => new BusinessApplication(new PersistentApplication()));
builder.Services.AddScoped<IBusinessRole>(x => new BusinessRole(new PersistentRole()));
builder.Services.AddScoped<IBusinessUser>(x => new BusinessUser(new PersistentUser()));
builder.Services.AddScoped<IBusiness<City>>(x => new BusinessCity(new PersistentCity()));
builder.Services.AddScoped<IBusiness<Country>>(x => new BusinessCountry(new PersistentCountry()));
builder.Services.AddScoped<IBusiness<IdentificationType>>(x => new BusinessIdentificationType(new PersistentIdentificationType()));
builder.Services.AddScoped<IBusiness<IncomeType>>(x => new BusinessIncomeType(new PersistentIncomeType()));
builder.Services.AddScoped<IBusiness<Office>>(x => new BusinessOffice(new PersistentOffice()));
builder.Services.AddScoped<IBusiness<Parameter>>(x => new BusinessParameter(new PersistentParameter()));
builder.Services.AddScoped<IBusiness<Plan>>(x => new BusinessPlan(new PersistentPlan()));
builder.Services.AddScoped<IBusiness<Notification>>(x => new BusinessNotification(new PersistentNotification()));
builder.Services.AddScoped<IBusiness<Template>>(x => new BusinessTemplate(new PersistentTemplate()));
builder.Services.AddScoped<IPersistentBase<LogComponent>>(x => new PersistentLogComponent());
builder.Services.AddSingleton<IAuthorizationHandler, DbAuthorizationHandler>(x => new DbAuthorizationHandler(builder.Configuration));

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseCors(x => x.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();