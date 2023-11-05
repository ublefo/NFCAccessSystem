# NFC Access System

> [!NOTE]
> I built this project in about two weeks back in 2022, starting with zero experience in ASP.NET Core and Entity Framework Core, so the code quality is probably not fantastic and only minimal comments are present. This is only meant to be a project showcase, it's not ready for production use by any means.


## Project overview

This is the software component for the final project I submitted for an introductory embedded systems unit at Deakin University. The goal of this project is to demonstrate my understanding of embedded systems concepts by implementing a basic NFC tag based door access system that offers better security than budget options available on the market.

The core idea of this project is to eliminate static passwords and offload secure secret storage to a smartphone, which the user will likely already have access to. This can be easily accomplished by using an NFC tag to provide a public user ID, and authenticate the user with a standard RFC 6238 TOTP code generated with an app. TOTP generator applications are widely available on all platforms, so it eliminates the need for developing a separate mobile app as well.

Although the security features of MIFARE Classic NFC tags have long been compromised, they're still perfectly adequate for providing an ID to be used as a public identifier. Since we are offloading secret storage to a smartphone or a similar device, more secure NFC tags such as the MIFARE Plus EV1 are not necessary, making this design much easier and economical to implement.


> [!NOTE]
> Additional details of the hardware setup and software architecture can be found in [Details.md](Details.md).

## Features

- Simple user management web interface
- Reading NFC tags with `libnfc` 
- Auto-fill for the UID field during the user enrolment process
- Reading PIN codes from USB keyboards
- Interfacing with LEDs and servo motors attached to the GPIO pins on a Raspberry Pi
- No static password, authenticate via NFC tag + single-use TOTP code
- Database synchronisation mechanism for fallback access
- Role based authorisation granting different access levels
- Fallback local authentication server to provide redundancy during network disruptions

## User interaction

During the user enrolment process, the user needs to scan the NFC tag on the server, and add the TOTP secret to their device of choice with the QR code or manual entry, then the secret is validated by verifying the current TOTP code. After the user is registered in the system, they will be able to tap the NFC tag on any of the client devices, input the current TOTP code on the pin pad, and then the door will be unlocked if everything checks out.


## Security considerations

All communication between the client and server applications are done over HTTPS which provides authenticity and confidentiality guarantees. The UID and TOTP are sent with HTTP Basic authentication, which is an easy approach that lets me utilise the proper authentication and authorisation features provided by ASP.NET Core thanks to [idunno.Authentication](https://github.com/blowdart/idunno.Authentication). 

Additionally, the project implements a primitive database synchronisation mechanism and runs a local authentication server on all client devices as well, which provides some levels of redundancy in case the network connection goes down. Database synchronisation is done with a dedicated API route, which is only accessible with the correct database synchronisation credentials. This fallback access is gated with role based authorisation, in order to prevent someone from disconnecting the client device from the network and delaying the rollout of access revocations.

Assuming the systems running the applications are properly secured (filesystem encryption, physical security, etc.), it is extremely difficult to perform MITM at any point of this system. Unlike a [Wiegand](https://en.wikipedia.org/wiki/Wiegand_interface) card reader where sniffing the unencrypted traffic on the wire will easily let you clone a working tag, sniffing the traffic from the NFC module and/or the USB keypad is completely useless. This is due to the UID stored on the NFC tags being a simple public ID that isn't useful on its own, and the TOTP code being single-use.


## Project layout

- `GPIOHelper`: simple GPIO helper class that abstracts hardware peripheral access.
- `InputHelper`: see next section.
- `NFCAccessSystem`: backend server that handles user management and authentication.
- `NFCAccessSystemClient`: client implementation that takes user input and sends them to the authentication server.
- `NFCHelper`: simple NFC Helper class to make interfacing with SharpNFC easier.
- `SharpNFC`: SharpNFC library with a new method added to read the UID of a tag.


## Libraries used in this project

- [Otp.NET](https://github.com/kspearrin/Otp.NET): An implementation TOTP RFC 6238 and HOTP RFC 4226 in C#.
- [idunno.Authentication](https://github.com/blowdart/idunno.Authentication): Source for HTTP Basic Auth implementation used in the project.
- [SharpNFC](https://github.com/episage/sharp-nfc): .NET wrapper for `libnfc`
- [InputHelper](https://www.grantbyrne.com/2021/10/28/how-to-create-global-keyboard-hook-with-c-in-linux/): Library for listening and parsing events from Linux input event devices.

## License

The MIT License applies to all the files in the `GPIOHelper`, `NFCAccessSystem` and `NFCAccessSystemClient` directories, which contains my own original work. The third party libraries have their own licenses and are not covered by the MIT license found in this repository.
