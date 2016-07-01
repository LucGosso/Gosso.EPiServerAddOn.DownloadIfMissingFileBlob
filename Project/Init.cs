using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using System.Linq;
using System.Web.Routing;

namespace Gosso.EPiServerAddOn.DownloadIfMissingFileBlob
{
    [EPiServer.Framework.InitializableModule]
    public class Init : IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {
            RouteTable.Routes.Add(new Route
            (
                Provider.DefaultUrl,
                new UrlResolverHelper()
            ));
        }

        public void Uninitialize(InitializationEngine context)
        {
            RouteTable.Routes.Remove(RouteTable.Routes.OfType<Route>().FirstOrDefault(x => x.Url == Provider.DefaultUrl));
        }
    }
}
