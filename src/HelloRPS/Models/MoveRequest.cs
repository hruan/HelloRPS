using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HelloRPS.Models
{
    public class MoveRequest
    {
        public string PlayerName { get; set; }
        public string Move { get; set; }
    }
}