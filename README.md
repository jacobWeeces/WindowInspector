# Window Inspector 🔍

A lightweight Windows UI inspection tool that helps you analyze and debug UI elements in any application. Perfect for developers working on UI automation, accessibility testing, or just exploring Windows UI structures.

## Features
- Real-time UI element inspection as you move your cursor
- Detailed element information including:
  - Basic properties (Name, Class, AutomationID)
  - Element hierarchy
  - Keyboard shortcuts
  - Process information
  - Supported UI Automation patterns
- Copy element details to clipboard (Ctrl+C)
- Pause/Resume inspection (Space)
- Always-on-top window
- Minimal resource usage

## Requirements
- Windows 10 or later
- .NET 8.0 Runtime

## Installation
1. Download the latest release from the [Releases](https://github.com/YOUR_USERNAME/WindowInspector/releases) page
2. Extract the ZIP file to your preferred location
3. Run `WindowInspector.App.exe`

## Building from Source
1. Clone the repository
2. Ensure you have the [.NET 8.0 SDK](https://dotnet.microsoft.com/download) installed
3. Open a terminal in the project directory
4. Run `dotnet restore`
5. Run `dotnet build --configuration Release`

## Usage
1. Launch the application
2. Hover over any UI element to inspect it
3. Use Space to pause/resume tracking
4. Use Ctrl+C to copy the current element details
5. The window will stay on top of other applications for easy reference

## Contributing
Contributions are welcome! Feel free to submit issues and pull requests.

## License
MIT License - See LICENSE file for details