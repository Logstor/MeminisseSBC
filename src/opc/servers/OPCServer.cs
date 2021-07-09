using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using Opc.Ua;
using Opc.Ua.Configuration;
using Opc.Ua.Server;

namespace Meminisse
{
    /// <summary>
    /// 
    /// </summary>
    public class OPCServer : StandardServer
    {
        private IDataAccess api;

        private CancellationToken cancellationToken;

        public OPCServer(IDataAccess dataAPI, CancellationToken cancellationToken)
            : base()
        {
            this.api = dataAPI;
            this.cancellationToken = cancellationToken;
        }

        protected override MasterNodeManager CreateMasterNodeManager(IServerInternal server, ApplicationConfiguration configuration)
        {
            Utils.Trace("Creating Node Manager");
            Logger.instance.D("Creating Node Manager");

            List<INodeManager> managers = new();

            // Create the Duet Node manager
            managers.Add(new DuetNodeManager(this.api, server, configuration));

            return new MasterNodeManager(server, configuration, null, managers.ToArray());
        }

        protected override ServerProperties LoadServerProperties()
        {
            return base.LoadServerProperties();
        }
    }
}

