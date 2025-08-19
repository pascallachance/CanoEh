var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Specify the overload explicitly to resolve ambiguity
    app.UseSwagger(options => { }); // Use the overload with SwaggerOptions
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/seller", (HttpContext context) =>
{
    return Results.Ok(new { });
})
.WithName("seller")
.WithOpenApi();

app.MapFallbackToFile("/index.html");

app.Run();
