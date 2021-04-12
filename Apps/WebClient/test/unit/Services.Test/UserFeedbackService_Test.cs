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
namespace HealthGateway.WebClient.Test.Services
{
    using System;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.WebClient.Services;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    public class UserFeedbackService_Test
    {
        [Fact]
        public void ShouldCreateRating()
        {
            Rating expectedRating = new Rating
            {
                RatingValue = 5,
                Skip = false,
            };

            DBResult<Rating> insertResult = new DBResult<Rating>
            {
                Payload = expectedRating,
                Status = DBStatusCode.Created,
            };

            Mock<IRatingDelegate> ratingDelegateMock = new Mock<IRatingDelegate>();
            ratingDelegateMock.Setup(s => s.InsertRating(It.Is<Rating>(r => r.RatingValue == expectedRating.RatingValue && r.Skip == expectedRating.Skip))).Returns(insertResult);

            IUserFeedbackService service = new UserFeedbackService(
                new Mock<ILogger<UserFeedbackService>>().Object,
                new Mock<IFeedbackDelegate>().Object,
                ratingDelegateMock.Object);

            RequestResult<Rating> actualResult = service.CreateRating(expectedRating);

            Assert.Equal(Common.Constants.ResultType.Success, actualResult.ResultStatus);
            Assert.True(actualResult.ResourcePayload?.IsDeepEqual(expectedRating));
        }

        [Fact]
        public void ShouldCreateRatingWithError()
        {
            Rating expectedRating = new Rating
            {
                RatingValue = 5,
                Skip = false,
            };

            DBResult<Rating> insertResult = new DBResult<Rating>
            {
                Payload = expectedRating,
                Status = DBStatusCode.Error,
            };

            Mock<IRatingDelegate> ratingDelegateMock = new Mock<IRatingDelegate>();
            ratingDelegateMock.Setup(s => s.InsertRating(It.Is<Rating>(r => r.RatingValue == expectedRating.RatingValue && r.Skip == expectedRating.Skip))).Returns(insertResult);

            IUserFeedbackService service = new UserFeedbackService(
                new Mock<ILogger<UserFeedbackService>>().Object,
                new Mock<IFeedbackDelegate>().Object,
                ratingDelegateMock.Object);

            RequestResult<Rating> actualResult = service.CreateRating(expectedRating);

            Assert.Equal(Common.Constants.ResultType.Error, actualResult.ResultStatus);
        }

        [Fact]
        public void ShouldCreateUserFeedback()
        {
            UserFeedback expectedUserFeedback = new UserFeedback
            {
                Comment = "Mocked Comment",
                Id = Guid.NewGuid(),
                UserProfileId = "Mocked UserProfileId",
                IsSatisfied = true,
                IsReviewed = true,
            };

            DBResult<UserFeedback> insertResult = new DBResult<UserFeedback>
            {
                Payload = expectedUserFeedback,
                Status = DBStatusCode.Created,
            };

            Mock<IFeedbackDelegate> userFeedbackDelegateMock = new Mock<IFeedbackDelegate>();
            userFeedbackDelegateMock.Setup(s => s.InsertUserFeedback(It.Is<UserFeedback>(r => r.Comment == expectedUserFeedback.Comment && r.Id == expectedUserFeedback.Id && r.UserProfileId == expectedUserFeedback.UserProfileId && r.IsSatisfied == expectedUserFeedback.IsSatisfied && r.IsReviewed == expectedUserFeedback.IsReviewed))).Returns(insertResult);

            IUserFeedbackService service = new UserFeedbackService(
                new Mock<ILogger<UserFeedbackService>>().Object,
                userFeedbackDelegateMock.Object,
                new Mock<IRatingDelegate>().Object);

            DBResult<UserFeedback> actualResult = service.CreateUserFeedback(expectedUserFeedback);

            Assert.Equal(DBStatusCode.Created, actualResult.Status);
            Assert.True(actualResult.Payload?.IsDeepEqual(expectedUserFeedback));
        }
    }
}
