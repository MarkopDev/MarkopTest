﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Enums;
using Application.Common.Exceptions;
using Application.Common.Models;
using MediatR;

namespace Application.Common.Behaviours
{
    public class UnhandledExceptionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        public UnhandledExceptionBehaviour()
        {
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next)
        {
            try
            {
                return await next();
            }
            catch (ServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Debugger.Break();
                // _loggerService.Error(ex);
                throw new ServiceException(new Error
                {
                    Code = ErrorCode.Unexpected,
                    Message = "An unexpected exception occurred"
                });
            }
        }
    }
}