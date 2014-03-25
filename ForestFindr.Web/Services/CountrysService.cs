
namespace ForestFindr.Web.Services
{
    using System.Linq;
    using System.ServiceModel.DomainServices.EntityFramework;
    using System.ServiceModel.DomainServices.Hosting;
    using ForestFindr.Web.Model;


    // Implements application logic using the ForestFindrEntities context.
    // TODO: Add your application logic to these methods or in additional methods.
    // TODO: Wire up authentication (Windows/ASP.NET Forms) and uncomment the following to disable anonymous access
    // Also consider adding roles to restrict access as appropriate.
    // [RequiresAuthentication]
    [EnableClientAccess()]
    public class CountrysService : LinqToEntitiesDomainService<ForestFindrEntities>
    {

        // TODO:
        // Consider constraining the results of your query method.  If you need additional input you can
        // add parameters to this method or create additional query methods with different names.
        // To support paging you will need to add ordering to the 'Countrys' query.
        public IQueryable<Country> GetCountrys()
        {
            return this.ObjectContext.Countrys;
        }
    }
}


