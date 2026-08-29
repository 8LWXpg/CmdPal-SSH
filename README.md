# SSH for CmdPal

Command Pallet extension for connecting to SSH clients that configured at `~/.ssh/config` file.

## Install

<a href="https://get.microsoft.com/installer/download/9PBWRSPJ5W92?referrer=appbadge" target="_self" >
	<img src="https://get.microsoft.com/images/en-us%20dark.svg" width="200"/>
</a>

### Install via winget

```sh
winget install 9PBWRSPJ5W92
```

## Usage

### Open SSH Connection in Terminal

![open a ssh connection](./assets/results.png)

## Install Dev Version

**Prerequisite**

- [winapp cli](https://github.com/microsoft/winappCli): `winget install Microsoft.WinAppCli`

**Steps**

1. Download both `SSH_<version>.msixbundle` and `cert.pfx`
2. `sudo winapp cert install cert.pfx` (only need to do this once)
3. Click to install or `Add-AppxPackage <msix>`

## Contributing

### Localization

If you want to help localize this plugin, please check the [localization guide](./Localizing.md)

