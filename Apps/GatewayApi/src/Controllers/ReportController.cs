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
namespace HealthGateway.GatewayApi.Controllers
{
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Asp.Versioning;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API to handle reports.
    /// </summary>
    [ApiVersion("1.0")]
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class ReportController
    {
        private readonly IReportService reportService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportController"/> class.
        /// </summary>
        /// <param name="reportService">The injected report service.</param>
        public ReportController(IReportService reportService)
        {
            this.reportService = reportService;
        }

        /// <summary>
        /// Gets a report based on the request provided.
        /// </summary>
        /// <param name="reportRequest">The report request model.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The report data.</returns>
        /// <response code="200">Returns the report data.</response>
        [HttpPost]
        public async Task<RequestResult<ReportModel>> GenerateReport([FromBody] ReportRequestModel reportRequest, CancellationToken ct)
        {
            Activity.Current?.AddBaggage("ReportType", EnumUtility.ToEnumString(reportRequest.Type, true));
            Activity.Current?.AddBaggage("TemplateType", EnumUtility.ToEnumString(reportRequest.Template, true));
            return await this.reportService.GetReportAsync(reportRequest, ct);
        }
    }
}
