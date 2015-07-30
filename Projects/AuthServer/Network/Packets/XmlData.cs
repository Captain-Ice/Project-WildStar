﻿// Copyright (c) Multi-Emu.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
using System.Xml;

namespace AuthServer.Network.Packets
{
    class XmlData
    {
        XmlWriter xmlWriter;
        StringBuilder builder;

        public XmlData()
        {
            var xmlWriterSettings = new XmlWriterSettings();

            xmlWriterSettings.NewLineChars = "\n";
            xmlWriterSettings.Indent = false;
            xmlWriterSettings.OmitXmlDeclaration = true;

            builder = new StringBuilder();
            xmlWriter = XmlWriter.Create(builder, xmlWriterSettings);

            xmlWriter.WriteStartDocument();
        }

        public void WriteElementRoot(string name)
        {
            xmlWriter.WriteStartElement(name);
            xmlWriter.WriteRaw("\n");
        }

        public void WriteElement(string name, string data)
        {
            xmlWriter.WriteElementString(name, data);
            xmlWriter.WriteRaw("\n");
        }

        public override string ToString()
        {
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();

            xmlWriter.Flush();
            xmlWriter.Close();

            return builder.ToString();
        }
    }
}