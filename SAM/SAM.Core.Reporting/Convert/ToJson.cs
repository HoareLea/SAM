// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace SAM.Core.Reporting
{
    public static partial class Convert
    {
        /// <summary>
        /// Renderer-independent JSON of a document, for golden snapshot tests and diagnostics. The output is
        /// deterministic: properties are written in a fixed order, indentation is two spaces, line endings are "\n",
        /// and non-ASCII text (m², °C, —) is kept readable. Image bytes are summarised by their length.
        /// </summary>
        public static string ToJson(this Document document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            JsonWriterOptions jsonWriterOptions = new JsonWriterOptions()
            {
                Indented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (Utf8JsonWriter utf8JsonWriter = new Utf8JsonWriter(memoryStream, jsonWriterOptions))
                {
                    Write(utf8JsonWriter, document);
                }

                return Encoding.UTF8.GetString(memoryStream.ToArray()).Replace("\r\n", "\n");
            }
        }

        private static void Write(Utf8JsonWriter utf8JsonWriter, Document document)
        {
            utf8JsonWriter.WriteStartObject();
            utf8JsonWriter.WriteString("id", document.Id);

            DocumentMetadata documentMetadata = document.Metadata;
            utf8JsonWriter.WriteStartObject("metadata");
            WriteString(utf8JsonWriter, "title", documentMetadata.Title);
            WriteString(utf8JsonWriter, "subject", documentMetadata.Subject);
            WriteString(utf8JsonWriter, "projectName", documentMetadata.ProjectName);
            WriteString(utf8JsonWriter, "projectNumber", documentMetadata.ProjectNumber);
            WriteString(utf8JsonWriter, "preparedBy", documentMetadata.PreparedBy);
            WriteString(utf8JsonWriter, "date", documentMetadata.Date?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            utf8JsonWriter.WriteEndObject();

            utf8JsonWriter.WriteStartArray("sections");
            foreach (DocumentSection documentSection in document.Sections)
            {
                utf8JsonWriter.WriteStartObject();
                utf8JsonWriter.WriteString("id", documentSection.Id);
                WriteString(utf8JsonWriter, "title", documentSection.Title);
                utf8JsonWriter.WriteString("width", documentSection.Width.ToString());
                utf8JsonWriter.WriteStartArray("blocks");
                foreach (DocumentBlock documentBlock in documentSection.Blocks)
                {
                    Write(utf8JsonWriter, documentBlock);
                }

                utf8JsonWriter.WriteEndArray();
                utf8JsonWriter.WriteEndObject();
            }

            utf8JsonWriter.WriteEndArray();

            utf8JsonWriter.WriteStartObject("footer");
            utf8JsonWriter.WriteStartArray("lines");
            foreach (string line in document.Footer.Lines)
            {
                utf8JsonWriter.WriteStringValue(line);
            }

            utf8JsonWriter.WriteEndArray();
            utf8JsonWriter.WriteStartArray("legend");
            foreach (string line in document.Footer.Legend)
            {
                utf8JsonWriter.WriteStringValue(line);
            }

            utf8JsonWriter.WriteEndArray();
            utf8JsonWriter.WriteEndObject();

            utf8JsonWriter.WriteEndObject();
        }

        private static void Write(Utf8JsonWriter utf8JsonWriter, DocumentBlock documentBlock)
        {
            utf8JsonWriter.WriteStartObject();
            utf8JsonWriter.WriteString("type", documentBlock.GetType().Name);
            utf8JsonWriter.WriteString("id", documentBlock.Id);

            switch (documentBlock)
            {
                case KeyValueBlock keyValueBlock:
                    WriteString(utf8JsonWriter, "title", keyValueBlock.Title);
                    utf8JsonWriter.WriteStartArray("rows");
                    foreach (KeyValueRow keyValueRow in keyValueBlock.Rows)
                    {
                        utf8JsonWriter.WriteStartObject();
                        utf8JsonWriter.WriteString("label", keyValueRow.Label);
                        WriteString(utf8JsonWriter, "subLabel", keyValueRow.SubLabel);
                        utf8JsonWriter.WritePropertyName("value");
                        Write(utf8JsonWriter, keyValueRow.Value);
                        utf8JsonWriter.WriteEndObject();
                    }

                    utf8JsonWriter.WriteEndArray();
                    break;

                case TableBlock tableBlock:
                    WriteString(utf8JsonWriter, "title", tableBlock.Title);
                    utf8JsonWriter.WriteBoolean("repeatHeader", tableBlock.RepeatHeader);
                    utf8JsonWriter.WriteStartArray("columns");
                    foreach (TableColumn tableColumn in tableBlock.Columns)
                    {
                        utf8JsonWriter.WriteStartObject();
                        utf8JsonWriter.WriteString("header", tableColumn.Header);
                        WriteString(utf8JsonWriter, "unit", tableColumn.Unit);
                        utf8JsonWriter.WriteString("alignment", tableColumn.Alignment.ToString());
                        WriteString(utf8JsonWriter, "group", tableColumn.Group);
                        utf8JsonWriter.WriteEndObject();
                    }

                    utf8JsonWriter.WriteEndArray();
                    utf8JsonWriter.WriteStartArray("rows");
                    foreach (TableRow tableRow in tableBlock.Rows)
                    {
                        utf8JsonWriter.WriteStartArray();
                        foreach (FormattedValue formattedValue in tableRow.Cells)
                        {
                            Write(utf8JsonWriter, formattedValue);
                        }

                        utf8JsonWriter.WriteEndArray();
                    }

                    utf8JsonWriter.WriteEndArray();
                    break;

                case NoticeBlock noticeBlock:
                    utf8JsonWriter.WriteString("level", noticeBlock.Level.ToString());
                    utf8JsonWriter.WriteString("text", noticeBlock.Text);
                    break;

                case TextBlock textBlock:
                    utf8JsonWriter.WriteString("text", textBlock.Text);
                    break;

                case ImageBlock imageBlock:
                    utf8JsonWriter.WriteNumber("pngLength", imageBlock.Png?.Length ?? 0);
                    WriteString(utf8JsonWriter, "caption", imageBlock.Caption);
                    utf8JsonWriter.WriteNumber("widthFraction", imageBlock.WidthFraction);
                    break;
            }

            utf8JsonWriter.WriteEndObject();
        }

        private static void Write(Utf8JsonWriter utf8JsonWriter, FormattedValue formattedValue)
        {
            utf8JsonWriter.WriteStartObject();
            utf8JsonWriter.WriteString("text", formattedValue.Text);
            WriteString(utf8JsonWriter, "unit", formattedValue.Unit);
            utf8JsonWriter.WriteString("availability", formattedValue.Availability.ToString());
            WriteString(utf8JsonWriter, "freshness", formattedValue.Freshness?.ToString());
            WriteString(utf8JsonWriter, "source", formattedValue.Source?.ToString());
            WriteString(utf8JsonWriter, "sourceTimestamp", formattedValue.SourceTimestamp?.ToString("o", CultureInfo.InvariantCulture));
            WriteString(utf8JsonWriter, "note", formattedValue.Note);
            utf8JsonWriter.WriteEndObject();
        }

        /// <summary>
        /// Writes the property only when the value is not null, so optional fields do not clutter snapshots.
        /// </summary>
        private static void WriteString(Utf8JsonWriter utf8JsonWriter, string propertyName, string value)
        {
            if (value != null)
            {
                utf8JsonWriter.WriteString(propertyName, value);
            }
        }
    }
}
