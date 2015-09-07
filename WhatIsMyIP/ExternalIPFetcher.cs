﻿namespace WhatIsMyIP
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
            // new IPRecipy("http://corz.org/ip", RawIpAddressAsText),
            new IPRecipy("http://quickhash.com/ip", RawIpAddressAsText),
            new IPRecipy("http://bot.whatismyipaddress.com/", RawIpAddressAsText),
            new IPRecipy("http://curlmyip.com/", RawIpAddressAsText),
            new IPRecipy("http://icanhazip.com/", RawIpAddressAsText),
            new IPRecipy("http://ip.qaros.com/", RawIpAddressAsText),
            new IPRecipy("http://ipecho.net/plain", RawIpAddressAsText),
            new IPRecipy("http://ipinfo.io/ip", RawIpAddressAsText),
            new IPRecipy("http://myexternalip.com/raw", RawIpAddressAsText),
            new IPRecipy("http://wtfismyip.com/text", RawIpAddressAsText),        
            new IPRecipy("http://api.ipify.org?format=json", _ => AsDynamicJson(_).ip),
            new IPRecipy("http://checkip.dyndns.org/", _ => _.Substring(_.IndexOf("Current IP Address: ")).Replace ("Current IP Address: ", "").Replace("</body></html>", "").Trim()),
            new IPRecipy("http://ip-api.com/json", _ => AsDynamicJson(_).query),
            new IPRecipy("http://ipinfo.io/json", _ => AsDynamicJson(_).ip),
            new IPRecipy("http://ipinfo.io/json", _ => AsDynamicJson(_).ip),
            new IPRecipy("http://what-is-my-ip.net/?json", _ => (string)AsDynamicJson(_)),
        };

        public static IEnumerable<IPAnswer> GetAddresses()
        {
            var tasks = recipies.Select(_ => _.GetAsync(CancellationToken.None)).ToArray();
            Task.WaitAll(tasks);
            return tasks.Select(_ => _.Result);
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