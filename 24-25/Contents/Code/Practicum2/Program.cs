#pragma warning disable CS1998

using System.Text.Json;
using Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddTransient<IPersonStorage, TextFilesPersonStorage>();
builder.Services.AddTransient<IAddressStorage, TextFilesAddressStorage>();
builder.Services.AddOptions();
builder.Services.Configure<HelloOptions>(builder.Configuration.GetSection("Hello"));

var app = builder.Build();

app.MapControllers();
app.Use(async (context, next) =>
{
  await next.Invoke();
  Console.WriteLine($"{context.Request.Path} was requested");
});

app.Run(); //"https://localhost:5000");

