using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AyoWare.Communication.Mediator
{
    public interface IHttpRequest : IRequest<IActionResult>
    {
    }
}
