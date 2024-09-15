using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Model
{
    public class CustomErrorResponse
    {
        public string Type { get; set; }
        public string Title { get; set; }
        public int Status { get; set; }
        public string Detail { get; set; }
        public List<InvalidParam> InvalidParams { get; set; }
    }
    public class InvalidParam
    {
        public string In { get; set; }
        public string Name { get; set; }
        public string Reason { get; set; }
    }
}
