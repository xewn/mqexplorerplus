using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using ICSharpCode.AvalonEdit.Document;

namespace Dotc.Wpf.Controls.SearchableTextBlock
{
    public class RegexSearchStrategy
    {
        readonly Regex searchPattern; 
 		readonly bool matchWholeWords; 
 		 
 		public RegexSearchStrategy(Regex searchPattern, bool matchWholeWords)
 		{ 
 			if (searchPattern == null) 
 				throw new ArgumentNullException(nameof(searchPattern)); 
 			this.searchPattern = searchPattern; 
 			this.matchWholeWords = matchWholeWords; 
 		}

        public IEnumerable<SearchResult> FindAll(ITextSource document, int offset, int length)
        {
            int endOffset = offset + length;
            foreach (Match result in searchPattern.Matches(document.Text))
            {
                int resultEndOffset = result.Length + result.Index;
                if (offset > result.Index || endOffset < resultEndOffset)
                    continue;
                if (matchWholeWords
                    && (!IsWordBorder(document, result.Index) || !IsWordBorder(document, resultEndOffset)))
                    continue;
                yield return new SearchResult
                             {
                                 StartOffset = result.Index,
                                 Length = result.Length,
                                 Data = result
                             };
            }
        }

        static bool IsWordBorder(ITextSource document, int offset)
 		{ 
 			return TextUtilities.GetNextCaretPosition(document, offset - 1, LogicalDirection.Forward, CaretPositioningMode.WordBorder) == offset; 
 		} 
 		 
 		public SearchResult FindNext(ITextSource document, int offset, int length)
 		{ 
 			return FindAll(document, offset, length).FirstOrDefault(); 
 		} 
 
    }
}
