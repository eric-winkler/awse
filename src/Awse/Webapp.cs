using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;

namespace Winkler.Awse.Owin
{
    class Webapp
    {
        private readonly string _hostname;

        public Webapp(string hostname)
        {
            _hostname = hostname;
        }

        public async Task CheckPrincipalPrivileges()
        {
            if (!(await BuildAzureClient().Subscriptions.ListAsync()).Any())
                throw new InvalidOperationException("MSI cannot access any subscriptions");

            if (!(await BuildAzureClient().WebApps.ListAsync()).Any())
                throw new InvalidOperationException("MSI cannot access any webapps");
        }

        public bool IsCertificateDueForRenewal()
        {
            return WebappsNeedingRenewal().Any();
        }

        public async Task InstallCertificateAsync(byte[] cert, string password)
        {
            foreach (var webapp in WebappsNeedingRenewal())
            {
                await webapp
                    .Update()
                    .DefineSslBinding()
                    .ForHostname(_hostname)
                    .WithPfxByteArrayToUpload(cert, "password")
                    .WithSniBasedSsl()
                    .Attach()
                    .ApplyAsync();

                Trace.WriteLine("Certificate installed");
            }
        }

        private IEnumerable<IWebApp> WebappsNeedingRenewal()
        {
            return BuildAzureClient()
                .WebApps
                .List()
                .Where(a => a.EnabledHostNames.Contains(_hostname))
                .Select(w => new
                {
                    WebApp = w,
                    ValidCerts = w.Manager.AppServiceCertificates.List().Where(IsValidCertificate)
                })
                .Where(w => !w.ValidCerts.Any())
                .Select(w => w.WebApp);
        }

        private bool IsValidCertificate(IAppServiceCertificate certificate)
        {
            return certificate.HostNames.Contains(_hostname)
                   && certificate.ExpirationDate > DateTime.Now.AddDays(7)
                   && certificate.Issuer.ToLower().Contains("let's encrypt");
        }

        private static IAzure BuildAzureClient()
        {
            return Azure
                .Authenticate(new AzureCredentials(new MSILoginInformation(MSIResourceType.AppService), AzureEnvironment.AzureGlobalCloud))
                .WithDefaultSubscription();
        }
    }
}