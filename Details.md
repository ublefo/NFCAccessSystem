## Hardware

### NFC tags

Readily available and cheap MIFARE Classic cards, which has a 4-byte UID is stored in the first sector. The security features on MIFARE Classic cards has long been defeated, we are only using the UID as a public identifier.

### Server hardware

- RaspberryPi 4B
- PN532 NFC module attached over I2C

### Client hardware

- RaspberryPi 3B+
- Custom HAT with GPIO LEDs, a buzzer and a PN532 NFC module attached over I2C
- Servo motor to simulate a door lock
- USB numpad for PIN entry

### Smartphone

- Any device with a compatible authenticator app (such as Google Authenticator) installed.

## Software

### OS

- Custom Linux image built with [Yocto](https://www.yoctoproject.org/)
- Contains only the bare minimum required by the applications to reduce attack surface

### Server software stack

- Two modes of operation
	- Client mode (db read-only, no web UI, only offline users are allowed)
	- Server mode (db read-write, web management UI, all authorised users allowed)

### Client software stack

- Credentials submitted over HTTP Basic authentication
- Connection to remote server is secured with HTTPS
