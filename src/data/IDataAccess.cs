using System.Threading;
using System.Threading.Tasks;

using DuetAPI;
using DuetAPI.ObjectModel;
using DuetAPIClient;

namespace Meminisse
{
    public interface IDataAccess 
    {
        /// <summary>
        /// Takes a command connection and retrieves the Position asynchronous.
        /// </summary>
        /// <param name="connection"></param>
        /// <returns>PositionEntity struct</returns>
        Task<PositionEntity> requestPosition();

        Task<MachineStatus> requestStatus();
    }
}
