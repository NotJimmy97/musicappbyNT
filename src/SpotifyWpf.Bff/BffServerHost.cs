using System;
using Microsoft.Owin.Hosting;

namespace SpotifyWpf.Bff
{
    public static class BffServerHost
    {
        public static IDisposable Start(string baseAddress = "http://localhost:5245")
        {
            return WebApp.Start<Startup>(baseAddress);
        }
    }
}
