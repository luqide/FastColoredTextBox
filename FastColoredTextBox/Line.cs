using System.Collections.Generic;
using System;
using System.Text;
using System.Drawing;

namespace FastColoredTextBoxNS
{
    /// <summary>
    /// Represents a single line of text in the FastColoredTextBox document.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Line class is a fundamental building block of the text storage system.
    /// It implements IList&lt;Char&gt; to provide list-like access to the characters
    /// in the line, where each character is represented by a Char struct (containing
    /// both the character and its style information).
    /// </para>
    /// <para>
    /// Key features:
    /// - Stores characters with style information
    /// - Supports code folding markers
    /// - Tracks change state and visit history
    /// - Provides custom background brush capability
    /// - Maintains a unique ID for identification
    /// - Supports auto-indent functionality
    /// </para>
    /// <para>
    /// Lines are managed by the TextSource class and should typically not be
    /// created directly by user code.
    /// </para>
    /// </remarks>
    public class Line : IList<Char>
    {
        protected List<Char> chars;

        /// <summary>
        /// Gets or sets the marker indicating the start of a foldable region.
        /// </summary>
        /// <remarks>
        /// This property is used by the code folding system to identify where
        /// a collapsible block begins. When set, this line can be collapsed to
        /// hide lines until the corresponding FoldingEndMarker is found.
        /// </remarks>
        public string FoldingStartMarker { get; set; }

        /// <summary>
        /// Gets or sets the marker indicating the end of a foldable region.
        /// </summary>
        /// <remarks>
        /// This property marks the end of a collapsible code block. It pairs with
        /// a FoldingStartMarker to define the boundaries of foldable regions.
        /// </remarks>
        public string FoldingEndMarker { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the text of this line has been modified.
        /// </summary>
        /// <remarks>
        /// This flag is set to true when the line's content is changed. It can be used
        /// to highlight modified lines or track which lines need to be saved. The flag
        /// can be cleared manually or via TextSource.ClearIsChanged().
        /// </remarks>
        public bool IsChanged { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of when the caret last visited this line.
        /// </summary>
        /// <remarks>
        /// This property enables navigation features like "Go to last edit location" or
        /// tracking the user's navigation history within the document. The timestamp is
        /// automatically updated when the caret moves to this line.
        /// </remarks>
        public DateTime LastVisit { get; set; }

        /// <summary>
        /// Gets or sets the background brush for this line.
        /// </summary>
        /// <remarks>
        /// When set, this brush will be used to draw the background of this specific line,
        /// overriding the default background color. This can be used for highlighting
        /// important lines, showing the current line, or creating custom visual effects.
        /// Set to null to use the default background.
        /// </remarks>
        public Brush BackgroundBrush { get; set;}

        /// <summary>
        /// Gets the unique identifier for this line.
        /// </summary>
        /// <remarks>
        /// Each line is assigned a unique ID when created. This ID remains constant
        /// throughout the line's lifetime and can be used to track specific lines
        /// even as their position in the document changes. IDs are assigned sequentially
        /// and are never reused within the same TextSource.
        /// </remarks>
        public int UniqueId { get; private set; }

        /// <summary>
        /// Gets or sets the count of spaces needed at the start of the line for auto-indent.
        /// </summary>
        /// <remarks>
        /// This property is used by the auto-indent feature to determine how many spaces
        /// should be inserted at the beginning of a new line to match the indentation
        /// of the current or previous line. The auto-indent logic sets this value based
        /// on the context (e.g., after opening braces, within blocks, etc.).
        /// </remarks>
        public int AutoIndentSpacesNeededCount
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the Line class with the specified unique ID.
        /// </summary>
        /// <param name="uid">The unique identifier for this line</param>
        /// <remarks>
        /// This constructor is internal because lines should be created through the
        /// TextSource.CreateLine() method, which ensures unique IDs are properly managed.
        /// </remarks>
        internal Line(int uid)
        {
            this.UniqueId = uid;
            chars = new List<Char>();
        }


        /// <summary>
        /// Clears the specified style from all characters in the line and removes folding markers.
        /// </summary>
        /// <param name="styleIndex">The style index to remove from all characters</param>
        /// <remarks>
        /// This method performs two operations:
        /// 1. Removes the specified style from every character in the line by clearing
        ///    the corresponding bit in each character's style mask.
        /// 2. Clears both the FoldingStartMarker and FoldingEndMarker properties.
        /// This is typically called when re-applying syntax highlighting to a line.
        /// </remarks>
        public void ClearStyle(StyleIndex styleIndex)
        {
            FoldingStartMarker = null;
            FoldingEndMarker = null;
            for (int i = 0; i < Count; i++)
            {
                Char c = this[i];
                c.style &= ~styleIndex;
                this[i] = c;
            }
        }

        /// <summary>
        /// Gets the text content of the line as a string.
        /// </summary>
        /// <remarks>
        /// This property constructs and returns a string containing all the characters
        /// in the line. The style information is not included - only the actual character
        /// data. This is useful for searching, exporting, or any operation that needs
        /// the plain text content without formatting.
        /// </remarks>
        public virtual string Text
        {
            get{
                StringBuilder sb = new StringBuilder(Count);
                foreach(Char c in this)
                    sb.Append(c.c);
                return sb.ToString();
            }
        }

        /// <summary>
        /// Clears both the folding start and end markers for this line.
        /// </summary>
        /// <remarks>
        /// This method removes folding markers without affecting the character styles.
        /// It's useful when recalculating code folding regions or when folding markers
        /// need to be reset without performing a full style clear.
        /// </remarks>
        public void ClearFoldingMarkers()
        {
            FoldingStartMarker = null;
            FoldingEndMarker = null;
        }

        /// <summary>
        /// Gets the number of consecutive space characters at the start of the line.
        /// </summary>
        /// <remarks>
        /// This property counts space characters from the beginning of the line until
        /// a non-space character is encountered. It's used for indentation analysis,
        /// auto-indent calculations, and formatting operations. Note that this counts
        /// only space characters (ASCII 32), not tabs or other whitespace.
        /// </remarks>
        public int StartSpacesCount
        {
            get
            {
                int spacesCount = 0;
                for (int i = 0; i < Count; i++)
                    if (this[i].c == ' ')
                        spacesCount++;
                    else
                        break;
                return spacesCount;
            }
        }

        /// <summary>
        /// Searches for the specified character and returns the zero-based index of the first occurrence.
        /// </summary>
        /// <param name="item">The character to locate</param>
        /// <returns>The zero-based index of the first occurrence of item, or -1 if not found</returns>
        public int IndexOf(Char item)
        {
            return chars.IndexOf(item);
        }

        /// <summary>
        /// Inserts a character at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the character should be inserted</param>
        /// <param name="item">The character to insert</param>
        public void Insert(int index, Char item)
        {
            chars.Insert(index, item);
        }

        /// <summary>
        /// Removes the character at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the character to remove</param>
        public void RemoveAt(int index)
        {
            chars.RemoveAt(index);
        }

        public Char this[int index]
        {
            get
            {
                return chars[index];
            }
            set
            {
                chars[index] = value;
            }
        }

        public void Add(Char item)
        {
            chars.Add(item);
        }

        public void Clear()
        {
            chars.Clear();
        }

        public bool Contains(Char item)
        {
            return chars.Contains(item);
        }

        public void CopyTo(Char[] array, int arrayIndex)
        {
            chars.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Chars count
        /// </summary>
        public int Count
        {
            get { return chars.Count; }
        }

        public bool IsReadOnly
        {
            get {  return false; }
        }

        public bool Remove(Char item)
        {
            return chars.Remove(item);
        }

        public IEnumerator<Char> GetEnumerator()
        {
            return chars.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return chars.GetEnumerator() as System.Collections.IEnumerator;
        }

        public virtual void RemoveRange(int index, int count)
        {
            if (index >= Count)
                return;
            chars.RemoveRange(index, Math.Min(Count - index, count));
        }

        public virtual void TrimExcess()
        {
            chars.TrimExcess();
        }

        public virtual void AddRange(IEnumerable<Char> collection)
        {
            chars.AddRange(collection);
        }
    }

    public struct LineInfo
    {
        List<int> cutOffPositions;
        //Y coordinate of line on screen
        internal int startY;
        internal int bottomPadding;
        //indent of secondary wordwrap strings (in chars)
        internal int wordWrapIndent;
        /// <summary>
        /// Visible state
        /// </summary>
        public VisibleState VisibleState;

        public LineInfo(int startY)
        {
            cutOffPositions = null;
            VisibleState = VisibleState.Visible;
            this.startY = startY;
            bottomPadding = 0;
            wordWrapIndent = 0;
        }
        /// <summary>
        /// Positions for wordwrap cutoffs
        /// </summary>
        public List<int> CutOffPositions
        {
            get
            {
                if (cutOffPositions == null)
                    cutOffPositions = new List<int>();
                return cutOffPositions;
            }
        }

        /// <summary>
        /// Count of wordwrap string count for this line
        /// </summary>
        public int WordWrapStringsCount
        {
            get
            {
                switch (VisibleState)
                {
                    case VisibleState.Visible:
                         if (cutOffPositions == null)
                            return 1;
                         else
                            return cutOffPositions.Count + 1;
                    case VisibleState.Hidden: return 0;
                    case VisibleState.StartOfHiddenBlock: return 1;
                }

                return 0;
            }
        }

        internal int GetWordWrapStringStartPosition(int iWordWrapLine)
        {
            return iWordWrapLine == 0 ? 0 : CutOffPositions[iWordWrapLine - 1];
        }

        internal int GetWordWrapStringFinishPosition(int iWordWrapLine, Line line)
        {
            if (WordWrapStringsCount <= 0)
                return 0;
            return iWordWrapLine == WordWrapStringsCount - 1 ? line.Count - 1 : CutOffPositions[iWordWrapLine] - 1;
        }

        /// <summary>
        /// Gets index of wordwrap string for given char position
        /// </summary>
        public int GetWordWrapStringIndex(int iChar)
        {
            if (cutOffPositions == null || cutOffPositions.Count == 0) return 0;
            for (int i = 0; i < cutOffPositions.Count; i++)
                if (cutOffPositions[i] >/*>=*/ iChar)
                    return i;
            return cutOffPositions.Count;
        }
    }

    public enum VisibleState: byte
    {
        Visible, StartOfHiddenBlock, Hidden
    }

    public enum IndentMarker
    {
        None,
        Increased,
        Decreased
    }
}
