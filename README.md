# NFC Access System

## Project archtecture
- NFCAccessSystem: backend, including web based CRUD and authentication.
- NFCAccessSystemClient: client that takes user input and sends them to the authentication server.
- NFCHelper: simple NFC Helper class to call SharpNFC.
- SharpNFC: modified SharpNFC library, with a new function added to read the UID of a tag.

## Third-party repos used
- [Otp.NET](https://github.com/kspearrin/Otp.NET): An implementation TOTP RFC 6238 and HOTP RFC 4226 in C#.
- [idunno.Authentication](https://github.com/blowdart/idunno.Authentication): Source for HTTP Basic Auth implementation used in the project.
- [SharpNFC](https://github.com/episage/sharp-nfc): .NET wrapper for `libnfc`
- [InputHelper](https://www.grantbyrne.com/2021/10/28/how-to-create-global-keyboard-hook-with-c-in-linux/): Library for listening and parsing events from Linux input event devices.
