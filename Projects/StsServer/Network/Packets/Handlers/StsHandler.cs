﻿// Copyright (c) Multi-Emu.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using StsServer.Attributes;
using StsServer.Constants.Net;
using Framework.Cryptography;
using Framework.Cryptography.BNet;
using Framework.Database;
using Framework.Database.Auth;
using Framework.Misc;
using Lappa_ORM;

namespace StsServer.Network.Packets.Handlers
{
    class StsHandler
    {
        [StsMessage(StsMessage.LoginStart)]
        public static void HandleAuthLoginStart(StsPacket packet, StsSession session)
        {
            // Can be an email or user name.
            var loginName = packet["LoginName"].ToString();

            session.Account = DB.Auth.Single<Account>(a => a.Email == loginName);

            // Support for email only.
            if (loginName != null && session.Account != null)
            {
                session.SecureRemotePassword = new SRP6a(session.Account.Salt, loginName, session.Account.PasswordVerifier);
                session.SecureRemotePassword.CalculateB();

                var keyData = new BinaryWriter(new MemoryStream());

                keyData.Write(session.SecureRemotePassword.S.Length);
                keyData.Write(session.SecureRemotePassword.S);
                keyData.Write(session.SecureRemotePassword.B.Length);
                keyData.Write(session.SecureRemotePassword.B);

                var reply = new StsPacket(StsReason.OK, packet.Header.Sequence);
                var xmlData = new XmlData();

                xmlData.WriteElementRoot("Reply");
                xmlData.WriteElement("KeyData", Convert.ToBase64String(keyData.ToArray()));

                reply.WriteXmlData(xmlData);

                session.Send(reply);
            }
            else
            {
                // Let's use ErrBadPasswd instead of ErrAccountNotFound.
                var reply = new StsPacket(StsReason.ErrBadPasswd, packet.Header.Sequence);

                reply.WriteString("<Error code=\"11\" server=\"0\" module=\"0\" line=\"0\"/>\n");

                session.Send(reply);
            }
        }

        [StsMessage(StsMessage.KeyData)]
        public static void HandleAuthKeyData(StsPacket packet, StsSession session)
        {
            var keyData = new BinaryReader(new MemoryStream(Convert.FromBase64String(packet["KeyData"].ToString())));
            var a = keyData.ReadBytes(keyData.ReadInt32());
            var m = keyData.ReadBytes(keyData.ReadInt32());

            session.SecureRemotePassword.CalculateU(a);
            session.SecureRemotePassword.CalculateClientM(a);

            if (session.SecureRemotePassword.ClientM.Compare(m))
            {
                session.SecureRemotePassword.CalculateServerM(m, a);

                session.ClientCrypt = new SARC4();
                session.ClientCrypt.PrepareKey(session.SecureRemotePassword.SessionKey);

                session.State = 1;

                var SKeyData = new BinaryWriter(new MemoryStream());

                SKeyData.Write(session.SecureRemotePassword.ServerM.Length);
                SKeyData.Write(session.SecureRemotePassword.ServerM);

                var reply = new StsPacket(StsReason.OK, packet.Header.Sequence);
                var xmlData = new XmlData();

                xmlData.WriteElementRoot("Reply");
                xmlData.WriteElement("KeyData", Convert.ToBase64String(SKeyData.ToArray()));

                reply.WriteXmlData(xmlData);

                session.Send(reply);
            }
            else
            {
                session.Account = null;

                var reply = new StsPacket(StsReason.ErrBadPasswd, packet.Header.Sequence);

                reply.WriteString("<Error code=\"11\" server=\"0\" module=\"0\" line=\"0\"/>\n");

                session.Send(reply);
            }
        }

        [StsMessage(StsMessage.LoginFinish)]
        public static void HandleAuthLoginFinish(StsPacket packet, StsSession session)
        {
            // Server packets are encrypted now.
            session.ServerCrypt = new SARC4();
            session.ServerCrypt.PrepareKey(session.SecureRemotePassword.SessionKey);

            var reply = new StsPacket(StsReason.OK, packet.Header.Sequence);
            var xmlData = new XmlData();

            xmlData.WriteElementRoot("Reply");

            xmlData.WriteElement("LocationId", "");
            xmlData.WriteElement("UserId", session.Account.Id.ToString());
            xmlData.WriteElement("UserCenter", "");
            xmlData.WriteElement("UserName", session.Account.LoginName);
            xmlData.WriteElement("AccessMask", "");
            xmlData.WriteElement("Roles", "");
            xmlData.WriteElement("Status", "");

            reply.WriteXmlData(xmlData);

            session.Send(reply);
        }

        [StsMessage(StsMessage.RequestGameToken)]
        public static void HandleAuthRequestGameToken(StsPacket packet, StsSession session)
        {
            if (DB.Auth.Update<Account>(a => a.Id == session.Account.Id, a => a.Online.Set(true)))
            {
                var reply = new StsPacket(StsReason.OK, packet.Header.Sequence);
                var xmlData = new XmlData();

                xmlData.WriteElementRoot("Reply");

                xmlData.WriteElement("Token", "");

                reply.WriteXmlData(xmlData);

                session.Send(reply);
            }
            else
                session.Dispose();
        }
    }
}
