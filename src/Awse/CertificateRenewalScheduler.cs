using System;
using System.Threading.Tasks;

namespace Winkler.Awse.Owin
{
    public class CertificateRenewalScheduler
    {
        public static async Task StartAsync(string hostname)
        {
            var random = new Random();
            var webapp = new Webapp();

            while (true)
            {
                if (webapp.IsCertificateDueForRenewal())
                {
                    var cert = await AcmeCertificate.CreateAsync(hostname, "");
                    await webapp.InstallCertificateAsync(cert.Bytes);
                }

                await Task.Delay(TimeSpan.FromMinutes(random.Next(0, 24 * 60)));
            }
        }
    }
}