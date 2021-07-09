using System.Runtime.Serialization;

using Opc.Ua;
using Opc.Ua.Server;
using Opc.Ua.Configuration;

using Duet;

namespace Meminisse
{
    [DataContract(Namespace = Duet.Namespaces.Duet)]
    public class DuetServerConfiguration
    {
        public DuetServerConfiguration()
        {
            this.Initialize();
        }

        /// <summary>
        /// Initializes the object during deserialization.
        /// </summary>
        /// <param name="context"></param>
        [OnDeserializing]
        private void Initialize(StreamingContext context)
        {
            this.Initialize();
        }

        /// <summary>
        /// Set private members to default values.
        /// </summary>
        private void Initialize()
        {

        }
    }

    
}