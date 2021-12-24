# Hash Pad

Hash Pad is a simple tool to compute and compare hash value including from Explorer's context menu. It is designed to complete this task in the minimum steps.

SHA1, SHA256, SHA384, SHA512 and MD5 are supported.

![Screenshot](Images/Screenshot_main_win10.png)<br>
(DPI: 175%)

Additonal languages:

 + German (de-DE) by @DocBrown101
 + Japanese (ja-JP) by @emoacht
 + Simplified Chinese (zh-Hans) by @nkh0472
 + Traditional Chinese (zh-Hant) by @nkh0472

## Requirements

 * Windows 10, 11
 * .NET 6.0

## Download

<a href='//www.microsoft.com/store/apps/9nrdj8214gbt?cid=storebadge&ocid=badge'><img src='https://developer.microsoft.com/store/badges/images/English_get-it-from-MS.png' alt='Hash Pad' width='142px' height='52px'/></a>

## Usage

 - To send a file to be compared from `Send to` of Explorer's context menu, open the menu by tapping hamburger button and check `Add to 'Send to' of Explorer's context menu`. Or drag and drop a file to the window. Or  select a file from file dialog.
 - Paste a string of expected value in `Expected Value`. Or read it directly from clipboard.
 - If the computed hash value matches the expected value, `Match` message will appear.

## Uninstall

Before uninstalling, if you checked `Add to 'Send to' of Explorer's context menu`, uncheck it to remove the shortcut in `Send to` folder. It can be done by `Remove from 'Send to'` in the jump list as well.

![Jump list](Images/JumpList.png)<br>

If you choose to remove it manually, type `shell:SendTo` in  Explorer's address bar to open that folder.

## History

Ver 2.10 2021-12-24

- Migrate to .NET 6.0

Ver 2.9 2021-9-19

- Modify UI

Ver 2.7 2021-1-31

- Change locales of Simplified Chinese (zh-Hans), Traditional Chinese (zh-Hant) languages

Ver 2.6 2021-1-14

- Enable to add to or remove from 'Send to' from JumpList

Ver 2.5 2020-12-31

- Add German (de-DE) language. Thanks to @DocBrown101!

Ver 2.4 2020-12-17

- Migrate to .NET 5.0

Ver 2.3 2020-12-10

- Fix bugs

Ver 2.2 2020-12-9

- Adjust location when called from Explorer's context menu

Ver 2.1 2020-11-27

- Add Japanese (ja-JP), Simplified Chinese (zh-CN), Traditional Chinese (zh-TW) languages. Thanks to @nkh0472!
- Change to remember folder path
- Change to remove spaces from expected value

Ver 2.0 2020-11-20

- Redesign and improve usability

## License

 - MIT License
