[![AGPL License](http://img.shields.io/badge/license-AGPL%20v3-red.svg?style=flat-square)](http://opensource.org/licenses/AGPL-3.0) [![Build Status](https://img.shields.io/jenkins/s/http/build.polaris-server.net/PolarisServer.svg?style=flat-square)](http://build.polaris-server.net)

## Table of Contents

* [What is it?](#what-is-it)
* [Installation](#installation)
  * [Building The Latest Version](#building-the-latest-version)
  * [Downloading The Latest Version](#downloading-the-latest-version)
* [Documentation](#documentation)
* [Licensing](#licensing)
  * [Third Party Licenses](#third-party-licenses)
* [Contributing and Feedback](#contributing-and-feedback)
  * [Core Maintainers](#core-maintainers)

## What is it?
`Polaris Private Server` is an open source game private server for the Japanese MMORPG Phantasy Star Online 2. It is currently work-in-progress alpha, under heavy development and is not yet recommended for production use.

## Installation
As `Polaris Private Server` is a work-in-progress there is no set way of installaing and running, in future releases we hope to have this information stored in an INSTALL file. Until then you can either build or download and run the latest version.

### Building The Latest Version
For the time being, we only support using mono in order to build Polaris.
You need to have MonoDevelop 4 / Xamarin studio installed. You also need the Mono runtime, even on Windows!
After installing MD / Xamarin and setting up the Mono runtime, open the solution and build!
In the future we will hopefully have a buildserver, xbuild / msbuild support and other fancy things.

### Downloading The Latest Version
As the `Polaris Private Server` is a work-in-progress alpha, you can find the latest unstable built version here @ [build.polaris-server.net](http://build.polaris-server.net/job/PolarisServer/lastSuccessfulBuild/artifact/PolarisServer/bin/Debug/PolarisServer-Bundle.zip)

### Documentation
All available documentation for the server can be found on the project wiki @ [wiki.polaris-server.net](http://wiki.polaris-server.net)

## Licensing
All code is licensed under the
[AGPL](https://github.com/PolarisTeam/PolarisServer/blob/master/LICENSE), v3 or later.

### Third Party Licenses
    Copyright (c) 2006 Damien Miller <djm@mindrot.org> (jBCrypt)
    Copyright (c) 2013 Ryan D. Emerle (.Net port)

    Permission to use, copy, modify, and distribute this software for any
    purpose with or without fee is hereby granted, provided that the above
    copyright notice and this permission notice appear in all copies.
    THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
    WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
    MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
    ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
    WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
    ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
    OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.

## Contributing and Feedback
Currently, you can contribute to the Polaris Private Server project by:
* Submitting a detailed [issue](https://github.com/PolarisTeam/PolarisServer/issues/new).
* [Forking the project](https://github.com/PolarisTeam/PolarisServer/fork), and sending a pull request back to for review.

There is an IRC channel `#pso2` on BadnikNET(irc.badniknet.net:6667), for talking directly
with testers and developers (when awake and present, etc.). However any questions pertaining to the release dates or asking for hacks will be ignored, and you may be banned from the channel.

### Core Maintainers

* "cyberkitsune" <https://github.com/cyberkitsune>
* "KeyPhact" <https://github.com/KeyPhact>
* "Kyle873" <https://github.com/Kyle873>
* "Lighting Dragon" <https://github.com/LightningDragon>
* "SonicFreak94" <https://github.com/SonicFreak94>
* "Treeki" <https://github.com/Treeki>
