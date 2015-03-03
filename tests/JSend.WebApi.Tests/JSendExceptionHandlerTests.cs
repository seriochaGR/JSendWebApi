﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;
using FluentAssertions;
using JSend.WebApi.Results;
using JSend.WebApi.Tests.FixtureCustomizations;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Idioms;
using Xunit;
using Xunit.Extensions;

namespace JSend.WebApi.Tests
{
    public class JSendExceptionHandlerTests
    {
        [Theory, JSendAutoData]
        public void IsExceptionHandler(JSendExceptionHandler handler)
        {
            // Exercise system
            handler.Should().BeAssignableTo<ExceptionHandler>();
        }

        [Theory, JSendAutoData]
        public void ConstructorsThrowWhenAnyArgumentIsNull(GuardClauseAssertion assertion)
        {
            // Exercise system
            assertion.Verify(typeof(JSendExceptionHandler).GetConstructors());
        }

        [Theory, JSendAutoDataAttribute]
        public void ThrowsWhenContextIsNull(JSendExceptionHandler handler)
        {
            // Exercise system and verify outcome
            Assert.Throws<ArgumentNullException>(() => handler.Handle(null));
        }

        [Theory, JSendAutoData]
        public void SetsResultToJSendExceptionResult(IFixture fixture, JSendExceptionHandler handler)
        {
            // Fixture setup
            fixture.Customize<ExceptionContext>(c => c.OmitAutoProperties().With(ctx => ctx.Request));
            var context = fixture.Create<ExceptionHandlerContext>();
            // Exercise system
            handler.Handle(context);
            // Verify outcome
            context.Result.Should().BeAssignableTo<JSendExceptionResult>();
        }
    }
}