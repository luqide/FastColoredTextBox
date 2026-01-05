# FastColoredTextBox Architecture

## Overview

FastColoredTextBox is a high-performance text editor component for .NET applications. It provides syntax highlighting, code folding, find/replace, autocomplete, and other advanced text editing features while maintaining good performance even with very large files.

## Core Architecture

### Design Philosophy

The component follows a **layered architecture** with clear separation of concerns:
- **Text Storage Layer**: Manages the actual text data
- **Rendering Layer**: Handles visual representation
- **Editing Layer**: Manages user input and text modifications
- **Syntax Layer**: Provides syntax highlighting and language support

### Key Components

#### 1. Text Storage (`TextSource.cs`)

The `TextSource` class is the foundation of the text editor. It:
- Stores text as a collection of `Line` objects
- Manages the undo/redo command stack via `CommandManager`
- Maintains style information for syntax highlighting
- Fires events when text changes

**Key Features:**
- Efficient line-based storage
- Command pattern for undo/redo operations
- Event-driven architecture for updates

#### 2. Data Structures

##### Place (`Place.cs`)
A fundamental struct representing a position in the text:
- `iChar`: Character index within a line (0-based)
- `iLine`: Line index (0-based)
- Supports comparison operators for ordering

##### Char (`Char.cs`)
Represents a single character with its style:
- `c`: The Unicode character
- `style`: StyleIndex bit mask indicating which styles apply

##### Line (`Line.cs`)
Represents a single line of text:
- Implements `IList<Char>` for character access
- Stores folding markers for code folding
- Tracks line state (changed, visited, etc.)

##### Range (`Range.cs`)
Represents a text selection or region:
- Defined by start and end `Place` positions
- Supports column selection mode
- Provides text manipulation methods

#### 3. Main Control (`FastColoredTextBox.cs`)

The main `FastColoredTextBox` class is a `UserControl` that:
- Inherits from `UserControl` and implements `ISupportInitialize`
- Orchestrates all text editing functionality
- Manages rendering, scrolling, and user input
- Contains ~8600 lines of code (the largest component)

**Major Responsibilities:**
- **Rendering**: Paints text, line numbers, bookmarks, folding indicators
- **Editing**: Handles keyboard/mouse input, text insertion/deletion
- **Selection**: Manages text selection and clipboard operations
- **Scrolling**: Custom scrollbar implementation
- **Caret**: Blinking cursor management
- **Events**: Fires events for text changes, selection changes, etc.

#### 4. Styling System (`Style.cs`)

The styling system uses an inheritance-based approach:
- `Style` (abstract base): Defines the rendering interface
- `TextStyle`: Renders colored text with font variations
- `SelectionStyle`: Renders selection backgrounds
- Other specialized styles for different visual effects

**Style Application:**
- Each character has a `StyleIndex` bit mask (supports 16 or 32 styles)
- Multiple styles can be applied to a single character
- Styles are applied via `AddStyle()` on ranges

#### 5. Syntax Highlighting (`SyntaxHighlighter.cs`)

Provides language-specific syntax highlighting:
- Uses regular expressions to identify language tokens
- Supports multiple built-in languages (C#, VB, HTML, SQL, etc.)
- Extensible via XML syntax descriptions (`SyntaxDescriptor.cs`)

**Process:**
1. Text changes trigger highlighting
2. Regular expressions match language patterns
3. Matched ranges get appropriate styles applied
4. Highlighting can be incremental for performance

#### 6. Command System (`CommandManager.cs`, `Commands.cs`)

Implements the **Command Pattern** for undo/redo:
- Each editing operation is a command object
- Commands stored in undo/redo stacks
- Examples: `InsertTextCommand`, `RemoveTextCommand`, `ReplaceTextCommand`

**Benefits:**
- Multi-level undo/redo
- Macro recording capability
- Atomic operations

#### 7. Autocomplete (`AutocompleteMenu.cs`, `AutocompleteItem.cs`)

Intelligent code completion system:
- `AutocompleteMenu`: Dropdown list of suggestions
- `AutocompleteItem`: Base class for completion items
- Supports custom completion logic and snippets

#### 8. Additional Features

##### Bookmarks (`Bookmarks.cs`)
- Mark important lines
- Navigate between bookmarks

##### Find/Replace (`FindForm.cs`, `ReplaceForm.cs`)
- Search with regular expressions
- Replace with undo support

##### Code Folding
- Collapse/expand code blocks
- Uses `FoldingStartMarker` and `FoldingEndMarker` in lines
- Tracks folding pairs in `foldingPairs` dictionary

##### Document Map (`DocumentMap.cs`)
- Miniature overview of entire document
- Quick navigation

##### Ruler (`Ruler.cs`)
- Visual ruler for measuring text width

##### Hints (`Hints.cs`)
- Tooltip-style hints
- Context-sensitive help

## Data Flow

### Text Modification Flow

```
User Input → Event Handler → Create Command → Execute Command 
          → Update TextSource → Fire TextChanged Event 
          → Invalidate Visual → Repaint
```

### Rendering Flow

```
Paint Event → Calculate Visible Range → For Each Line:
           → Get Line Text → Apply Styles → Draw to Graphics
```

### Syntax Highlighting Flow

```
Text Changed → SyntaxHighlighter.HighlightSyntax()
            → Apply RegEx Patterns → Identify Tokens
            → Apply Styles to Ranges → Store in TextSource
```

## Performance Optimizations

1. **Lazy Rendering**: Only visible lines are rendered
2. **Incremental Highlighting**: Only changed regions are re-highlighted
3. **Bit Masks for Styles**: Efficient style storage (16/32 styles max)
4. **Line-Based Storage**: Efficient line operations
5. **Cached Measurements**: Character width/height cached
6. **Double Buffering**: Smooth rendering without flicker

## Extension Points

### Adding a New Language

1. Create syntax descriptor (XML or code)
2. Define regular expressions for language tokens
3. Create styles for different token types
4. Register with `SyntaxHighlighter`

Example:
```csharp
var highlighter = new SyntaxHighlighter(textBox);
highlighter.CurrentLanguage = Language.CSharp;
```

### Custom Styles

1. Inherit from `Style` class
2. Override `Draw()` method
3. Add style to `TextSource.Styles[]`
4. Apply to ranges using `AddStyle()`

### Custom Commands

1. Inherit from `UndoableCommand`
2. Implement `Execute()` and `Undo()`
3. Execute via `textBox.TextSource.Manager.ExecuteCommand()`

## Dependencies

- .NET Framework 2.0+ (original version)
- System.Windows.Forms (UI)
- System.Drawing (rendering)
- No external libraries required

## Threading Model

- **Single-threaded**: All operations on UI thread
- Background syntax highlighting possible but not default
- Timer-based delayed operations (e.g., delayed syntax highlighting)

## Memory Management

- Implements `IDisposable` for proper cleanup
- Styles should be disposed when no longer needed
- TextSource manages line lifecycle
- Careful with event handlers to avoid memory leaks

## Testing

The `Tester` project contains numerous samples demonstrating:
- Basic syntax highlighting
- Custom styles
- Autocomplete
- Code folding
- Bookmarks
- Find/Replace
- Custom text sources
- And many more features

## Common Patterns

### Adding Text Programmatically
```csharp
textBox.Text = "New text";
// or for better performance:
textBox.AppendText("More text");
```

### Applying Styles
```csharp
Range range = textBox.GetRange(0, 10); // First 10 chars
range.SetStyle(StyleIndex.Style1);
```

### Finding Text
```csharp
Range found = textBox.GetRange().FindNext("searchText", true);
if (found != null)
    found.Inverse(); // Highlight
```

### Custom Syntax Highlighting
```csharp
private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
{
    e.ChangedRange.ClearStyle(StyleIndex.All);
    e.ChangedRange.SetStyle(myStyle, @"\bkeyword\b");
}
```

## File Organization

```
FastColoredTextBox/
├── FastColoredTextBox.cs      # Main control (8600+ lines)
├── TextSource.cs              # Text storage
├── Line.cs                    # Line representation
├── Range.cs                   # Text range/selection
├── Place.cs                   # Position struct
├── Char.cs                    # Character + style
├── Style.cs                   # Style base class
├── SyntaxHighlighter.cs       # Language highlighting
├── CommandManager.cs          # Undo/redo
├── Commands.cs                # Command implementations
├── AutocompleteMenu.cs        # Code completion
├── Bookmarks.cs               # Bookmark management
├── FindForm.cs                # Find dialog
├── ReplaceForm.cs             # Replace dialog
├── DocumentMap.cs             # Document overview
├── Ruler.cs                   # Visual ruler
├── Hints.cs                   # Tooltips/hints
└── Export*.cs                 # HTML/RTF export

Tester/                        # Sample applications
Help/                          # Documentation
```

## Summary

FastColoredTextBox is a well-architected text editor component with:
- Clear separation of concerns
- Efficient data structures
- Extensible design
- Good performance characteristics
- Rich feature set

The codebase prioritizes performance (especially for large files) while maintaining flexibility for customization and extension.
