using System.Threading;
using System.Threading.Tasks;

using DuetAPI;
using DuetAPI.ObjectModel;
using DuetAPIClient;

namespace Meminisse
{

    /// <summary>
    /// Delegate which is used to subscribe to IDataAccess objects.
    /// </summary>
    /// <param name="model">DuetAPI.ObjectModel</param>
    public delegate void OnUpdateHandler(ObjectModel model); 

    public interface IDataAccess 
    {
        event OnUpdateHandler OnObjectModelChange;

        Task<ObjectModel> requestObjectModel();

        Task<EntityWrap> requestFull();

        /// <summary>
        /// Takes a command connection and retrieves the Position asynchronous.
        /// </summary>
        /// <param name="connection"></param>
        /// <returns>PositionEntity struct</returns>
        Task<Position> requestPosition();

        Task<MachineStatus> requestStatus();

        /// <summary>
        /// Getting the full path of the file currently being printed.
        /// </summary>
        /// <returns>string</returns>
        Task<string> requestCurrentFilePath();
    }
}
