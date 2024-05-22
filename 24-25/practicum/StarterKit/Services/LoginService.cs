using StarterKit.Models;
using StarterKit.Utils;

namespace StarterKit.Services;

public enum LoginStatus { IncorrectPassword, IncorrectUsername, Success }
public class LoginService : ILoginService
{

    private readonly DatabaseContext _context;

    public LoginService(DatabaseContext context)
    {
        _context = context;
    }

    public LoginStatus CheckPassword(string username, string inputPassword)
    {
        using (var context = _context)
        {
            var admin = context.Admin.Where(x => x.UserName == username).FirstOrDefault();
            if (admin == null)
            {
                return LoginStatus.IncorrectUsername;
            }

            string referenceHash = admin.Password;
            string inputHash = EncryptionHelper.EncryptPassword(inputPassword);

            if (inputHash == referenceHash)
            {
                return LoginStatus.Success;
            }
        }
        return LoginStatus.IncorrectPassword;
    }
}