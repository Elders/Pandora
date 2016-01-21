using System;
using System.Text;
using RestSharp.Extensions.MonoHttp;

namespace Elders.Pandora.UI.Common
{
    public static class MvcRouteHelper
    {
        public static string GenerateUrl(string host, string baseUrl, params string[] parameters)
        {
            if (string.IsNullOrWhiteSpace(host)) throw new ArgumentNullException(nameof(host));
            if (string.IsNullOrWhiteSpace(baseUrl)) throw new ArgumentNullException(nameof(baseUrl));

            var sb = new StringBuilder();

            sb.Append(host);

            if (host.EndsWith("/") == false && baseUrl.StartsWith("/") == false)
            {
                sb.Append("/");
            }

            sb.Append(baseUrl);

            if (baseUrl.EndsWith("/") == false)
            {
                sb.Append("/");
            }

            if (ReferenceEquals(null, parameters) == false && parameters.Length != 0)
            {
                sb.Append(string.Join("/", parameters));
            }

            return sb.ToString();
        }

        public static string EncodeParameter(string parameter)
        {
            if (string.IsNullOrWhiteSpace(parameter)) throw new ArgumentNullException(nameof(parameter));

            return Convert.ToBase64String(HttpUtility.UrlEncodeToBytes(parameter));
        }
    }
}