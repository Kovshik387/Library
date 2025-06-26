using System.Data;
using System.Threading.Tasks;
using Library.Repositories.Enums;

namespace Library.Repositories.Interfaces
{
    public interface IDictionaryRepository
    {
        DataTable GetMetaIdxTables();
        void RebuildDictionary(DataTable metaIdxTables);

        DataBaseType GetProvider();
    }
}