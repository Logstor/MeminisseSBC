using System.Threading.Tasks;

namespace Meminisse
{
    public interface ILogController<Entity>
    {
        void Init(string filename);

        void Close();

        void Add(Entity entity);

        void FlushToFile();
    }
}