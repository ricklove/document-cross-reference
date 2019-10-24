using System.Collections.Generic;

namespace DocumentCrossReference.Library
{
    public class DocumentTextIndex
    {
        public Dictionary<string, List<DocumentTextLocation>> TextEntries { get; set; }
    }

    public class DocumentTextLocation
    {
        public string DocumentFilePath { get; set; }
        public string Text { get; set; }
        public int PageIndex { get; set; }
        public int LineNumber { get; set; }
        public int ColumnNumber { get; set; }
        public float PageXRatio { get; set; }
        public float PageYRatio { get; set; }
    }
}
