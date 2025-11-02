# Powered Parachute Flight Log App - Project Status

**Last Updated:** November 2, 2025
**Platform:** .NET MAUI 9.0 (iOS, Android, Windows, MacCatalyst)
**Target Device:** iPhone 15 Pro

## Current Status

### ✅ Completed Features

#### 1. **Checklist System**
- **Location:** `Services/ChecklistService.cs`, `ViewModels/ChecklistDetailViewModel.cs`
- Pre-flight checklists (Detailed & Abbreviated)
- Warm-up checklist
- Pre-start & Takeoff checklist
- Wing Layout checklist
- In-Flight Practice checklist (with counter UI)
- Post-flight checklist
- All checklists with completion tracking

#### 2. **In-Flight Practice Counter UI**
- **Location:** `Views/ChecklistDetailPage.xaml` (lines 57-138)
- **Design:** Two-row layout for practice items
  - **Top row:** `[-] [+] [Count]` buttons horizontally centered
  - **Bottom row:** Description text centered below controls
- Fixed bug where checkboxes weren't appearing (wrapped both layouts in parent Grid)
- Counter items use `HasCounter=true` property
- Regular checklists use checkbox on left, description on right

#### 3. **Flight Log System**
- **Location:** `ViewModels/FlightLogViewModel.cs`, `Services/DatabaseService.cs`
- Automatically groups checklist logs by date into Flight records
- **Flight Log List Page** shows:
  - Date display (e.g., "Wed, Nov 2, 2025")
  - Start time
  - Number of checklists completed
  - Flight duration (calculated from start to end times)
  - Location (if entered)
- **Flight Detail Page** (`Views/FlightDetailPage.xaml`):
  - Full date and time range display
  - Duration breakdown
  - List of all checklists completed with timestamps and completion percentages
  - Editable fields: Location, Weather Conditions, Notes
  - Save/Delete buttons

#### 4. **Database Layer**
- **Location:** `Services/DatabaseService.cs`
- SQLite database: `pegasus_flight.db3`
- Tables: `flights`, `checklist_logs`
- Models: `Flight.cs`, `ChecklistLog.cs`, `ChecklistItem.cs`
- **Key Fix:** Line 138 - Changed from SQLite LINQ query using `.Date` property to in-memory comparison to avoid crashes

#### 5. **Navigation & Routing**
- **Registered routes in `AppShell.xaml.cs`:**
  - HomePage
  - ChecklistsPage
  - ChecklistDetailPage
  - FlightLogPage
  - FlightDetailPage (new)

#### 6. **Dependency Injection**
- **Location:** `MauiProgram.cs`
- All ViewModels and Pages registered
- Services: DatabaseService, ChecklistService

## Recent Fixes

### Fix #1: ChecklistDetailPage Layout (Nov 2)
**Problem:** After adding Flight Log system, checklists showed no text or checkboxes
**Cause:** Border element had two direct children (checkbox Grid and counter VerticalStackLayout) which is invalid XAML
**Solution:** Wrapped both in parent Grid container with `IsVisible` bindings
**File:** `Views/ChecklistDetailPage.xaml` line 57
**Commit:** Latest commit

### Fix #2: Flight Log Crash (Nov 2)
**Problem:** App crashed when opening Flight Log page
**Cause:** SQLite doesn't support `.Date` property in LINQ queries (`f.FlightDate.Date == dateGroup.Key`)
**Solution:** Load all flights into memory first, then use in-memory LINQ with `.Date`
**File:** `Services/DatabaseService.cs` lines 125-138
**Commit:** `ec5c0c6`

## Known Issues

### Current Issue: iOS Deployment
**Status:** Being investigated
**Symptom:** Deployment errors to iPhone 15
**Build Status:** Build succeeds with only warnings (no errors)
**Next Steps:** User restarting Visual Studio, may need to check:
- Provisioning profiles
- Code signing
- Device connection
- iOS deployment logs

## Architecture

### Data Flow
1. User completes checklist → `ChecklistDetailViewModel.CompleteChecklist()`
2. Creates `ChecklistLog` record → `DatabaseService.SaveChecklistLogAsync()`
3. Flight Log page loads → `FlightLogViewModel.LoadFlightsAsync()`
4. Groups logs by date → `DatabaseService.GetFlightsWithLogsAsync()`
5. Creates/retrieves `Flight` records automatically
6. Displays flight summaries with navigation to detail page

### Key Files
- **Checklists:** `Services/ChecklistService.cs` (lines 361-403 for In-Flight Practice)
- **Flight Tracking:** `Services/DatabaseService.cs` (lines 113-172)
- **UI - Checklist:** `Views/ChecklistDetailPage.xaml` (counter layout lines 57-138)
- **UI - Flight Log:** `Views/FlightLogPage.xaml` (flight list with tap navigation)
- **UI - Flight Detail:** `Views/FlightDetailPage.xaml` (detail view with edit/save/delete)

## Counter Items Configuration

In-Flight Practice checklist items use counters instead of checkboxes:
- **Implementation:** `ChecklistService.cs` line 399-402
- **Method:** `AddCounterItems()` sets `HasCounter = true`
- **Clone Fix:** `ChecklistItem.Clone()` now preserves `HasCounter` property (line 47-48)

## Git Repository

**Branch:** main
**Latest Commits:**
1. `6eba980` - Add all remaining files and update git repo before restart
2. `ec5c0c6` - Fix Flight Log crash - avoid SQLite Date property in query
3. `0ae52fc` - Add all project folders and files
4. `6667185` - Initial commit with flight log tracking system

## Next Steps

1. Resolve iOS deployment issue (restart Visual Studio)
2. Test Flight Log functionality on device
3. Test In-Flight Practice counter UI on device
4. Verify all checklist types work correctly
5. Test flight detail page editing and saving

## Color Scheme

Documented in `COLOR_SCHEME.md`:
- Primary: Blue (#512BD4)
- Success: Green
- Secondary: Orange/Red
- Large touch targets optimized for outdoor use on iPhone 15 Pro

## Build Status

✅ **Build:** Succeeds (warnings only, no errors)
❓ **iOS Deployment:** Under investigation
✅ **Database:** Tables created, migrations not needed
✅ **Navigation:** All routes registered
✅ **DI:** All services and pages registered
