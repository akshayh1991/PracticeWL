﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Data.SQLCipher
{
    internal class SysFeat
    {
        [Key]
        public ulong Id { get; set; }
        public string? Name { get; set; }
        public bool Common { get; set; } = false;
        public List<SysFeatLang>? Langs { get; set; }
        public List<SysFeatProp>? SysFeatProps { get; set; } = [];
    }
}
