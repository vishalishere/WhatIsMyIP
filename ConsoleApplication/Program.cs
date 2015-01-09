namespace ConsoleApplication
{
    using System;
    using System.Linq;
    using WhatIsMyIP;

    class Program
    {
        static void Main(string[] args)
        {
            ExternalIPFetcher.GetAddresses().ToList().ForEach(_ => Console.WriteLine("{0} (says {1})", _.IPAddress, _.ServiceURL));
        }
    }
}
