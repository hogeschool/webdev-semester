using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.Urls.Add("http://localhost:5000");

app.MapGet("/hello", () => "Hello World!!!");

app.MapGet("/hello123/{who1}/{who2}/{who3}", (string who1, string who2, string who3) => $"Hello World from {who1}, {who2}, and {who3}!!!");
app.MapPost("/person", (Person who) => $"The following person was posted: {who}");

app.Run();


record Person(string Name, int Age);