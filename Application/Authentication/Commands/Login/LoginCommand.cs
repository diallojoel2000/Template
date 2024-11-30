using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Authentication.Commands
{
    public record LoginCommand:IRequest
    {
        public string Username { get; init; } = null!;
        public string Password { get; init; } = null!;
    }
}
