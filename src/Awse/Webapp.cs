using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Rest;

namespace Winkler.Awse.Owin
{
    class Webapp
    {
        private readonly string _hostname;

        public Webapp(string hostname)
        {
            _hostname = hostname;
        }

        public async Task<bool> IsCertificateDueForRenewalAsync()
        {
            return (await WebappsNeedingRenewalAsync())
                .Any();
        }

        public async Task InstallCertificateAsync(byte[] cert, string password)
        {
            var webapps = await WebappsNeedingRenewalAsync();

            foreach (var webapp in webapps)
            {
                webapp
                    .Update()
                    .DefineSslBinding()
                    .ForHostname(_hostname)
                    .WithPfxByteArrayToUpload(cert, "password")
                    .WithSniBasedSsl()
                    .Attach();
            }
        }

        private async Task<IEnumerable<IWebApp>> WebappsNeedingRenewalAsync()
        {
            return (await BuildAzureClientAsync())
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

        private async Task<IAzure> BuildAzureClientAsync()
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var token = await azureServiceTokenProvider.GetAccessTokenAsync("https://management.core.windows.net/");
            var tenantId = azureServiceTokenProvider.PrincipalUsed.TenantId;

            var client = RestClient
                .Configure()
                .WithEnvironment(AzureEnvironment.AzureGlobalCloud)
                .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                .WithCredentials(new AzureCredentials(new TokenCredentials(token), tenantId, AzureEnvironment.AzureGlobalCloud))
                .Build();

            return Azure
                .Authenticate(client, tenantId)
                .WithDefaultSubscription();
        }
    }
}