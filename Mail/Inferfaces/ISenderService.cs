using System.Threading.Tasks;
using Mail.Models;

namespace Mail.Services
{
    public interface ISenderService
    {
        Task<bool> SendEmailAsync(getMailRequestOnQueue mailRequest);

    }
}

