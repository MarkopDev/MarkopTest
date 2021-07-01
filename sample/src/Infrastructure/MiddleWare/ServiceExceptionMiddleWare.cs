using System;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Application.Common.Enums;
using Application.Common.Exceptions;
using Application.Common.Models;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.MiddleWare
{
    public class SystemExceptionMiddleWare
    {
        private readonly RequestDelegate _next;

        public SystemExceptionMiddleWare(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (ServiceException e)
            {
                context.Response.StatusCode = (int) HttpStatusCode.NotAcceptable;
                context.Response.ContentType = "application/json; charset=utf-8";
                var newResponse = JsonSerializer.Serialize(new
                {
                    e.Errors
                }, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                });
                await context.Response.WriteAsync(newResponse);
            }
            catch (Exception e)
            {
                Debugger.Break();
                // loggerService.Error(e);
                context.Response.StatusCode = (int) HttpStatusCode.NotAcceptable;
                context.Response.ContentType = "application/json; charset=utf-8";
                var newResponse = JsonSerializer.Serialize(new
                    {
                        Errors = new[]
                        {
                            new Error
                            {
                                Code = ErrorCode.Unexpected,
                                Message = "An unexpected exception occurred."
                            }
                        }
                    },
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    });
                await context.Response.WriteAsync(newResponse);
            }
        }
    }
}