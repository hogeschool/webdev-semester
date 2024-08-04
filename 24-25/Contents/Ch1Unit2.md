### [Back to List of Topics](Contents.md)

# Unit 2 - Controllers, services, and dependency injection for storage
> **Learning goals**
> After reading this chapter, the student will be able to:
> - define a controller;
> - define GET, POST, PUT, and DELETE endpoints;
> - work with all combinations of url parameters, request bodies, and composite types;
> - define services and consume them via dependency injection;
> - consume services abstractly via dependency injection through interfaces;
> - define a custom middleware;
> - define a custom filter.


> For this tutorial, please refer to the [ControllersAndServices](./ControllersAndServices) folder.

ASP.Net offers a set of primitives that make it possible to split the different aspects of a web application in order to keep code organized. As much as the minimal API as we have seen it so far would make it possible to build any arbitrary web application, the complexity of the unorganized code would quickly become overwhelming, and while the framework does not mind, the humans working on the code definitely do.

An ASP.Net application is organized by splitting the various concerns. When a REST request arrives, we must first parse the url, query parameters, body, etc. This first stage is performed by the _routing_ module, which uses the _controllers_ defined by a developer to decide which piece of code will respond to which request. The controller picked by the routing module will then instantiate one or more _services_ which contain the appropriate business logic that determines what the application is supposed to do when receiving a given request.

Next to controllers and services, we can define pre- and post-processing steps that are performed while processing a request: _middlewares_. Logging, some forms of authentication, injecting headers, etc. are all forms of pre- or post-processing that would be implemented via middlewares.

Finally, we can define C# attributes that work as coarse _filters_ in order to easily mark controllers and controller endpoints. For example, we could define a filter that only accepts requests from a given port for a certain controller.

## Routing and dispatching: controllers
Controllers are the first line of processing of our web applications. First we must tell the application that we actually want to enable all the services needed to use controllers via `builder.Services.AddControllers` (or via `AddMvcCore`, which also adds services useful for the views) and also that we want to turn the controllers on via `app.MapControllers`:

```c#
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
var app = builder.Build();

app.MapControllers();

app.Run("https://localhost:5000");
``` 

The application does not do much at this point. We can create a new controller by simply inheriting from the `Controller` class and defining the base route this controller responds to via the `Route` attribute:

```c#
[Route("api/person")]
public class PersonController : Controller {}
``` 

The methods inside a controller can respond to HTTP REST calls. We just need to add a method with the `HttpGet` attribute, as well as the url relative to the route of the controller. A controller may return a result (of type `IActionResult` or `ActionResult<T>` if we want to be more specific and type-safe) by passing it to the various utility methods such as `Ok` that wrap the result inside a valid HTTP response:

```c#
  [HttpGet("SayHello")]
  public async Task<IActionResult> SayHello() => Ok("Hello!");
```

> We can invoke and test our new endpoint with the following url `GET https://localhost:5000/api/person/sayHello`.

Let's turn our controller into a so-called CRUD (Create, Read, Update, Delete) controller (actually the Update is so similar to create that we will just skip it for now, as an exercise to the reader) for a `Person` data structure:

```c#
public record Person(Guid Id, string Name, string Surname, DateTime birthday);
```

> Notice that `Person` has a `Guid` instead of a much more dangerous `int` for the `Id`. This is very important for security, because `Guid`s cannot be enumerated, so a hacker cannot try to enumerate all available `Id`s to get information on all users. Statistically, `Guid`s are unguessable, so if one does not know a `Guid` then the chance of finding it out via a brute force is extremely limited. `Guid`s have numerous other advantages: they make synchronization of two databases much easier than with auto-increment integer `Id`, and more. *In short: use `Guid`s*.

 We allow the creation of a `Person` via a `POST` endpoint that accepts an instance of `Person` from the body. This requires marking the `Person` parameter of the method with the `FromBody` attribute:

 ```c#
  [HttpPost()]
  public async Task<IActionResult> CreatePerson([FromBody] Person person) {
    // person = person with { Id = Guid.NewGuid() }; // <- THIS IS MUCH BETTER FOR SECURITY, we should not trust an id coming from the client!!!
    var path = $"people/{person.Id}.json";
    await System.IO.File.WriteAllTextAsync(path, JsonSerializer.Serialize(person));
    return Ok();
  }
 ```

 > Notice that we are saving files, which is definitely not a best practice when dealing with permanent storage. Files will have all sorts of problems related to concurrent access, performance, and more. The proper way to implement permanent storage is with an actual relational, document, or graph database. Sometimes reinventing the wheel can be useful, but remember that systems such as PostgreSQL offer you literally decades of work, research, and debugging for free. Do not easily discard the wisdom of your forebearers.
 > Still, in order to keep our tutorial simple, files will do for now.

> Also, notice that we are accepting whatever `Id` is passed to us. This is no good practice from a security standpoint, because we should barely trust the client-side, and blindly accepting `Id`s is something we should not do. The `Id` for a newly created entity should ideally be determined by the server, as the commented first line of the method does. 
> Still, for ease of testing `Id`s generated client-side will do.

We can invoke our `POST` endpoint with the right URL (the one of the controller itself) and a `Person` body as follows:

```rest
POST https://localhost:5000/api/person HTTP/1.1
content-type: application/json

{ 
  "Id": "948029cd-2fae-4e06-a770-d636224626ee",
  "Name": "Giuseppe",
  "Surname": "Maggiore",
  "Birthday": "1985-03-02"
}
#####

POST https://localhost:5000/api/person HTTP/1.1
content-type: application/json

{ 
  "Id": "6201236f-a77a-4c27-aae6-88395bc9766f",
  "Name": "Ivan",
  "Surname": "Igorski",
  "Birthday": "1975-09-11"
}
#####
```

Now that we have acquired the ability to create entities, we can retrieve them with a `GET` endpoint which accepts a `personId` as a query parameter in the URL:

```c#
  [HttpGet()]
  public async Task<IActionResult> GetPerson([FromQuery] Guid personId) {
    var path = $"people/{personId}.json";
    if (!System.IO.File.Exists(path)) return NotFound();
    var person = JsonSerializer.Deserialize<Person>(await System.IO.File.ReadAllTextAsync(path));
    return Ok(person);
  }
```

Note that we are not just returning `Ok`: if the requested `Id` is not found, then we return `NotFound`. We can invoke this endpoint as follows:

```rest
GET https://localhost:5000/api/person?personId=948029cd-2fae-4e06-a770-d636224626ee
#####
```

Sometimes an application needs to fetch multiple entities in one go. It is much faster to do so with a batched endpoint instead of performing many `GET` calls one after the other, given that each `GET` call might require a new TCP connection which is slow to create. We can accept an array of `Id` as a single query parameter:

```c#
  [HttpGet("batch")]
  public async Task<IActionResult> GetPeople([FromQuery] Guid[] personIds) {
    var people = new List<Person>();
    foreach (var personId in personIds)
    {
      var path = $"people/{personId}.json";
      if (System.IO.File.Exists(path)) {
        var person = JsonSerializer.Deserialize<Person>(await System.IO.File.ReadAllTextAsync(path));
        if (person is not null)
          people.Add(person);
      }
    }
    return Ok(people.ToArray());
  }
```

Note that the `HttpGet` attribute now has a string argument: `"batch"`. We now have two `GET` endpoints defined: the previous one listens on the controller base URL (`https://localhost:5000/api/person`), but this new one listens on the relative `batch` path (`https://localhost:5000/api/person/batch`). We can pass the array parameter quite simply by using the same query parameter in the URL:

```rest
GET https://localhost:5000/api/person/batch?personIds=948029cd-2fae-4e06-a770-d636224626ee&personIds=6201236f-a77a-4c27-aae6-88395bc9766f
```

## Business logic: services
The principle of separation of concerns dictates that we should split units of code so that they have a precise, intuitively understandable single goal that can be expressed in a single sentence. This is for the benefit of the programmers who will then be able to quickly zoom in on the purpose of each unit of code, in order to find, extend, or debug their application in the future. Again, all of these software engineering principles are not there for the machine, which does not care whether or not our code is a jumbled, unintelligible mess, but rather for us, the humans who have to work with the code: working with code requires understanding, and understanding requires fitting within the limiting confines of what a human brain can easily achieve.

> You might be taken by a moment of macho pride, suggesting that your superior intelligence can easily cope with complex code "no problemo!", but that might be naïve. Intelligence is usually not the first bottleneck, whereas having a bad day, not having slept properly, deadlines, etc. will have a lot of influence in the effectively available brainpower that a developer can make use of within a short amount of time.

Our controller does not respect the principle of separation of concerns, because it is doing both the routing and parsing of arguments, as well as the execution of the underlying business logic (saving and reading the data from files). We want to start by isolating the logic of saving files into a separate class, which we will call a _service_. Services do not need to inherit from any specific class, so we can just define our service class as follows:

```c#
public class TextFilesPersonStorage
{
}
```

We now _register_ the service so that it's available to all other controllers and services through dependency injection:

```c#
builder.Services.AddTransient<TextFilesPersonStorage>();
```

> There are multiple types of services: transient, scoped, and singleton, depending on when and how often they are initialized for each request. Transient is the most common, and is the one we will use almost everywhere. Services which initialization takes a lot of computation power might be better as singleton or scoped services, but that is a rare use occurrence.

We now declare that our controller depends on our service. We do so by adding a constructor which accepts an instance of the service (which ASP.Net will construct for us):

```c#
public class PersonController : Controller {
  readonly TextFilesPersonStorage TextFilesPersonStorage;

  public PersonController(TextFilesPersonStorage personStorage)
  {
    this.personStorage = personStorage;
  }

  ...
}
```

We now move the logic from each controller endpoint to the service. Let's start with the creation logic:

```c#
public class TextFilesPersonStorage
{
  public async Task SavePerson(Person person) =>
    await System.IO.File.WriteAllTextAsync($"people/{person.Id}.json", JsonSerializer.Serialize(person));
}
```

we then invoke the appropriate service method from the controller:

```c#
  [HttpPost()]
  public async Task<IActionResult> CreatePerson([FromBody] Person person)
  {
    await personStorage.SavePerson(person);
    return Ok();
  }
```

You might notice that as we keep moving logic away from our controller, the controller becomes much more streamlined and easy to follow. After all, its job is now simplified from doing everything to only receiving and dispatching requests. The service is also relatively easy to follow, because all it does are straightforward queries to the filesystem without any "noise" from controllers, endpoints, REST, bodies, and so on.

We have successfully split the ASP.Net web application-related code from the underlying business logic, and our application is easier to maintain for this!

### Abstraction
Later on, as our knowledge about databases starts taking off, we will probably want to do something better than save and read data to plain text files. We therefore want to make it easier to swap which service is used by our controller. We can do this by introducing an interface. Our controller will not depend on the `TextFilesPersonStorage` service, but rather the interface it implements.

We extract the interface of `TextFilesPersonStorage`:

```c#
public interface IPersonStorage
{
  Task Create(Person person);
  Task Delete(Guid personId);
  Task<Person?> Find(Guid personId);
  Task<List<Person>> FindMany(Guid[] personIds);
}

public class TextFilesPersonStorage : IPersonStorage
{
  ...
}
```

Notice that `IPersonStorage` does not mention files anywhere. It is just an interface modeling the operations for accessing (read/write/delete) people entries. Whether or not the data comes and goes to files, databases, or cloud storage, is not mentioned. This suggests that `IPersonStorage` is a decent interface that does not expose too much context or information.

We then change the way we register our service. In particular, we now register the service through its interface, instead of directly:

```c#
builder.Services.AddTransient<IPersonStorage, TextFilesPersonStorage>();
```

This means that a controller or a service cannot depend on `TextFilesPersonStorage` directly, but only through `IPersonStorage`. If we ran the application with this change only and we tried to invoke an controller endpoint, we would get an error complaining that the dependency from `PersonController` to `TextFilesPersonStorage` cannot be resolved:

```
Unable to resolve service for type 'TextFilesPersonStorage' while attempting to activate 'PersonController'.
```

This is fixed as we adjust the dependency in the controller towards the interface. Notice that, because `TextFilesPersonStorage` and `IPersonStorage` implement exactly the same methods with the same signatures, just changing the constructor and the field declaration is enough:

```c#
public class PersonController : Controller {
  readonly IPersonStorage personStorage;

  public PersonController(IPersonStorage personStorage)
  {
    this.personStorage = personStorage;
  }
  
  ...
}
```

Now we could define a new service that implements `IPersonStorage`, say `PostgreSQLPersonStorage`, register it among the services as follows:

```c#
builder.Services.AddTransient<IPersonStorage, PostgreSQLPersonStorage>();
```

And the application would continue working without any changes. The ability to easily swap services can be useful not only for refactoring purposes, but also if we want to test our application through automated tests that need to inject _mocked_ services that implement a tiny sliver of the business logic of the dependencies needed for a single test. In short, using interfaces as an abstraction mechanism for the consumption of our services makes our application more flexible, easier to change in the future, easier to test, and much more.


**Exercises**
- Extend our controller to also serve people's addresses; this will require endpoints that get all addresses of a given person, add an address to a person, remove an address from a person, and so on;
- Move the storage logic of the previous exercise from the controller to a service;
- Define an interface for the data access around addresses and use that interface instead of the service you just defined so that we can define another implementation of that service later on;
- The path of the folder where we store the addresses should be customized: extend your program to read it from the appsettings;


**Extra reading materials**
- https://docs.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-6.0
- https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-6.0
