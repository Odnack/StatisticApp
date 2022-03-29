using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbLayer.Tables.Public
{
    public class User
    {
        public User()
        {
            Applications = new List<Application>();
        }
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public IEnumerable<Application> Applications { get; set; }
    }
}
