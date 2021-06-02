using AutoMapper;
using DIGNDB.App.SmitteStop.Core.Contracts;
using DIGNDB.App.SmitteStop.Domain.Db;
using DIGNDB.App.SmitteStop.Domain.Enums;
using FederationGatewayApi.Contracts;
using FederationGatewayApi.Models;
using System;
using System.Collections.Generic;

namespace FederationGatewayApi.Services
{
    public class KeyFilter : IKeyFilter
    {
        private readonly IMapper _mapper;
        private readonly IKeyValidator _keyValidator;
        
        public KeyFilter(IMapper mapper, IKeyValidator keyValidator)
        {
            _mapper = mapper;
            _keyValidator = keyValidator;
        }

        public IList<TemporaryExposureKey> ValidateKeys(IList<TemporaryExposureKey> temporaryExposureKeys, out IList<string> validationErrors)
        {
            validationErrors = new List<string>();
            IList<TemporaryExposureKey> acceptedKeys = new List<TemporaryExposureKey>();

            foreach (TemporaryExposureKey exposureKey in temporaryExposureKeys)
            {
                if (_keyValidator.ValidateKeyGateway(exposureKey, out var errorMessage))
                {
                    acceptedKeys.Add(exposureKey);
                }
                else
                {
                    validationErrors.Add($"The key {"0x" + BitConverter.ToString(exposureKey.KeyData).Replace("-", "")} was  rejected: {Environment.NewLine}\t{errorMessage}");
                }
            }
            return acceptedKeys;
        }

        public IList<TemporaryExposureKey> MapKeys(IList<TemporaryExposureKeyGatewayDto> temporaryExposureKeys)
        {
            IList<TemporaryExposureKey> mappedKeys = new List<TemporaryExposureKey>();

            foreach (TemporaryExposureKeyGatewayDto exposureKeyDto in temporaryExposureKeys)
            {
                var exposureKey = _mapper.Map<TemporaryExposureKeyGatewayDto, TemporaryExposureKey>(exposureKeyDto);
                exposureKey.KeySource = KeySource.Gateway;

                mappedKeys.Add(exposureKey);
            }

            return mappedKeys;
        }
    }
}
