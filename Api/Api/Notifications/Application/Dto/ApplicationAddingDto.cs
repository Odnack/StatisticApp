using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Api.Notifications.Application
{
    public class ApplicationAddingDto
    {
        public string Name { get; set; }
        public DateTime CreationDate { get; set; }
        public int UserId { get; set; }
        public string Description { get; set; }
    }
}
