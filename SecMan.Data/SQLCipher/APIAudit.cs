using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Data.SQLCipher
{
    public class APIAudit
    {
        [Key]
        public int RecordID { get; set; }
        public string? ActionBy { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public string? APIResponseCode { get; set; } = string.Empty;
        public string? APIDescription { get; set; } = string.Empty;
        public string? Status { get; set; } = string.Empty;
        public string? EntityName { get; set; } = string.Empty;
        public string? Component { get; set; } = string.Empty;
        public string? ServerIP { get; set; } = string.Empty;    
        public double ResponseTime { get; set; }
        public DateTime? RequestedDate { get; set; }
    }
}
