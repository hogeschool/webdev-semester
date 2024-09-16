var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.Urls.Add("https://localhost:5000");
app.MapGet("/hello/{who}/{numHearts}", (string who, int numHearts) => $"Hello world from {who} {String.Join("", Enumerable.Range(0, numHearts).Select(_ => "❤️").ToArray())}");
app.MapPost("/person", (Person who) =>
{
    return $"Person {who} has been submitted";
});

app.Run();

record Person(string Name, int Age);

// Add services to the container.

// builder.Services.AddControllers();
// // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();


// // Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

// app.UseHttpsRedirection();

// app.UseAuthorization();

// app.MapControllers();

// app.Run();
