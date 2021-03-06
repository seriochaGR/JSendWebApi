﻿using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using FluentAssertions;
using JSend.WebApi.Responses;
using JSend.WebApi.Results;
using JSend.WebApi.Tests.FixtureCustomizations;
using Newtonsoft.Json;
using Ploeh.AutoFixture.Idioms;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace JSend.WebApi.Tests.Results
{
    public class JSendRedirectResultTests
    {
        [Theory, JSendAutoData]
        public void IsHttpActionResult(JSendRedirectResult result)
        {
            // Exercise system and verify outcome
            result.Should().BeAssignableTo<IHttpActionResult>();
        }

        [Theory, JSendAutoData]
        public void ConstructorsThrowWhenAnyArgumentIsNull(GuardClauseAssertion assertion)
        {
            // Exercise system and verify outcome
            assertion.Verify(typeof (JSendRedirectResult).GetConstructors());
        }

        [Theory, JSendAutoData]
        public void CanBeCreatedWithControllerWithoutProperties(Uri location,
            [NoAutoProperties] TestableJSendApiController controller)
        {
            // Exercise system and verify outcome
            Action ctor = () => new JSendRedirectResult(location, controller);
            ctor.ShouldNotThrow();
        }

        [Theory, JSendAutoData]
        public void ResponseIsCorrectlyInitialized(Uri location, ApiController controller)
        {
            // Fixture setup
            var expectedResponse = new SuccessResponse();
            // Exercise system
            var result = new JSendRedirectResult(location, controller);
            // Verify outcome
            result.Response.ShouldBeEquivalentTo(expectedResponse);
        }

        [Theory, JSendAutoData]
        public void StatusCodeIs302(JSendRedirectResult result)
        {
            // Exercise system and verify outcome
            result.StatusCode.Should().Be(HttpStatusCode.Redirect);
        }

        [Theory, JSendAutoData]
        public void RequestIsCorrectlyInitializedUsingController(Uri location, ApiController controller)
        {
            // Exercise system
            var result = new JSendRedirectResult(location, controller);
            // Verify outcome
            result.Request.Should().Be(controller.Request);
        }

        [Theory, JSendAutoData]
        public void LocationIsCorrectlyInitialized(Uri location, ApiController controller)
        {
            // Exercise system
            var result = new JSendRedirectResult(location, controller);
            // Verify outcome
            result.Location.Should().Be(location);
        }

        [Theory, JSendAutoData]
        public async Task ResponseIsSerializedIntoBody(JSendRedirectResult result)
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
        public async Task SetsStatusCode(JSendRedirectResult result)
        {
            // Exercise system
            var message = await result.ExecuteAsync(new CancellationToken());
            // Verify outcome
            message.StatusCode.Should().Be(result.StatusCode);
        }

        [Theory, JSendAutoData]
        public async Task SetsContentTypeHeader(JSendRedirectResult result)
        {
            // Exercise system
            var message = await result.ExecuteAsync(new CancellationToken());
            // Verify outcome
            message.Content.Headers.ContentType.MediaType.Should().Be("application/json");
        }

        [Theory, JSendAutoData]
        public async Task SetsLocationHeader(JSendRedirectResult result)
        {
            // Exercise system
            var message = await result.ExecuteAsync(new CancellationToken());
            // Verify outcome
            message.Headers.Location.Should().Be(result.Location);
        }
    }
}
