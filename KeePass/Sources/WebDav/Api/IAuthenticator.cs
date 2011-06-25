using System;
using System.Net;

namespace KeePass.Sources.WebDav.Api
{
    internal interface IAuthenticator
    {
        /// <summary>
        /// Gets the token.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="method">The method.</param>
        /// <returns></returns>
        string GetToken(Uri uri, string method);

        /// <summary>
        /// Gets the next authenticator to retry.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <returns></returns>
        IAuthenticator Next(WebException ex);
    }
}