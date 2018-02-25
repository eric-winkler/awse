using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Logging;

namespace Winkler.Awse.Owin
{
    public class AcmeChallengeHandler
    {
        private readonly ILogger _logger;

        public AcmeChallengeHandler(ILogger logger)
        {
            _logger = logger;
        }

        public Task Handle(IOwinContext context)
        {
            var keyAuthorisation = AcmeCertificate.Current?.KeyAuthorisation;
            if (string.IsNullOrWhiteSpace(keyAuthorisation))
            {
                _logger.WriteWarning("Unknown Acme challenge");
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                return Task.FromResult(404);
            }
            
            _logger.WriteInformation("Responding to Acme challenge with key authorisation");
            context.Response.StatusCode = (int) HttpStatusCode.OK;
            context.Response.ContentType = "application/octet-stream";
            return context.Response.WriteAsync(Encoding.ASCII.GetBytes(keyAuthorisation));
        }
    }
}