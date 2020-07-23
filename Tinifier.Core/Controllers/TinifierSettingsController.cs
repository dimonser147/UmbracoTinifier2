using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using Tinifier.Core.Infrastructure;
using Tinifier.Core.Models;
using Tinifier.Core.Models.Db;
using Tinifier.Core.Services.Settings;
using Umbraco.Core.Events;
using Umbraco.Web.WebApi;

namespace Tinifier.Core.Controllers
{
    public class TinifierSettingsController : UmbracoAuthorizedApiController
    {
        private readonly ISettingsService _settingsService;

        public TinifierSettingsController(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        /// <summary>
        /// Get user settings
        /// </summary>
        /// <returns>Response(StatusCode, settings)</returns>
        [HttpGet]
        //[Route("umbraco/api/GetTSetting")]
        public HttpResponseMessage GetTSetting()
        {
            //Thread.Sleep(2000);
            var tsetting = _settingsService.GetSettings() ?? new TSetting();
            return Request.CreateResponse(HttpStatusCode.OK, tsetting);
        }

        /// <summary>
        /// Post user settings
        /// </summary>
        /// <param name="setting">TSetting</param>
        /// <returns>Response(StatusCode, message)</returns>
        [HttpPost]
        public HttpResponseMessage PostTSetting(TSetting setting)
        {
            if (ModelState.IsValid)
            {
                _settingsService.CreateSettings(setting);
                return Request.CreateResponse(HttpStatusCode.Created, new TNotification("Created", PackageConstants.ApiKeyMessage, EventMessageType.Success));
            }

            return Request.CreateResponse(HttpStatusCode.BadRequest,
                new TNotification("Ooops", PackageConstants.ApiKeyError, EventMessageType.Error));
        }
    }
}