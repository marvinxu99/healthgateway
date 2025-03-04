﻿// -------------------------------------------------------------------------
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
namespace HealthGateway.Admin.Client.Services
{
    using System;

    /// <summary>
    /// Injectable service to provide data conversion methods.
    /// </summary>
    public interface IDateConversionService
    {
        /// <summary>
        /// Converts UTC datetime values to the system configured timezone.
        /// </summary>
        /// <param name="utcDateTime">A UTC DateTime instance.</param>
        /// <returns>A DateTime instance in the configured timezone.</returns>
        public DateTime ConvertFromUtc(DateTime utcDateTime);

        /// <summary>
        /// Converts a nullable UTC datetime values to the system configured timezone.
        /// </summary>
        /// <param name="utcDateTime">Nullable utcDateTime.</param>
        /// <param name="returnNowIfNull">If utcDateTime is null, this flag can be used to return now.</param>
        /// <returns>DateTime converted or null if returnNowIfNull is false else will return now.</returns>
        public DateTime? ConvertFromUtc(DateTime? utcDateTime, bool returnNowIfNull = false);

        /// <summary>
        /// Converts UTC datetime values to the system configured timezone and formats to a short date and time.
        /// </summary>
        /// <param name="utcDateTime">A UTC DateTime instance.</param>
        /// <param name="fallbackString">In the event utcDateTime is null, provide a fallback string to return.</param>
        /// <returns>The short formatted date and time string.</returns>
        public string ConvertToShortFormatFromUtc(DateTime? utcDateTime, string fallbackString = "-");
    }
}
