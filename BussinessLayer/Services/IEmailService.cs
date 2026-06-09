using System.Threading.Tasks;

namespace BussinessLayer.Services
{
    public interface IEmailService
    {
        Task SendAccountCreatedEmailAsync(string toEmail, string username, string password, string role);
    }
}
