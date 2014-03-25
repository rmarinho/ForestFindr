using ForestFindr.XAMLayersService;

namespace ForestFindr.Services
{
    public static class SpatialServices
    {
		
        private static XAMLClient _xamlClient;
        public static XAMLClient XAMLService
        {
            get
            {
                if (_xamlClient == null)
                {
                    _xamlClient = new XAMLClient();
                }

                return _xamlClient;
            }
        } 

    }
}
