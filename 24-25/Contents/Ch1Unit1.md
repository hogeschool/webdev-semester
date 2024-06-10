### [Back to List of Topics](Contents.md)

# Unit 1 - Introduction to ASP.Net and MVC and Hello world in ASP.Net

## Introduction to ASP.Net
ASP.Net is a modern, high performance, cross-platform framework for building web applications with a variety of components and functionality.

At its core, ASP.Net is a web application server. This means that it governs the transfer of data between a server, where permanent data storage might reside, and a client, where the user interactions take place.

Given the many different requirements when it comes to data transfer, ASP.Net has come to support multiple different protocols. The most often protocols are based on HTTP, in particular following the REST conventions (GET, POST, PUT, DELETE). Remote procedure calls (gRPC) are also encountered in practice. HTTP REST and gRPC are mostly one-directional: clients invoke the server and get a response. A recent addition borne out of the necessity for two-way communication, are websockets, which are supported via the SignalR framework. Thanks to SignalR applications can communicate in real-time in two directions, supporting notifications, real-time chat, and more.

Of course ASP.Net also contains the tools for a disciplined organization of one's code. The "hello world" of an ASP.Net application is very simple to write and fits a single file on a single screen, but of course a real-life web application will contain much more code. ASP.Net offers a series of conceptual tools to distribute code:
- services contain the underlying business logic of an application module, for example the logic for tracking and managing payment statuses;
- controllers for receiving, validating, and dispatching REST, SignalR, or RPC requests to the right services;
- dependency injection to discover and invoke services from both services and controllers (and most other ASP.Net entities).


Moreover, ASP.Net offers a series of low-level tools, known as middlewares and filters, that facilitate the processing and dispatching of incoming requests. Middlewares and filters implement chains of validation and transformation of requests. Examples are routing (that parse a request URL in order to determine what code will compute the response), authorization and authentication, IP whitelisting, throttling, and much more.

[PIC: services, controllers, DI, routing, IP whitelisting]

(Persistent) data access plays a fundamental role in any application, and web applications are no exception. Data access is supported by various external frameworks, chief among which is Entity Framework (with LINQ as a language extension to C# that allows us to write SQL-like queries that are seamlessly integrated with the rest of our C# code: type system and all).

ASP.Net also integrates with multiple rendering mechanisms. Razor is a server-side templating engine capable of producing HTML code by integrating the HTML template with data computed dynamically. Blazor features a client-side rendering engine that compiles C# code into web assembly in order to run code based on the same models defined in the server, but on the client. Web assembly is also much faster than JavaScript, so Blazor also offers a good performance boost. Finally, ASP.Net integrates seamlessly with JavaScript frameworks such as Angular, React, and VueJS in order to support Single Page Applications (SPA).

In this text we will mostly focus on SPA's in React, with the underlying assumption that at this moment the alternatives (Razor and Blazor) do not offer an adequate option. Razor represents an old school way of working which is slowly being obsoleted because of the wish of end users to have reactive, responsive applications that exhibit some intelligence client side. Blazor is cutting edge, but this means that it is still not stable and mature enough for use in production. The choice for React rather than Angular and VueJS is somewhat arbitrary, but also not that much. React is mature, stable, based on functional programming principles, has a large enterprise backing it and ensuring a smooth and high quality development cycle, and is thus an excellent choice. Is it the best? We will never know. Does it suit our needs? Definitely!

### Hello World

> **Learning goals**
> After reading this chapter, the student will be able to:
> - create a basic ASP.Net project with a simple web api;
> - define GET, POST, PUT, and DELETE endpoints;
> - work with url parameters;
> - work with the body of requests.

> For this tutorial, please refer to the [Hello world](./HelloWorld) folder.

The structure of the project, which we created as a `webapi` with the `dotnet new` command, is based on a few files. On one hand we have the C# project file, with `.csproj` extension, which defines the series of tasks performed when compiling and running our code. In our case, we can see that the top element of this xml file is:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
...
</Project>
```

because we are using `Microsoft.NET.Sdk.Web` as the underlying SDK.

The `appsettings.json` file contains the settings that our web application may use. These settings may be extended with custom settings that we define specifically for our application. In general, any settings of any component of our application (database connection, basic authentication, etc.) will come out of this file.

There is a different set of appsettings for each deployment target. Developers will use specific appsettings that only make sense on their local development machine, whereas testing, acceptance, and production environments will all make use of their own, specific appsettings. There may be multiple different `appsettings.X.json` files, where `X` denotes the environment for which each appsettings file is meant. You can see that we have indeed just an `appsettings.development.json` file.

The source code of our whole application lies within the `Program.cs` file. We will eventually spread out our code across multiple files, but for now our application is so small that the benefits of keeping it short and simple overweight the advantages of splitting and organizing.

> Of course real applications do not fit in a single file, and there is no "your mileage may vary" wisdom here that will change it. As soon as an application needs to do more than one simple trivial thing like a simple HTTP echo, multiple files will be required to, at the very least, preserve the sanity of the people working after you on the applicatoin.

We start by defining a web application builder from the given arguments:

```c#
var builder = WebApplication.CreateBuilder(args);
```

The builder is used to define which services (business logic, security, utilities, and much more) the application supports. Our simple application has no need for any services, but we will make massive use of services later on.

After all services we need have been instantiated, we create the web application itself and start defining urls the application responds to, as well as middlewares that process requests and responses (for example to check for security or add headers):

```c#
var app = builder.Build();
```

We start by defining to which url shall we listen to (we could have multiple):

```c#
app.Urls.Add("https://localhost:5000");
```

We then add a single GET endpoint, `hello`, that returns the string `"Hello World!!!"` when invoked.

```c#
app.MapGet("/hello", () => "Hello World!!!");
```

Now that we have configured our application, we can just fire it up:

```c#
app.Run();
```

We can test this application by running it in watch mode (`dotnet watch run --environment Development`) and then by creating a `.rest` file for invoking the endpoints:

```rest
GET https://localhost:5000/hello
#####
```

It is very common for an endpoint to accept some parameters that specify some modifiers or extra details of what the endpoint is supposed to do. For example, `GET /person/1234567` could fetch a person from the database with `ID=1234567`. We can do this by using a url template, where the parameters are specified between curly brackets in the url, and then by using a parameter with the appropriate type in the response lambda:

```c#
app.MapGet("/hello/{who}", (string who) => $"Hello World from {who}!!!");
```

Url parameters may be much more than just a single string. For example, we could have multiple parameters:

```c#
app.MapGet("/hello123/{who1}/{who2}/{who3}", (string who1, string who2, string who3) => $"Hello World from {who1}, {who2}, and {who3}!!!");
```

Which can be tested with:

```rest
GET https://localhost:5000/hello123/Giuseppe/Francesco/Ivan
#####
```

Arrays or data structures are also common and supported in ASP.Net, but they are not possible with the _minimal APIs_ that we are currently using, so we will have to wait for proper controllers in order to use more advanced data structures.

Some REST methods, such as `POST`, support a body. The body can be an arbitrarily complex data structure, such as:

```c#
record Person(string Name, int Age);
```

We could define a REST `POST` method that accepts a body of type `Person` as follows:

```c#
app.MapPost("/person", (Person who) => $"The following person was posted: {who}");
```

Which we can invoke with:

```rest
POST https://localhost:5000/person HTTP/1.1
content-type: application/json

{ 
  "Name": "Giuseppe",
  "Age": 37
}
#####
```

Note that ASP.Net will be relatively lenient when parsing the body (extra attributes will be gracefully ignored, and missing attributes will be added with a default value) meaning that requests that could arguably fail will be saved anyway. The values given though must have valid types, or else we will encounter a runtime error, like for the following request where `Age` is given a `string` instead of the expected `int`:

```rest
POST https://localhost:5000/person HTTP/1.1
content-type: application/json

{ 
  "Name": "Giuseppe",
  "Age": "thirty-seven"
}
#####
``` 

**Exercises**
- build a basic webapi application;
- define an in-memory database containing people (a dictionary of `Id->Person`) and their friends (a dictionary of `Id->List<Id>`);
- add a POST endpoint to create a person (throw an error if the `ID` is already present);
- add a GET endpoint to fetch a person from their id;
- add a PUT/PATCH endpoint to update a person by `Id` (throw an error if the `ID` does not exist);
- add a DELETE endpoint to remove a person with a given `Id`.
- add a POST endpoint to befriend another person, taking as input two person ids. Throw an error if any of the ids does not exist or if the relationship already exists.
- add a GET endpoint to find all the friends of a person given his `Id`. Throw an error if the id does not exist.
- update the DELETE endpoint to delete also all the person's friendships.


**Extra reading materials**
- https://docs.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-6.0
- https://docs.microsoft.com/en-us/aspnet/core/fundamentals/environments?view=aspnetcore-6.0
