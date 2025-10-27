using System;

namespace Notebook
{
    public class Note
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Category { get; set; }

        public bool IsDone { get; set; }

        public override string ToString()
        {
            return $"{CreatedAt: yyyy-MM-dd HH:mm} - {Title}";
        }
    }
}
