using System;
using System.Linq;
using System.Threading.Tasks;
using Certes;
using Certes.Acme;
using Certes.Pkcs;

namespace Winkler.Awse.Owin
{
    class AcmeCertificate : IDisposable
    {
        private readonly AcmeClient _acmeClient;
        private Challenge _challenge;
        private Uri _challengeLocation;

        public string Hostname { get; private set; }
        public string KeyAuthorisation { get; private set; }
        public byte[] Bytes { get; private set; }

        private static readonly object Lock = new object();
        public static AcmeCertificate Current { get; private set; }
        
        private AcmeCertificate(AcmeClient acmeClient, string hostname)
        {
            _acmeClient = acmeClient;
            Hostname = hostname;
        }

        public static async Task<AcmeCertificate> CreateAsync(string hostname, string password)
        {
            // TODO: pick the right environment
            var client = new AcmeClient(WellKnownServers.LetsEncryptStaging);
            var account = await client.NewRegistraton();
            account.Data.Agreement = account.GetTermsOfServiceUri();
            await client.UpdateRegistration(account);

            lock (Lock)
            {
                Current?.Dispose();
                Current = new AcmeCertificate(client, hostname);
            }

            await Current.StartChallenge();
            await Current.CompleteChallenge();
            await Current.CreateCertificate(password);

            return Current;
        }

        private async Task StartChallenge()
        {
            var authz = await _acmeClient.NewAuthorization(new AuthorizationIdentifier
            {
                Type = AuthorizationIdentifierTypes.Dns,
                Value = Hostname
            });

            _challenge = authz.Data.Challenges.First(c => c.Type == ChallengeTypes.Http01);
            KeyAuthorisation = _acmeClient.ComputeKeyAuthorization(_challenge);
        }

        private async Task CompleteChallenge()
        {
            if(_challenge == null)
                throw new InvalidOperationException("Challenge must be started before it can be completed");

            _challengeLocation = (await _acmeClient.CompleteChallenge(_challenge)).Location;
        }

        private async Task CreateCertificate(string password)
        {
            if(_challengeLocation == null)
                throw new InvalidOperationException("Challenge must be completed before certificate creation");

            var authorisation = await _acmeClient.GetAuthorization(_challengeLocation);
            while (authorisation.Data.Status == EntityStatus.Pending)
            {
                await Task.Delay(10000);
                authorisation = await _acmeClient.GetAuthorization(_challengeLocation);
            }

            if(authorisation.Data.Status != EntityStatus.Valid)
                throw new InvalidOperationException($"Authorisation failed, status = {authorisation.Data.Status}");

            var csr = new CertificationRequestBuilder();
            csr.AddName("CN", Hostname);
            var cert = await _acmeClient.NewCertificate(csr);
            Bytes = cert.ToPfx().Build(Hostname, password);
        }

        public void Dispose()
        {
            _acmeClient?.Dispose();
        }
    }
}