using System;
using System.Threading.Tasks;

namespace Winkler.Awse.Owin
{
    public class CertificateRenewalScheduler
    {
        private static Random _random;

        public static async Task StartAsync(string hostname)
        {
            _random = new Random();
            var webapp = new Webapp(hostname);

            while (true)
            {
                if (await webapp.IsCertificateDueForRenewalAsync())
                {
                    var cert = await AcmeCertificate.CreateAsync(hostname, "password");
                    await webapp.InstallCertificateAsync(cert.Bytes, "password");
                }

                await RetryDelay();
            }
        }

        internal static Task RetryDelay()
        {
            return Task.Delay(TimeSpan.FromMinutes(_random.Next(0, 24 * 60)));
        }
    }
}