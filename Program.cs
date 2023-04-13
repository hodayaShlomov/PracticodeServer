using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using ToDoApi;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ToDoDBContext>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options =>
{
    options.AddPolicy("OpenPolicy",
                          policy =>
                          {
                              policy.WithOrigins("http://localhost:3000")
                                                  .AllowAnyHeader()
                                                  .AllowAnyMethod();
                          });
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Todo API", Description = "Keep track of your tasks", Version = "v1" });
});
var app = builder.Build();
app.UseCors("OpenPolicy");
    app.UseSwagger();
    app.UseSwaggerUI(options =>
{
   options.SwaggerEndpoint("/swagger/v1/swagger.json", "Todo API V1");
   options.RoutePrefix = string.Empty;
});
app.MapGet("/", () => "Hello World!");
app.MapGet("/items",(ToDoDBContext context)=>{
    return context.Items.ToList();
});
app.MapPost("/items", async(ToDoDBContext context, Item item)=>{
    context.Add(item);
    await context.SaveChangesAsync();
    return item;
});
app.MapPut("/items/{id}/{isComplete}", async( int id,bool isComplete,ToDoDBContext context)=>{
    var existItem = await context.Items.FindAsync(id);
    if(existItem is null) return Results.NotFound();
    existItem.IsComplete = isComplete;

    await context.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/items/{id}", async(ToDoDBContext context, int id)=>{
    var existItem = await context.Items.FindAsync(id);
    if(existItem is null) return Results.NotFound();

    context.Items.Remove(existItem);
    await context.SaveChangesAsync();

    return Results.NoContent();
});

app.Run();
