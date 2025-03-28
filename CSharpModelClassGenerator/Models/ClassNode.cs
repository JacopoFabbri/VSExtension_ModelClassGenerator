﻿using System.Collections.Generic;

namespace CSharpModelClassGenerator.Models
{
    public class ClassNode
    {
        public string Name { get; set; }
        public List<PropertyNode> Properties { get; set; } = new List<PropertyNode>();
        public bool IsEnum { get; set; } = false;
        public List<string> EnumValues { get; set; } = new List<string>();
        public string Namespace { get; set; }
    }
}
