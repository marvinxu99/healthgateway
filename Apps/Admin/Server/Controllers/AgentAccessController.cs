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
namespace HealthGateway.Admin.Server.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Asp.Versioning;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Admin.Server.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API to manage agent access to the admin website.
    /// </summary>
    /// <param name="agentAccessService">The injected agent access service.</param>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = "AdminUser")]
    public class AgentAccessController(IAgentAccessService agentAccessService) : ControllerBase
    {
        /// <summary>
        /// Provisions agent access to the admin website.
        /// </summary>
        /// <param name="agent">The agent model.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The created agent.</returns>
        /// <response code="200">Returns the created agent.</response>
        /// <response code="401">The client must authenticate itself to perform the operation.</response>
        /// <response code="403">
        /// The client does not have access rights to perform the operation; that is, it is unauthorized.
        /// Unlike 401, the client's identity is known to the server.
        /// </response>
        /// <response code="502">Unable to get response from Keycloak.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status502BadGateway, Type = typeof(ProblemDetails))]
        public async Task<AdminAgent> ProvisionAgentAccess(AdminAgent agent, CancellationToken ct)
        {
            return await agentAccessService.ProvisionAgentAccessAsync(agent, ct);
        }

        /// <summary>
        /// Retrieves agents with access to the admin website that match the query.
        /// </summary>
        /// <param name="query">The query string to match agents against.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The collection of matching agents.</returns>
        /// <response code="200">Returns the collection of matching agents.</response>
        /// <response code="401">The client must authenticate itself to get the requested resource.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        /// <response code="502">Unable to get response from Keycloak.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status502BadGateway, Type = typeof(ProblemDetails))]
        public async Task<IEnumerable<AdminAgent>> GetAgents(string query, CancellationToken ct)
        {
            return await agentAccessService.GetAgentsAsync(query, ct: ct);
        }

        /// <summary>
        /// Updates agent access to the admin website.
        /// </summary>
        /// <param name="agent">The agent model.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The updated agent.</returns>
        /// <response code="200">Returns the updated agent.</response>
        /// <response code="401">The client must authenticate itself to perform the operation.</response>
        /// <response code="403">
        /// The client does not have access rights to perform the operation; that is, it is unauthorized.
        /// Unlike 401, the client's identity is known to the server.
        /// </response>
        /// <response code="502">Unable to get response from Keycloak.</response>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status502BadGateway, Type = typeof(ProblemDetails))]
        public async Task<AdminAgent> UpdateAgentAccess(AdminAgent agent, CancellationToken ct)
        {
            Activity.Current?.AddBaggage("AgentId", agent.Id.ToString());
            return await agentAccessService.UpdateAgentAccessAsync(agent, ct);
        }

        /// <summary>
        /// Removes an agent's access to the admin website.
        /// </summary>
        /// <param name="id">The unique identifier of the agent whose access should be terminated.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>An empty result.</returns>
        /// <response code="200">The agent no longer has access to the admin website.</response>
        /// <response code="401">The client must authenticate itself to perform the operation.</response>
        /// <response code="403">
        /// The client does not have access rights to perform the operation; that is, it is unauthorized.
        /// Unlike 401, the client's identity is known to the server.
        /// </response>
        /// <response code="502">Unable to get response from Keycloak.</response>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status502BadGateway, Type = typeof(ProblemDetails))]
        public async Task<IActionResult> RemoveAgentAccess(Guid id, CancellationToken ct)
        {
            Activity.Current?.AddBaggage("AgentId", id.ToString());
            await agentAccessService.RemoveAgentAccessAsync(id, ct);
            return this.Ok();
        }
    }
}
