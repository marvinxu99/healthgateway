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
namespace HealthGateway.Immunization.Test.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Immunization.Delegates;
    using HealthGateway.Immunization.Models;
    using HealthGateway.Immunization.Models.PHSA;
    using HealthGateway.Immunization.Models.PHSA.Recommendation;
    using HealthGateway.Immunization.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    public class ImmsService_Test
    {
        private readonly IConfiguration configuration;

        private readonly string recomendationSetId = "set-recomendation-id";
        private readonly string diseaseEligibleDateString = "2021-02-02";
        private readonly string diseaseName = "Human papillomavirus infection";
        private readonly string vaccineName = "Human Papillomavirus-HPV9 Vaccine";
        private readonly string antigenName = "HPV-9";

        public ImmsService_Test()
        {
            this.configuration = GetIConfigurationRoot();
        }

        [Fact]
        public void ValidateImmunization()
        {
            var mockDelegate = new Mock<IImmunizationDelegate>();
            RequestResult<PhsaResult<ImmunizationResponse>> delegateResult = new RequestResult<PhsaResult<ImmunizationResponse>>()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                ResourcePayload = new PhsaResult<ImmunizationResponse>()
                {
                    LoadState = new PhsaLoadState() { RefreshInProgress = false, },
                    Result = new ImmunizationResponse(
                        new List<ImmunizationViewResponse>()
                        {
                            new ImmunizationViewResponse()
                            {
                                Id = Guid.NewGuid(),
                                Name = "MockImmunization",
                                OccurrenceDateTime = DateTime.Now,
                                SourceSystemId = "MockSourceID",
                            },
                        },
                        new List<ImmunizationRecommendationResponse>()),
                },
                PageIndex = 0,
                PageSize = 5,
                TotalResultCount = 1,
            };
            RequestResult<ImmunizationResult> expectedResult = new RequestResult<ImmunizationResult>()
            {
                ResultStatus = delegateResult.ResultStatus,
                ResourcePayload = new ImmunizationResult(
                    LoadStateModel.FromPHSAModel(delegateResult.ResourcePayload.LoadState),
                    ImmunizationEvent.FromPHSAModelList(delegateResult.ResourcePayload.Result.ImmunizationViews),
                    new List<ImmunizationRecommendation>()),
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
        public void ValidateRecomendation()
        {
            var immzRecommendationResponse = this.GetImmzRecommendationResponse();
            var mockDelegate = new Mock<IImmunizationDelegate>();
            RequestResult<PhsaResult<ImmunizationResponse>> delegateResult = GetPhsaResult(immzRecommendationResponse);
            RequestResult<ImmunizationResult> expectedResult = new RequestResult<ImmunizationResult>()
            {
                ResultStatus = delegateResult.ResultStatus,
                ResourcePayload = new ImmunizationResult(
                    LoadStateModel.FromPHSAModel(delegateResult?.ResourcePayload?.LoadState),
                    new List<ImmunizationEvent>(),
                    ImmunizationRecommendation.FromPHSAModelList(delegateResult?.ResourcePayload?.Result?.Recommendations)),
                PageIndex = delegateResult.PageIndex,
                PageSize = delegateResult.PageSize,
                TotalResultCount = delegateResult.TotalResultCount,
            };

            mockDelegate.Setup(s => s.GetImmunizations(It.IsAny<string>(), It.IsAny<int>())).Returns(Task.FromResult(delegateResult));
            IImmunizationService service = new ImmunizationService(new Mock<ILogger<ImmunizationService>>().Object, mockDelegate.Object);

            var actualResult = service.GetImmunizations(string.Empty, 0);
            Assert.True(expectedResult.IsDeepEqual(actualResult.Result));
            Assert.Equal(1, expectedResult.ResourcePayload.Recommendations.Count);
            var recomendationResult = expectedResult.ResourcePayload.Recommendations[0];
            Assert.Equal(this.recomendationSetId, recomendationResult.RecommendationSetId);
            Assert.Equal(this.vaccineName, recomendationResult.Immunization.Name);
            Assert.Collection(
                recomendationResult.Immunization.ImmunizationAgents,
                item => Assert.Equal(this.antigenName, item.Name));
            Assert.Equal(this.antigenName, recomendationResult.Immunization.ImmunizationAgents.First().Name);
            Assert.Collection(
                recomendationResult?.TargetDiseases,
                item => Assert.Equal(immzRecommendationResponse.Recommendations.First().TargetDisease?.TargetDiseaseCodes?.FirstOrDefault()?.Code, item.Code));
            Assert.Equal(this.diseaseName, recomendationResult?.TargetDiseases.First().Name);
            Assert.Equal(DateTime.Parse(this.diseaseEligibleDateString, CultureInfo.CurrentCulture), recomendationResult?.DisseaseEligibleDate);
            Assert.Null(recomendationResult?.DiseaseDueDate);
            Assert.Null(recomendationResult?.AgentDueDate);
            Assert.Null(recomendationResult?.AgentEligibleDate);
        }

        [Fact]
        public void ValidateImmunizationError()
        {
            var mockDelegate = new Mock<IImmunizationDelegate>();
            RequestResult<PhsaResult<ImmunizationResponse>> delegateResult = new RequestResult<PhsaResult<ImmunizationResponse>>()
            {
                ResultStatus = Common.Constants.ResultType.Error,
                ResultError = new RequestResultError()
                {
                    ResultMessage = "Mock Error",
                    ErrorCode = "MOCK_BAD_ERROR",
                },
            };
            RequestResult<IEnumerable<ImmunizationEvent>> expectedResult = new RequestResult<IEnumerable<ImmunizationEvent>>()
            {
                ResultStatus = delegateResult.ResultStatus,
                ResultError = delegateResult.ResultError,
            };

            mockDelegate.Setup(s => s.GetImmunizations(It.IsAny<string>(), It.IsAny<int>())).Returns(Task.FromResult(delegateResult));
            IImmunizationService service = new ImmunizationService(new Mock<ILogger<ImmunizationService>>().Object, mockDelegate.Object);

            var actualResult = service.GetImmunizations(string.Empty, 0);
            Assert.True(expectedResult.IsDeepEqual(actualResult.Result));
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            return new ConfigurationBuilder()

                // .SetBasePath(outputPath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddJsonFile("appsettings.local.json", optional: true)
                .Build();
        }

        private static RequestResult<PhsaResult<ImmunizationResponse>> GetPhsaResult(ImmunizationRecommendationResponse immzRecommendationResponse)
        {
            return new RequestResult<PhsaResult<ImmunizationResponse>>()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                ResourcePayload = new PhsaResult<ImmunizationResponse>()
                {
                    LoadState = new () { RefreshInProgress = false, },
                    Result = new (
                        new List<ImmunizationViewResponse>(),
                        new List<ImmunizationRecommendationResponse>()
                        {
                            immzRecommendationResponse,
                        }),
                },
                PageIndex = 0,
                PageSize = 5,
                TotalResultCount = 1,
            };
        }

        private ImmunizationRecommendationResponse GetImmzRecommendationResponse()
        {
            ImmunizationRecommendationResponse immzRecommendationResponse = new ImmunizationRecommendationResponse()
            {
                ForecastCreationDate = DateTime.Now,
                RecommendationId = this.recomendationSetId,
                RecommendationSourceSystem = "MockSourceSystem",
                RecommendationSourceSystemId = "MockSourceID",
            };

            RecommendationResponse recommendationResponse = new RecommendationResponse();
            recommendationResponse.ForecastStatus.ForecastStatusText = "Eligible";
            recommendationResponse.TargetDisease.TargetDiseaseCodes.Add(new SystemCode()
            {
                Code = "240532009",
                CommonType = "DiseaseCode",
                Display = this.diseaseName,
                System = "https://ehealthbc.ca/NamingSystem/ca-bc-panorama-immunization-disease-code",
            });
            recommendationResponse.VaccineCode.VaccineCodeText = this.vaccineName;
            recommendationResponse.VaccineCode.VaccineCodes.Add(new SystemCode()
            {
                Code = "BCYSCT_AN032",
                CommonType = "AntiGenCode",
                Display = this.antigenName,
                System = "https://ehealthbc.ca/NamingSystem/ca-bc-panorama-immunization-antigen-code",
            });

            recommendationResponse.DateCriterions.Add(new DateCriterion()
            {
                DateCriterionCode = new DateCriterionCode()
                {
                    Text = "Forecast by Disease Eligible Date",
                },
                Value = this.diseaseEligibleDateString,
            });

            immzRecommendationResponse.Recommendations.Add(recommendationResponse);
            return immzRecommendationResponse;
        }
    }
}
