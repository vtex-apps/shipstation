using System;
using System.Collections.Generic;
using System.Text;

namespace ShipStation.Models
{
    public class ResponseWrapper
    {
        public string ResponseText { get; set; }
        public string Message { get; set; }
        public bool IsSuccess { get; set; }
    }
}
