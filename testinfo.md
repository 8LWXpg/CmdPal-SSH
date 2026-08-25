# Testing Instructions

## Prerequisites

### Required Software
1. **PowerToys with Command Palette**
   - Install from Microsoft Store: https://apps.microsoft.com/detail/xp89dcgq3k6vld
   - Includes Microsoft Edge WebView2 Runtime
   - Available for both Windows 11 and Windows 10
2. **Windows Terminal**
   - Preinstalled on most Windows 11 systems; otherwise install from the
     Microsoft Store
   - This is the terminal the extension launches by default

## Installation Steps

### Step 1: Install PowerToys
1. Open Microsoft Store
2. Search for "PowerToys" or use the direct link above
3. Click "Get" or "Install"
4. Wait for installation to complete

### Step 2: Install Extension
1. Download the MSIX package
2. Right-click the MSIX file
3. Select "Install" from the context menu
4. Follow the installation prompts
5. The extension will automatically register with Command Palette

### Step 3: Activate Command Palette
1. Open PowerToys
2. Navigate to Command Palette tool
3. Enable Command Palette if not already active
4. Set your preferred hotkey (default: Win + Alt + Space)

## Testing the Extension

### Setup: SSH Config
The extension reads host entries from `%USERPROFILE%\.ssh\config`. If this
file doesn't already exist, create it with a minimal entry (no real
credentials needed — the entry just needs to exist for the list to
populate):

```
Host local
    HostName 127.0.0.1
    User user
```

### Basic Functionality Test
1. Press your Command Palette hotkey
2. In the search box, type "Reload"
3. Select "Reload, Reload Command Palette extensions" and press Enter
4. Type "SSH" in the search box and select the **SSH** top-level command
5. Confirm the list shows the host(s) from `~/.ssh/config` (e.g. `local`),
   with subtitle `user@127.0.0.1` and a details pane showing that host's
   config properties
6. Select the host and invoke **Open in Terminal**

### Expected Behavior

A terminal window (Windows Terminal by default) opens and runs `ssh local`
(or the selected host). The SSH connection itself does not need to succeed —
since `127.0.0.1` typically has no SSH server listening, the command
failing/timing out inside the terminal is expected and fine. What's being
validated is that the extension launches the terminal with the correct
command, not that the connection completes.

## Troubleshooting

### Extension Not Appearing
- Restart PowerToys
- Reload Command Palette extensions (Step 3 in testing)
- Verify the MSIX package installed successfully in Windows Settings > Apps
