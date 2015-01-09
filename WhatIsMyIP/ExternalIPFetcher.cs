namespace WhatIsMyIP
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
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

        internal async Task<IPAnswer> GetAsync(CancellationToken ct)
        {
            var client = new HttpClient();
            var response = await client.GetAsync(this.ServiceURL, ct);
            var body = await response.Content.ReadAsStringAsync();
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
        internal static dynamic AsDynamicJson(string input) { return Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(input); }

        private static readonly List<IPRecipy> recipies = new List<IPRecipy> 
        {
            // new IPRecipy("http://ifconfig.me/ip", RawIpAddressAsText),
            new IPRecipy("http://myexternalip.com/raw", RawIpAddressAsText),
            new IPRecipy("http://wtfismyip.com/text", RawIpAddressAsText),
            new IPRecipy("http://ipinfo.io/json", _ => AsDynamicJson(_).ip),
            new IPRecipy("http://what-is-my-ip.net/?json", _ => (string)AsDynamicJson(_)),
            new IPRecipy("http://api.ipify.org?format=json", _ => AsDynamicJson(_).ip),
            new IPRecipy("http://ip-api.com/json", _ => AsDynamicJson(_).query),
            new IPRecipy("http://ipinfo.io/json", _ => AsDynamicJson(_).ip),
            new IPRecipy("http://checkip.dyndns.org/", _ => _.Substring(_.IndexOf("Current IP Address: "))
                .Replace ("Current IP Address: ", "")
                .Replace("</body></html>", "").Trim())
        };

        public static IEnumerable<IPAnswer> GetAddresses()
        {
            return recipies.Select(_ => _.GetAsync(CancellationToken.None).Result);
        }

        public static IPAnswer GetAddress()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            var tasks = recipies.Select(_ => _.GetAsync(cts.Token)).ToArray();
            var i = Task.WaitAny(tasks);
            var result = tasks[i].Result;
            cts.Cancel();
            return result;
        }
    }
}