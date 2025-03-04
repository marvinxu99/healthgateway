//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace HealthGateway.Common.Delegates.PHSA
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Api;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using Microsoft.Extensions.Logging;
    using Refit;

    /// <summary>
    /// Implementation that uses HTTP to sends notification settings to PHSA.
    /// </summary>
    public class RestNotificationSettingsDelegate : INotificationSettingsDelegate
    {
        private readonly ILogger<RestNotificationSettingsDelegate> logger;
        private readonly INotificationSettingsApi notificationSettingsApi;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestNotificationSettingsDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="notificationSettingsApi">The injected Refit API.</param>
        public RestNotificationSettingsDelegate(
            ILogger<RestNotificationSettingsDelegate> logger,
            INotificationSettingsApi notificationSettingsApi)
        {
            this.logger = logger;
            this.notificationSettingsApi = notificationSettingsApi;
        }

        private static ActivitySource ActivitySource { get; } = new(typeof(RestNotificationSettingsDelegate).FullName);

        /// <inheritdoc/>
        public async Task<RequestResult<NotificationSettingsResponse>> SetNotificationSettingsAsync(
            NotificationSettingsRequest notificationSettings,
            string bearerToken,
            CancellationToken ct = default)
        {
            using Activity? activity = ActivitySource.StartActivity();

            RequestResult<NotificationSettingsResponse> retVal = new()
            {
                ResultStatus = ResultType.Error,
            };

            try
            {
                this.logger.LogDebug("Sending notification settings update to PHSA");
                NotificationSettingsResponse notificationSettingsResponse = await this.notificationSettingsApi
                    .SetNotificationSettingsAsync(notificationSettings, notificationSettings.SubjectHdid, bearerToken, ct);

                retVal.ResultStatus = ResultType.Success;
                retVal.TotalResultCount = 1;
                retVal.ResourcePayload = notificationSettingsResponse;
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                this.logger.LogError(e, "Error sending notification settings update to PHSA");

                string errorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa);
                if (e is ApiException { StatusCode: HttpStatusCode.UnprocessableEntity })
                {
                    errorCode = ErrorTranslator.ServiceError(ErrorType.SmsInvalid, ServiceType.Phsa);
                }

                retVal.ResultError = new RequestResultError
                {
                    ResultMessage = "Error while sending notification settings to PHSA",
                    ErrorCode = errorCode,
                };
            }

            return retVal;
        }
    }
}
