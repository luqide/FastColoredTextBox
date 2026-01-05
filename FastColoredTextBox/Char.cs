using System;

namespace FastColoredTextBoxNS
{
    /// <summary>
    /// Represents a single character with its associated style information.
    /// </summary>
    /// <remarks>
    /// The Char struct is the basic unit of text storage in FastColoredTextBox.
    /// It combines a Unicode character with a StyleIndex bit mask that determines
    /// how the character should be rendered. Multiple styles can be applied to a
    /// single character by setting multiple bits in the style field.
    /// </remarks>
    public struct Char
    {
        /// <summary>
        /// The Unicode character.
        /// </summary>
        /// <remarks>
        /// This field stores the actual text character. It supports the full Unicode
        /// character set, allowing for international text and special symbols.
        /// </remarks>
        public char c;

        /// <summary>
        /// Style bit mask indicating which styles apply to this character.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Each bit position in the StyleIndex corresponds to a style in the
        /// FastColoredTextBox.Styles array. If bit N is set to 1, the character
        /// will be rendered using the style at Styles[N].
        /// </para>
        /// <para>
        /// By default, FastColoredTextBox supports 16 simultaneous styles, but can
        /// be compiled with the Styles32 directive to support 32 styles.
        /// </para>
        /// <para>
        /// Example: If style has bits 0 and 3 set, the character will be rendered
        /// with both Styles[0] and Styles[3] applied (colors and formatting combined).
        /// </para>
        /// </remarks>
        public StyleIndex style;

        /// <summary>
        /// Initializes a new instance of the Char struct with the specified character.
        /// </summary>
        /// <param name="c">The Unicode character to store</param>
        /// <remarks>
        /// Creates a new Char with the specified character and no styles applied
        /// (style is set to StyleIndex.None).
        /// </remarks>
        public Char(char c)
        {
            this.c = c;
            style = StyleIndex.None;
        }
    }
}
