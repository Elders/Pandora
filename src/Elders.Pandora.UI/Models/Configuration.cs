using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Elders.Pandora.UI.Models
{
    public class Configuration
    {
        public Configuration(string name, string content)
        {
            Name = name;
            Content = content;
        }

        public string Name { get; set; }

        public string Content { get; set; }
    }
}