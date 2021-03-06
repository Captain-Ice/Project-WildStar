﻿// Copyright (c) Multi-Emu.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Lappa_ORM;

namespace Framework.Database.Auth
{
    public class Account : Entity
    {
        [AutoIncrement]
        public uint Id                 { get; set; }
        public string Email            { get; set; }
        public string LoginName        { get; set; }
        public string PasswordVerifier { get; set; }
        public string Salt             { get; set; }
        public bool Online             { get; set; }

        public virtual List<GameAccount> GameAccounts { get; set; }
    }
}
