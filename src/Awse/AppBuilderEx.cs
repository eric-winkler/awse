using Microsoft.Owin;
using Microsoft.Owin.Logging;
using Owin;

namespace Winkler.Awse.Owin
{
    public static class AppBuilderEx
    {
        public static IAppBuilder UseAcmeSsl(this IAppBuilder app, string hostname)
        {
            var acmeChallengePath = new PathString("/.well-known/acme-challenge");
            var handler = new AcmeChallengeHandler(app.CreateLogger(typeof(AcmeChallengeHandler)));
            app.MapWhen(c => c.Request.Path.StartsWithSegments(acmeChallengePath), a => a.Run(handler.Handle));

            // maintain certificates in the background
            #pragma warning disable 4014
            CertificateRenewalScheduler.StartAsync(hostname);
            #pragma warning restore 4014

            return app;
        }
    }
}
