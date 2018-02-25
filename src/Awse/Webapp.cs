using System.Threading.Tasks;

namespace Winkler.Awse.Owin
{
    class Webapp
    {
        public bool IsCertificateDueForRenewal()
        {
            // !IsCertificateInstalled || IsCertificateCloseToExpired
            return false;
        }

        // * Install Certificate
        // * Check for expired\missing certificate
        public Task InstallCertificateAsync(byte[] cert)
        {
            throw new System.NotImplementedException();
        }
    }
}