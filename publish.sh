#!/bin/bash

dotnet publish -r linux-arm64 --configuration=Release -f "net6.0" --self-contained

cp -r NFCAccessSystem/bin/Release/net6.0/linux-arm64/publish/* /opt/yocto/sources/meta-nfcaccesssystem/recipes-acs/acs-serverapp/files/
cp -r NFCAccessSystemClient/bin/Release/net6.0/linux-arm64/publish/* /opt/yocto/sources/meta-nfcaccesssystem/recipes-acs/acs-clientapp/files/
