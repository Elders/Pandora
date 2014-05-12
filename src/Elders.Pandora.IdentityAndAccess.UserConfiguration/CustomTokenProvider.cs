using Microsoft.Owin.Security.DataProtection;
using System;
using System.Text;

namespace Elders.Pandora.IdentityAndAccess.UserConfiguration
{
    public class CustomTokenProvider : IDataProtector
    {

        public byte[] Protect(byte[] userData)
        {
            return Encoding.UTF8.GetBytes(Convert.ToBase64String(userData));

        }

        public byte[] Unprotect(byte[] protectedData)
        {
            return Convert.FromBase64String(Encoding.UTF8.GetString(protectedData));
        }
    }
}
