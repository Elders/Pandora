using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Elders.Pandora.UI.Common
{
    public class Error
    {
        public string Message { get; set; }
        public string ExceptionMessage { get; set; }
        public string StackTrace { get; set; }
    }
}