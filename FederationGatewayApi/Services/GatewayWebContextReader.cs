﻿using AutoMapper;
using FederationGatewayApi.Contracts;
using FederationGatewayApi.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace FederationGatewayApi.Services
{
    public class GatewayWebContextReader : IGatewayWebContextReader
    {
        private readonly IMapper _mapper;

        public GatewayWebContextReader(IMapper mapper)
        {
            _mapper = mapper;
        }
        public string ReadHttpContextStream(HttpResponseMessage webContext)
        {
            using (var reader = new StreamReader(webContext.Content.ReadAsStreamAsync().Result))
            {
                return reader.ReadToEndAsync().Result;
            }
        }

        public IList<TemporaryExposureKeyGatewayDto> GetItemsFromRequest(string responseBody)
        {
            var batchProtoObject = Models.Proto.TemporaryExposureKeyGatewayBatchDto.Parser.ParseJson(responseBody);
            List<TemporaryExposureKeyGatewayDto> ret = new List<TemporaryExposureKeyGatewayDto>();
            var batchKeys = batchProtoObject.Keys.ToList().Select(entityKey => _mapper.Map<TemporaryExposureKeyGatewayDto>(entityKey)).ToList();
            return batchKeys ?? new List<TemporaryExposureKeyGatewayDto>();
        }
    }
}