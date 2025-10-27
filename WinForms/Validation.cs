using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notebook
{
    public static class Validation
    {
        public static bool StringIsNullOrEmpty(string text)
        { 
            return string.IsNullOrEmpty(text);
        }

        public static bool StringIsNullOrWhiteSpace(string text)
        { 
            return string.IsNullOrWhiteSpace(text);
        }
    }
}
