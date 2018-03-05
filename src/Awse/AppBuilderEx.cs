using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;

namespace Winkler.Awse.Owin
{
    public static class AppBuilderEx
    {
        public static IAppBuilder UseAcmeSsl(this IAppBuilder app, string hostname)
        {
            var acmeChallengePath = new PathString("/.well-known/acme-challenge");
            var handler = new AcmeChallengeHandler();
            app.MapWhen(c => c.Request.Path.StartsWithSegments(acmeChallengePath), a => a.Run(handler.Handle));

            // maintain certificates in the background
            #pragma warning disable 4014
            Task.Delay(TimeSpan.FromSeconds(30))
                .ContinueWith(_ => CertificateRenewalScheduler.StartAsync(hostname)
                    .ContinueWith(t => LogErrorAndRestart(t, hostname)));
            #pragma warning restore 4014

            return app;
        }

        private static void LogErrorAndRestart(Task task, string hostname)
        {
            if(task.Exception == null)
                Trace.WriteLine("Certificate renewal scheduler unexpectedly exited without an exception");
            else
                Trace.WriteLine("Certificate renewal scheduler failure" + Environment.NewLine +  task.Exception);

            CertificateRenewalScheduler.RetryDelay()
                .ContinueWith(_ => CertificateRenewalScheduler.StartAsync(hostname)
                    .ContinueWith(t => LogErrorAndRestart(t, hostname)));
        }
    }
}
