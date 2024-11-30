#!/usr/bin/env bash

publishDir="${1}"
arch="${2}"
deb_arch="${arch}"
SCRIPT_DIR=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" &>/dev/null && pwd)
version="$(<${SCRIPT_DIR}/../version)"

echo Cleanup
rm -R ${SCRIPT_DIR}/${arch}/

if [ "$arch" = "x64" ]; then
    deb_arch="amd64"
elif [ "$arch" = "arm64" ]; then
    deb_arch="aarch64"
fi

echo Write Debian control
mkdir -p ${SCRIPT_DIR}/${arch}/DEBIAN
cat >${SCRIPT_DIR}/${arch}/DEBIAN/control <<EOL
Package: mindbank
Version: ${version}
Section: utils
Priority: optional
Architecture: ${deb_arch}
Depends: libx11-6, libice6, libsm6, libfontconfig1, ca-certificates, tzdata, libc6, libgcc1 | libgcc-s1, libgssapi-krb5-2, libstdc++6, zlib1g, libssl1.0.0 | libssl1.0.2 | libssl1.1 | libssl3, libicu | libicu74 | libicu72 | libicu71 | libicu70 | libicu69 | libicu68 | libicu67 | libicu66 | libicu65 | libicu63 | libicu60 | libicu57 | libicu55 | libicu52
Maintainer: haltroy <thehaltroy@gmail.com>
Homepage: https://github.com/haltroy/FluxionViewer
Description: Advanced note taking application.
Copyright: 2024 haltroy <thehaltroy@gmail.com>
EOL

echo Copy shortcut script
mkdir -p ${SCRIPT_DIR}/${arch}/usr/bin
cp ${SCRIPT_DIR}/shortcut ${SCRIPT_DIR}/${arch}/usr/bin/mindbank
chmod +x ${SCRIPT_DIR}/${arch}/usr/bin/mindbank

echo Copy desktop entry
mkdir -p ${SCRIPT_DIR}/${arch}/usr/share/applications
cp ${SCRIPT_DIR}/desktop.desktop ${SCRIPT_DIR}/${arch}/usr/share/applications/mindbank.desktop

echo Copy icon
mkdir -p ${SCRIPT_DIR}/${arch}/usr/share/icons/hicolor/2048x2048/apps
cp ${SCRIPT_DIR}/../src/FluxionViewer/Assets/logo.png ${SCRIPT_DIR}/${arch}/usr/share/icons/hicolor/2048x2048/apps/mindbank.png

echo Copy executable
mkdir -p ${SCRIPT_DIR}/${arch}/usr/lib/mindbank
cp -r ${publishDir}/. ${SCRIPT_DIR}/${arch}/usr/lib/mindbank/

echo Make the package
dpkg-deb --root-owner-group --build ${SCRIPT_DIR}/${arch}/ ${SCRIPT_DIR}/mindbank.${version}.${deb_arch}.deb

echo Cleanup
rm -R ${SCRIPT_DIR}/${arch}/
