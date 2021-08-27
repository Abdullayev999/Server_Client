using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLib.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Password { get; set; }
        public string Login { get; set; }
        public string Username { get; set; }
        public DateTime Date { get; set; }
        public override string ToString()
        {
            return Username;
        }
    }
}
