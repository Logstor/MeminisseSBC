using System.Threading.Tasks;

namespace Meminisse
{
    public interface ILogController<Entity>
    {
        void Init(string filename);

        void Reset();

        void Add(Entity entity);

        void FlushToFile();
    }
}