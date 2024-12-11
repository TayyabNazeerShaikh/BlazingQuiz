using BlazingQuiz.Api.Data;
using BlazingQuiz.Api.Data.Entities;
using BlazingQuiz.Api.Endpoints;
using BlazingQuiz.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register the IPasswordHasher<User> service
builder.Services.AddTransient<IPasswordHasher<User>, PasswordHasher<User>>();

// Register the QuizContext with SQLite
builder.Services.AddDbContext<QuizContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
{
            var secretKey = builder.Configuration.GetValue<string>("Jwt:Secret");
            var symmetrickey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secretKey));
            options.TokenValidationParameters = new TokenValidationParameters 
            {
                IssuerSigningKey = symmetrickey,
                ValidIssuer = builder.Configuration.GetValue<string>("Jwt:Issuer"),
                ValidAudience = builder.Configuration.GetValue<string>("Jwt:Audience"),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true
            };
        });

builder.Services.AddTransient<AuthService>();

var app = builder.Build();

#if DEBUG
ApplyDbMigrations(app.Services);
#endif

// Initialize SQLitePCL (optional but recommended)
SQLitePCL.Batteries.Init();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.MapAuthEndpoints();

app.Run();

static void ApplyDbMigrations(IServiceProvider sp)
{
    var scope = sp.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<QuizContext>();
    if (context.Database.GetPendingMigrations().Any()) 
    {
        context.Database.Migrate();
    }
}