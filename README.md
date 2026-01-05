FastColoredTextBox
==================

Fast Colored TextBox is text editor component for .NET.
Allows you to create custom text editor with syntax highlighting.
It works well with small, medium, large and very-very large files.

It has such settings as foreground color, font style, background color which can be adjusted for arbitrarily selected text symbols. One can easily gain access to a text with the use of regular expressions. WordWrap, Find/Replace, Code folding and multilevel Undo/Redo are supported as well. 

![Fast Colored TextBox](http://www.codeproject.com/KB/edit/FastColoredTextBox_/fastcoloredtextbox2.png)

More details http://www.codeproject.com/Articles/161871/Fast-Colored-TextBox-for-syntax-highlighting

Nuget package https://www.nuget.org/packages/FCTB/

## Documentation

### Code Documentation

This project includes comprehensive code documentation to help developers understand and use the FastColoredTextBox component:

#### 1. Architecture Documentation
See [ARCHITECTURE.md](ARCHITECTURE.md) for a detailed explanation of:
- Overall architecture and design philosophy
- Core components and their responsibilities
- Data structures (Place, Char, Line, Range, TextSource)
- Styling system
- Syntax highlighting mechanism
- Command pattern for undo/redo
- Performance optimizations
- Extension points and common patterns

#### 2. XML Documentation Comments
All key classes, methods, and properties include comprehensive XML documentation comments:
- **Place**: Position representation in the text
- **Char**: Character with style information
- **Line**: Single line of text with metadata
- **Range**: Text selection and manipulation
- **Style**: Base class for visual rendering
- **TextStyle**: Colored text rendering
- **TextSource**: Text storage and management
- **FastColoredTextBox**: Main control with extensive API documentation

#### 3. IntelliSense Support
When using this component in Visual Studio or other .NET IDEs:
1. XML documentation comments provide IntelliSense tooltips
2. Parameter descriptions appear as you type
3. Comprehensive remarks explain usage patterns and examples
4. Navigate to definitions to see detailed documentation

#### 4. Using the Documentation

**In your IDE:**
```csharp
// Hover over any class, property, or method to see documentation
var textBox = new FastColoredTextBox();
textBox.Language = Language.CSharp;  // Hover to see supported languages
textBox.Selection.Text = "Hello";     // Hover to see Selection details
```

**Building API Documentation:**
To generate HTML documentation from XML comments, use tools like:
- Sandcastle Help File Builder
- DocFX
- Doxygen with C# support

**Quick Start Example:**
```csharp
using FastColoredTextBoxNS;

// Create and configure the text box
var editor = new FastColoredTextBox();
editor.Language = Language.CSharp;
editor.ShowLineNumbers = true;

// Set some code with syntax highlighting
editor.Text = @"using System;
class Program {
    static void Main() {
        Console.WriteLine(""Hello World"");
    }
}";

// Work with selections
editor.Selection.Start = new Place(0, 0);
editor.Selection.End = new Place(5, 0);
Console.WriteLine(editor.Selection.Text);  // Outputs: "using"

// Apply custom styles
var boldStyle = new TextStyle(Brushes.Black, null, FontStyle.Bold);
editor.Range.SetStyle(boldStyle, @"\bclass\b");
```

For more examples, see the `Tester` project in this repository.

### Additional Resources

- **Help File**: See `Help/FastColoredTextBox_Help.chm` for compiled help documentation
- **Sample Projects**: Explore the `Tester` folder for numerous usage examples
- **CodeProject Article**: Detailed tutorial at the link above
