# Powered Parachute Flight Log App - Claude Context

## Quick Start - Read This First! 🚀

This is a .NET MAUI 10.0 powered parachute flight log app for iPhone 15 Pro. The app tracks checklists and flight sessions.

**Current Status:** App builds successfully. Upgraded to .NET 10 / VS 2026.

**Last Session Date:** February 9, 2026

---

## 📱 App Overview

A pilot-focused app for powered parachute flights with:
- 7 different pre-flight, in-flight, and post-flight checklists
- Automatic flight log tracking grouped by date
- In-Flight Practice mode with counters for repeated maneuvers
- Flight detail pages with weather, location, and notes

---

## 🏗️ Architecture

### Tech Stack
- **.NET MAUI 10.0** (Cross-platform: iOS, Android, Windows, MacCatalyst)
- **SQLite** database (`pegasus_flight.db3`)
- **MVVM** pattern with CommunityToolkit.Mvvm
- **Target Device:** iPhone 15 Pro (outdoor readability optimized)

### Database Schema
```
flights
  - Id (PK)
  - FlightDate
  - StartTime, EndTime
  - DurationMinutes (calculated)
  - Location, WeatherConditions, Notes

checklist_logs
  - Id (PK)
  - FlightId (FK, nullable)
  - ChecklistType (enum)
  - CompletedAt
  - TotalItems, CheckedItems
```

---

## 🎯 Key Features & Implementation

### 1. Checklist System
**Files:** `Services/ChecklistService.cs`, `ViewModels/ChecklistDetailViewModel.cs`, `Views/ChecklistDetailPage.xaml`

**Checklist Types:**
1. Preflight (Detailed) - 30+ items
2. Preflight (Abbreviated) - Quick version
3. Warm Up
4. Pre-Start & Takeoff
5. Wing Layout
6. In-Flight Practice - **Uses counters, not checkboxes**
7. Post Flight

**Important Details:**
- In-Flight Practice items have `HasCounter = true`
- ChecklistService.cs lines 361-403: Counter items defined with `AddCounterItems()`
- ChecklistItem.Clone() line 47-48: MUST preserve `HasCounter` property

### 2. In-Flight Practice Counter UI ⚠️ CRITICAL
**File:** `Views/ChecklistDetailPage.xaml` lines 57-138

**Layout Structure:**
```
Border (outer container)
  └─ Grid (wrapper - REQUIRED!)
      ├─ Grid (checkbox layout, IsVisible when HasCounter=false)
      │   ├─ Border (checkbox)
      │   └─ Label (description on right)
      │
      └─ VerticalStackLayout (counter layout, IsVisible when HasCounter=true)
          ├─ HorizontalStackLayout (top row, centered)
          │   ├─ Button "-" (orange/Secondary)
          │   ├─ Button "+" (green/Success)
          │   └─ Label (count number)
          └─ Label (description, centered below)
```

**Why the wrapper Grid is critical:**
- Border can only have ONE child in XAML
- Without wrapper Grid, checkboxes/text don't appear
- Both layouts (checkbox & counter) share same parent Grid
- Only one visible at a time via `IsVisible` bindings

### 3. Flight Log System
**Files:**
- `ViewModels/FlightLogViewModel.cs`
- `Services/DatabaseService.cs` lines 113-172
- `Views/FlightLogPage.xaml`
- `Views/FlightDetailPage.xaml` (new)

**Data Flow:**
1. User completes checklist → Creates `ChecklistLog`
2. Flight Log page opens → `LoadFlightsAsync()`
3. Groups logs by date → `GetFlightsWithLogsAsync()`
4. Auto-creates `Flight` records for each date
5. Links logs to flights via `FlightId`

**⚠️ CRITICAL BUG FIX:**
DatabaseService.cs line 138:
```csharp
// ❌ WRONG - crashes in SQLite:
var existingFlight = await _database.Table<Flight>()
    .Where(f => f.FlightDate.Date == dateGroup.Key)
    .FirstOrDefaultAsync();

// ✅ CORRECT - load into memory first:
var allFlights = await _database.Table<Flight>().ToListAsync();
var existingFlight = allFlights.FirstOrDefault(f => f.FlightDate.Date == flightDate);
```

**Flight Log UI:**
- List page: Shows date, time, checklist count, duration, location
- Tap flight → Navigate to detail page
- Detail page: Edit location, weather, notes; Delete flight

---

## 🐛 Recent Bug Fixes

### Bug #1: Checkboxes Not Appearing (Nov 2)
**Symptom:** After adding Flight Log, checklists showed empty boxes with no text
**Cause:** Border had TWO direct children (checkbox Grid + counter VerticalStackLayout) - invalid XAML
**Fix:** Wrapped both in parent Grid container (line 57)
**Commit:** Latest

### Bug #2: Flight Log Crash (Nov 2)
**Symptom:** App crashed when opening Flight Log
**Cause:** SQLite doesn't support `.Date` property in LINQ queries
**Fix:** Load all flights into memory, then use in-memory LINQ
**Commit:** ec5c0c6

---

## 📂 Key File Locations

### Models
- `Models/ChecklistItem.cs` - Item with `HasCounter` property, Clone() method
- `Models/ChecklistLog.cs` - Completed checklist record
- `Models/Flight.cs` - Flight session record
- `Models/ChecklistType.cs` - Enum of checklist types

### Services
- `Services/ChecklistService.cs`
  - Lines 361-403: In-Flight Practice counter items
  - Lines 399-402: `AddCounterItems()` method
- `Services/DatabaseService.cs`
  - Lines 113-172: `GetFlightsWithLogsAsync()` - CRITICAL for flight grouping
  - Lines 125-138: SQLite query fix location

### ViewModels
- `ViewModels/ChecklistDetailViewModel.cs` - Checklist completion logic
- `ViewModels/FlightLogViewModel.cs` - Flight list display
- `ViewModels/FlightDetailViewModel.cs` - Flight detail editing

### Views
- `Views/ChecklistDetailPage.xaml`
  - Lines 57-138: Counter UI layout - **MOST COMPLEX PART**
  - Line 57: Wrapper Grid - DO NOT REMOVE!
- `Views/FlightLogPage.xaml` - Flight list with navigation
- `Views/FlightDetailPage.xaml` - Flight detail page

### Configuration
- `MauiProgram.cs` - DI registration
- `AppShell.xaml.cs` - Route registration

---

## 🎨 Design Guidelines

### Color Scheme (COLOR_SCHEME.md)
- **Primary:** Blue (#512BD4)
- **Success:** Green (for completed items, + button)
- **Secondary:** Orange/Red (for - button)
- **Large touch targets** for outdoor use on iPhone 15 Pro

### UI Principles
- Font size 18+ for outdoor readability
- Bold text for important items
- Minimum button size 50x50 for easy tapping
- High contrast for sunlight visibility

---

## ⚠️ GOTCHAS - Read Before Coding!

### 1. Border Can Only Have One Child
If you modify ChecklistDetailPage.xaml:
- Keep the wrapper Grid at line 57
- Both checkbox and counter layouts MUST be inside it
- Don't try to make them direct children of Border

### 2. SQLite LINQ Limitations
- Can't use `.Date`, `.Year`, `.Month` in queries
- Load into memory with `.ToListAsync()` first
- Then use LINQ on in-memory collection

### 3. ChecklistItem.Clone()
- MUST copy `HasCounter` property (line 47-48)
- If missing, counter UI breaks (shows checkboxes instead)

### 4. In-Flight Practice Counter Setup
- Use `AddCounterItems()` in ChecklistService
- Sets `HasCounter = true` and `Count = 0`
- Don't use `AddItems()` for repeatable practice items

---

## 🚀 Git Repository

**Branch:** main
**Location:** C:\Users\joelw\Documents\GitHub\powered-parachute

**Recent Commits:**
```
f1585b3 - Add comprehensive project status documentation
6eba980 - Add all remaining files
ec5c0c6 - Fix Flight Log crash - SQLite Date property fix
0ae52fc - Add all project folders
6667185 - Initial commit with flight log tracking
```

---

## 🧪 Testing Checklist

When resuming work:
1. ✅ Build succeeds (should have 0 errors, warnings OK)
2. ✅ Checklists display correctly (checkboxes visible with text)
3. ✅ In-Flight Practice shows counter UI (- + Count on top, description below)
4. ✅ Flight Log opens without crashing
5. ⏳ iOS deployment to iPhone 15 (was having issues)
6. ⏳ Flight detail page navigation works
7. ⏳ Editing/saving flight details works

---

## 📝 Next Steps

**Current Blocker:** iOS deployment to iPhone 15
- User is restarting Visual Studio
- Check provisioning profiles
- Verify code signing
- Check device connection

**After Deployment Fixed:**
1. Test all checklist types on device
2. Complete a full flight session test
3. Verify flight log grouping
4. Test flight detail editing
5. Test counter increment/decrement

---

## 📚 Additional Resources

- **Full status:** See `PROJECT_STATUS.md` in root
- **Color scheme:** See `COLOR_SCHEME.md`
- **Reference docs:**
  - Powrachute-Pegasus-POH.pdf (manufacturer documentation)

---

## 🆘 Common Issues & Solutions

**Issue:** Checkboxes don't appear
→ Check ChecklistDetailPage.xaml line 57 - wrapper Grid must exist

**Issue:** Flight Log crashes
→ Check DatabaseService.cs line 138 - no `.Date` in SQLite query

**Issue:** Counter items show checkboxes
→ Check ChecklistItem.Clone() preserves `HasCounter`
→ Check ChecklistService uses `AddCounterItems()` for In-Flight Practice

**Issue:** Build errors
→ Run `dotnet build` to see full output
→ Check all ViewModels and Pages registered in MauiProgram.cs
→ Check all routes registered in AppShell.xaml.cs

---

**Last Updated:** November 2, 2025
**Status:** Ready for iOS deployment testing after VS restart
