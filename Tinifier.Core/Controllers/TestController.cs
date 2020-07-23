using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Tinifier.Core.Controllers;
using Tinifier.Core.Infrastructure;
using Tinifier.Core.Infrastructure.Enums;
using Tinifier.Core.Models;
using Tinifier.Core.Models.Db;
using Tinifier.Core.Repository.History;
using Tinifier.Core.Repository.Image;
using Tinifier.Core.Repository.State;
using Tinifier.Core.Services.Validation;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Events;
using Umbraco.Web.WebApi;

namespace Umbraco8.Components
{


    public class TestController : UmbracoApiController
    {
        IImageRepository imageRepository;
        IDeveloperLogger _logger;
        IValidationService _validationService;
        private readonly IStateRepository stateRepository;
        IHistoryRepository historyRepository;

        public TestController(IDeveloperLogger developerLogger, IValidationService validationService, IStateRepository stateRepository,
            IImageRepository imageRepository, IHistoryRepository historyRepository)
        {
            _logger = developerLogger;
            _validationService = validationService;
            this.stateRepository = stateRepository;
            this.imageRepository = imageRepository;
            this.historyRepository = historyRepository;
        }

        [HttpGet]
        [Route("Test")]
        public HttpResponseMessage Test()
        {

            TinyPNGResponseHistory tModel = new TinyPNGResponseHistory()
            {
                Id = 1,
                ImageId = "1156",
                IsOptimized = true,
                OccuredAt = DateTime.Now,
                OptimizedSize = 77872,
                OriginSize = 29744,
                Ratio = 0.382,
                Error = ""
            };
            //
            //historyRepository.Create(tModel);
            // historyRepository.Delete("1156");


            //var res = imageRepository.Get(1156);
            //imageRepository.Move(res, 1157);
            //return Request.CreateResponse(HttpStatusCode.OK, "Success");

            return Request.CreateResponse(HttpStatusCode.OK,
    new TNotification(PackageConstants.TinifyingFinished,
        "1/1 images were optimized. Enjoy the package? Click the message and rate us!", EventMessageType.Success)
    {
        url = "https://our.umbraco.org/projects/backoffice-extensions/tinifier/"
    });
        }
    }
}

