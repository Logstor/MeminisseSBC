using System;
using System.Collections.Generic;
using System.Reflection;

using Opc.Ua;
using Opc.Ua.Server;
using Duet;

using DuetAPI;
using DuetAPI.ObjectModel;

namespace Meminisse
{
    /// <summary>
    /// 
    /// </summary>
    public class DuetNodeManager : CustomNodeManager2
    {
        private IDataAccess api;
        private Duet.ModelObjectState duetState;
        private DuetServerConfiguration config;

        public DuetNodeManager(IDataAccess api, IServerInternal server, ApplicationConfiguration configuration)
            : base(server, configuration)
        {
            this.api = api;

            // Set namespaces
            SetNamespaces(new string[]{ Duet.Namespaces.Duet, Duet.Namespaces.Duet + "/instance" });

            // Get configuration from NodeManager
            this.config = configuration.ParseExtension<DuetServerConfiguration>() ?? new DuetServerConfiguration();
        }

        /// <summary>
        /// Override Dispose method to make sure to remove listener from API.
        /// </summary>
        public new void Dispose()
        {
            this.api.OnObjectModelChange -= this.OnUpdateHandler;
            base.Dispose();
        }

        protected override NodeStateCollection LoadPredefinedNodes(ISystemContext context)
        {
            Logger.instance.D("Loading Predefined Nodes");
            NodeStateCollection predefinedNodes = new NodeStateCollection();
            predefinedNodes.LoadFromResource(context, Config.OPCPredefinedNodesXMLPath, typeof(DuetNodeManager).GetTypeInfo().Assembly, true);
            return predefinedNodes;
        }

        public override void CreateAddressSpace(IDictionary<NodeId, IList<IReference>> externalReferences)
        {
            Logger.instance.D("Creating Address Space");
            lock (Lock)
            {
                LoadPredefinedNodes(SystemContext, externalReferences);

                BaseObjectState passiveNode = (BaseObjectState)FindPredefinedNode(new NodeId(Duet.Objects.ObjectModel, 
                    NamespaceIndexes[0]), typeof(BaseObjectState));

                this.duetState = new Duet.ModelObjectState(null);
                this.duetState.Create(SystemContext, passiveNode);

                AddPredefinedNode(SystemContext, this.duetState);
            }
            this.api.OnObjectModelChange += this.OnUpdateHandler;
        }

        protected void OnUpdateHandler(ObjectModel model)
        {
            Logger.instance.D("OnUpdateHandler DuetNodeManager");

            // Get simpler objects
            KinematicsState kinematics  = this.duetState.CurrentMove.Accelerations;
            SpeedState speeds           = this.duetState.CurrentMove.Speeds;
            CurrentMove currentMove     = model.Move.CurrentMove;

            // Set Kinematic values
            kinematics.Acceleration.Value = currentMove.Acceleration;
            kinematics.Deceleration.Value = currentMove.Deceleration;

            // Set Speed values
            speeds.RequestedSpeed.Value = currentMove.RequestedSpeed;
            speeds.TopSpeed.Value       = currentMove.TopSpeed;
        }
    }
}