using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace Filters
{
  public class HelloHeaderActionFilter : Attribute, IAsyncActionFilter
  {
    public async Task OnActionExecutionAsync(
        ActionExecutingContext actionContext, ActionExecutionDelegate next)
    {
      var context = actionContext.HttpContext;
      var helloOptions =
        context.RequestServices.GetService<IOptions<HelloOptions>>() switch
        {
          { Value: var __ } => __,
          _ => new HelloOptions() { ApiToken = Guid.NewGuid().ToString() }
        };
      if (!context.Request.Headers.ContainsKey("HelloApiToken"))
      {
        Console.WriteLine($"{context.Request.Path} was requested but there is no HelloApiToken header");
        context.Response.StatusCode = 401;
        return;
      }
      if (context.Request.Headers["HelloApiToken"] != helloOptions.ApiToken)
      {
        Console.WriteLine($"{context.Request.Path} was requested but the HelloApiToken is {context.Request.Headers["HelloApiToken"]} instead of {helloOptions.ApiToken}");
        context.Response.StatusCode = 401;
        return;
      }
      await next();
      // Do something after the action executes.
    }
  }

  public class HelloOptions
  {
    public string ApiToken { get; set; } = null!;
  }
}