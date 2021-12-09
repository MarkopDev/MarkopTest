using System;
using System.Collections.Generic;
using Application.Common.Models;

namespace Application.Common.Exceptions;

public class ServiceException : Exception
{
    public IEnumerable<Error> Errors { get; }

    public ServiceException(Error error)
    {
        Errors = new[] {error};
    }

    public ServiceException(IEnumerable<Error> errors)
    {
        Errors = errors;
    }
}