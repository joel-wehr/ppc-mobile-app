# Pegasus Flight - Powered Parachute Assistant

A professional .NET MAUI app designed for Powrachute Pegasus 582 pilots to manage checklists and flight logs on iOS devices.

## Features

### ✅ Implemented (v1.0)

#### Interactive Checklists
- **6 Complete Checklists** from the Pegasus POH:
  - Preflight (Detailed) - 60+ inspection items
  - Preflight (Abbreviated) - Quick reference checklist
  - Warm Up - Engine warm-up procedure
  - Pre-Start & Takeoff - Pre-flight startup checklist
  - Wing Layout - Wing deployment and setup
  - Post Flight - Shutdown and securing procedures

#### Optimized for Field Use (iPhone 15 Pro - 1179x2556, 6.1")
- **Extra-large touch targets** (80-110px height) - Easy to use with gloves
- **Bold, high-contrast design** - Readable in bright sunlight
- **50px checkboxes with Navy Blue borders** - Large, visible, easy-to-tap while walking around aircraft
- **Increased font sizes** (18-22px for content) - Optimized for outdoor readability
- **Progress tracking** - See completion status at a glance with large progress bar
- **One-tap reset** - 60px tall button for easy access
- **Material Icons** - Professional iconography throughout

#### Professional UI
- **Navy Blue & White Theme** (#003366) - Professional aviation-inspired design
- **Warm Orange Accents** (#FF9933) - Clear CTAs and action buttons
- **Success Green** (#28A745) - Clear visual feedback for completed items
- **Uranium UI Material Design** - Modern, polished interface with Material Icons
- **Clean navigation** - Simple, intuitive flow with large touch targets
- **Grouped sections** - Checklists organized by aircraft area
- **Visual feedback** - Green borders, strikethrough, and color changes for completed items
- **High contrast** - Optimized for outdoor visibility in bright sunlight on iPhone 15 Pro

### 🚧 Coming Soon (v1.1+)

- **Flight Logging** - Track date, time, duration, location
- **Weather Integration** - Automatic weather conditions
- **Flight History** - View all past flights
- **Statistics** - Total hours, flight count, etc.

## Technology Stack

- **.NET MAUI 9** - Cross-platform framework
- **Uranium UI 2.13** - Premium UI components
- **SQLite** - Local database for flight logs
- **MVVM Pattern** - Clean, maintainable architecture
- **CommunityToolkit.Mvvm** - Modern MVVM helpers

## Project Structure

```
powered-parachute/
├── Models/           # Data models (Flight, ChecklistItem)
├── ViewModels/       # MVVM view models
├── Views/            # XAML pages
├── Services/         # Business logic (Database, Checklists)
├── Converters/       # XAML value converters
└── Resources/        # Images, fonts, styles
```

## Building for iOS

### Prerequisites
- macOS with Xcode installed
- .NET 9 SDK
- iOS workload: `dotnet workload install ios`

### Build Commands
```bash
# Restore packages
dotnet restore

# Build for iOS
dotnet build -f net9.0-ios

# Run in iOS Simulator
dotnet build -f net9.0-ios -t:Run
```

## App Store Preparation

### App Information
- **Bundle ID**: `com.pegasus.flightlog`
- **Display Name**: Pegasus Flight
- **Version**: 1.0.0
- **Min iOS Version**: 15.0
- **Target Device**: iPhone (optimized for iPhone 15 Pro)

### TODO Before App Store Submission
- [ ] Create app icon (1024x1024)
- [ ] Update splash screen branding
- [ ] Add Privacy Policy (required for App Store)
- [ ] Configure Info.plist for required permissions
- [ ] Create App Store screenshots
- [ ] Write app description for App Store listing
- [ ] Test on physical iPhone device
- [ ] Set up Apple Developer account
- [ ] Create App Store Connect listing

## Checklists Source

All checklists are directly extracted from the **Powrachute Pegasus 582 Pilot Operating Handbook** (POH), sections 5-15. The app implements:

- Detailed preflight inspection procedures (pages 11-14)
- Abbreviated quick-check procedures (page 15)
- Engine warm-up steps (page 31)
- Pre-flight startup (page 32)
- Wing layout and deployment (page 33)
- Post-flight securing (page 34)

## Safety Notice

⚠️ **This app is a checklist assistant only. Always refer to the official Powrachute Pegasus POH for authoritative information. Proper flight training is essential - never fly without qualified instruction.**

## License

This app is built for personal use. Powrachute trademarks and POH content are property of Powrachute, LLC.

## Development

Built with ❤️ for pilots who want a clean, simple checklist app optimized for outdoor use.
