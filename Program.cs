using Microsoft.EntityFrameworkCore;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);

// הגדרת ה-DbContext
builder.Services.AddDbContext<ToDoDbContext>(options =>
    {
        var connectionString = Environment.GetEnvironmentVariable("ToDoDB");
            options.UseMySql(connectionString, Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.40-mysql"));
    });
// הוספת שירותי CORS עם מדיניות פתוחה
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build(); // יש ליצור את המשתנה כאן

// שימוש ב-Swagger אם מדובר בסביבת פיתוח
// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI();
//}

// שימוש במדיניות ה-CORS הפתוחה
app.UseCors("AllowAll");

app.MapGet("/", () => "Welcome to Todo API");
// הגדרת המסלולים
app.MapGet("/Items", async (ToDoDbContext db) =>
    await db.Items.ToListAsync());

app.MapGet("/Item/{id}", async (int id, ToDoDbContext db) =>
    await db.Items.FindAsync(id) is Item item
        ? Results.Ok(item)
        : Results.NotFound());
        
app.MapPost("/Item", async (Item newItem, ToDoDbContext db) =>
{
    newItem.IsComplete = false; // ברירת מחדל
    db.Items.Add(newItem);
    await db.SaveChangesAsync();
    return Results.Created($"/Item/{newItem.Id}", newItem);
});

// Update item
app.MapPut("/Item/{id}", async (int id,  bool IsComplete, ToDoDbContext db) =>
{
    var item = await db.Items.FindAsync(id);

    if (item is null) return Results.NotFound();

    item.IsComplete = IsComplete;

    await db.SaveChangesAsync();

    return Results.Ok(item); // מחזיר את הפריט המעודכן
});

app.MapDelete("/Item/{id}", async (int id, ToDoDbContext db) =>
{
    var item = await db.Items.FindAsync(id);

    if (item is null) return Results.NotFound();

    db.Items.Remove(item);
    await db.SaveChangesAsync();

    return Results.Ok(item);
});

app.Run();
