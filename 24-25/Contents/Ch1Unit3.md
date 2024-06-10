### [Back to List of Topics](Contents.md)

# Unit 2 - Middlewares, filters, and CQRS

## Pre- and post- processing: middleware
The core underlying mechanism of ASP.Net is centered around middlewares. Middlewares are a chain of (anonymous) functions which can inspect the request, write to the response, and if all is well invoke the next step in the chain. Middlewares may also decide *not* to invoke the next step in the chain, and given that the last step is the execution of one of our controllers, this can effectively act as a filter (useful when implementing security restrictions for example). Let's start with a middleware that does nothing whatsoever. We pass to `app.Use` an anonymous function which takes as inputs the `context`, which handles reading and writing to the request and response respectively, and `next`, which represents the rest of the middleware chain. Our first middleware simply ensures that the chain is not interrupted by invoking `next.Invoke`:

```c#
app.Use(async (context, next) =>
{
  await next.Invoke();
});
```

We could add some logic in order to log requests (ideally not just to the console, maybe to some fast permanent storage):

```c#
app.Use(async (context, next) =>
{
  await next.Invoke();
  Console.WriteLine($"{context.Request.Path} was handled");
});
```

A common security measure is to check for a specific header containing for example an API token. Let's lock down the `/api/person/sayHello` (finally, that thing was a security nightmare!) so that whenever our middleware detects a request to this path, we check whether or not the headers provided contain the key-value pair `(HelloApiToken, MyHelloApiToken)`:

```c#
app.Use(async (context, next) =>
{
  if (context.Request.Path == "/api/person/sayHello") {
    if (!context.Request.Headers.ContainsKey("HelloApiToken")) {
      Console.WriteLine($"{context.Request.Path} was requested but there is no HelloApiToken header");
      context.Response.StatusCode = 401;
      return;
    }
    if (context.Request.Headers["HelloApiToken"] != "MyHelloApiToken") {
      Console.WriteLine($"{context.Request.Path} was requested but the HelloApiToken is {context.Request.Headers["HelloApiToken"]} instead of 'MyHelloApiToken'");
      context.Response.StatusCode = 401;
      return;
    }
  }
  await next.Invoke();
});
```

Whenever the security check fails, we then set the status code of the response to `401` and skip the `next.Invoke`, thereby refusing to further process the unauthorized request.

The api token we are using, `MyHelloApiToken`, is unsafe. Moreover, an api token should never be part of the source code, but should rather come from the app settings so that it's easier to switch to a different api token for each environment (development, testing, or acceptance). Let's add a safer api token to the `appsettings.Development.json`:

```json
{
  ...
  "HelloApiToken": "e5e6c507-f46c-459f-a40c-15aac76f9638"
}
```

We can now turn the previous, unsafe version `if (context.Request.Headers["HelloApiToken"] != "MyHelloApiToken") {`, into a safer version that uses the token from the appsettings:

```c#
if (context.Request.Headers["HelloApiToken"] != builder.Configuration["HelloApiToken"] as string) {
```

We can test the endpoint with an api token as follows:

```rest
GET https://localhost:5000/api/person/sayHello
HelloApiToken: e5e6c507-f46c-459f-a40c-15aac76f9638
#####
```

## Filters
Middlewares are global mechanisms, meaning that they are applied to every request indiscriminately. This can be appropriate for some logic such as logging of all requests, but the latest example which manually checks for a given URL against the requested path is less than ideal because the logic actually belongs with a specific method of a specific controller, and those two pieces of code (the middleware and the controller method) are too far from each other. We can make the link between controller/method and filter more explicit by introducing an action filter. An action filter is a middleware which is not applied everywhere indiscriminately, but rather it is encapsulated in a class inheriting both `Attribute` and `IAsyncActionFilter`:

```c#
public class HelloHeaderActionFilter : Attribute, IAsyncActionFilter
{
  public async Task OnActionExecutionAsync(
      ActionExecutingContext actionContext, ActionExecutionDelegate next)
  {
    var context = actionContext.HttpContext;
    if (!context.Request.Headers.ContainsKey("HelloApiToken")) {
      Console.WriteLine($"{context.Request.Path} was requested but there is no HelloApiToken header");
      context.Response.StatusCode = 401;
      return;
    }
    if (context.Request.Headers["HelloApiToken"] != "MyHelloApiToken") {
      Console.WriteLine($"{context.Request.Path} was requested but the HelloApiToken is {context.Request.Headers["HelloApiToken"]} instead of 'MyHelloApiToken'");
      context.Response.StatusCode = 401;
      return;
    }
    await next();
    // Do something after the action executes.
  }
}
```

Setting the status code of the response and then returning before calling `next()` prevents the execution of the endpoint. Inheriting from `Attribute` makes it possible to use `HelloHeaderActionFilter` as an attribute, that is an annotation between square brackets on top of a method:

```c#
  [HelloHeaderActionFilter]
  [HttpGet("SayHello")]
  public async Task<IActionResult> SayHello() => Ok("Hello!");
```

Doing so, we are telling ASP.Net to execute the `OnActionExecutionAsync` right before the endpoint' method. Unlike the previous middleware implementation though we are only applying this filter to the method it is being attributed to, and not globally. Also, if we change the path via `HttpGet("SayHello")`, then the filter will be still applied correctly, thereby reducing the extra risk of making silly mistakes such as changing the url in the endpoint but not in the middleware.

### Options
If we look carefully at the filter implementation, we might notice that something went wrong: we went back to the implementation where the api token is hardcoded, and this is not good. Let's fix this.

First, we define the `HelloOptions` class:

```c#
public class HelloOptions{
  public string ApiToken {get; set; }
}
```

Then we add an instance of these options to the appsettings (it can have any name we want, but in this case we go with `Hello`):

```json
  "Hello": {
    "ApiToken": "e5e6c507-f46c-459f-a40c-15aac76f9638"
  }
```

We register in our dependency injection that we want to make these options available. In such a case we speak of a configuration rather than a service:

```c#
builder.Services.Configure<HelloOptions>(builder.Configuration.GetSection("Hello"));
```

Now we can extract an `IOptions<HelloOptions>` from the services in the attribute as follows:

```c#
    var helloOptions = 
      context.RequestServices.GetService<IOptions<HelloOptions>>() switch {
        { Value: var __ } => __,
        _ => new HelloOptions() { ApiToken = Guid.NewGuid().ToString() }
      };
```

> Note that the `{ Value: var __ } => __,` is extracting the `Value` from the `IOption<HelloOptions>`. If no `HelloOptions` have been correctly registered, we generate a random `Guid` that will certainly not match with any api token provided as a fallback. A random `Guid` is much safer than an empty string, which would match with empty api tokens!

We top it off by checking the provided header agains `helloOptions.ApiToken`:

```c#
if (context.Request.Headers["HelloApiToken"] != helloOptions.ApiToken) {
```

### CQRS: 
- TODO: still to come .....

**Exercises**
- Define a middleware that logs all requests to a file; the path of the log file should come from the appsettings;
- Define a middleware which stops all requests to a given endpoint unless the request comes from a given IP;
- Define a middleware which stops all requests to a given endpoint unless the request uses one of a series of username/password combinations provided via basic authentication (the valid username/password combinations should come from the appsettings).


**Extra reading materials**
- https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-6.0
- https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/filters?view=aspnetcore-6.0
