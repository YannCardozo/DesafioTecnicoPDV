using InfraData.Context;
using InfraData.DAO;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);




//EF SQL SERVER
var connectionString = builder.Configuration.GetConnectionString("Default")!;
builder.Services.AddDbContext<Contexto>(opt => opt.UseSqlServer(connectionString));

//IDENTITY
builder.Services
    .AddIdentity<Usuario, IdentityRole<int>>(opts =>
    {
        opts.Password.RequireDigit = true;
        opts.Password.RequireUppercase = true;
        opts.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<Contexto>()
    .AddDefaultTokenProviders();


//dados do appsettings.json
var jwtSettings = builder.Configuration.GetSection("JWT");
var key = Encoding.UTF8.GetBytes(jwtSettings["Secret"]!);
//JWT
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["ValidIssuer"],
            ValidAudience = jwtSettings["ValidAudience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero
        };
    });


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

//Swagger
const string ApiTitle = "PdvNet – API V1";
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = ApiTitle,
        Version = "v1",
        Description = "Documentação gerada pelo Swagger"
    });

    // esquema Bearer
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Digite: TOKEN DO LOGIN",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    // aplica a todas as rotas
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

//cors
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin()
     .AllowAnyMethod()
     .AllowAnyHeader()));


var app = builder.Build();


//tanto para DESENVOLVIMENTO quanto para PRODUÇÂO.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", ApiTitle);
    c.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
