using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RE2REmakeSRT
{
    public class JSONServer : IDisposable
    {
        private readonly IConfigurationRoot config;
        private readonly IWebHostBuilder hostBuilder;
        private readonly IWebHost host;

        public JSONServer()
        {
            string contentRoot = Directory.GetCurrentDirectory();

            config = new ConfigurationBuilder()
                .SetBasePath(contentRoot)
                .Build();

            hostBuilder = new WebHostBuilder()
                .UseKestrel()
                .UseUrls("http://localhost:7190")
                .UseContentRoot(contentRoot)
                .UseStartup<JSONServerStartup>()
                .UseEnvironment("Development")
                .UseConfiguration(config);

            host = hostBuilder.Build();
        }

        public Task Start(CancellationToken cToken) => Task.Run(() => host.Run(), cToken);

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    host?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~JSONServer()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
