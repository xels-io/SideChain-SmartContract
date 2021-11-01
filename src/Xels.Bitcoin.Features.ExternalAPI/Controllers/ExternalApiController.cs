﻿using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using NLog;
using Xels.Bitcoin.Features.ExternalApi;
using Xels.Bitcoin.Utilities.JsonErrors;

namespace Xels.Features.ExternalApi.Controllers
{
    [ApiVersion("1")]
    [Route("api/[controller]")]
    public class ExternalApiController : Controller
    {
        public const string EstimateConversionGasEndpoint = "estimateconversiongas";
        public const string EstimateConversionFeeEndpoint = "estimateconversionfee";
        public const string GasPriceEndpoint = "gasprice";
        public const string XelsPriceEndpoint = "xelsprice";
        public const string EthereumPriceEndpoint = "ethereumprice";

        private readonly IExternalApiPoller externalApiPoller;

        private readonly ILogger logger;

        public ExternalApiController(IExternalApiPoller externalApiPoller)
        {
            this.externalApiPoller = externalApiPoller;
            this.logger = LogManager.GetCurrentClassLogger();
        }

        [Route(EstimateConversionGasEndpoint)]
        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult EstimateConversionGas()
        {
            try
            {
                return this.Json(this.externalApiPoller.EstimateConversionTransactionGas());
            }
            catch (Exception e)
            {
                this.logger.Error("Exception occurred: {0}", e.ToString());
                return ErrorHelpers.BuildErrorResponse(HttpStatusCode.BadRequest, e.Message, e.ToString());
            }
        }

        [Route(EstimateConversionFeeEndpoint)]
        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult EstimateConversionFee()
        {
            try
            {
                return this.Json(this.externalApiPoller.EstimateConversionTransactionFee());
            }
            catch (Exception e)
            {
                this.logger.Error("Exception occurred: {0}", e.ToString());
                return ErrorHelpers.BuildErrorResponse(HttpStatusCode.BadRequest, e.Message, e.ToString());
            }
        }

        [Route(GasPriceEndpoint)]
        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GasPrice()
        {
            try
            {
                return this.Json(this.externalApiPoller.GetGasPrice());
            }
            catch (Exception e)
            {
                this.logger.Error("Exception occurred: {0}", e.ToString());
                return ErrorHelpers.BuildErrorResponse(HttpStatusCode.BadRequest, e.Message, e.ToString());
            }
        }

        [Route(XelsPriceEndpoint)]
        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult XelsPrice()
        {
            try
            {
                return this.Json(this.externalApiPoller.GetXelsPrice());
            }
            catch (Exception e)
            {
                this.logger.Error("Exception occurred: {0}", e.ToString());
                return ErrorHelpers.BuildErrorResponse(HttpStatusCode.BadRequest, e.Message, e.ToString());
            }
        }

        [Route(EthereumPriceEndpoint)]
        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult EthereumPrice()
        {
            try
            {
                return this.Json(this.externalApiPoller.GetEthereumPrice());
            }
            catch (Exception e)
            {
                this.logger.Error("Exception occurred: {0}", e.ToString());
                return ErrorHelpers.BuildErrorResponse(HttpStatusCode.BadRequest, e.Message, e.ToString());
            }
        }
    }
}