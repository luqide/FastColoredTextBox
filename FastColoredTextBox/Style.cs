using System.Drawing;
using System;
using System.Drawing.Drawing2D;
using System.Collections.Generic;

namespace FastColoredTextBoxNS
{
    /// <summary>
    /// Base class for all text rendering styles.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Style class is the foundation of the styling system in FastColoredTextBox.
    /// It defines how ranges of text should be visually rendered. All styles must inherit
    /// from this class and implement the Draw() method.
    /// </para>
    /// <para>
    /// Key concepts:
    /// - Each style is stored in the TextSource.Styles array (max 16 or 32 styles)
    /// - Characters reference styles through a bit mask in their StyleIndex
    /// - Multiple styles can be applied to the same character
    /// - Styles support export to HTML and RTF formats
    /// </para>
    /// <para>
    /// Common style implementations include:
    /// - TextStyle: Renders colored text with font variations
    /// - SelectionStyle: Renders selection highlighting
    /// - FoldedBlockStyle: Renders collapsed code blocks
    /// - Custom styles for special rendering effects
    /// </para>
    /// </remarks>
    public abstract class Style : IDisposable
    {
        /// <summary>
        /// Gets or sets a value indicating whether this style should be exported to external formats.
        /// </summary>
        /// <remarks>
        /// When true (default), this style will be included when exporting to HTML, RTF, or other
        /// formats. Set to false for styles that are purely visual aids (like selection highlighting)
        /// that shouldn't appear in exported documents.
        /// </remarks>
        public virtual bool IsExportable { get; set; }

        /// <summary>
        /// Occurs when a user clicks on a StyleVisualMarker associated with this style.
        /// </summary>
        /// <remarks>
        /// StyleVisualMarkers are interactive UI elements that can be added by styles during
        /// rendering. This event allows handling user interaction with those markers, enabling
        /// features like clickable hyperlinks, expandable code blocks, or interactive tooltips.
        /// </remarks>
        public event EventHandler<VisualMarkerEventArgs> VisualMarkerClick;

        /// <summary>
        /// Initializes a new instance of the Style class.
        /// </summary>
        /// <remarks>
        /// Sets IsExportable to true by default, making the style visible in exported documents.
        /// </remarks>
        public Style()
        {
            IsExportable = true;
        }

        /// <summary>
        /// Renders the specified range of text with this style.
        /// </summary>
        /// <param name="gr">The Graphics object to draw on</param>
        /// <param name="position">The position in absolute control coordinates where rendering should start</param>
        /// <param name="range">The range of text to render with this style</param>
        /// <remarks>
        /// This is the core method that all style implementations must override. It is called
        /// during the paint cycle for each range of text that has this style applied.
        /// The implementation should:
        /// 1. Draw backgrounds (if any) at the specified position
        /// 2. Draw text characters with appropriate formatting
        /// 3. Optionally add visual markers for interactivity
        /// Note: The position is in control coordinates, not screen coordinates.
        /// </remarks>
        public abstract void Draw(Graphics gr, Point position, Range range);

        /// <summary>
        /// Raises the VisualMarkerClick event.
        /// </summary>
        /// <param name="tb">The FastColoredTextBox control where the click occurred</param>
        /// <param name="args">Event arguments containing information about the visual marker</param>
        /// <remarks>
        /// This method is called internally when a user clicks on a visual marker associated
        /// with this style. Override this method to customize click handling behavior, or
        /// subscribe to the VisualMarkerClick event in your code.
        /// </remarks>
        public virtual void OnVisualMarkerClick(FastColoredTextBox tb, VisualMarkerEventArgs args)
        {
            if (VisualMarkerClick != null)
                VisualMarkerClick(tb, args);
        }

        /// <summary>
        /// Adds a visual marker to the control.
        /// </summary>
        /// <param name="tb">The FastColoredTextBox control to add the marker to</param>
        /// <param name="marker">The StyleVisualMarker to add</param>
        /// <remarks>
        /// Call this method in your Draw() implementation when you need to add interactive
        /// visual elements. Visual markers can respond to mouse clicks and other user interactions.
        /// Examples include clickable hyperlinks, expand/collapse buttons for code folding, or
        /// custom UI widgets embedded in the text.
        /// </remarks>
        protected virtual void AddVisualMarker(FastColoredTextBox tb, StyleVisualMarker marker)
        {
            tb.AddVisualMarker(marker);
        }

        /// <summary>
        /// Gets the size of the specified range in pixels.
        /// </summary>
        /// <param name="range">The range to measure</param>
        /// <returns>A Size structure containing the width and height in pixels</returns>
        /// <remarks>
        /// This helper method calculates the rectangular size of a text range based on
        /// the control's character width and height. It assumes monospaced font rendering.
        /// Width = (end char - start char) x character width
        /// Height = character height
        /// </remarks>
        public static Size GetSizeOfRange(Range range)
        {
            return new Size((range.End.iChar - range.Start.iChar) * range.tb.CharWidth, range.tb.CharHeight);
        }

        /// <summary>
        /// Creates a GraphicsPath representing a rounded rectangle.
        /// </summary>
        /// <param name="rect">The rectangle to round</param>
        /// <param name="d">The diameter of the rounded corners</param>
        /// <returns>A GraphicsPath object representing the rounded rectangle</returns>
        /// <remarks>
        /// This utility method creates a path for drawing rounded rectangles, commonly used
        /// for rendering styled backgrounds, tooltips, or selection highlights with rounded corners.
        /// The corners are circular arcs with diameter d.
        /// </remarks>
        public static GraphicsPath GetRoundedRectangle(Rectangle rect, int d)
        {
            GraphicsPath gp = new GraphicsPath();

            gp.AddArc(rect.X, rect.Y, d, d, 180, 90);
            gp.AddArc(rect.X + rect.Width - d, rect.Y, d, d, 270, 90);
            gp.AddArc(rect.X + rect.Width - d, rect.Y + rect.Height - d, d, d, 0, 90);
            gp.AddArc(rect.X, rect.Y + rect.Height - d, d, d, 90, 90);
            gp.AddLine(rect.X, rect.Y + rect.Height - d, rect.X, rect.Y + d / 2);

            return gp;
        }

        /// <summary>
        /// Performs cleanup of resources used by this style.
        /// </summary>
        /// <remarks>
        /// Override this method to dispose of any GDI+ resources (brushes, pens, fonts)
        /// allocated by your style. The base implementation does nothing.
        /// </remarks>
        public virtual void Dispose()
        {
            ;
        }

        /// <summary>
        /// Returns the CSS representation of this style for HTML export.
        /// </summary>
        /// <returns>A CSS string defining this style's visual properties</returns>
        /// <remarks>
        /// Override this method to provide CSS styling when exporting to HTML format.
        /// The returned string should contain valid CSS property declarations without
        /// selectors (e.g., "color:red;font-weight:bold;"). Return an empty string if
        /// this style doesn't apply to HTML export.
        /// </remarks>
        public virtual string GetCSS()
        {
            return "";
        }

        /// <summary>
        /// Returns the RTF style descriptor for RTF export.
        /// </summary>
        /// <returns>An RTFStyleDescriptor object describing this style in RTF format</returns>
        /// <remarks>
        /// Override this method to provide RTF styling when exporting to Rich Text Format.
        /// The RTFStyleDescriptor contains properties for colors, font styles, and RTF-specific
        /// formatting tags. Return an empty descriptor if this style doesn't apply to RTF export.
        /// </remarks>
        public virtual RTFStyleDescriptor GetRTF()
        {
            return new RTFStyleDescriptor();
        }
    }

    /// <summary>
    /// A style for rendering colored text with font variations.
    /// </summary>
    /// <remarks>
    /// TextStyle is the most commonly used style implementation. It allows rendering text with:
    /// - Custom foreground color (text color)
    /// - Custom background color
    /// - Font style variations (bold, italic, underline, strikeout)
    /// This style supports export to both HTML and RTF formats.
    /// </remarks>
    public class TextStyle : Style
    {
        /// <summary>
        /// Gets or sets the brush used to draw the text characters.
        /// </summary>
        /// <remarks>
        /// This brush determines the color of the text. If null, the default ForeColor
        /// of the FastColoredTextBox will be used.
        /// </remarks>
        public Brush ForeBrush { get; set; }

        /// <summary>
        /// Gets or sets the brush used to draw the background behind the text.
        /// </summary>
        /// <remarks>
        /// This brush fills the rectangular area behind the text. If null, no background
        /// is drawn (the control's background shows through).
        /// </remarks>
        public Brush BackgroundBrush { get; set; }

        /// <summary>
        /// Gets or sets the font style (bold, italic, underline, strikeout) for the text.
        /// </summary>
        /// <remarks>
        /// This property determines font variations applied to the base font.
        /// Multiple FontStyle flags can be combined using bitwise OR.
        /// </remarks>
        public FontStyle FontStyle { get; set; }

        /// <summary>
        /// Gets or sets the string format used for rendering text.
        /// </summary>
        /// <remarks>
        /// The string format controls text rendering details. By default, it's configured
        /// to measure trailing spaces, ensuring proper character spacing.
        /// </remarks>
        public StringFormat stringFormat;

        /// <summary>
        /// Initializes a new instance of the TextStyle class.
        /// </summary>
        /// <param name="foreBrush">The brush for drawing text (null to use default)</param>
        /// <param name="backgroundBrush">The brush for drawing background (null for transparent)</param>
        /// <param name="fontStyle">The font style variations to apply</param>
        public TextStyle(Brush foreBrush, Brush backgroundBrush, FontStyle fontStyle)
        {
            this.ForeBrush = foreBrush;
            this.BackgroundBrush = backgroundBrush;
            this.FontStyle = fontStyle;
            stringFormat = new StringFormat(StringFormatFlags.MeasureTrailingSpaces);
        }

        public override void Draw(Graphics gr, Point position, Range range)
        {
            //draw background
            if (BackgroundBrush != null)
                gr.FillRectangle(BackgroundBrush, position.X, position.Y, (range.End.iChar - range.Start.iChar) * range.tb.CharWidth, range.tb.CharHeight);
            //draw chars
            using(var f = new Font(range.tb.Font, FontStyle))
            {
                Line line = range.tb[range.Start.iLine];
                float dx = range.tb.CharWidth;
                float y = position.Y + range.tb.LineInterval/2;
                float x = position.X - range.tb.CharWidth/3;

                if (ForeBrush == null)
                    ForeBrush = new SolidBrush(range.tb.ForeColor);

                if (range.tb.ImeAllowed)
                {
                    //IME mode
                    for (int i = range.Start.iChar; i < range.End.iChar; i++)
                    {
                        SizeF size = FastColoredTextBox.GetCharSize(f, line[i].c);

                        var gs = gr.Save();
                        float k = size.Width > range.tb.CharWidth + 1 ? range.tb.CharWidth/size.Width : 1;
                        gr.TranslateTransform(x, y + (1 - k)*range.tb.CharHeight/2);
                        gr.ScaleTransform(k, (float) Math.Sqrt(k));
                        gr.DrawString(line[i].c.ToString(), f, ForeBrush, 0, 0, stringFormat);
                        gr.Restore(gs);
                        x += dx;
                    }
                }
                else
                {
                    //classic mode 
                    for (int i = range.Start.iChar; i < range.End.iChar; i++)
                    {
                        //draw char
                        gr.DrawString(line[i].c.ToString(), f, ForeBrush, x, y, stringFormat);
                        x += dx;
                    }
                }
            }
        }

        public override string GetCSS()
        {
            string result = "";

            if (BackgroundBrush is SolidBrush)
            {
                var s =  ExportToHTML.GetColorAsString((BackgroundBrush as SolidBrush).Color);
                if (s != "")
                    result += "background-color:" + s + ";";
            }
            if (ForeBrush is SolidBrush)
            {
                var s = ExportToHTML.GetColorAsString((ForeBrush as SolidBrush).Color);
                if (s != "")
                    result += "color:" + s + ";";
            }
            if ((FontStyle & FontStyle.Bold) != 0)
                result += "font-weight:bold;";
            if ((FontStyle & FontStyle.Italic) != 0)
                result += "font-style:oblique;";
            if ((FontStyle & FontStyle.Strikeout) != 0)
                result += "text-decoration:line-through;";
            if ((FontStyle & FontStyle.Underline) != 0)
                result += "text-decoration:underline;";

            return result;
        }

        public override RTFStyleDescriptor GetRTF()
        {
            var result = new RTFStyleDescriptor();

            if (BackgroundBrush is SolidBrush)
                result.BackColor = (BackgroundBrush as SolidBrush).Color;
            
            if (ForeBrush is SolidBrush)
                result.ForeColor = (ForeBrush as SolidBrush).Color;
            
            if ((FontStyle & FontStyle.Bold) != 0)
                result.AdditionalTags += @"\b";
            if ((FontStyle & FontStyle.Italic) != 0)
                result.AdditionalTags += @"\i";
            if ((FontStyle & FontStyle.Strikeout) != 0)
                result.AdditionalTags += @"\strike";
            if ((FontStyle & FontStyle.Underline) != 0)
                result.AdditionalTags += @"\ul";

            return result;
        }
    }

    /// <summary>
    /// Renderer for folded block
    /// </summary>
    public class FoldedBlockStyle : TextStyle
    {
        public FoldedBlockStyle(Brush foreBrush, Brush backgroundBrush, FontStyle fontStyle):
            base(foreBrush, backgroundBrush, fontStyle)
        {
        }

        public override void Draw(Graphics gr, Point position, Range range)
        {
            if (range.End.iChar > range.Start.iChar)
            {
                base.Draw(gr, position, range);

                int firstNonSpaceSymbolX = position.X;
                
                //find first non space symbol
                for (int i = range.Start.iChar; i < range.End.iChar; i++)
                    if (range.tb[range.Start.iLine][i].c != ' ')
                        break;
                    else
                        firstNonSpaceSymbolX += range.tb.CharWidth;

                //create marker
                range.tb.AddVisualMarker(new FoldedAreaMarker(range.Start.iLine, new Rectangle(firstNonSpaceSymbolX, position.Y, position.X + (range.End.iChar - range.Start.iChar) * range.tb.CharWidth - firstNonSpaceSymbolX, range.tb.CharHeight)));
            }
            else
            {
                //draw '...'
                using(Font f = new Font(range.tb.Font, FontStyle))
                    gr.DrawString("...", f, ForeBrush, range.tb.LeftIndent, position.Y - 2);
                //create marker
                range.tb.AddVisualMarker(new FoldedAreaMarker(range.Start.iLine, new Rectangle(range.tb.LeftIndent + 2, position.Y, 2 * range.tb.CharHeight, range.tb.CharHeight)));
            }
        }
    }

    /// <summary>
    /// Renderer for selected area
    /// </summary>
    public class SelectionStyle : Style
    {
        public Brush BackgroundBrush{ get; set;}
        public Brush ForegroundBrush { get; private set; }

        public override bool IsExportable
        {
            get{return false;}  set{}
        }

        public SelectionStyle(Brush backgroundBrush, Brush foregroundBrush = null)
        {
            this.BackgroundBrush = backgroundBrush;
            this.ForegroundBrush = foregroundBrush;
        }

        public override void Draw(Graphics gr, Point position, Range range)
        {
            //draw background
            if (BackgroundBrush != null)
            {
                gr.SmoothingMode = SmoothingMode.None;
                var rect = new Rectangle(position.X, position.Y, (range.End.iChar - range.Start.iChar) * range.tb.CharWidth, range.tb.CharHeight);
                if (rect.Width == 0)
                    return;
                gr.FillRectangle(BackgroundBrush, rect);
                //
                if (ForegroundBrush != null)
                {
                    //draw text
                    gr.SmoothingMode = SmoothingMode.AntiAlias;

                    var r = new Range(range.tb, range.Start.iChar, range.Start.iLine,
                                      Math.Min(range.tb[range.End.iLine].Count, range.End.iChar), range.End.iLine);
                    using (var style = new TextStyle(ForegroundBrush, null, FontStyle.Regular))
                        style.Draw(gr, new Point(position.X, position.Y - 1), r);
                }
            }
        }
    }

    /// <summary>
    /// Marker style
    /// Draws background color for text
    /// </summary>
    public class MarkerStyle : Style
    {
        public Brush BackgroundBrush{get;set;}

        public MarkerStyle(Brush backgroundBrush)
        {
            this.BackgroundBrush = backgroundBrush;
            IsExportable = true;
        }

        public override void Draw(Graphics gr, Point position, Range range)
        {
            //draw background
            if (BackgroundBrush != null)
            {
                Rectangle rect = new Rectangle(position.X, position.Y, (range.End.iChar - range.Start.iChar) * range.tb.CharWidth, range.tb.CharHeight);
                if (rect.Width == 0)
                    return;
                gr.FillRectangle(BackgroundBrush, rect);
            }
        }

        public override string GetCSS()
        {
            string result = "";

            if (BackgroundBrush is SolidBrush)
            {
                var s = ExportToHTML.GetColorAsString((BackgroundBrush as SolidBrush).Color);
                if (s != "")
                    result += "background-color:" + s + ";";
            }

            return result;
        }
    }

    /// <summary>
    /// Draws small rectangle for popup menu
    /// </summary>
    public class ShortcutStyle : Style
    {
        public Pen borderPen;

        public ShortcutStyle(Pen borderPen)
        {
            this.borderPen = borderPen;
        }

        public override void Draw(Graphics gr, Point position, Range range)
        {
            //get last char coordinates
            Point p = range.tb.PlaceToPoint(range.End);
            //draw small square under char
            Rectangle rect = new Rectangle(p.X - 5, p.Y + range.tb.CharHeight - 2, 4, 3);
            gr.FillPath(Brushes.White, GetRoundedRectangle(rect, 1));
            gr.DrawPath(borderPen, GetRoundedRectangle(rect, 1));
            //add visual marker for handle mouse events
            AddVisualMarker(range.tb, new StyleVisualMarker(new Rectangle(p.X-range.tb.CharWidth, p.Y, range.tb.CharWidth, range.tb.CharHeight), this));
        }
    }

    /// <summary>
    /// This style draws a wavy line below a given text range.
    /// </summary>
    /// <remarks>Thanks for Yallie</remarks>
    public class WavyLineStyle : Style
    {
        private Pen Pen { get; set; }

        public WavyLineStyle(int alpha, Color color)
        {
            Pen = new Pen(Color.FromArgb(alpha, color));
        }

        public override void Draw(Graphics gr, Point pos, Range range)
        {
            var size = GetSizeOfRange(range);
            var start = new Point(pos.X, pos.Y + size.Height - 1);
            var end = new Point(pos.X + size.Width, pos.Y + size.Height - 1);
            DrawWavyLine(gr, start, end);
        }

        private void DrawWavyLine(Graphics graphics, Point start, Point end)
        {
            if (end.X - start.X < 2)
            {
                graphics.DrawLine(Pen, start, end);
                return;
            }

            var offset = -1;
            var points = new List<Point>();

            for (int i = start.X; i <= end.X; i += 2)
            {
                points.Add(new Point(i, start.Y + offset));
                offset = -offset;
            }

            graphics.DrawLines(Pen, points.ToArray());
        }

        public override void Dispose()
        {
            base.Dispose();

            if (Pen != null)
                Pen.Dispose();
        }
    }

    /// <summary>
    /// This style is used to mark range of text as ReadOnly block
    /// </summary>
    /// <remarks>You can inherite this style to add visual effects of readonly text</remarks>
    public class ReadOnlyStyle : Style
    {
        public ReadOnlyStyle()
        {
            IsExportable = false;
        }

        public override void Draw(Graphics gr, Point position, Range range)
        {
            //
        }
    }
}
