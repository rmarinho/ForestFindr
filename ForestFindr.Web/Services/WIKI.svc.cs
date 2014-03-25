using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using ForestFindr.Web.ServicesContracts;
using LinqToWikipedia;
using System.Collections.ObjectModel;
using System.ServiceModel.Activation;

namespace ForestFindr.Web.Services
{
      
    
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class WIKI : IWIKI
    {
        public WikipediaContext datacontext = new WikipediaContext();
        public Collection<WikipediaOpenSearchResult> GetWikiOpenSearch(string pesquisa, int count)
        {
          
            var opensearch = (
              from wikipedia in datacontext.OpenSearch
              where wikipedia.Keyword == pesquisa
              select wikipedia).Take(count);

            return new Collection<WikipediaOpenSearchResult>(opensearch.ToList());
        }
    }
}
