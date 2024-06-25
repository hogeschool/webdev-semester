using System.Text;
using Microsoft.AspNetCore.Mvc;
using StarterKit.Services;

namespace StarterKit.Controllers;


[Route("api/v1/Login")]
public class LoginController : Controller
{
    private readonly ILoginService _loginService;
    

    public LoginController(ILoginService loginService)
    {
        _loginService = loginService;
    }

    [HttpPost("Login")]
    public IActionResult Login([FromBody] LoginBody loginBody)
    {
        if (string.IsNullOrEmpty(loginBody.Password) || string.IsNullOrEmpty(loginBody.Username)) return BadRequest("No login body");

        LoginStatus loginStatus = _loginService.CheckPassword(loginBody.Username, loginBody.Password);

        if (loginStatus == LoginStatus.Success)
        {
            HttpContext.Session.Set(ADMIN_SESSION_KEY.adminLoggedIn.ToString(), Encoding.ASCII.GetBytes(loginBody.Username));
            return Ok("Correct password");
        } else if (loginStatus == LoginStatus.IncorrectUsername) {
            return Unauthorized("Incorrect username");
        }
        return Unauthorized("Incorrect password");
    }

    [HttpGet("IsAdminLoggedIn")]
    public IActionResult IsAdminLoggedIn()
    {
        var currentSession = HttpContext.Session.Get(ADMIN_SESSION_KEY.adminLoggedIn.ToString());
        if (currentSession != null)
        {
            return Ok(Encoding.ASCII.GetString(currentSession));
        }
        return Unauthorized("You are not logged in");
    }

    [HttpGet("Logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Remove(ADMIN_SESSION_KEY.adminLoggedIn.ToString());
        return Ok("Logged out");
    }

}

public class LoginBody
{
    public string? Username { get; set; }
    public string? Password { get; set; }
}
