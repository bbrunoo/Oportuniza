using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Oportuniza.API.Controllers.Rooms
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RoomsController : ControllerBase
    {
        private readonly ISender _sender;
        public RoomsController(ISender sender)
        {
            _sender = sender;
        }

    }
}
