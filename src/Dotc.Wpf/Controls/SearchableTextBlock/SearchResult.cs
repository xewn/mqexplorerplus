using System.Text.RegularExpressions;
using ICSharpCode.AvalonEdit.Document;

namespace Dotc.Wpf.Controls.SearchableTextBlock
{
    public class SearchResult : TextSegment
    {
        public Match Data { get; set; }
    }
}
