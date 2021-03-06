﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using FluentAssertions;
using JSend.WebApi.Responses;
using JSend.WebApi.Results;
using JSend.WebApi.Tests.FixtureCustomizations;
using Newtonsoft.Json;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Idioms;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace JSend.WebApi.Tests.Results
{
    public class JSendInvalidModelStateResultTests
    {
        private class InvalidModelCustomization : ICustomization
        {
            public void Customize(IFixture fixture)
            {
                fixture.Customize<ModelStateDictionary>(c =>
                    c.Do(dic => dic.AddModelError("key", "value")));
            }
        }

        private class InvalidModelStateAttribute : CustomizeAttribute
        {
            public override ICustomization GetCustomization(ParameterInfo parameter)
            {
                return new InvalidModelCustomization();
            }
        }

        [Theory, JSendAutoData]
        public void IsHttpActionResult([InvalidModelState] JSendInvalidModelStateResult result)
        {
            // Exercise system and verify outcome
            result.Should().BeAssignableTo<IHttpActionResult>();
        }

        [Theory, JSendAutoData]
        public void ConstructorsThrowWhenAnyArgumentIsNull([InvalidModelState] GuardClauseAssertion assertion)
        {
            // Exercise system and verify outcome
            assertion.Verify(typeof (JSendInvalidModelStateResult).GetConstructors());
        }

        [Theory, JSendAutoData]
        public void ThrowsIfModelStateIsValid(ModelStateDictionary modelState, ApiController controller)
        {
            // Exercise system and verify outcome
            Assert.Throws<ArgumentException>(() => new JSendInvalidModelStateResult(modelState, controller));
        }

        [Theory, JSendAutoData]
        public void CanBeCreatedWithControllerWithoutProperties([InvalidModelState] ModelStateDictionary modelState,
            [NoAutoProperties] TestableJSendApiController controller)
        {
            // Exercise system and verify outcome
            Action ctor = () => new JSendInvalidModelStateResult(modelState, controller);
            ctor.ShouldNotThrow();
        }

        [Theory, JSendAutoData]
        public void StatusCodeIs400([InvalidModelState] JSendInvalidModelStateResult result)
        {
            // Exercise system and verify outcome
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Theory, JSendAutoData]
        public void RequestIsCorrectlyInitializedUsingController(
            [InvalidModelState] ModelStateDictionary modelState, ApiController controller)
        {
            // Exercise system
            var result = new JSendInvalidModelStateResult(modelState, controller);
            // Verify outcome
            result.Request.Should().Be(controller.Request);
        }

        [Theory, JSendAutoData]
        public void ResponseDataMatchesModelState([InvalidModelState] JSendInvalidModelStateResult result)
        {
            // Exercise system and verify outcome
            result.Response.Data.ShouldBeEquivalentTo(result.ModelState);
        }

        [Theory, JSendAutoData]
        public void ResponseIsFail([InvalidModelState] JSendInvalidModelStateResult result)
        {
            // Exercise system and verify outcome
            result.Response.Should().BeAssignableTo<FailResponse>();
        }

        [Theory, JSendAutoData]
        public async Task ResponseIsSerializedIntoBody([InvalidModelState] JSendInvalidModelStateResult result)
        {
            // Fixture setup
            var serializedResponse = JsonConvert.SerializeObject(result.Response);
            // Exercise system
            var httpResponse = await result.ExecuteAsync(new CancellationToken());
            // Verify outcome
            var content = await httpResponse.Content.ReadAsStringAsync();
            content.Should().Be(serializedResponse);
        }

        [Theory, JSendAutoData]
        public void ExtractsErrorMessages_FromModelState(IFixture fixture)
        {
            // Fixture setup
            fixture.Customize<ModelStateDictionary>(c =>
                c.Do(dic => dic.AddModelError("age", "error1"))
                    .Do(dic => dic.AddModelError("age", "error2")));

            var expectedValidationErrors = new Dictionary<string, IEnumerable<string>>
            {
                {"age", new[] {"error1", "error2"}}
            };

            // Exercise system
            var result = fixture.Create<JSendInvalidModelStateResult>();
            // Verify outcome
            result.ModelState.ShouldBeEquivalentTo(expectedValidationErrors);
        }

        [Theory, JSendAutoData]
        public void ExtractsExceptionMessages_FromModelState(IFixture fixture)
        {
            // Fixture setup
            fixture.Customize<ModelStateDictionary>(c =>
                c.Do(dic => dic.AddModelError("age", new Exception("exceptionMessage1")))
                    .Do(dic => dic.AddModelError("age", new Exception("exceptionMessage2"))));
            fixture.Freeze<ApiController>().RequestContext.IncludeErrorDetail = true;

            var expectedValidationErrors = new Dictionary<string, IEnumerable<string>>
            {
                {"age", new[] {"exceptionMessage1", "exceptionMessage2"}}
            };

            // Exercise system
            var result = fixture.Create<JSendInvalidModelStateResult>();

            // Verify outcome
            result.ModelState.ShouldBeEquivalentTo(expectedValidationErrors);
        }

        [Theory, JSendAutoData]
        public void InsertsDefaultMessageInsteadOfExceptionMessage_If_ControllerIsConfiguredToNotIncludeErrorDetails(
            IFixture fixture, [Frozen] ApiController controller)
        {
            // Fixture setup
            fixture.Customize<ModelStateDictionary>(c =>
                c.Do(dic => dic.AddModelError("age", new Exception("exceptionMessage1"))));
            controller.RequestContext.IncludeErrorDetail = false;
            // Exercise system
            var result = fixture.Create<JSendInvalidModelStateResult>();
            // Verify outcome
            result.ModelState.As<IDictionary<string, IEnumerable<string>>>()
                .Should().ContainKey("age")
                .WhichValue.Should().ContainSingle(ageError => ageError != "exceptionMessage1");
        }

        [Theory, JSendAutoData]
        public void InsertsDefaultMessage_If_ErrorMessageIsEmpty(IFixture fixture)
        {
            // Fixture setup
            fixture.Customize<ModelStateDictionary>(c =>
                c.Do(dic => dic.AddModelError("age", errorMessage: "")));

            // Exercise system
            var result = fixture.Create<JSendInvalidModelStateResult>();

            // Verify outcome
            result.ModelState.As<IDictionary<string, IEnumerable<string>>>()
                .Should().ContainKey("age")
                .WhichValue.Should().ContainSingle(ageError => !string.IsNullOrWhiteSpace(ageError));
        }

        [Theory, JSendAutoData]
        public async Task SetsStatusCode([InvalidModelState] JSendInvalidModelStateResult result)
        {
            // Exercise system
            var message = await result.ExecuteAsync(new CancellationToken());
            // Verify outcome
            message.StatusCode.Should().Be(result.StatusCode);
        }

        [Theory, JSendAutoData]
        public async Task SetsContentTypeHeader([InvalidModelState] JSendInvalidModelStateResult result)
        {
            // Exercise system
            var message = await result.ExecuteAsync(new CancellationToken());
            // Verify outcome
            message.Content.Headers.ContentType.MediaType.Should().Be("application/json");
        }
    }
}
