using System.Collections.ObjectModel;
using System.ServiceModel;
using LinqToWikipedia;
using System.Linq;

namespace ForestFindr.Web.ServicesContracts
{
    [ServiceContract]
    public interface IWIKI
    {
        [OperationContract]
        Collection<WikipediaOpenSearchResult> GetWikiOpenSearch(string pesquisa, int count);
    }
}