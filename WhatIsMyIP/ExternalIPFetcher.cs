namespace WhatIsMyIP
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    internal class IPRecipy
    {
        internal string ServiceURL;
        private Func<string, string> Parser;

        public IPRecipy(string serviceUrl, Func<string, string> parser)
        {
            this.ServiceURL = serviceUrl;
            this.Parser = parser;
        }

        internal async Task<IPAnswer> GetAsync()
        {
            var client = new HttpClient();
            var body = await client.GetStringAsync(this.ServiceURL);
            var ipString = this.Parser(body).Trim();
            return new IPAnswer { ServiceURL = this.ServiceURL, IPAddress = IPAddress.Parse(ipString) };
        }
    }

    public class IPAnswer
    {
        public string ServiceURL { get; internal set; }
        public IPAddress IPAddress { get; internal set; }
    }

    public static class ExternalIPFetcher
    {
        internal static string RawIpAddressAsText(string input) { return input; }
        internal static dynamic DynamicJson(string input) { return Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(input); }

        private static readonly List<IPRecipy> recipies = new List<IPRecipy> 
        {
            // new IPRecipy("http://ifconfig.me/ip", RawIpAddressAsText),
            new IPRecipy("http://myexternalip.com/raw", RawIpAddressAsText),
            new IPRecipy("http://wtfismyip.com/text", RawIpAddressAsText),
            new IPRecipy("http://ipinfo.io/json", _ => DynamicJson(_).ip),
            new IPRecipy("http://what-is-my-ip.net/?json", _ => (string)DynamicJson(_)),
            new IPRecipy("http://api.ipify.org?format=json", _ => DynamicJson(_).ip),
            new IPRecipy("http://ip-api.com/json", _ => DynamicJson(_).query),
            new IPRecipy("http://ipinfo.io/json", _ => DynamicJson(_).ip),
            new IPRecipy("http://checkip.dyndns.org/", _ => _.Substring(_.IndexOf("Current IP Address: ")).Replace ("Current IP Address: ", "").Replace("</body></html>", "").Trim())
        };

        public static IEnumerable<IPAnswer> GetAddresses()
        {
            return recipies.Select(_ => _.GetAsync().Result);
        }
    }
}
