using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using KeePass.IO;

namespace KeePass.Sources.WebDav.Api
{
    internal class DigestToken
    {
        private readonly IDictionary<string, string> _values;
        private int _nc;

        public DigestToken(string challenge,
            string username, string password)
        {
            _values = Regex
                .Matches(challenge, Properties.Resources.DigestRegex)
                .Cast<Match>()
                .Select(x => x.Groups)
                .ToDictionary(x => x[1].Value, x => x[2].Value);

            _values.Add("username", username);
            _values.Add("password", password);
            _values.Add("cnonce", Guid.NewGuid().ToString("N"));
        }

        public string GetDigest(string uri, string method)
        {
            _nc++;

            var values = new Dictionary<string, string>(_values)
            {
                {"uri", uri},
                {"method", method},
                {"nc", _nc.ToString("00000000")},
            };

            var a1 = GetHash(GetKeys(values,
                "username", "realm", "password"));
            values.AddOrSet("a1", a1);

            var a2 = GetHash(GetKeys(
                values, "method", "uri"));
            values.AddOrSet("a2", a2);

            var response = GetHash(GetKeys(values,
                "a1", "nonce", "nc", "cnonce", "qop", "a2"));
            values.AddOrSet("response", response);

            return GetDigest(values,
                "username", "realm", "nonce", "uri",
                "response", "opaque", "qop", "nc", "cnonce");
        }

        private static string GetDigest(
            IDictionary<string, string> values,
            params string[] keys)
        {
            var sb = new StringBuilder("Digest ");
            foreach (var key in keys)
            {
                sb.Append(key);
                sb.Append("=\"");

                string value;
                if (!values.TryGetValue(key, out value))
                    value = string.Empty;

                sb.Append(value);
                sb.Append("\", ");
            }

            sb.Length -= 2;

            return sb.ToString();
        }

        private static string GetHash(string text)
        {
            var bytes = MD5Core.GetHash(text);

            return BitConverter.ToString(bytes)
                .Replace("-", string.Empty)
                .ToLowerInvariant();
        }

        private static string GetKeys(
            IDictionary<string, string> values,
            params string[] keys)
        {
            if (keys == null)
                throw new ArgumentNullException("keys");

            return string.Join(":", keys
                .Select(x => values[x])
                .ToArray());
        }
    }
}