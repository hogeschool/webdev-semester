using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using StarterKit.Models;
using StarterKit.Utils;

namespace StarterKit.Controllers;


[Route("api/v1/Login")]
public class LoginController : Controller
{
    private readonly ILogger<LoginController> _logger;

    private readonly DatabaseContext _context;


    private readonly string ADMIN_SESSION_KEY = "adminLoggedIn";

    public LoginController(ILogger<LoginController> logger, DatabaseContext context)
    {
        _logger = logger;
        _context = context;
    }

    [HttpPost("Login")]
    public IActionResult Login([FromBody] LoginBody loginBody)
    {
        if (string.IsNullOrEmpty(loginBody.Password) || string.IsNullOrEmpty(loginBody.Username)) return BadRequest("No login body");

        using (var context = _context)
        {
            var admin = context.Admin.Where(x => x.UserName == loginBody.Username).FirstOrDefault();
            if (admin == null) return Unauthorized("Username does not exist");

            string referenceHash = admin.Password;
            string inputHash = EncryptionHelper.EncryptPassword(loginBody.Password);

            if (referenceHash == inputHash)
            {
                HttpContext.Session.Set(ADMIN_SESSION_KEY, Encoding.ASCII.GetBytes(loginBody.Username));
                return Ok("Correct password");
            }
        }
        return Unauthorized("Incorrect password");
    }

    [HttpGet("IsAdminLoggedIn")]
    public IActionResult IsAdminLoggedIn()
    {
        var currentSession = HttpContext.Session.Get(ADMIN_SESSION_KEY);
        if (currentSession != null) {
            return Ok(Encoding.ASCII.GetString(currentSession));
        }
        return Unauthorized("You are not logged in");
    } 

    [HttpGet("Logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Remove(ADMIN_SESSION_KEY);
        return Ok("Logged out");
    }

}

public class LoginBody
{
    public string? Username { get; set; }
    public string? Password { get; set; }
}
