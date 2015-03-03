﻿using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using JSend.WebApi.Responses;
using Newtonsoft.Json;

namespace JSend.WebApi.Results
{
    /// <summary>
    /// Represents an action result that returns a <see cref="SuccessResponse"/> with the specified content and
    /// with status code <see cref="HttpStatusCode.OK"/>.
    /// </summary>
    /// <typeparam name="T">The type of the content in the entity body.</typeparam>
    public class JSendOkResult<T> : IHttpActionResult
    {
        private readonly JSendResult<SuccessResponse> _result;

        /// <summary>Initializes a new instance of <see cref="JSendOkResult{T}"/>.</summary>
        /// <param name="content">The content value to format in the entity body.</param>
        /// <param name="controller">The controller from which to obtain the dependencies needed for execution.</param>
        public JSendOkResult(T content, JSendApiController controller)
        {
            if (controller == null) throw new ArgumentNullException("controller");

            _result = InitializeResult(content, controller.JsonSerializerSettings, controller.Encoding, controller.Request);
        }

        /// <summary>Initializes a new instance of <see cref="JSendOkResult{T}"/>.</summary>
        /// <param name="content">The content value to format in the entity body.</param>
        /// <param name="serializerSettings">The serializer settings.</param>
        /// <param name="encoding">The content encoding.</param>
        /// <param name="request">The request message which led to this result.</param>
        public JSendOkResult(T content, JsonSerializerSettings serializerSettings, Encoding encoding,
            HttpRequestMessage request)
        {
            _result = InitializeResult(content, serializerSettings, encoding, request);
        }

        private static JSendResult<SuccessResponse> InitializeResult(T content,
            JsonSerializerSettings serializerSettings, Encoding encoding, HttpRequestMessage request)
        {
            var response = new SuccessResponse(content);

            return new JSendResult<SuccessResponse>(HttpStatusCode.OK, response, serializerSettings, encoding, request);
        }

        /// <summary>Gets the response to be formatted into the message's body.</summary>
        public SuccessResponse Response
        {
            get { return _result.Response; }
        }

        /// <summary>Gets the HTTP status code for the response message.</summary>
        public HttpStatusCode StatusCode
        {
            get { return _result.StatusCode; }
        }

        /// <summary>Gets the content value to format in the entity body.</summary>
        public T Content
        {
            get { return (T) _result.Response.Data; }
        }

        /// <summary>Creates an <see cref="HttpResponseMessage"/> asynchronously.</summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that, when completed, contains the <see cref="HttpResponseMessage"/>.</returns>
        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return _result.ExecuteAsync(cancellationToken);
        }
    }
}