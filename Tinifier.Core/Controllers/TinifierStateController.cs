using System.Net;
using System.Net.Http;
using System.Web.Http;
using Tinifier.Core.Services.State;
using Umbraco.Web.WebApi;

namespace Tinifier.Core.Controllers
{
    public class TinifierStateController : UmbracoAuthorizedApiController
    {
        private readonly IStateService _stateService;

        public TinifierStateController(IStateService stateService)
        {
            _stateService = stateService;
        }

        /// <summary>
        /// Get current tinifing state
        /// </summary>
        /// <returns>Response(StatusCode, state)</returns>
        [HttpGet]
        public HttpResponseMessage GetCurrentTinifingState()
        {
            var state = _stateService.GetState();
            return Request.CreateResponse(HttpStatusCode.OK, state);
        }

        [HttpDelete]
        public HttpResponseMessage DeleteActiveState()
        {
            _stateService.Delete();
            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}