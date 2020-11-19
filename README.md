# Hash Pad

Hash Pad is a simple tool to compute and compare hash value including from Explorer's context menu. It is designed to complete this task in short steps. It supports: SHA1, SHA256, SHA384, SHA512 and MD5.

![Screenshot](Images/Screenshot_main.png)<br>
(DPI: 175%)

## Requirements

 * Windows 10
 * .NET Framework 4.8

## Download

:floppy_disk: [Download](https://github.com/emoacht/HashPad/releases/latest)

## Usage

 - To send a file to be compared from `Send to` of Explorer's context menu, open the menu by tapping hamburger icon and check `Add to 'Send to' of Explorer's context menu`. Or drag and drop a file to the window. Or select a file from file dialog.
 - Paste a string of expected value in `Expected Value`. Or read it directly from clipboard.
 - If the hash value computed matches the expected value, `Match` messages will appear.

## Remarks

 - Before uninstalling, if you checked `Add to 'Send to' of Explorer's context menu`, uncheck it to remove the the shortcut in `Send to` folder. If you choose to remove it manually, type `shell:SendTo` in address bar of Explorer to open the folder.

## License

 - MIT License
