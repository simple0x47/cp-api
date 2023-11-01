using System.Net.Mail;

namespace Cuplan.Organization.Utils;

public class Validation
{
    public static bool IsEmailValid(string email)
    {
        if (email is null) return false;

        if (email.Length == 0) return false;

        try
        {
            MailAddress mailAddress = new(email);
        }
        catch (Exception e)
        {
            return false;
        }

        return true;
    }

    public static bool IsPasswordValid(string password)
    {
        if (password is null) return false;

        if (password.Length < 8) return false;

        return true;
    }
}