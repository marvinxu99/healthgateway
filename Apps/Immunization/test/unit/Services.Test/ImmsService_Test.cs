// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------
// <auto-generated />
namespace HealthGateway.Immunization.Test.Service
{
    using DeepEqual.Syntax;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Immunization.Delegates;
    using HealthGateway.Immunization.Models;
    using HealthGateway.Immunization.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Xunit;

    public class ImmsService_Test
    {
        private readonly IConfiguration configuration;

        public ImmsService_Test()
        {
            this.configuration = GetIConfigurationRoot(string.Empty);
        }

        [Fact]
        public void ValidateImmunization()
        {

            var mockDelegate = new Mock<IImmunizationDelegate>();
            RequestResult<PHSAResult<ImmunizationResponse>> delegateResult = new RequestResult<PHSAResult<ImmunizationResponse>>()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                ResourcePayload = new PHSAResult<ImmunizationResponse>()
                {
                    LoadState = new PHSALoadState() { RefreshInProgress = false, },
                    Result = new List<ImmunizationResponse>()
                    {
                        new ImmunizationResponse()
                        {
                            Id = Guid.NewGuid(),
                            Name = "MockImmunization",
                            OccurrenceDateTime = DateTime.Now,
                            SourceSystemId = "MockSourceID"
                        },
                    },
                },
                PageIndex = 0,
                PageSize = 5,
                TotalResultCount = 1,
            };
            RequestResult<ImmunizationResult> expectedResult = new RequestResult<ImmunizationResult>()
            {
                ResultStatus = delegateResult.ResultStatus,
                ResourcePayload = new ImmunizationResult()
                {
                    LoadState = LoadStateModel.FromPHSAModel(delegateResult.ResourcePayload.LoadState),
                    Immunizations = ImmunizationModel.FromPHSAModelList(delegateResult.ResourcePayload.Result),
                },
                PageIndex = delegateResult.PageIndex,
                PageSize = delegateResult.PageSize,
                TotalResultCount = delegateResult.TotalResultCount,
            };

            mockDelegate.Setup(s => s.GetImmunizations(It.IsAny<string>(), It.IsAny<int>())).Returns(Task.FromResult(delegateResult));
            IImmunizationService service = new ImmunizationService(new Mock<ILogger<ImmunizationService>>().Object, mockDelegate.Object);

            var actualResult = service.GetImmunizations(string.Empty, 0);
            Assert.True(expectedResult.IsDeepEqual(actualResult.Result));
        }

        [Fact]
        public void ValidateImmunizationError()
        {

            var mockDelegate = new Mock<IImmunizationDelegate>();
            RequestResult<PHSAResult<ImmunizationResponse>> delegateResult = new RequestResult<PHSAResult<ImmunizationResponse>>()
            {
                ResultStatus = Common.Constants.ResultType.Error,
                ResultError = new RequestResultError()
                {
                    ResultMessage = "Mock Error",
                    ErrorCode = "MOCK_BAD_ERROR",
                },
            };
            RequestResult<IEnumerable<ImmunizationModel>> expectedResult = new RequestResult<IEnumerable<ImmunizationModel>>()
            {
                ResultStatus = delegateResult.ResultStatus,
                ResultError = delegateResult.ResultError,
            };

            mockDelegate.Setup(s => s.GetImmunizations(It.IsAny<string>(), It.IsAny<int>())).Returns(Task.FromResult(delegateResult));
            IImmunizationService service = new ImmunizationService(new Mock<ILogger<ImmunizationService>>().Object, mockDelegate.Object);

            var actualResult = service.GetImmunizations(string.Empty, 0);
            Assert.True(expectedResult.IsDeepEqual(actualResult.Result));
        }

        private static IConfigurationRoot GetIConfigurationRoot(string outputPath)
        {
            return new ConfigurationBuilder()
                // .SetBasePath(outputPath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddJsonFile("appsettings.local.json", optional: true)
                .Build();
        }
    }
}
