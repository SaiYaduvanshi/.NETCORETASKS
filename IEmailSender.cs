using System.Threading.Tasks;


namespace UserProfileApp
{

    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
