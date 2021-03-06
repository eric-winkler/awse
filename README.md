# A.W.S.E.
ACME Webapp SSL Endpoint

[![Build status](https://ci.appveyor.com/api/projects/status/iahryjtqb74w0o6a?svg=true)](https://ci.appveyor.com/project/eric-winkler/awse)

Streamlines the integration of an ACME generated SSL certificate into an Azure Website.

### Usage
After installing the package;
* for an owin based application, add the following where you have access to an IAppBuilder:
`app.UseAcmeSsl("awesome.domainname.com");`

### Known Limitations
* Currently only generates test certificates (from the Let's Encrypt staging servers)
* Uses static variables to persist state for certificate generation handshake - this won't work on any multi-instance \ scaled out application
* Requires managed service identity to be enabled for the app service, and for the principal to be granted contributor rights to the resource group (ie not just the app service plan & webapp)

### Troubleshooting
* The certificate renewal mechanism logs failures to the Trace output, which is available in the 'verbose' log stream of an azure webapp
* After a failure, the next attempt will be randomly scheduled within the next 24 hours. To force an immediate re-attempt restart the webapp.  Be careful of the let's encrypt [rate limits](https://letsencrypt.org/docs/rate-limits/)
