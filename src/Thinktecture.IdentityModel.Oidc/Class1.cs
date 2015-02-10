using System;
using System.Collections.Generic;
using System.IdentityModel;
using System.IdentityModel.Services;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using Thinktecture.IdentityModel.Web;
namespace Thinktecture.IdentityModel.SystemWeb
{
    public class SelfProtectedCookie
    {
        private List<CookieTransform> _transforms;
        private static ChunkedCookieHandler _handler;
        private static object locker = new object();
        private static void InitializeHandler(bool enforceSSL)
        {
            if (_handler == null)
                lock (locker)
                {
                    if (_handler == null)
                    {
                        _handler = new ChunkedCookieHandler();
                        _handler.RequireSsl = enforceSSL;
                    }
                }

        }
        // DPAPI protection (single server)
        public SelfProtectedCookie(bool enforceSSL = true)
        {

            InitializeHandler(enforceSSL);
            SetDpapiTransforms();
        }

        public SelfProtectedCookie(ProtectionMode mode, bool enforceSSL )
        {
            InitializeHandler(enforceSSL);
            switch (mode)
            {
                case ProtectionMode.DPAPI:
                    SetDpapiTransforms();
                    return;
                case ProtectionMode.MachineKey:
                    SetMachineKeyTransforms();
                    return;
                default:
                    throw new ArgumentException("mode");
            }
        }
        // RSA protection (load balanced)
        public SelfProtectedCookie(X509Certificate2 protectionCertificate, bool enforceSSL)
        {
            InitializeHandler(enforceSSL);
            _transforms = new List<CookieTransform>
            { 
                new DeflateCookieTransform(), 
                new RsaSignatureCookieTransform(protectionCertificate),
                new RsaEncryptionCookieTransform(protectionCertificate)
            };
        }

        // custom transform pipeline
        public SelfProtectedCookie(List<CookieTransform> transforms, bool enforceSSL )
        {
            InitializeHandler(enforceSSL);
            _transforms = transforms;
        }

        public void Write(string name, string value, DateTime expirationTime, HttpContext context = null)
        {
            byte[] encodedBytes = EncodeCookieValue(value);

            _handler.Write(encodedBytes, name, expirationTime, context ?? HttpContext.Current);
        }

        public void Write(string name, string value, DateTime expirationTime, string domain, string path,
            HttpContext context = null)
        {
            byte[] encodedBytes = EncodeCookieValue(value);

            _handler.Write(encodedBytes,
                           name,
                           path,
                           domain,
                           expirationTime,
                           true,
                           true,
                           context ?? HttpContext.Current);
        }

        public string Read(string name, HttpContext context = null)
        {
            var bytes = _handler.Read(name, context ?? HttpContext.Current);

            if (bytes == null || bytes.Length == 0)
            {
                return null;
            }

            return DecodeCookieValue(bytes);
        }

        public static void Delete(string name, HttpContext context = null)
        {
            _handler.Delete(name, context ?? HttpContext.Current);
        }

        public static void Delete(string name, string domain, string path,
            HttpContext context = null)
        {
            _handler.Delete(name, path, domain, context ?? HttpContext.Current);
        }

        protected virtual byte[] EncodeCookieValue(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            byte[] buffer = bytes;

            foreach (var transform in _transforms)
            {
                buffer = transform.Encode(buffer);
            }

            return buffer;
        }

        protected virtual string DecodeCookieValue(byte[] bytes)
        {
            var buffer = bytes;

            for (int i = _transforms.Count; i > 0; i--)
            {
                buffer = _transforms[i - 1].Decode(buffer);
            }

            return Encoding.UTF8.GetString(buffer);
        }

        private void SetDpapiTransforms()
        {
            _transforms = new List<CookieTransform>
            { 
                new DeflateCookieTransform(), 
                new ProtectedDataCookieTransform() 
            };
        }

        private void SetMachineKeyTransforms()
        {
            _transforms = new List<CookieTransform>
            { 
                new DeflateCookieTransform(), 
                new MachineKeyTransform() 
            };
        }
    }
}