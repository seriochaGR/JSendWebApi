﻿using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using FluentAssertions;
using JSend.WebApi.Tests.FixtureCustomizations;
using JSend.WebApi.Tests.TestTypes;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Idioms;
using Xunit;
using Xunit.Extensions;

namespace JSend.WebApi.Tests
{
    public class ValueActionFilterTests
    {
        private class HttpActionContextCustomization : ICustomization
        {
            public void Customize(IFixture fixture)
            {
                fixture.Customize<HttpActionContext>(
                    c => c.OmitAutoProperties()
                        .With(ctx => ctx.ActionDescriptor));
            }
        }

        [Theory, JSendAutoData]
        public void IsActionFilter(ValueActionFilter filter)
        {
            // Exercise system and verify outcome
            filter.Should().BeAssignableTo<IActionFilter>();
        }

        [Theory, JSendAutoData]
        public void ConstructorThrowsWhenAnyArgumentIsNull(IFixture fixture, GuardClauseAssertion assertion)
        {
            // Fixture setup
            fixture.Customize(new HttpActionContextCustomization());
            // Exercise system and verify outcome
            assertion.Verify(typeof (ValueActionFilter).GetConstructors());
        }

        [Theory, JSendAutoData]
        public void ThrowsWhenActionContextIsNull(Func<Task<HttpResponseMessage>> continuation, ValueActionFilter filter)
        {
            // Exercise system and verify outcome
            Assert.Throws<ArgumentNullException>(
                () => filter.ExecuteActionFilterAsync(null, CancellationToken.None, continuation));
        }

        [Theory, JSendAutoData]
        public void ThrowsWhenContinuationIsNull(IFixture fixture, ValueActionFilter filter)
        {
            // Fixture setup
            fixture.Customize(new HttpActionContextCustomization());
            var context = fixture.Create<HttpActionContext>();
            // Exercise system and verify outcome
            Assert.Throws<ArgumentNullException>(
                () => filter.ExecuteActionFilterAsync(context, CancellationToken.None, null));
        }

        [Theory, JSendAutoData]
        public async Task WrapsActionDescriptor_WithDelegatingActionDescriptor_WhenActionReturnsValue(
            IFixture fixture, Func<Task<HttpResponseMessage>> continuation, ValueActionFilter filter)
        {
            // FIxture  setup
            fixture.Customize(new HttpActionContextCustomization());
            var initialDescriptorMock = fixture.Freeze<Mock<HttpActionDescriptor>>();
            initialDescriptorMock.SetupGet(des => des.ReturnType)
                .Returns(typeof (Model));

            var context = fixture.Create<HttpActionContext>();
            // Exercise system
            await filter.ExecuteActionFilterAsync(context, CancellationToken.None, continuation);
            // Verify outcome
            var descriptor = context.ActionDescriptor;

            descriptor.Should().BeOfType<DelegatingActionDescriptor>();
            descriptor.As<DelegatingActionDescriptor>()
                .InnerActionDescriptor.Should().Be(initialDescriptorMock.Object);
        }

        [Theory]
        [InlineJSendAutoData(typeof (string), typeof (JSendValueResultConverter<string>))]
        [InlineJSendAutoData(typeof (Model), typeof (JSendValueResultConverter<Model>))]
        [InlineJSendAutoData(typeof (object), typeof (JSendValueResultConverter<object>))]
        public async Task ComposesActionDescriptor_WithValueResultConverter_WhenActionReturnsValue(
            Type actionType, Type expectedConverterType, IFixture fixture, Func<Task<HttpResponseMessage>> continuation,
            ValueActionFilter filter)
        {
            // Fixture setup
            fixture.Customize(new HttpActionContextCustomization());
            fixture.Freeze<Mock<HttpActionDescriptor>>()
                .SetupGet(des => des.ReturnType)
                .Returns(actionType);

            var context = fixture.Create<HttpActionContext>();
            // Exercise system
            await filter.ExecuteActionFilterAsync(context, CancellationToken.None, continuation);
            // Verify outcome
            context.ActionDescriptor.ResultConverter.Should().BeOfType(expectedConverterType);
        }

        [Theory]
        [InlineJSendAutoData(null)]
        [InlineJSendAutoData(typeof (IHttpActionResult))]
        [InlineJSendAutoData(typeof (HttpResponseMessage))]
        public async Task DoesNotChangeDescriptor_WhenActionDoesNotReturnValue(
            Type actionType, IFixture fixture, Func<Task<HttpResponseMessage>> continuation, ValueActionFilter filter)
        {
            // Fixture setup
            fixture.Customize(new HttpActionContextCustomization());
            fixture.Freeze<Mock<HttpActionDescriptor>>()
                .SetupGet(des => des.ReturnType)
                .Returns(actionType);

            var context = fixture.Create<HttpActionContext>();
            var initialDescriptor = context.ActionDescriptor;
            // Exercise system
            await filter.ExecuteActionFilterAsync(context, CancellationToken.None, continuation);
            // Verify outcome
            context.ActionDescriptor.Should().BeSameAs(initialDescriptor);
        }

        [Theory, JSendAutoData]
        public async Task ReturnsContinuationMessage(IFixture fixture, Func<Task<HttpResponseMessage>> continuation,
            ValueActionFilter filter)
        {
            // Fixture setup
            fixture.Customize(new HttpActionContextCustomization());
            var continuationMessage = await continuation();
            var context = fixture.Create<HttpActionContext>();
            // Exercise system
            var actualMessage = await filter.ExecuteActionFilterAsync(context, CancellationToken.None, continuation);
            // Verify outcome
            actualMessage.Should().Be(continuationMessage);
        }
    }
}