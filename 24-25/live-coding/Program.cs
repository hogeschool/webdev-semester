/* REST
  pre-processing/post-processing middlewares = always log requests to some NoSQL database like Elastic

  routing = which piece of code shall process the request and produce a response
  controller = a group of related endpoints (UserController)
    related = that operate on the same domain
  endpoint fires = a function/method that processes the sepcific respecific request (UserController::resetPassword)
  instantiate one or more services = anything we might need to perform a given task (DBContext, SessionManager, OAuth2, LLMs, ML, ...)
    the services and their methods will be available throughout the body of the method of the endpoint
*/

using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

// Func<int, int> incr = x => x + 1;
// Func<int, int> decr = x => x - 1;

// void applyTwice(Func<int,int> f, int x) {
//   Console.WriteLine(f(f(x)));
// }

// Console.WriteLine("==========");
// Console.WriteLine("==========");
// Console.WriteLine("==========");
// applyTwice(incr, 2);
// applyTwice(decr, 2);
// Console.WriteLine("==========");
// Console.WriteLine("==========");
// Console.WriteLine("==========");


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTransient<IPersonStorage, TextFilesPersonStorage>();
builder.Services.AddControllers();
builder.Services.Configure<HelloOptions>(builder.Configuration.GetSection("Hello"));

var app = builder.Build();
app.Urls.Add("https://localhost:5000");
// // GET = give me something back (based on some parameters)
// app.MapGet("/hello/{who}/{numHearts}", (string who, int numHearts) => 
//     $"Hello world from {who} {String.Join("", Enumerable.Range(0, numHearts).Select(_ => "❤️").ToArray())}");
// // POST/PATCH/PUT = here is something to save/process/whatever
// // they have a body
// app.MapPost("/person", (Person who) =>
// {
//     return $"Person {who} has been submitted";
// });
app.MapControllers();
// app.Use(async (context, next) =>
// {
//   Console.WriteLine(context.Request.Path);
//   if (context.Request.Path.Value.Contains("/api/person/sayHello"))
//   {
//     if (!context.Request.Headers.ContainsKey("HelloApiToken"))
//     {
//       context.Response.StatusCode = 401;
//       return;
//     }
//     if (context.Request.Headers["HelloApiToken"] != builder.Configuration["HelloApiToken"])
//     {
//       context.Response.StatusCode = 401;
//       return;
//     }
//   }
//   await next.Invoke();
// });

app.Run();

///////////////////////////////////////
// Person/State.cs
///////////////////////////////////////
public record Person(Guid Id, string Name, DateTime Birthday);
public record PersonWithoutId(string Name, DateTime Birthday);

///////////////////////////////////////
// Person/Controller.csproj <- referenced by live-coding.csproj -> references ONLY IPersonStorage.csproj
// Person/Controller.cs
///////////////////////////////////////
[Route("api/person")]
public class PersonController : Controller
{
  readonly IPersonStorage personStorage;
  readonly HelloOptions helloOptions;
  public PersonController(IPersonStorage personStorage, IOptions<HelloOptions> helloOptions) { 
    this.personStorage = personStorage; 
    this.helloOptions = helloOptions.Value;
    }

  [HelloHeaderActionFilter]
  [HttpGet("sayGutenTag")]
  public async Task<IActionResult> SayHello([FromQuery] Guid[] personIds)
  {
    return Ok("hello!");
  }

  [HttpPost()]
  public async Task<IActionResult> CreatePerson([FromBody] PersonWithoutId personWithoutId)
  {
    var personId = await personStorage.CreatePerson(personWithoutId);
    return Ok(personId);
  }

  // api/person?personId=...
  [HttpGet()]
  public async Task<IActionResult> GetPerson([FromQuery] Guid personId)
  {
    var maybePerson = await personStorage.GetPerson(personId);
    if (maybePerson is null) return NotFound();
    return Ok(maybePerson);
  }

  [HttpGet("batch")]
  public async Task<IActionResult> GetPeople([FromQuery] Guid[] personIds)
  {
    var people = await personStorage.GetPeople(personIds);
    return Ok(people.ToArray());
  }

  [HttpDelete()]
  public IActionResult DeletePerson([FromQuery] Guid personId)
  {
    personStorage.DeletePerson(personId);
    return Ok();
  }
}

// IPersonStorage.csproj <- referenced by live-coding.csproj
// IPersonStorage.cs
public interface IPersonStorage
{
  Task<Guid> CreatePerson(PersonWithoutId personWithoutId);
  Task<Person?> GetPerson(Guid personId);
  Task<IEnumerable<Person>> GetPeople(IEnumerable<Guid> personIds);
  void DeletePerson(Guid personId);
}

// TextFilesPersonStorage.csproj <- referenced by live-coding.csproj -> references IPersonStorage.csproj
// TextFilesPersonStorage.cs
public class TextFilesPersonStorage : IPersonStorage
{
  public TextFilesPersonStorage() { }

  public async Task<Guid> CreatePerson(PersonWithoutId personWithoutId)
  {
    var person = new Person(Guid.NewGuid(), personWithoutId.Name, personWithoutId.Birthday);
    var path = $"data/people/{person.Id}.json";
    await System.IO.File.WriteAllTextAsync(path, JsonSerializer.Serialize(person));
    return person.Id;
  }

  public async Task<Person?> GetPerson(Guid personId)
  {
    var path = $"data/people/{personId}.json";
    if (!System.IO.File.Exists(path)) return null;
    return JsonSerializer.Deserialize<Person>(await System.IO.File.ReadAllTextAsync(path));
  }

  public async Task<IEnumerable<Person>> GetPeople(IEnumerable<Guid> personIds)
  {
    var people = new List<Person>();
    foreach (var personId in personIds)
    {
      var maybePerson = await this.GetPerson(personId);
      if (maybePerson is null) continue;
      people.Add(maybePerson);
    }
    return people;
  }

  public void DeletePerson(Guid personId)
  {
    var path = $"data/people/{personId}.json";
    if (!System.IO.File.Exists(path)) return;
    System.IO.File.Delete(path);
  }
}


// check-hello-header-token.cs
class HelloHeaderActionFilter : Attribute, IAsyncActionFilter
{
  public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
  {
    var helloOptions = context.HttpContext.RequestServices.GetService<IOptions<HelloOptions>>().Value;

    if (!context.HttpContext.Request.Headers.ContainsKey("HelloApiToken"))
    {
      context.HttpContext.Response.StatusCode = 401;
      return;
    }
    if (context.HttpContext.Request.Headers["HelloApiToken"] != helloOptions.ApiToken)
    {
      context.HttpContext.Response.StatusCode = 401;
      return;
    }
    await next.Invoke();
  }
}

// hello-options.cs
public class HelloOptions {
  public string ApiToken { get; set;}
}
