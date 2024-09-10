using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class Role
    {
        public int roleId {  get; set; }
        public string roleName { get; set; }
        public ICollection<User> users { get; set; }
    }
}
