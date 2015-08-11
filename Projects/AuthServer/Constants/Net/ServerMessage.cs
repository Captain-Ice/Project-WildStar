﻿// Copyright (c) Multi-Emu.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AuthServer.Constants.Net
{
    enum ServerMessage : ushort
    {
        State1         = 0x000,
        State2         = 0x001,
        SHello         = 0x003,
        MultiPacket    = 0x06D,
        ConnectToRealm = 0x36A,
        AuthComplete   = 0x537,
    }
}
