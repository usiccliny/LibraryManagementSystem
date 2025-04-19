using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Models
{
    public class ServerInfo
    {
        public string Role { get; set; }
        public string IpAddress { get; set; }
        public DateTime LastSeen { get; set; }
    }
}
