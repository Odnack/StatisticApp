using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Api.Authentication.Dto
{
    public class RegisterUserDto : UserDto
    {
        public string Password { get; set; }
    }
}
