using System;

namespace YZXYResourceTools.Models
{
    public class ResourceItem
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public long SizeBytes { get; set; }
        public string FullPath { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public bool IsText { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }

        public ResourceItem(string name, string type, long sizeBytes, string fullPath)
        {
            Name = name;
            Type = type;
            SizeBytes = sizeBytes;
            FullPath = fullPath;
            Category = "File";
            Description = string.Empty;
            IsText = false;
            Width = null;
            Height = null;
        }
    }
}

