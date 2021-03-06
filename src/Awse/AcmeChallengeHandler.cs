﻿using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace Winkler.Awse.Owin
{
    public class AcmeChallengeHandler
    {
        public Task Handle(IOwinContext context)
        {
            var keyAuthorisation = AcmeCertificate.Current?.KeyAuthorisation;
            if (string.IsNullOrWhiteSpace(keyAuthorisation))
            {
                Trace.WriteLine("WARNING: Unknown Acme challenge");
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                return Task.FromResult(404);
            }
            
            Trace.WriteLine("Responding to Acme challenge with key authorisation");
            context.Response.StatusCode = (int) HttpStatusCode.OK;
            context.Response.ContentType = "application/octet-stream";
            return context.Response.WriteAsync(Encoding.ASCII.GetBytes(keyAuthorisation));
        }
    }
}