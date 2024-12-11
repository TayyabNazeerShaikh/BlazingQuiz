using BlazingQuiz.Api.Data;
using BlazingQuiz.Api.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

static void ApplyDbMigrations(IServiceProvider sp)
{
    var scope = sp.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<QuizContext>();
    if (context.Database.GetPendingMigrations().Any()) 
    {
        context.Database.Migrate();
    }
}