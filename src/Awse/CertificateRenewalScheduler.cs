using System;
using System.Threading.Tasks;

namespace Winkler.Awse.Owin
{
    public class CertificateRenewalScheduler
    {
        public static async Task StartAsync(string hostname)
        {
            var random = new Random();
            var webapp = new Webapp(hostname);

            while (true)
            {
                if (await webapp.IsCertificateDueForRenewalAsync())
                {
                    var cert = await AcmeCertificate.CreateAsync(hostname, "password");
                    await webapp.InstallCertificateAsync(cert.Bytes, "password");
                }

                await Task.Delay(TimeSpan.FromMinutes(random.Next(0, 24 * 60)));
            }
        }
    }
}