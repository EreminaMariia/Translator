using System;
using System.Collections.Generic;
using System.Text;

namespace TranslatorWPF
{
    public class Subtitle
    {
        public int Id { get; set; }
        public TimeSpan Start { get; set; }
        public TimeSpan Finish { get; set; }
        public string Text { get; set; }
        public string Translated { get; set; }
        public string Info { get; set; }
    }
}
