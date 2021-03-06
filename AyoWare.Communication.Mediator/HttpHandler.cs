﻿using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace AyoWare.Communication.Mediator
{
    public abstract class HttpHandler<TRequest> : IRequestHandler<TRequest, IActionResult>
       where TRequest : class, IHttpRequest, new()
    {
        private readonly ILogger _logger;

        protected HttpHandler()
        {
            _logger = new LoggerConfiguration()
                        .WriteTo.Console()
                        .CreateLogger();
        }

        protected HttpHandler(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> Handle(TRequest request, CancellationToken cancellationToken)
        {
            _logger.Information(nameof(Handle));
            if (request == null)
            {
                return new NotFoundResult();
            }

            _logger.Debug("Beginning request: {@request}", request);
            var model = await HandleAsync(request, cancellationToken).ConfigureAwait(false);
            _logger.Debug("Got a response: {@response}", model);

            if (!model.Succeeded)
            {
                return new ContentResult
                {
                    Content = model.ErrorMessage,
                    StatusCode = (int)model.HttpStatusCode
                };
            }

            if (model.Response != null)
            {
                return new OkObjectResult(model.Response);
            }
            return new StatusCodeResult((int)model.HttpStatusCode);
        }

        protected abstract Task<HttpResponse> HandleAsync(TRequest input, CancellationToken cancellationToken = default);

        protected HttpResponse Ok<TResponse>(TResponse response) where TResponse : class
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            return new HttpResponse
            {
                Response = response,
                HttpStatusCode = HttpStatusCode.OK
            };
        }

        /// <summary>
        /// Respond successfully to a HTTP request with a 201 (Created) response.
        /// </summary>
        /// <returns></returns>
        protected HttpResponse Created()
        {
            return new HttpResponse
            {
                HttpStatusCode = HttpStatusCode.Created
            };
        }

        /// <summary>
        /// Respond successfully to a HTTP request with a 201 (Created) response and model.
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="response">The type of model to respond with.</param>
        /// <returns></returns>
        protected HttpResponse Created<TResponse>(TResponse response) where TResponse : class
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            return new HttpResponse
            {
                Response = response,
                HttpStatusCode = HttpStatusCode.Created
            };
        }

        /// <summary>
        /// Respond successfully to a HTTP request with a 204 (No Content) response.
        /// </summary>
        /// <returns></returns>
        protected HttpResponse NoContent()
        {
            return new HttpResponse
            {
                HttpStatusCode = HttpStatusCode.NoContent
            };
        }

        /// <summary>
        /// Return a 404 (Not Found) response.
        /// </summary>
        /// <returns></returns>
        protected HttpResponse NotFound()
        {
            return new HttpResponse
            {
                ErrorMessage = "Status Code: 404; Not Found",
                HttpStatusCode = HttpStatusCode.NotFound
            };
        }

        /// <summary>
        /// Return a 409 (Conflict) response.
        /// </summary>
        /// <param name="errorMessage">The error message to respond with.</param>
        /// <returns></returns>
        protected HttpResponse Conflict(string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                throw new ArgumentNullException(nameof(errorMessage));
            }

            return new HttpResponse
            {
                ErrorMessage = errorMessage,
                HttpStatusCode = HttpStatusCode.Conflict
            };
        }
    }
}