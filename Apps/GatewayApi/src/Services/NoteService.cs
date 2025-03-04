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
namespace HealthGateway.GatewayApi.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using HealthGateway.Common.Factories;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.GatewayApi.Models;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    public class NoteService : INoteService
    {
        private readonly IGatewayApiMappingService mappingService;
        private readonly ICryptoDelegate cryptoDelegate;
        private readonly ILogger logger;
        private readonly INoteDelegate noteDelegate;
        private readonly IUserProfileDelegate profileDelegate;
        private readonly IPatientRepository patientRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="noteDelegate">Injected Note delegate.</param>
        /// <param name="profileDelegate">Injected Profile delegate.</param>
        /// <param name="cryptoDelegate">Injected Crypto delegate.</param>
        /// <param name="patientRepository">Injected patient repository provider.</param>
        /// <param name="mappingService">The injected mapping service.</param>
        public NoteService(
            ILogger<NoteService> logger,
            INoteDelegate noteDelegate,
            IUserProfileDelegate profileDelegate,
            ICryptoDelegate cryptoDelegate,
            IPatientRepository patientRepository,
            IGatewayApiMappingService mappingService)
        {
            this.logger = logger;
            this.noteDelegate = noteDelegate;
            this.profileDelegate = profileDelegate;
            this.cryptoDelegate = cryptoDelegate;
            this.patientRepository = patientRepository;
            this.mappingService = mappingService;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<UserNote>> CreateNoteAsync(UserNote userNote, CancellationToken ct = default)
        {
            UserProfile? profile = await this.profileDelegate.GetUserProfileAsync(userNote.HdId, ct: ct);
            string? key = profile?.EncryptionKey;
            if (key == null)
            {
                this.logger.LogError("Profile does not have an encryption key");
                return RequestResultFactory.Error<UserNote>(ErrorType.InvalidState, "Profile Key not set");
            }

            Note note = this.mappingService.MapToNote(userNote, key);

            DbResult<Note> dbNote = await this.noteDelegate.AddNoteAsync(note, ct: ct);

            if (dbNote.Status != DbStatusCode.Created)
            {
                return RequestResultFactory.ServiceError<UserNote>(ErrorType.CommunicationInternal, ServiceType.Database, dbNote.Message);
            }

            return RequestResultFactory.Success(this.mappingService.MapToUserNote(dbNote.Payload, key));
        }

        /// <inheritdoc/>
        public async Task<RequestResult<IEnumerable<UserNote>>> GetNotesAsync(string hdId, int page = 0, int pageSize = 500, CancellationToken ct = default)
        {
            if (!await this.patientRepository.CanAccessDataSourceAsync(hdId, DataSource.Note, ct))
            {
                return new RequestResult<IEnumerable<UserNote>>
                {
                    ResultStatus = ResultType.Success,
                    ResourcePayload = Enumerable.Empty<UserNote>(),
                    TotalResultCount = 0,
                };
            }

            int offset = page * pageSize;
            DbResult<IList<Note>> dbNotes = await this.noteDelegate.GetNotesAsync(hdId, offset, pageSize, ct);

            UserProfile? profile = await this.profileDelegate.GetUserProfileAsync(hdId, ct: ct);
            string? key = profile?.EncryptionKey;

            // If there is no key yet, generate one and store it in the profile. Only valid while not all profiles have an encryption key.
            if (key == null)
            {
                key = await this.EncryptFirstTimeAsync(profile, dbNotes.Payload, ct);
                dbNotes = await this.noteDelegate.GetNotesAsync(hdId, offset, pageSize, ct);
            }

            if (dbNotes.Status != DbStatusCode.Read)
            {
                return RequestResultFactory.ServiceError<IEnumerable<UserNote>>(ErrorType.CommunicationInternal, ServiceType.Database, dbNotes.Message);
            }

            return RequestResultFactory.Success(
                dbNotes.Payload.Select(c => this.mappingService.MapToUserNote(c, key)),
                dbNotes.Payload.Count,
                page,
                pageSize);
        }

        /// <inheritdoc/>
        public async Task<RequestResult<UserNote>> UpdateNoteAsync(UserNote userNote, CancellationToken ct = default)
        {
            UserProfile? profile = await this.profileDelegate.GetUserProfileAsync(userNote.HdId, ct: ct);
            string? key = profile?.EncryptionKey;
            if (key == null)
            {
                this.logger.LogError("Profile does not have an encryption key");
                return RequestResultFactory.Error<UserNote>(ErrorType.InvalidState, "Profile Key not set");
            }

            Note note = this.mappingService.MapToNote(userNote, key);
            DbResult<Note> dbResult = await this.noteDelegate.UpdateNoteAsync(note, ct: ct);

            if (dbResult.Status != DbStatusCode.Updated)
            {
                return RequestResultFactory.ServiceError<UserNote>(ErrorType.CommunicationInternal, ServiceType.Database, dbResult.Message);
            }

            return RequestResultFactory.Success(this.mappingService.MapToUserNote(dbResult.Payload, key));
        }

        /// <inheritdoc/>
        public async Task<RequestResult<UserNote>> DeleteNoteAsync(UserNote userNote, CancellationToken ct = default)
        {
            UserProfile? profile = await this.profileDelegate.GetUserProfileAsync(userNote.HdId, ct: ct);
            string? key = profile?.EncryptionKey;
            if (key == null)
            {
                this.logger.LogError("Profile does not have an encryption key");
                return RequestResultFactory.Error<UserNote>(ErrorType.InvalidState, "Profile Key not set");
            }

            Note note = this.mappingService.MapToNote(userNote, key);
            DbResult<Note> dbResult = await this.noteDelegate.DeleteNoteAsync(note, ct: ct);

            if (dbResult.Status != DbStatusCode.Deleted)
            {
                return RequestResultFactory.ServiceError<UserNote>(ErrorType.CommunicationInternal, ServiceType.Database, dbResult.Message);
            }

            return RequestResultFactory.Success(this.mappingService.MapToUserNote(dbResult.Payload, key));
        }

        private async Task<string> EncryptFirstTimeAsync(UserProfile profile, IList<Note> dbNotes, CancellationToken ct)
        {
            this.logger.LogDebug("Generating encryption key for profile");
            string key = this.cryptoDelegate.GenerateKey();
            profile.EncryptionKey = key;
            DbResult<UserProfile> userProfileUpdateResult = await this.profileDelegate.UpdateAsync(profile, false, ct);
            if (userProfileUpdateResult.Status != DbStatusCode.Deferred)
            {
                throw new DatabaseException(userProfileUpdateResult.Message);
            }

            foreach (Note note in dbNotes)
            {
                note.Title = this.cryptoDelegate.Encrypt(key, note.Title ?? string.Empty);
                note.Text = this.cryptoDelegate.Encrypt(key, note.Text ?? string.Empty);
            }

            DbResult<IEnumerable<Note>> batchUpdateResult = await this.noteDelegate.BatchUpdateAsync(dbNotes, ct: ct);
            if (batchUpdateResult.Status != DbStatusCode.Updated)
            {
                throw new DatabaseException(batchUpdateResult.Message);
            }

            return key;
        }
    }
}
