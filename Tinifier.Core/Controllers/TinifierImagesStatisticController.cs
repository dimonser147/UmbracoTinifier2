using System.Net;
using System.Net.Http;
using System.Web.Http;
using Tinifier.Core.Infrastructure;
using Tinifier.Core.Services.History;
using Tinifier.Core.Services.Settings;
using Tinifier.Core.Services.Statistic;
using Umbraco.Web.WebApi;

namespace Tinifier.Core.Controllers
{

    public class TinifierImagesStatisticController : UmbracoAuthorizedApiController
    {
        private readonly ISettingsService _settingsService;
        private readonly IStatisticService _statisticService;
        private readonly IHistoryService _historyService;

        public TinifierImagesStatisticController(ISettingsService settingsService, IStatisticService statisticService,
            IHistoryService historyService)
        {
            _settingsService = settingsService;
            _statisticService = statisticService;
            _historyService = historyService;
        }

        /// <summary>
        /// Get Images Statistic
        /// </summary>
        /// <returns>Response(StatusCode, {statistic, tsettings, history, requestLimit})</returns>
        [HttpGet]
        public HttpResponseMessage GetStatistic()
        {
            var statistic = _statisticService.GetStatistic();
            var tsetting = _settingsService.GetSettings();
            var history = _historyService.GetStatisticByDays();

            return Request.CreateResponse(HttpStatusCode.OK, new { statistic, tsetting, history, PackageConstants.MonthlyRequestsLimit });
        }
    }
}