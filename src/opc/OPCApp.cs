using System;
using System.Threading;
using System.Threading.Tasks;

using Opc.Ua;
using Opc.Ua.Server;
using Opc.Ua.Configuration;

namespace Meminisse
{
    public class OPCApp
    {
        private ApplicationInstance app;

        private StandardServer server;

        private CancellationToken cancellationToken;

        public OPCApp(IDataAccess api, CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
            this.app = new ApplicationInstance();
            this.server = new OPCServer(api, cancellationToken);

            // Configuration
            app.ApplicationType = ApplicationType.Server;
            app.ConfigSectionName = "DuetOPCServer";
        }

        public async Task Start()
        {
            // Load Configuration
            Logger.instance.D("Loading OPC Configuration");
            await app.LoadApplicationConfiguration(Config.OPCServerConfFullPath, false);

            // Start server
            Logger.instance.D("OPC Server Starting");
            await app.Start(this.server);
        }

        public async Task Start(ApplicationConfiguration config)
        {
            // Set configuration
            app.ApplicationConfiguration = config;

            // Start
            await app.Start(this.server);
        }
    }
}