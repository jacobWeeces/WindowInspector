# Window Inspector Project Roadmap

## Initial Setup
- [x] Install [Visual Studio 2022 Community](https://visualstudio.microsoft.com/) with .NET Desktop workload
- [x] Install [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- [x] Create new WPF project in Visual Studio (`File > New > Project > WPF App`)

## Phase 1: Basic Element Detection
**Core Foundation**
- [x] T1: Create blank window with status text block
- [x] T2: Implement cursor position tracking
  ```csharp
  // In Win32Helpers.cs
  public static Point GetCursorPosition()
  ```
- [x] T3: Console output test (display mouse coordinates in real time)
- [x] T4: Basic element detection under cursor
  ```csharp
  // In ElementTracker.cs
  public static AutomationElement GetElementAt(Point screenPoint)
  ```
- [x] T5: Null element handling (show "Desktop" when no element found)

## Phase 2: Display System
**UI Components**
- [x] T6: Add hierarchy TextBox with vertical scroll
- [x] T7: Create "Copy to Clipboard" button stub
- [ ] T8: Implement pause toggle button UI

**Data Formatting**
- [ ] T9: Build element details string
  ```csharp
  
  // Include: ControlType, Name, AutomationId, Bounds
  ```
- [ ] T10: Parent hierarchy collection (child → root order)

## Phase 3: Real-Time Tracking
- [ ] T11: Implement HoverWatcher service
  ```csharp
  // Use System.Timers.Timer with 100ms interval
  ```
- [ ] T12: Connect watcher to UI updates
- [ ] T13: Add error handling for stale elements
- [ ] T14: Implement pause functionality

## Phase 4: Polish & Safety
- [ ] T15: Secure Win32 API calls
  ```csharp
  // Proper DllImport declarations in Win32Helpers.cs
  ```
- [ ] T16: Add exception logging
  ```csharp
  // Implement Logger.cs with file writing
  ```
- [ ] T17: Create app manifest for admin privileges

## Testing Checklist
- [ ] Basic detection: Hover over Notepad text area
- [ ] Hierarchy order: Verify child-first display
- [ ] Copy function: Test pasting into Notepad
- [ ] Memory check: Monitor for leaks during long sessions
- [ ] Edge case: Hover over taskbar clock

## Future Phase Snippets

Phase 5: User Experience
[ ] System tray icon with quick menu
[ ] Configurable refresh rate slider
[ ] Magnifier overlay (optional)
Phase 6: Deployment
[ ] Create installer using WiX Toolset
[ ] Add automatic update checker
[ ] Code signing certificate setup

## Current Progress Tracker
Completed Tasks: 0/17