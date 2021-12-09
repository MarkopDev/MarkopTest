using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

public class ControllerBase : Controller
{
    protected IMediator Mediator { get; }

    public ControllerBase(IMediator mediator)
    {
        Mediator = mediator;
    }
}