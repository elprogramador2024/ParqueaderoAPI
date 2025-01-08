using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ParqueaderoAPI.Models;
using ParqueaderoAPI.Services;
using ParqueaderoAPI.Services.Comunes;
using ParqueaderoAPI.Utility;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();


//Documentacion Swagger
var info = new OpenApiInfo()
{
    Title = "ParqueaderoAPI",
    Version = "v1",
    Description = "API Rest para el control de vehículos por parqueaderos.",
    Contact = new OpenApiContact()
    {
        Name = "Jonathan Macias",
        Email = "jemacias464@gmail.com",
    }

};

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", info);

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

//servicios
var configuration = builder.Configuration;

//conexion base de datos
builder.Services.AddDbContext<ApplicationDbContext>(options => options
    .UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

//servicios identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 1;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddPasswordValidator<PasswordValidator>();

//Authentication
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
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = configuration["Jwt:Issuer"],
        ValidAudience = configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
    };
});

//Servicios de BlackList Token
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ITokenBlackListService, TokenBlackListService>();

//API Cliente
builder.Services.AddHttpClient<ApiClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//Implementacion/Validacion de BlackList Token
app.UseMiddleware<TokenValidationMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

//Inicializamos Servicio para crear Roles y Usuario admin por defecto
await Seed.Inicializar(app.Services.CreateScope());

app.Run();
