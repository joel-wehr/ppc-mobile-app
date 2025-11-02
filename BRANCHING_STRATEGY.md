# Branching Strategy

## Overview
This project uses a feature-branch workflow to maintain code quality and enable easy rollbacks.

## Branch Types

### `main` Branch
- **Purpose:** Stable, production-ready code
- **Protection:** Only merge tested, working features
- **Commits:** Performance improvements, bug fixes, completed features
- **Deployment:** This is what gets deployed to devices

### Feature Branches
- **Naming:** `feature/short-description`
- **Examples:**
  - `feature/add-weather-api`
  - `feature/gps-location-tracking`
  - `feature/flight-statistics`
- **Lifecycle:** Create → Develop → Test → Merge to main → Delete
- **Purpose:** Isolate new feature development

### Bug Fix Branches
- **Naming:** `fix/short-description`
- **Examples:**
  - `fix/checklist-counter-reset`
  - `fix/flight-date-timezone`
  - `fix/database-crash`
- **Lifecycle:** Create → Fix → Test → Merge to main → Delete
- **Purpose:** Isolate bug fixes from features

### Experimental Branches
- **Naming:** `experiment/short-description`
- **Examples:**
  - `experiment/offline-maps`
  - `experiment/voice-commands`
- **Lifecycle:** Create → Explore → Keep or Discard
- **Purpose:** Try risky or exploratory ideas without affecting main

## Workflow

### Creating a New Feature
```bash
# Start from main
git checkout main
git pull

# Create feature branch
git checkout -b feature/my-new-feature

# Work on the feature
# ... make changes ...

# Commit frequently
git add .
git commit -m "Add initial structure for my feature"

# When done, merge to main
git checkout main
git merge feature/my-new-feature

# Delete the feature branch
git branch -d feature/my-new-feature
```

### Quick Rollback
```bash
# If something breaks after a merge
git log --oneline  # Find the commit before the problem
git reset --hard <commit-hash>

# Or revert a specific commit
git revert <commit-hash>
```

### View All Branches
```bash
git branch -a
```

### Switch Between Branches
```bash
git checkout <branch-name>
```

## Commit Message Guidelines

### Format
```
Brief summary (50 chars or less)

Detailed description:
- What changed
- Why it changed
- Any breaking changes

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

### Examples of Good Commit Messages
```
Add GPS location tracking to flight logs

- Integrate MAUI Geolocation API
- Auto-populate location field on checklist completion
- Add permission handling for iOS and Android
- Store lat/long coordinates in Flight model

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

## Best Practices

1. **Always create a branch for new work**
   - Never commit directly to main for experimental features
   - Use main only for proven, tested code

2. **Keep branches short-lived**
   - Merge within 1-2 days when possible
   - Long-lived branches cause merge conflicts

3. **Test before merging to main**
   - Build succeeds
   - App runs on device
   - Feature works as expected
   - No regressions in existing features

4. **Delete merged branches**
   - Keeps branch list clean
   - Prevents confusion about what's active

5. **Use descriptive branch names**
   - ✅ `feature/export-to-pdf`
   - ❌ `new-stuff`

6. **Commit often, push when stable**
   - Small commits are easier to review
   - Push to remote when feature is working

## Emergency Rollback

If the app is broken after a merge:

```bash
# See recent commits
git log --oneline -10

# Roll back to last known good commit
git reset --hard <commit-hash>

# Or create a new commit that undoes changes
git revert <bad-commit-hash>
```

## Current Branch Strategy

Going forward, we will:
- Keep `main` stable and deployable
- Create feature branches for all new work
- Test thoroughly before merging
- Delete branches after merge
- This allows easy rollback if issues arise

## Tools

```bash
# See what branch you're on
git branch

# See recent commits with graph
git log --oneline --graph --all --decorate

# See uncommitted changes
git status

# Compare branches
git diff main..feature/my-branch
```

---

**Last Updated:** November 2, 2025
