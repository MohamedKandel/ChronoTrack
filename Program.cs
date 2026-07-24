using System.Text;
using Encryption;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

void registerDbContext<T>() where T : DbContext
{
    builder.Services.AddDbContext<T>(options =>
        options.UseSqlServer(EncryptionUtils.decrypt(builder.Configuration.GetConnectionString("DefaultConnection"),"Maa%QS7Ejx5k43h3")
            ));

        Console.WriteLine(EncryptionUtils.decrypt(builder.Configuration.GetConnectionString("DefaultConnection"),"Maa%QS7Ejx5k43h3"));
}

// Add services to the container.
builder.Services.AddControllers();

// register DBContexts
registerDbContext<AppDBContext>();

// Configure CORS for Android / mobile clients
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .SetIsOriginAllowed(_ => true);
    });
});

// Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "My API",
            Version = "v1"
        });


    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",

            Type = SecuritySchemeType.Http,

            Scheme = "bearer",

            BearerFormat = "JWT",

            In = ParameterLocation.Header,

            Description =
            "Enter JWT token only"
        });


    options.AddSecurityRequirement(document =>
    {
        return new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecuritySchemeReference(
                    "Bearer",
                    document
                ),

                new List<string>()
            }
        };
    });

});



// auth
builder.Services.AddAuthentication("Bearer")
.AddJwtBearer("Bearer", options =>
{

    options.TokenValidationParameters =
        new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer =
                builder.Configuration["Jwt:Issuer"],
            ValidAudience =
                builder.Configuration["Jwt:Audience"],
            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        builder.Configuration["Jwt:Key"]
                    )
                )
        };
});

builder.Services.AddAuthorization();
//////////////////////////////////
// register services 
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<JWTService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ProfileService>();
builder.Services.AddScoped<SessionService>();
//////////////////////////////////
var app = builder.Build();

// Use CORS
app.UseCors();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();