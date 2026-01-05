using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Drawing;
using System.IO;

namespace FastColoredTextBoxNS
{
    /// <summary>
    /// Manages the text content, styles, and command history for a FastColoredTextBox.
    /// </summary>
    /// <remarks>
    /// <para>
    /// TextSource is the central data management class in FastColoredTextBox. It stores:
    /// - All text lines (as Line objects)
    /// - Style definitions (in the Styles array)
    /// - Command history for undo/redo (via CommandManager)
    /// - Event handlers for text changes
    /// </para>
    /// <para>
    /// The class implements IList&lt;Line&gt; to provide collection-like access to lines,
    /// and IDisposable for proper resource cleanup. It serves as the Model in an MVC-like
    /// architecture, with FastColoredTextBox as the View/Controller.
    /// </para>
    /// <para>
    /// Key responsibilities:
    /// - Line storage and management
    /// - Style registry (16 or 32 styles depending on compilation)
    /// - Undo/redo command execution
    /// - Event notification for text changes
    /// - Coordination between multiple FastColoredTextBox instances (multi-view)
    /// </para>
    /// <para>
    /// TextSource can be shared between multiple FastColoredTextBox controls to provide
    /// synchronized multi-view editing of the same document.
    /// </para>
    /// </remarks>
    public class TextSource: IList<Line>, IDisposable
    {
        readonly protected List<Line> lines = new List<Line>();
        protected LinesAccessor linesAccessor;
        int lastLineUniqueId;

        /// <summary>
        /// Gets or sets the command manager for undo/redo operations.
        /// </summary>
        /// <remarks>
        /// The CommandManager maintains the undo and redo stacks, allowing users to reverse
        /// and replay text modifications. All text changes should go through commands
        /// executed via this manager to maintain proper undo/redo support.
        /// </remarks>
        public CommandManager Manager { get; set; }

        FastColoredTextBox currentTB;

        /// <summary>
        /// Gets the array of available styles.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This array contains all style definitions that can be applied to characters.
        /// By default, it holds 16 styles, but can be compiled to support 32 styles
        /// using the Styles32 compiler directive.
        /// </para>
        /// <para>
        /// To use a style:
        /// 1. Create a Style object (e.g., new TextStyle(...))
        /// 2. Assign it to an index: Styles[0] = myStyle
        /// 3. Apply it to characters via Range.SetStyle(StyleIndex.Style0, ...)
        /// </para>
        /// <para>
        /// Index mapping: StyleIndex.Style0 → Styles[0], StyleIndex.Style1 → Styles[1], etc.
        /// </para>
        /// </remarks>
        public readonly Style[] Styles;

        /// <summary>
        /// Occurs when a line is inserted or added to the document.
        /// </summary>
        /// <remarks>
        /// This event fires after one or more lines have been inserted. Subscribers can
        /// use this to update line-based data structures like bookmarks, breakpoints, or
        /// line number displays.
        /// </remarks>
        public event EventHandler<LineInsertedEventArgs> LineInserted;

        /// <summary>
        /// Occurs when a line is removed from the document.
        /// </summary>
        /// <remarks>
        /// This event fires after one or more lines have been removed. The event arguments
        /// include the IDs of removed lines, allowing subscribers to update references
        /// or cleanup associated data.
        /// </remarks>
        public event EventHandler<LineRemovedEventArgs> LineRemoved;

        /// <summary>
        /// Occurs when text content has been changed.
        /// </summary>
        /// <remarks>
        /// This is the primary event for responding to text modifications. It fires after
        /// changes are made, providing the range that was affected. Use this for triggering
        /// syntax highlighting, auto-completion, or other text-dependent features.
        /// </remarks>
        public event EventHandler<TextChangedEventArgs> TextChanged;

        /// <summary>
        /// Occurs when recalculation of layout or rendering is needed.
        /// </summary>
        /// <remarks>
        /// This event signals that some aspect of the text requires recalculation,
        /// such as line heights, folding regions, or other layout-dependent properties.
        /// </remarks>
        public event EventHandler<TextChangedEventArgs> RecalcNeeded;

        /// <summary>
        /// Occurs when word wrap recalculation is needed.
        /// </summary>
        /// <remarks>
        /// Fires when changes to the text require the word wrap positions to be recalculated.
        /// This is separate from general recalculation to allow optimized handling of
        /// word wrap-specific updates.
        /// </remarks>
        public event EventHandler<TextChangedEventArgs> RecalcWordWrap;

        /// <summary>
        /// Occurs before text is about to be changed.
        /// </summary>
        /// <remarks>
        /// This event fires before text modifications are applied, allowing subscribers to
        /// cancel the operation or prepare for the change. Set the Cancel property in the
        /// event arguments to prevent the change from occurring.
        /// </remarks>
        public event EventHandler<TextChangingEventArgs> TextChanging;

        /// <summary>
        /// Occurs after the CurrentTB property has changed.
        /// </summary>
        /// <remarks>
        /// Fires when the currently focused FastColoredTextBox changes. This is relevant
        /// when a single TextSource is shared among multiple controls.
        /// </remarks>
        public event EventHandler CurrentTBChanged;

        /// <summary>
        /// Gets or sets the currently focused FastColoredTextBox control.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When a TextSource is shared among multiple FastColoredTextBox controls,
        /// this property indicates which control currently has focus. Commands and
        /// operations will target this control.
        /// </para>
        /// <para>
        /// Setting this property fires the CurrentTBChanged event if the value changes.
        /// </para>
        /// </remarks>
        public FastColoredTextBox CurrentTB {
            get { return currentTB; }
            set {
                if (currentTB == value)
                    return;
                currentTB = value;
                OnCurrentTBChanged(); 
            }
        }

        /// <summary>
        /// Clears the IsChanged flag on all lines.
        /// </summary>
        /// <remarks>
        /// This method resets the change tracking state for all lines in the document.
        /// Typically called after saving the document to mark all content as "saved".
        /// </remarks>
        public virtual void ClearIsChanged()
        {
            foreach(var line in lines)
                line.IsChanged = false;
        }
        
        /// <summary>
        /// Creates a new line with a unique ID.
        /// </summary>
        /// <returns>A new Line instance with an auto-generated unique ID</returns>
        /// <remarks>
        /// This is the proper way to create new Line objects. It ensures each line
        /// gets a unique identifier that persists throughout the line's lifetime.
        /// User code should call this method rather than constructing Line objects directly.
        /// </remarks>
        public virtual Line CreateLine()
        {
            return new Line(GenerateUniqueLineId());
        }

        /// <summary>
        /// Raises the CurrentTBChanged event.
        /// </summary>
        private void OnCurrentTBChanged()
        {
            if (CurrentTBChanged != null)
                CurrentTBChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets or sets the default text style.
        /// </summary>
        /// <remarks>
        /// This style is applied to characters that have no other style set (StyleIndex.None).
        /// It defines the base appearance for unstyled text. By default, it renders text
        /// with regular font style and the control's default colors.
        /// </remarks>
        public TextStyle DefaultStyle { get; set; }

        /// <summary>
        /// Initializes a new instance of the TextSource class.
        /// </summary>
        /// <param name="currentTB">The FastColoredTextBox control that will use this TextSource</param>
        /// <remarks>
        /// Creates a new TextSource with:
        /// - Empty line collection
        /// - New CommandManager for undo/redo
        /// - Styles array (16 or 32 elements depending on compilation)
        /// - Default style initialization
        /// </remarks>
        public TextSource(FastColoredTextBox currentTB)
        {
            this.CurrentTB = currentTB;
            linesAccessor = new LinesAccessor(this);
            Manager = new CommandManager(this);

            if (Enum.GetUnderlyingType(typeof(StyleIndex)) == typeof(UInt32))
                Styles = new Style[32];
            else
                Styles = new Style[16];

            InitDefaultStyle();
        }

        /// <summary>
        /// Initializes the default text style.
        /// </summary>
        /// <remarks>
        /// Creates a basic TextStyle with regular font and default colors.
        /// Override this method in derived classes to customize the default appearance.
        /// </remarks>
        public virtual void InitDefaultStyle()
        {
            DefaultStyle = new TextStyle(null, null, FontStyle.Regular);
        }

        /// <summary>
        /// Gets or sets the line at the specified index.
        /// </summary>
        /// <param name="i">The zero-based index of the line</param>
        /// <returns>The Line at the specified index</returns>
        /// <exception cref="NotImplementedException">Thrown when attempting to set a line (not supported)</exception>
        /// <remarks>
        /// Getting a line by index is a constant-time operation. Setting lines directly
        /// is not supported; use InsertLine() and RemoveLine() instead to maintain
        /// proper event notification and internal consistency.
        /// </remarks>
        public virtual Line this[int i]
        {
            get{
                 return lines[i];
            }
            set {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Determines whether the line at the specified index is loaded in memory.
        /// </summary>
        /// <param name="iLine">The zero-based line index to check</param>
        /// <returns>true if the line is loaded; false otherwise</returns>
        /// <remarks>
        /// This method is primarily used by lazy-loading implementations (e.g., FileTextSource)
        /// to determine if a line needs to be loaded from disk. In the base TextSource,
        /// all lines are always loaded, so this always returns true (or false if the line is null).
        /// </remarks>
        public virtual bool IsLineLoaded(int iLine)
        {
            return lines[iLine] != null;
        }

        /// <summary>
        /// Gets the collection of text lines as strings.
        /// </summary>
        /// <returns>An IList&lt;string&gt; providing access to line text</returns>
        /// <remarks>
        /// This returns a LinesAccessor wrapper that provides string-based access to the lines.
        /// Getting a line through this accessor returns the text content; setting a line
        /// replaces its content with the specified string. This is a convenient alternative
        /// to working directly with Line objects and Char structs.
        /// </remarks>
        public virtual IList<string> GetLines()
        {
            return linesAccessor;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the line collection.
        /// </summary>
        /// <returns>An IEnumerator&lt;Line&gt; for the lines</returns>
        public IEnumerator<Line> GetEnumerator()
        {
            return lines.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the line collection (non-generic).
        /// </summary>
        /// <returns>An IEnumerator for the lines</returns>
        IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (lines  as IEnumerator);
        }

        /// <summary>
        /// Searches for a line using binary search.
        /// </summary>
        /// <param name="item">The line to search for</param>
        /// <param name="comparer">The comparer to use for ordering</param>
        /// <returns>The zero-based index of the line if found; otherwise, a negative number</returns>
        /// <remarks>
        /// This method requires the lines to be sorted according to the comparer.
        /// It's used internally for efficient lookup operations.
        /// </remarks>
        public virtual int BinarySearch(Line item, IComparer<Line> comparer)
        {
            return lines.BinarySearch(item, comparer);
        }

        /// <summary>
        /// Generates a new unique line ID.
        /// </summary>
        /// <returns>A unique integer identifier</returns>
        /// <remarks>
        /// Line IDs are assigned sequentially starting from 0. Each ID is used exactly once
        /// within a TextSource instance. IDs are never reused, even after lines are deleted.
        /// </remarks>
        public virtual int GenerateUniqueLineId()
        {
            return lastLineUniqueId++;
        }

        /// <summary>
        /// Inserts a line at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which to insert the line</param>
        /// <param name="line">The line to insert</param>
        /// <remarks>
        /// This method inserts the line and fires the LineInserted event. All line-based
        /// operations should use this method rather than directly modifying the internal
        /// lines collection to ensure proper event notification.
        /// </remarks>
        public virtual void InsertLine(int index, Line line)
        {
            lines.Insert(index, line);
            OnLineInserted(index);
        }

        /// <summary>
        /// Raises the LineInserted event for a single line.
        /// </summary>
        /// <param name="index">The index where the line was inserted</param>
        public virtual void OnLineInserted(int index)
        {
            OnLineInserted(index, 1);
        }

        /// <summary>
        /// Raises the LineInserted event for one or more lines.
        /// </summary>
        /// <param name="index">The index where the lines were inserted</param>
        /// <param name="count">The number of lines inserted</param>
        public virtual void OnLineInserted(int index, int count)
        {
            if (LineInserted != null)
                LineInserted(this, new LineInsertedEventArgs(index, count));
        }

        /// <summary>
        /// Removes a single line at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the line to remove</param>
        /// <remarks>
        /// This is a convenience method that calls RemoveLine(index, 1).
        /// The LineRemoved event will be fired with the removed line's ID.
        /// </remarks>
        public virtual void RemoveLine(int index)
        {
            RemoveLine(index, 1);
        }

        /// <summary>
        /// Gets a value indicating whether removed line IDs need to be collected.
        /// </summary>
        /// <remarks>
        /// Returns true if there are LineRemoved event subscribers that need the IDs
        /// of removed lines. This optimization avoids building the ID list when no
        /// subscribers need it.
        /// </remarks>
        public virtual bool IsNeedBuildRemovedLineIds
        {
            get { return LineRemoved != null; }
        }

        /// <summary>
        /// Removes one or more lines starting at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the first line to remove</param>
        /// <param name="count">The number of lines to remove</param>
        /// <remarks>
        /// This method removes the specified range of lines and collects their unique IDs
        /// (if subscribers need them). It then fires the LineRemoved event with the IDs,
        /// allowing subscribers to update their line-based data structures.
        /// </remarks>
        public virtual void RemoveLine(int index, int count)
        {
            List<int> removedLineIds = new List<int>();
            //
            if (count > 0)
                if (IsNeedBuildRemovedLineIds)
                    for (int i = 0; i < count; i++)
                        removedLineIds.Add(this[index + i].UniqueId);
            //
            lines.RemoveRange(index, count);

            OnLineRemoved(index, count, removedLineIds);
        }

        /// <summary>
        /// Raises the LineRemoved event.
        /// </summary>
        /// <param name="index">The index where lines were removed</param>
        /// <param name="count">The number of lines removed</param>
        /// <param name="removedLineIds">The unique IDs of the removed lines</param>
        public virtual void OnLineRemoved(int index, int count, List<int> removedLineIds)
        {
            if (count > 0)
                if (LineRemoved != null)
                    LineRemoved(this, new LineRemovedEventArgs(index, count, removedLineIds));
        }

        public virtual void OnTextChanged(int fromLine, int toLine)
        {
            if (TextChanged != null)
                TextChanged(this, new TextChangedEventArgs(Math.Min(fromLine, toLine), Math.Max(fromLine, toLine) ));
        }

        public class TextChangedEventArgs : EventArgs
        {
            public int iFromLine;
            public int iToLine;

            public TextChangedEventArgs(int iFromLine, int iToLine)
            {
                this.iFromLine = iFromLine;
                this.iToLine = iToLine;
            }
        }

        public virtual int IndexOf(Line item)
        {
            return lines.IndexOf(item);
        }

        public virtual void Insert(int index, Line item)
        {
            InsertLine(index, item);
        }

        public virtual void RemoveAt(int index)
        {
            RemoveLine(index);
        }

        public virtual void Add(Line item)
        {
            InsertLine(Count, item);
        }

        public virtual void Clear()
        {
            RemoveLine(0, Count);
        }

        public virtual bool Contains(Line item)
        {
            return lines.Contains(item);
        }

        public virtual void CopyTo(Line[] array, int arrayIndex)
        {
            lines.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Lines count
        /// </summary>
        public virtual int Count
        {
            get { return lines.Count; }
        }

        public virtual bool IsReadOnly
        {
            get { return false; }
        }

        public virtual bool Remove(Line item)
        {
            int i = IndexOf(item);
            if (i >= 0)
            {
                RemoveLine(i);
                return true;
            }
            else
                return false;
        }

        public virtual void NeedRecalc(TextChangedEventArgs args)
        {
            if (RecalcNeeded != null)
                RecalcNeeded(this, args);
        }

        public virtual void OnRecalcWordWrap(TextChangedEventArgs args)
        {
            if (RecalcWordWrap != null)
                RecalcWordWrap(this, args);
        }

        public virtual void OnTextChanging()
        {
            string temp = null;
            OnTextChanging(ref temp);
        }

        public virtual void OnTextChanging(ref string text)
        {
            if (TextChanging != null)
            {
                var args = new TextChangingEventArgs() { InsertingText = text };
                TextChanging(this, args);
                text = args.InsertingText;
                if (args.Cancel)
                    text = string.Empty;
            };
        }

        public virtual int GetLineLength(int i)
        {
            return lines[i].Count;
        }

        public virtual bool LineHasFoldingStartMarker(int iLine)
        {
            return !string.IsNullOrEmpty(lines[iLine].FoldingStartMarker);
        }

        public virtual bool LineHasFoldingEndMarker(int iLine)
        {
            return !string.IsNullOrEmpty(lines[iLine].FoldingEndMarker);
        }

        public virtual void Dispose()
        {
            ;
        }

        public virtual void SaveToFile(string fileName, Encoding enc)
        {
            using (StreamWriter sw = new StreamWriter(fileName, false, enc))
            {
                for (int i = 0; i < Count - 1;i++ )
                    sw.WriteLine(lines[i].Text);

                sw.Write(lines[Count-1].Text);
            }
        }
    }
}
