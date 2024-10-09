using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Model
{
    public class AuditDto
    {
        public string? ActionBy { get; set; }
        public string? Description { get; set; }
        public string? APIResponseCode { get; set; }
        public string? APIDescription { get; set; }
        public string? Status { get; set; }
        public string? EntityName { get; set; }
        public string? Component { get; set; }
        public string? ServerIP { get; set; }
        public double ResponseTime { get; set; }
    }
}
