using System;

namespace FastColoredTextBoxNS
{
    /// <summary>
    /// Represents a position in the text by line and character index.
    /// </summary>
    /// <remarks>
    /// Place is a fundamental data structure used throughout FastColoredTextBox to identify
    /// a specific position in the text. It uses zero-based indexing for both line and character positions.
    /// The struct is immutable and provides comparison operators for ordering positions.
    /// </remarks>
    public struct Place : IEquatable<Place>
    {
        /// <summary>
        /// Character index within the line (zero-based).
        /// </summary>
        /// <remarks>
        /// Represents the horizontal position within a line. For example, iChar=0 is the first character,
        /// iChar=5 is the sixth character. When iChar equals the line's character count, it represents
        /// the position at the end of the line.
        /// </remarks>
        public int iChar;

        /// <summary>
        /// Line index in the document (zero-based).
        /// </summary>
        /// <remarks>
        /// Represents the vertical position in the document. For example, iLine=0 is the first line,
        /// iLine=10 is the eleventh line.
        /// </remarks>
        public int iLine;

        /// <summary>
        /// Initializes a new instance of the Place struct with specified character and line indices.
        /// </summary>
        /// <param name="iChar">Character index within the line (zero-based)</param>
        /// <param name="iLine">Line index in the document (zero-based)</param>
        public Place(int iChar, int iLine)
        {
            this.iChar = iChar;
            this.iLine = iLine;
        }

        /// <summary>
        /// Offsets the position by the specified character and line deltas.
        /// </summary>
        /// <param name="dx">Number of characters to move horizontally (positive for right, negative for left)</param>
        /// <param name="dy">Number of lines to move vertically (positive for down, negative for up)</param>
        /// <remarks>
        /// This method modifies the current Place instance. Use this for relative positioning.
        /// Note that this method does not validate whether the resulting position is within document bounds.
        /// </remarks>
        public void Offset(int dx, int dy)
        {
            iChar += dx;
            iLine += dy;
        }

        /// <summary>
        /// Determines whether this Place is equal to another Place.
        /// </summary>
        /// <param name="other">The Place to compare with this instance</param>
        /// <returns>true if both iChar and iLine values are equal; otherwise, false</returns>
        /// <remarks>
        /// Two Places are considered equal if they point to the same position in the text,
        /// meaning both their character and line indices must match.
        /// </remarks>
        public bool Equals(Place other)
        {
            return iChar == other.iChar && iLine == other.iLine;
        }

        /// <summary>
        /// Determines whether this Place is equal to the specified object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance</param>
        /// <returns>true if obj is a Place and is equal to this instance; otherwise, false</returns>
        public override bool Equals(object obj)
        {
            return (obj is Place) && Equals((Place)obj);
        }

        /// <summary>
        /// Returns a hash code for this Place.
        /// </summary>
        /// <returns>A hash code combining the iChar and iLine values</returns>
        public override int GetHashCode()
        {
            return iChar.GetHashCode() ^ iLine.GetHashCode();
        }

        /// <summary>
        /// Determines whether two Place instances are not equal.
        /// </summary>
        /// <param name="p1">The first Place to compare</param>
        /// <param name="p2">The second Place to compare</param>
        /// <returns>true if the Place instances are not equal; otherwise, false</returns>
        public static bool operator !=(Place p1, Place p2)
        {
            return !p1.Equals(p2);
        }

        /// <summary>
        /// Determines whether two Place instances are equal.
        /// </summary>
        /// <param name="p1">The first Place to compare</param>
        /// <param name="p2">The second Place to compare</param>
        /// <returns>true if the Place instances are equal; otherwise, false</returns>
        public static bool operator ==(Place p1, Place p2)
        {
            return p1.Equals(p2);
        }

        /// <summary>
        /// Determines whether the first Place is less than the second Place.
        /// </summary>
        /// <param name="p1">The first Place to compare</param>
        /// <param name="p2">The second Place to compare</param>
        /// <returns>true if p1 comes before p2 in the text; otherwise, false</returns>
        /// <remarks>
        /// Comparison is done first by line, then by character within the line.
        /// For example, (0,1) &lt; (5,1) &lt; (0,2).
        /// </remarks>
        public static bool operator <(Place p1, Place p2)
        {
            if (p1.iLine < p2.iLine) return true;
            if (p1.iLine > p2.iLine) return false;
            if (p1.iChar < p2.iChar) return true;
            return false;
        }

        /// <summary>
        /// Determines whether the first Place is less than or equal to the second Place.
        /// </summary>
        /// <param name="p1">The first Place to compare</param>
        /// <param name="p2">The second Place to compare</param>
        /// <returns>true if p1 comes before or is equal to p2 in the text; otherwise, false</returns>
        public static bool operator <=(Place p1, Place p2)
        {
            if (p1.Equals(p2)) return true;
            if (p1.iLine < p2.iLine) return true;
            if (p1.iLine > p2.iLine) return false;
            if (p1.iChar < p2.iChar) return true;
            return false;
        }

        /// <summary>
        /// Determines whether the first Place is greater than the second Place.
        /// </summary>
        /// <param name="p1">The first Place to compare</param>
        /// <param name="p2">The second Place to compare</param>
        /// <returns>true if p1 comes after p2 in the text; otherwise, false</returns>
        /// <remarks>
        /// Comparison is done first by line, then by character within the line.
        /// </remarks>
        public static bool operator >(Place p1, Place p2)
        {
            if (p1.iLine > p2.iLine) return true;
            if (p1.iLine < p2.iLine) return false;
            if (p1.iChar > p2.iChar) return true;
            return false;
        }

        /// <summary>
        /// Determines whether the first Place is greater than or equal to the second Place.
        /// </summary>
        /// <param name="p1">The first Place to compare</param>
        /// <param name="p2">The second Place to compare</param>
        /// <returns>true if p1 comes after or is equal to p2 in the text; otherwise, false</returns>
        public static bool operator >=(Place p1, Place p2)
        {
            if (p1.Equals(p2)) return true;
            if (p1.iLine > p2.iLine) return true;
            if (p1.iLine < p2.iLine) return false;
            if (p1.iChar > p2.iChar) return true;
            return false;
        }

        /// <summary>
        /// Adds two Place instances component-wise.
        /// </summary>
        /// <param name="p1">The first Place to add</param>
        /// <param name="p2">The second Place to add</param>
        /// <returns>A new Place with the sum of corresponding components</returns>
        /// <remarks>
        /// This operation adds iChar values together and iLine values together.
        /// It's primarily used for offset calculations.
        /// </remarks>
        public static Place operator +(Place p1, Place p2)
        {
            return new Place(p1.iChar + p2.iChar, p1.iLine + p2.iLine);
        }

        /// <summary>
        /// Gets an empty Place instance with both iChar and iLine set to 0.
        /// </summary>
        /// <remarks>
        /// This represents the very first position in the document (top-left corner).
        /// </remarks>
        public static Place Empty
        {
            get { return new Place(); }
        }

        /// <summary>
        /// Returns a string representation of this Place.
        /// </summary>
        /// <returns>A string in the format "(iChar,iLine)"</returns>
        /// <remarks>
        /// Example output: "(10,5)" represents character 10 on line 5.
        /// </remarks>
        public override string ToString()
        {
            return "(" + iChar + "," + iLine + ")";
        }
    }
}
