using System;

namespace YZXYResourceTools.Models
{
    public class ResourceItem
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public long SizeBytes { get; set; }
        public string FullPath { get; set; }

        public ResourceItem(string name, string type, long sizeBytes, string fullPath)
        {
            Name = name;
            Type = type;
            SizeBytes = sizeBytes;
            FullPath = fullPath;
        }
    }
}
