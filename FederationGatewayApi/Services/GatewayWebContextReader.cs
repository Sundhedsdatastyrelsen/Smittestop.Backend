using AutoMapper;
using FederationGatewayApi.Contracts;
using FederationGatewayApi.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace FederationGatewayApi.Services
{
    public class GatewayWebContextReader : IGatewayWebContextReader
    {
        private readonly IMapper _mapper;
        private readonly ILogger<IGatewayWebContextReader> _logger;

        public GatewayWebContextReader(IMapper mapper, ILogger<IGatewayWebContextReader> logger)
        {
            _mapper = mapper;
            _logger = logger;
        }

        public string ReadHttpContextStream(HttpResponseMessage webContext)
        {
            using var reader = new StreamReader(webContext.Content.ReadAsStreamAsync().Result);
            return reader.ReadToEndAsync().Result;
        }

        public IList<TemporaryExposureKeyGatewayDto> GetItemsFromRequest(string responseBody)
        {
            try
            {
                var batchProtoObject = Models.Proto.TemporaryExposureKeyGatewayBatchDto.Parser.ParseJson(responseBody);
                var batchKeys = batchProtoObject.Keys.ToList();
                var mappedKeys = batchKeys.Select(entityKey => _mapper.Map<TemporaryExposureKeyGatewayDto>(entityKey)).ToList();
                return mappedKeys.ToList();
            }
            catch (Exception e)
            {
                _logger.LogError($"{e.Message} - {e.StackTrace}");
                throw;
            }
            
        }
    }
}
