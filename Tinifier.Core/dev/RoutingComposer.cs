using System.Net;
using System.Net.Http;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Web.WebApi;

namespace Umbraco8.Components
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class MapHttpRoutesComposer : ComponentComposer<MapHttpRoutesComponent>
    {
        // nothing needed to be done here!
    }
    public class MapHttpRoutesComponent : IComponent
    {
        public MapHttpRoutesComponent()
        {
            GlobalConfiguration.Configuration.MapHttpAttributeRoutes();

        }
        // initialize: runs once when Umbraco starts
        public void Initialize()
        {
        }

        public void Terminate()
        {

        }
    }
}
