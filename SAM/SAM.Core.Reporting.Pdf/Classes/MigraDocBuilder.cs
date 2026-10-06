// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MigraDocument = MigraDoc.DocumentObjectModel.Document;

namespace SAM.Core.Reporting.Pdf
{
    /// <summary>
    /// Lays out a renderer-neutral <see cref="Document"/> as a MigraDoc document (approved v2 layout,
    /// documentation/Reporting-LAYOUT.md). It prints the formatted text and units it is given; it never converts,
    /// recomputes or reinterprets a value. Layout follows generic hints only:
    /// <list type="bullet">
    /// <item>the <see cref="PdfRenderer.HeaderSectionId"/> section is printed in the header band;</item>
    /// <item>consecutive <see cref="SectionWidth.Half"/> sections are set side by side;</item>
    /// <item>in a full-width section, consecutive titled key/value blocks are set side by side (up to three);</item>
    /// <item>an untitled key/value block directly followed by a table is placed with it as one unit;</item>
    /// <item>side-by-side content too tall to place as one unit is stacked instead, so it breaks across pages.</item>
    /// </list>
    /// </summary>
    internal sealed class MigraDocBuilder
    {
        private const string DaggerSuffix = " †";
        private const int MaxColumns = 3;

        /// <summary>
        /// Horizontal padding of a notice callout, in mm.
        /// </summary>
        private const double NoticePadding = 2;

        /// <summary>
        /// Characters after which an over-long token is preferably broken (the character stays on the first line).
        /// </summary>
        private const string BreakAfter = "_-/\\.,;:)]}|+&";

        /// <summary>
        /// Allowance for rounding between this measurement and MigraDoc's, in mm.
        /// </summary>
        private const double MeasureTolerance = 0.3;

        private readonly Document document;
        private readonly DocumentMetadata metadata;
        private readonly Color accent;
        private readonly string fontFamily;
        private readonly TextMeasure textMeasure;

        private MigraDocBuilder(Document document)
        {
            this.document = document;
            metadata = document.Metadata;
            accent = PdfLayout.Accent(document.Style.AccentColor);
            fontFamily = string.IsNullOrWhiteSpace(document.Style.FontFamily) ? NotoSansFontResolver.FamilyName : document.Style.FontFamily;
            textMeasure = new TextMeasure(fontFamily);
        }

        public static MigraDocument Build(Document document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            // Layout measures text, so the fonts must resolve before anything else.
            NotoSansFontResolver.Register();
            return new MigraDocBuilder(document).Build();
        }

        private MigraDocument Build()
        {
            MigraDocument migraDocument = CreateDocument();
            migraDocument.Info.Title = string.IsNullOrWhiteSpace(metadata.Subject) ? metadata.Title : string.Format("{0} — {1}", metadata.Title, metadata.Subject);
            migraDocument.Info.Subject = metadata.Subject;
            migraDocument.Info.Author = metadata.PreparedBy;

            Section section = migraDocument.AddSection();
            PageSetup pageSetup = section.PageSetup;
            pageSetup.PageWidth = Unit.FromMillimeter(PdfLayout.PageWidth);
            pageSetup.PageHeight = Unit.FromMillimeter(PdfLayout.PageHeight);
            pageSetup.Orientation = Orientation.Portrait;
            pageSetup.LeftMargin = Unit.FromMillimeter(PdfLayout.Margin);
            pageSetup.RightMargin = Unit.FromMillimeter(PdfLayout.Margin);
            pageSetup.TopMargin = Unit.FromMillimeter(PdfLayout.Margin);
            pageSetup.BottomMargin = Unit.FromMillimeter(PdfLayout.BottomMargin);
            pageSetup.HeaderDistance = Unit.FromMillimeter(PdfLayout.HeaderDistance);
            pageSetup.FooterDistance = Unit.FromMillimeter(PdfLayout.FooterDistance);
            pageSetup.DifferentFirstPageHeaderFooter = true;

            AddRunningHeader(section.Headers.Primary);
            AddFooter(section.Footers.Primary);
            AddFooter(section.Footers.FirstPage);

            DocumentSection headerSection = document.Sections.FirstOrDefault(x => x.Id == PdfRenderer.HeaderSectionId);
            AddTitleBlock(section.Elements, headerSection);

            List<DocumentSection> documentSections = document.Sections.Where(x => x != headerSection).ToList();
            for (int i = 0; i < documentSections.Count; i++)
            {
                // Space between section rows, never after the last (it would start an empty page).
                if (i != 0)
                {
                    AddSpacer(section.Elements, 4.5);
                }

                DocumentSection documentSection = documentSections[i];
                if (documentSection.Width == SectionWidth.Half)
                {
                    List<DocumentSection> pair = new List<DocumentSection>() { documentSection };
                    if (i + 1 < documentSections.Count && documentSections[i + 1].Width == SectionWidth.Half)
                    {
                        pair.Add(documentSections[++i]);
                    }

                    double width = (PdfLayout.ContentWidth - PdfLayout.Gap) / 2;
                    AddSideBySide(section.Elements, pair.Select(x => (Action<DocumentElements, double>)((elements, w) => AddSection(elements, x, w))).ToList(), pair.Select(x => width).ToList());
                }
                else
                {
                    AddSection(section.Elements, documentSection, PdfLayout.ContentWidth);
                }
            }

            return migraDocument;
        }

        private MigraDocument CreateDocument()
        {
            MigraDocument migraDocument = new MigraDocument();

            Style style = migraDocument.Styles[StyleNames.Normal];
            style.Font.Name = fontFamily;
            style.Font.Size = Unit.FromPoint(PdfLayout.Body);
            style.Font.Color = PdfLayout.Text;
            style.ParagraphFormat.SpaceBefore = 0;
            style.ParagraphFormat.SpaceAfter = 0;

            return migraDocument;
        }

        // ------------------------------------------------------------------ header band, running header, footer

        private void AddTitleBlock(DocumentElements elements, DocumentSection headerSection)
        {
            bool mark = document.Style.ShowSamLogo;
            byte[] companyLogo = document.Style.CompanyLogoPng;

            const double markWidth = 13;
            const double markGap = 4;
            const double rightWidth = 58;

            Table table = elements.AddTable();
            SetPadding(table, 0, 0);
            if (mark)
            {
                table.AddColumn(Unit.FromMillimeter(markWidth));
                table.AddColumn(Unit.FromMillimeter(markGap));
            }

            table.AddColumn(Unit.FromMillimeter(PdfLayout.ContentWidth - rightWidth - (mark ? markWidth + markGap : 0)));
            table.AddColumn(Unit.FromMillimeter(rightWidth));

            Row row = table.AddRow();
            row.VerticalAlignment = VerticalAlignment.Top;
            row.BottomPadding = Unit.FromMillimeter(2.5);
            row.Borders.Bottom.Width = Unit.FromPoint(1);
            row.Borders.Bottom.Color = accent;

            int index = 0;
            if (mark)
            {
                AddSamMark(row.Cells[index], markWidth);
                index += 2;
            }

            double titleWidth = PdfLayout.ContentWidth - rightWidth - (mark ? markWidth + markGap : 0);
            Cell cell = row.Cells[index];
            Paragraph paragraph = AddWrappedText(cell.AddParagraph(), (metadata.Title ?? string.Empty).ToUpper(CultureInfo.InvariantCulture), titleWidth, PdfLayout.Title, true);
            paragraph.Format.Font.Size = Unit.FromPoint(PdfLayout.Title);
            paragraph.Format.Font.Bold = true;

            if (!string.IsNullOrWhiteSpace(metadata.Subject))
            {
                paragraph = AddWrappedText(cell.AddParagraph(), metadata.Subject, titleWidth, PdfLayout.Subject, true);
                paragraph.Format.Font.Size = Unit.FromPoint(PdfLayout.Subject);
                paragraph.Format.Font.Bold = true;
                paragraph.Format.SpaceBefore = Unit.FromMillimeter(0.5);
            }

            // The header section's key/value rows, less the one that repeats the subject: "Level: … · Internal condition: …".
            List<string> identity = new List<string>();
            List<DocumentBlock> otherBlocks = new List<DocumentBlock>();
            foreach (DocumentBlock documentBlock in headerSection?.Blocks ?? Enumerable.Empty<DocumentBlock>())
            {
                if (documentBlock is KeyValueBlock keyValueBlock)
                {
                    identity.AddRange(keyValueBlock.Rows.Where(x => x.Value.Text != metadata.Subject).Select(x => string.Format("{0}: {1}", x.Label, ValueText(x.Value, true))));
                }
                else
                {
                    otherBlocks.Add(documentBlock);
                }
            }

            if (identity.Count != 0)
            {
                paragraph = AddWrappedText(cell.AddParagraph(), string.Join("  ·  ", identity), titleWidth);
                paragraph.Format.Font.Color = PdfLayout.Muted;
                paragraph.Format.SpaceBefore = Unit.FromMillimeter(0.8);
            }

            cell = row.Cells[index + 1];
            cell.Format.Alignment = ParagraphAlignment.Right;
            if (companyLogo != null && companyLogo.Length != 0)
            {
                (double width, double height) = Fit(companyLogo, 45, 14);
                paragraph = cell.AddParagraph();
                paragraph.Format.SpaceAfter = Unit.FromMillimeter(1.5);
                Image image = paragraph.AddImage(ImageSource(companyLogo));
                image.Width = Unit.FromMillimeter(width);
                image.Height = Unit.FromMillimeter(height);
                image.LockAspectRatio = true;
            }

            if (!string.IsNullOrWhiteSpace(metadata.ProjectName))
            {
                paragraph = AddWrappedText(cell.AddParagraph(), metadata.ProjectName, rightWidth, 9.5, true);
                paragraph.Format.Font.Bold = true;
                paragraph.Format.Font.Size = Unit.FromPoint(9.5);
            }

            foreach (string text in new[]
            {
                string.IsNullOrWhiteSpace(metadata.ProjectNumber) ? null : "Project no. " + metadata.ProjectNumber,
                string.IsNullOrWhiteSpace(metadata.PreparedBy) ? null : "Prepared by " + metadata.PreparedBy,
                metadata.Date?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            })
            {
                if (text != null)
                {
                    AddWrappedText(cell.AddParagraph(), text, rightWidth);
                }
            }

            AddSpacer(elements, 4);

            // Anything in the header section other than key/value rows (a notice, say) is printed under the band.
            if (otherBlocks.Count != 0)
            {
                AddBlocks(elements, otherBlocks, PdfLayout.ContentWidth, false);
                AddSpacer(elements, 3);
            }
        }

        /// <summary>
        /// The SAM mark: a solid accent square with "SAM" in white, drawn as vector shapes so it is crisp at any zoom.
        /// </summary>
        private void AddSamMark(Cell cell, double size)
        {
            TextFrame textFrame = cell.AddTextFrame();
            textFrame.Width = Unit.FromMillimeter(size);
            textFrame.Height = Unit.FromMillimeter(size);
            textFrame.FillFormat.Color = accent;
            textFrame.LineFormat.Visible = false;
            textFrame.MarginLeft = 0;
            textFrame.MarginRight = 0;
            textFrame.MarginTop = Unit.FromMillimeter((size - 4.6) / 2);
            textFrame.MarginBottom = 0;

            Paragraph paragraph = textFrame.AddParagraph("SAM");
            paragraph.Format.Alignment = ParagraphAlignment.Center;
            paragraph.Format.Font.Bold = true;
            paragraph.Format.Font.Size = Unit.FromPoint(PdfLayout.Mark);
            paragraph.Format.Font.Color = PdfLayout.White;
        }

        /// <summary>
        /// Pages after the first: title and subject, small, over a rule.
        /// </summary>
        private void AddRunningHeader(HeaderFooter headerFooter)
        {
            string text = string.IsNullOrWhiteSpace(metadata.Subject) ? metadata.Title : string.Format("{0}  ·  {1}", metadata.Title, metadata.Subject);
            Paragraph paragraph = AddWrappedText(headerFooter.AddParagraph(), text, PdfLayout.ContentWidth, PdfLayout.Footer);
            paragraph.Format.Font.Size = Unit.FromPoint(PdfLayout.Footer);
            paragraph.Format.Font.Color = PdfLayout.Muted;
            paragraph.Format.Borders.Bottom.Width = Unit.FromPoint(0.5);
            paragraph.Format.Borders.Bottom.Color = PdfLayout.Rule;
            paragraph.Format.Borders.DistanceFromBottom = Unit.FromMillimeter(1);
        }

        /// <summary>
        /// Footer lines on the left; the legend of the markers used, then "Page n / N", on the right.
        /// </summary>
        private void AddFooter(HeaderFooter headerFooter)
        {
            const double rightWidth = 55;

            Table table = headerFooter.AddTable();
            SetPadding(table, 0, 0);
            table.AddColumn(Unit.FromMillimeter(PdfLayout.ContentWidth - rightWidth));
            table.AddColumn(Unit.FromMillimeter(rightWidth));
            table.Format.Font.Size = Unit.FromPoint(PdfLayout.Footer);
            table.Format.Font.Color = PdfLayout.Muted;

            Row row = table.AddRow();
            row.TopPadding = Unit.FromMillimeter(1.5);
            row.Borders.Top.Width = Unit.FromPoint(0.5);
            row.Borders.Top.Color = PdfLayout.HeaderRule;

            foreach (string line in document.Footer.Lines)
            {
                AddWrappedText(row.Cells[0].AddParagraph(), line, PdfLayout.ContentWidth - rightWidth, PdfLayout.Footer);
            }

            Cell cell = row.Cells[1];
            cell.Format.Alignment = ParagraphAlignment.Right;
            foreach (string legend in document.Footer.Legend)
            {
                AddWrappedText(cell.AddParagraph(), legend, rightWidth, PdfLayout.Footer);
            }

            Paragraph paragraph = cell.AddParagraph("Page ");
            paragraph.AddPageField();
            paragraph.AddText(" / ");
            paragraph.AddNumPagesField();
        }

        // ------------------------------------------------------------------ sections and side-by-side rows

        private void AddSection(DocumentElements elements, DocumentSection documentSection, double width)
        {
            Paragraph paragraph = AddWrappedText(elements.AddParagraph(), (documentSection.Title ?? string.Empty).ToUpper(CultureInfo.InvariantCulture), width, PdfLayout.SectionHeading, true);
            paragraph.Format.Font.Size = Unit.FromPoint(PdfLayout.SectionHeading);
            paragraph.Format.Font.Bold = true;
            paragraph.Format.Font.Color = accent;
            paragraph.Format.Borders.Bottom.Width = Unit.FromPoint(0.5);
            paragraph.Format.Borders.Bottom.Color = accent;
            paragraph.Format.Borders.DistanceFromBottom = Unit.FromMillimeter(0.8);
            paragraph.Format.SpaceAfter = Unit.FromMillimeter(1.4);
            paragraph.Format.KeepWithNext = true;

            AddBlocks(elements, documentSection.Blocks, width, width >= PdfLayout.ContentWidth);
        }

        private void AddBlocks(DocumentElements elements, IReadOnlyList<DocumentBlock> documentBlocks, double width, bool allowColumns)
        {
            for (int i = 0; i < documentBlocks.Count; i++)
            {
                if (i != 0 && !(documentBlocks[i] is NoticeBlock noticeBlock && noticeBlock.Level == NoticeLevel.Note))
                {
                    AddSpacer(elements, 1.6);
                }

                // A run of titled key/value blocks is set side by side, at most three to a row.
                if (allowColumns && TitledKeyValue(documentBlocks[i]))
                {
                    List<KeyValueBlock> run = new List<KeyValueBlock>();
                    while (i < documentBlocks.Count && run.Count < MaxColumns && TitledKeyValue(documentBlocks[i]))
                    {
                        run.Add((KeyValueBlock)documentBlocks[i++]);
                    }

                    i--;
                    if (run.Count > 1)
                    {
                        List<double> widths = ColumnWidths(run, width);
                        AddSideBySide(elements, run.Select(x => (Action<DocumentElements, double>)((e, w) => AddKeyValue(e, x, w, true))).ToList(), widths);
                        continue;
                    }

                    AddKeyValue(elements, run[0], width, false);
                    continue;
                }

                // An untitled key/value block that introduces a table (a line or two naming what the table shows) is
                // placed with it as one unit, so a page break never leaves it behind on its own.
                if (documentBlocks[i] is KeyValueBlock keyValueBlock_Lead && string.IsNullOrWhiteSpace(keyValueBlock_Lead.Title) && i + 1 < documentBlocks.Count && documentBlocks[i + 1] is TableBlock tableBlock_Next)
                {
                    AddSideBySide(elements, new List<Action<DocumentElements, double>>()
                    {
                        (e, w) =>
                        {
                            AddKeyValue(e, keyValueBlock_Lead, w, false);
                            AddSpacer(e, 1.6);
                            AddTable(e, tableBlock_Next, w);
                        },
                    }, new List<double>() { width });

                    i++;
                    continue;
                }

                AddBlock(elements, documentBlocks[i], width);
            }
        }

        private static bool TitledKeyValue(DocumentBlock documentBlock)
        {
            return documentBlock is KeyValueBlock keyValueBlock && !string.IsNullOrWhiteSpace(keyValueBlock.Title);
        }

        /// <summary>
        /// Widths for key/value blocks set side by side: shared in proportion to what each needs on one line.
        /// </summary>
        private List<double> ColumnWidths(List<KeyValueBlock> keyValueBlocks, double width)
        {
            double available = width - ((keyValueBlocks.Count - 1) * PdfLayout.Gap);
            List<double> natural = keyValueBlocks.Select(x => Math.Max(20, NaturalWidth(x, true))).ToList();
            double total = natural.Sum();
            return natural.Select(x => available * x / total).ToList();
        }

        /// <summary>
        /// Sets content side by side, each part in a frame of its measured height, as one unit that does not break
        /// across pages. When the tallest part exceeds <see cref="PdfLayout.MaxSideBySideFraction"/> of the body
        /// height, the parts are stacked at the full width instead, so long content paginates rather than clips.
        /// </summary>
        private void AddSideBySide(DocumentElements elements, List<Action<DocumentElements, double>> parts, List<double> widths)
        {
            List<double> heights = parts.Select((x, i) => Measure(x, widths[i])).ToList();
            if (heights.Max() > PdfLayout.BodyHeight * PdfLayout.MaxSideBySideFraction)
            {
                double width = widths.Sum() + ((widths.Count - 1) * PdfLayout.Gap);
                for (int i = 0; i < parts.Count; i++)
                {
                    if (i != 0)
                    {
                        AddSpacer(elements, 4.5);
                    }

                    parts[i](elements, width);
                }

                return;
            }

            Table table = elements.AddTable();
            SetPadding(table, 0, 0);
            for (int i = 0; i < parts.Count; i++)
            {
                if (i != 0)
                {
                    table.AddColumn(Unit.FromMillimeter(PdfLayout.Gap));
                }

                table.AddColumn(Unit.FromMillimeter(widths[i]));
            }

            // A lone half-width section still occupies only its half.
            Row row = table.AddRow();
            row.Height = Unit.FromMillimeter(heights.Max());
            row.HeightRule = RowHeightRule.Exactly;

            for (int i = 0; i < parts.Count; i++)
            {
                TextFrame textFrame = row.Cells[i * 2].AddTextFrame();
                textFrame.Width = Unit.FromMillimeter(widths[i]);
                textFrame.Height = Unit.FromMillimeter(heights[i]);
                textFrame.MarginLeft = 0;
                textFrame.MarginRight = 0;
                textFrame.MarginTop = 0;
                textFrame.MarginBottom = 0;
                parts[i](textFrame.Elements, widths[i]);
            }
        }

        /// <summary>
        /// Height of content laid out at the given width, in mm, measured by formatting it on its own page.
        /// </summary>
        private double Measure(Action<DocumentElements, double> part, double width)
        {
            MigraDocument migraDocument = CreateDocument();
            Section section = migraDocument.AddSection();
            section.PageSetup.PageWidth = Unit.FromMillimeter(width);
            section.PageSetup.PageHeight = Unit.FromMillimeter(10 * PdfLayout.PageHeight);
            section.PageSetup.LeftMargin = 0;
            section.PageSetup.RightMargin = 0;
            section.PageSetup.TopMargin = 0;
            section.PageSetup.BottomMargin = 0;
            part(section.Elements, width);

            DocumentRenderer documentRenderer = new DocumentRenderer(migraDocument);
            documentRenderer.PrepareDocument();
            if (documentRenderer.FormattedDocument.PageCount > 1)
            {
                return double.MaxValue;
            }

            RenderInfo[] renderInfos = documentRenderer.GetRenderInfoFromPage(1);
            if (renderInfos == null || renderInfos.Length == 0)
            {
                return 1;
            }

            double bottom = renderInfos.Max(x => x.LayoutInfo.ContentArea.Y.Millimeter + x.LayoutInfo.ContentArea.Height.Millimeter);
            return bottom + 0.5;
        }

        // ------------------------------------------------------------------ blocks

        private void AddBlock(DocumentElements elements, DocumentBlock documentBlock, double width)
        {
            switch (documentBlock)
            {
                case KeyValueBlock keyValueBlock:
                    AddKeyValue(elements, keyValueBlock, width, false);
                    break;

                case TableBlock tableBlock:
                    AddTable(elements, tableBlock, width);
                    break;

                case NoticeBlock noticeBlock:
                    AddNotice(elements, noticeBlock, width);
                    break;

                case TextBlock textBlock:
                    AddWrappedText(elements.AddParagraph(), textBlock.Text, width);
                    break;

                case ImageBlock imageBlock:
                    AddImage(elements, imageBlock, width);
                    break;

                default:
                    throw new NotSupportedException(string.Format("Block type {0} is not supported by the PDF renderer.", documentBlock.GetType().Name));
            }
        }

        private void AddBlockHeading(DocumentElements elements, string title, double width)
        {
            Paragraph paragraph = AddWrappedText(elements.AddParagraph(), title.ToUpper(CultureInfo.InvariantCulture), width, PdfLayout.BlockHeading, true);
            paragraph.Format.Font.Size = Unit.FromPoint(PdfLayout.BlockHeading);
            paragraph.Format.Font.Bold = true;
            paragraph.Format.Font.Color = PdfLayout.Muted;
            paragraph.Format.SpaceAfter = Unit.FromMillimeter(0.6);
            paragraph.Format.KeepWithNext = true;
        }

        /// <summary>
        /// Label | value | unit. Values are right-aligned against a fixed-width unit column so numbers align down the
        /// block; a text value (a name) spans the value and unit columns.
        /// </summary>
        private void AddKeyValue(DocumentElements elements, KeyValueBlock keyValueBlock, double width, bool narrow)
        {
            if (!string.IsNullOrWhiteSpace(keyValueBlock.Title))
            {
                AddBlockHeading(elements, keyValueBlock.Title, width);
            }

            if (keyValueBlock.Rows.Count == 0)
            {
                return;
            }

            (double label, double value, double unit) = KeyValueWidths(keyValueBlock, width, narrow);

            Table table = elements.AddTable();
            SetPadding(table, PdfLayout.CellPadding, PdfLayout.RowPadding);
            table.AddColumn(Unit.FromMillimeter(label));
            table.AddColumn(Unit.FromMillimeter(value)).Format.Alignment = ParagraphAlignment.Right;
            table.AddColumn(Unit.FromMillimeter(unit)).Format.Alignment = ParagraphAlignment.Left;

            for (int i = 0; i < keyValueBlock.Rows.Count; i++)
            {
                KeyValueRow keyValueRow = keyValueBlock.Rows[i];
                Row row = table.AddRow();
                if (i < keyValueBlock.Rows.Count - 1)
                {
                    RowRule(row);
                }

                AddWrappedText(row.Cells[0].AddParagraph(), keyValueRow.Label, label - (2 * PdfLayout.CellPadding));
                if (!string.IsNullOrWhiteSpace(keyValueRow.SubLabel))
                {
                    Paragraph paragraph = AddWrappedText(row.Cells[0].AddParagraph(), keyValueRow.SubLabel, label - (2 * PdfLayout.CellPadding), PdfLayout.SubLabel);
                    paragraph.Format.Font.Size = Unit.FromPoint(PdfLayout.SubLabel);
                    paragraph.Format.Font.Color = PdfLayout.Muted;
                }

                FormattedValue formattedValue = keyValueRow.Value;
                if (IsText(formattedValue))
                {
                    row.Cells[1].MergeRight = 1;
                }

                AddValue(row.Cells[1], formattedValue, (IsText(formattedValue) ? value + unit : value) - (2 * PdfLayout.CellPadding));
                if (!IsText(formattedValue))
                {
                    AddUnit(row.Cells[2], formattedValue.Unit, unit - (2 * PdfLayout.CellPadding));
                }
            }
        }

        private (double Label, double Value, double Unit) KeyValueWidths(KeyValueBlock keyValueBlock, double width, bool narrow)
        {
            double padding = 2 * PdfLayout.CellPadding;
            double unit = Math.Max(narrow ? PdfLayout.UnitColumnNarrow : PdfLayout.UnitColumn, keyValueBlock.Rows.Where(x => !IsText(x.Value)).Select(x => textMeasure.Width(x.Value.Unit, PdfLayout.Body) + padding).DefaultIfEmpty(0).Max());
            unit = Math.Min(unit, width / 3);

            double label = keyValueBlock.Rows.Select(x => textMeasure.Width(x.Label, PdfLayout.Body) + padding).DefaultIfEmpty(0).Max();
            double value = keyValueBlock.Rows.Where(x => !IsText(x.Value)).Select(x => textMeasure.Width(ValueText(x.Value, false), PdfLayout.Body) + padding).DefaultIfEmpty(0).Max();
            double text = keyValueBlock.Rows.Where(x => IsText(x.Value)).Select(x => textMeasure.Width(ValueText(x.Value, false), PdfLayout.Body) + padding).DefaultIfEmpty(0).Max();

            // A number never wraps; the label takes what it needs of the rest. A name (spanning value and unit) is
            // given room too, but never squeezes the label below 45 %: past that, the name wraps.
            label = Math.Min(label, width - unit - value);
            if (text > width - label)
            {
                label = Math.Min(label, Math.Max(width - text, 0.45 * width));
            }

            label = Math.Max(label, 0.3 * width);
            return (label, width - label - unit, unit);
        }

        private double NaturalWidth(KeyValueBlock keyValueBlock, bool narrow)
        {
            double padding = 2 * PdfLayout.CellPadding;
            double label = keyValueBlock.Rows.Select(x => textMeasure.Width(x.Label, PdfLayout.Body) + padding).DefaultIfEmpty(0).Max();
            double value = keyValueBlock.Rows.Where(x => !IsText(x.Value)).Select(x => textMeasure.Width(ValueText(x.Value, false), PdfLayout.Body) + padding).DefaultIfEmpty(0).Max();
            double text = keyValueBlock.Rows.Where(x => IsText(x.Value)).Select(x => textMeasure.Width(ValueText(x.Value, false), PdfLayout.Body) + padding).DefaultIfEmpty(0).Max();
            double unit = Math.Max(narrow ? PdfLayout.UnitColumnNarrow : PdfLayout.UnitColumn, keyValueBlock.Rows.Where(x => !IsText(x.Value)).Select(x => textMeasure.Width(x.Value.Unit, PdfLayout.Body) + padding).DefaultIfEmpty(0).Max());
            double heading = textMeasure.Width((keyValueBlock.Title ?? string.Empty).ToUpper(CultureInfo.InvariantCulture), PdfLayout.BlockHeading, true);

            return Math.Max(heading, label + Math.Max(value + unit, text));
        }

        /// <summary>
        /// A table. A right-aligned column whose cells carry their own units is split into a value and a unit column
        /// so numbers and units align; a unit shared by the whole column is printed once in its header. Columns are
        /// sized to their content; spare width goes to the left-aligned (text) columns.
        /// </summary>
        private void AddTable(DocumentElements elements, TableBlock tableBlock, double width)
        {
            if (!string.IsNullOrWhiteSpace(tableBlock.Title))
            {
                AddBlockHeading(elements, tableBlock.Title, width);
            }

            if (tableBlock.Columns.Count == 0)
            {
                return;
            }

            double padding = 2 * PdfLayout.CellPadding;
            IReadOnlyList<TableColumn> columns = tableBlock.Columns;
            IReadOnlyList<TableRow> rows = tableBlock.Rows;

            // Per logical column: split into value + unit, and natural widths.
            bool[] split = new bool[columns.Count];
            List<(int Column, bool Unit, double Natural, bool Flexible)> layout = new List<(int, bool, double, bool)>();
            for (int j = 0; j < columns.Count; j++)
            {
                TableColumn tableColumn = columns[j];
                double header = textMeasure.Width(HeaderText(tableColumn), PdfLayout.TableHeader, true) + padding;
                bool flexible = tableColumn.Alignment == ColumnAlignment.Left;

                split[j] = j != 0 && tableColumn.Alignment == ColumnAlignment.Right && rows.Any(x => CellUnit(tableColumn, x.Cells[j]) != null);
                if (split[j])
                {
                    double value = rows.Select(x => textMeasure.Width(ValueText(x.Cells[j], false), PdfLayout.Body) + padding).DefaultIfEmpty(0).Max();
                    double unit = rows.Select(x => textMeasure.Width(CellUnit(tableColumn, x.Cells[j]), PdfLayout.Body) + padding).DefaultIfEmpty(0).Max();
                    // Extra space before the number keeps it clear of the previous column's unit.
                    layout.Add((j, false, Math.Max(value, header - unit) + 2.5, false));
                    layout.Add((j, true, unit, false));
                }
                else
                {
                    double text = rows.Select(x => textMeasure.Width(CellText(tableColumn, x.Cells[j]), PdfLayout.Body) + padding).DefaultIfEmpty(0).Max();
                    layout.Add((j, false, Math.Max(text, header), flexible || j == 0));
                }
            }

            List<double> widths = DistributeWidths(layout.Select(x => (x.Natural, x.Flexible)).ToList(), width);

            Table table = elements.AddTable();
            SetPadding(table, PdfLayout.CellPadding, PdfLayout.RowPadding);
            for (int k = 0; k < layout.Count; k++)
            {
                Column column = table.AddColumn(Unit.FromMillimeter(widths[k]));
                column.Format.Alignment = layout[k].Unit ? ParagraphAlignment.Left : Alignment(columns[layout[k].Column].Alignment, layout[k].Column);
            }

            // Optional group row: one merged heading over adjacent columns of the same group.
            if (columns.Any(x => !string.IsNullOrWhiteSpace(x.Group)))
            {
                Row row = table.AddRow();
                row.HeadingFormat = tableBlock.RepeatHeader;
                HeaderStyle(row, false);
                int k = 0;
                for (int j = 0; j < columns.Count;)
                {
                    int span = 1;
                    while (!string.IsNullOrWhiteSpace(columns[j].Group) && j + span < columns.Count && columns[j + span].Group == columns[j].Group)
                    {
                        span++;
                    }

                    int cells = Enumerable.Range(j, span).Sum(x => split[x] ? 2 : 1);
                    Cell cell = row.Cells[k];
                    cell.MergeRight = cells - 1;
                    cell.Format.Alignment = ParagraphAlignment.Center;
                    AddWrappedText(cell.AddParagraph(), columns[j].Group, widths.Skip(k).Take(cells).Sum() - (2 * PdfLayout.CellPadding), PdfLayout.TableHeader, true);
                    k += cells;
                    j += span;
                }
            }

            Row row_Header = table.AddRow();
            row_Header.HeadingFormat = tableBlock.RepeatHeader;
            HeaderStyle(row_Header, true);
            for (int k = 0, j = 0; j < columns.Count; j++)
            {
                Cell cell = row_Header.Cells[k];
                cell.Format.Alignment = Alignment(columns[j].Alignment, j);
                Paragraph paragraph = AddWrappedText(cell.AddParagraph(), columns[j].Header, widths.Skip(k).Take(split[j] ? 2 : 1).Sum() - (2 * PdfLayout.CellPadding), PdfLayout.TableHeader, true);
                if (!string.IsNullOrWhiteSpace(columns[j].Unit))
                {
                    FormattedText formattedText = paragraph.AddFormattedText(" (" + columns[j].Unit + ")");
                    formattedText.Bold = false;
                }

                if (split[j])
                {
                    cell.MergeRight = 1;
                    k += 2;
                }
                else
                {
                    k += 1;
                }
            }

            for (int i = 0; i < rows.Count; i++)
            {
                Row row = table.AddRow();
                if (i < rows.Count - 1)
                {
                    RowRule(row);
                }

                for (int k = 0, j = 0; j < columns.Count; j++)
                {
                    FormattedValue formattedValue = rows[i].Cells[j];
                    if (split[j])
                    {
                        AddValue(row.Cells[k], formattedValue, widths[k] - (2 * PdfLayout.CellPadding));
                        AddUnit(row.Cells[k + 1], CellUnit(columns[j], formattedValue), widths[k + 1] - (2 * PdfLayout.CellPadding));
                        k += 2;
                    }
                    else
                    {
                        AddValue(row.Cells[k], formattedValue, widths[k] - (2 * PdfLayout.CellPadding), CellUnit(columns[j], formattedValue));
                        k += 1;
                    }
                }
            }
        }

        /// <summary>
        /// Natural widths when they fit, spare width to the flexible columns; otherwise flexible columns shrink (and
        /// wrap) first, then everything scales.
        /// </summary>
        private static List<double> DistributeWidths(List<(double Natural, bool Flexible)> columns, double width)
        {
            List<double> result = columns.Select(x => x.Natural).ToList();
            List<int> flexible = Enumerable.Range(0, columns.Count).Where(x => columns[x].Flexible).ToList();
            if (flexible.Count == 0)
            {
                flexible.Add(0);
            }

            double total = result.Sum();
            double flexibleTotal = flexible.Sum(x => result[x]);
            if (total <= width)
            {
                double spare = width - total;
                foreach (int index in flexible)
                {
                    result[index] += flexibleTotal > 0 ? spare * result[index] / flexibleTotal : spare / flexible.Count;
                }

                return result;
            }

            double excess = total - width;
            double shrinkable = flexible.Sum(x => result[x] * 0.6);
            if (shrinkable > 0)
            {
                double factor = Math.Min(1, excess / shrinkable);
                foreach (int index in flexible)
                {
                    result[index] -= result[index] * 0.6 * factor;
                }
            }

            total = result.Sum();
            return total > width ? result.Select(x => x * width / total).ToList() : result;
        }

        private void AddNotice(DocumentElements elements, NoticeBlock noticeBlock, double width)
        {
            if (noticeBlock.Level == NoticeLevel.Note)
            {
                Paragraph paragraph = AddWrappedText(elements.AddParagraph(), noticeBlock.Text, width - PdfLayout.CellPadding, PdfLayout.Note);
                paragraph.Format.Font.Size = Unit.FromPoint(PdfLayout.Note);
                paragraph.Format.Font.Color = PdfLayout.Muted;
                paragraph.Format.SpaceBefore = Unit.FromMillimeter(0.8);
                paragraph.Format.LeftIndent = Unit.FromMillimeter(PdfLayout.CellPadding);
                return;
            }

            bool warning = noticeBlock.Level == NoticeLevel.Warning;

            Table table = elements.AddTable();
            SetPadding(table, NoticePadding, 1.1);
            table.AddColumn(Unit.FromMillimeter(width));
            Row row = table.AddRow();
            row.Shading.Color = warning ? PdfLayout.WarningShading : PdfLayout.NoticeShading;
            row.Borders.Left.Width = Unit.FromPoint(1.5);
            row.Borders.Left.Color = warning ? PdfLayout.WarningBar : accent;
            Paragraph paragraph_Notice = AddWrappedText(row.Cells[0].AddParagraph(), noticeBlock.Text, width - (2 * NoticePadding), PdfLayout.NoticeText);
            paragraph_Notice.Format.Font.Size = Unit.FromPoint(PdfLayout.NoticeText);
        }

        private void AddImage(DocumentElements elements, ImageBlock imageBlock, double width)
        {
            byte[] png = imageBlock.Png;
            if (png != null && png.Length != 0)
            {
                (double w, double h) = Fit(png, width * imageBlock.WidthFraction, PdfLayout.BodyHeight * 0.8);
                Paragraph paragraph = elements.AddParagraph();
                Image image = paragraph.AddImage(ImageSource(png));
                image.Width = Unit.FromMillimeter(w);
                image.Height = Unit.FromMillimeter(h);
                image.LockAspectRatio = true;
            }

            if (!string.IsNullOrWhiteSpace(imageBlock.Caption))
            {
                Paragraph paragraph = AddWrappedText(elements.AddParagraph(), imageBlock.Caption, width, PdfLayout.BlockHeading);
                paragraph.Format.Font.Size = Unit.FromPoint(PdfLayout.BlockHeading);
                paragraph.Format.Font.Color = PdfLayout.Muted;
                paragraph.Format.SpaceBefore = Unit.FromMillimeter(0.8);
            }
        }

        // ------------------------------------------------------------------ values

        /// <summary>
        /// A value that is a name rather than a number: available, with no unit.
        /// </summary>
        private static bool IsText(FormattedValue formattedValue)
        {
            return formattedValue.Availability == Availability.Available && string.IsNullOrEmpty(formattedValue.Unit);
        }

        /// <summary>
        /// Value text as supplied, with the out-of-date dagger when flagged. Placeholders ("—", "n/a", "not set")
        /// and zeros are printed exactly as the builders formatted them.
        /// </summary>
        private static string ValueText(FormattedValue formattedValue, bool withUnit)
        {
            string text = formattedValue.Text ?? string.Empty;
            if (withUnit && !string.IsNullOrEmpty(formattedValue.Unit))
            {
                text += " " + formattedValue.Unit;
            }

            return formattedValue.IsOutOfDate ? text + DaggerSuffix : text;
        }

        private static string CellUnit(TableColumn tableColumn, FormattedValue formattedValue)
        {
            string unit = formattedValue.Unit;
            return string.IsNullOrEmpty(unit) || unit == tableColumn.Unit ? null : unit;
        }

        private static string CellText(TableColumn tableColumn, FormattedValue formattedValue)
        {
            string unit = CellUnit(tableColumn, formattedValue);
            string text = ValueText(formattedValue, false);
            return unit == null ? text : text + " " + unit;
        }

        private static string HeaderText(TableColumn tableColumn)
        {
            return string.IsNullOrWhiteSpace(tableColumn.Unit) ? tableColumn.Header ?? string.Empty : string.Format("{0} ({1})", tableColumn.Header, tableColumn.Unit);
        }

        private void AddValue(Cell cell, FormattedValue formattedValue, double width, string unit = null)
        {
            Paragraph paragraph = AddWrappedText(cell.AddParagraph(), ValueText(formattedValue, false), width);
            if (formattedValue.Availability != Availability.Available)
            {
                paragraph.Format.Font.Color = PdfLayout.Muted;
            }

            if (!string.IsNullOrEmpty(unit))
            {
                FormattedText formattedText = paragraph.AddFormattedText(" " + unit);
                formattedText.Color = PdfLayout.Muted;
            }
        }

        private void AddUnit(Cell cell, string unit, double width)
        {
            if (!string.IsNullOrEmpty(unit))
            {
                Paragraph paragraph = AddWrappedText(cell.AddParagraph(), unit, width);
                paragraph.Format.Font.Color = PdfLayout.Muted;
            }
        }

        // ------------------------------------------------------------------ long tokens

        /// <summary>
        /// Adds text to a paragraph so that it cannot run past the given width (mm). MigraDoc breaks lines only at
        /// spaces, so a single token wider than the space available (a long underscore-delimited name, a path, a
        /// run of characters with no break) would overflow its column. Such a token is split into pieces that each
        /// fit, preferably after a separator (<see cref="BreakAfter"/>), otherwise after the last character that
        /// fits, with a line break between the pieces. No character is removed or added and the font size is
        /// unchanged. Text whose tokens all fit is added unchanged, as one text element.
        /// </summary>
        private Paragraph AddWrappedText(Paragraph paragraph, string text, double width, double size = PdfLayout.Body, bool bold = false)
        {
            List<string> lines = BreakLongTokens(text ?? string.Empty, width, size, bold);
            for (int i = 0; i < lines.Count; i++)
            {
                if (i != 0)
                {
                    paragraph.AddLineBreak();
                }

                if (lines[i].Length != 0)
                {
                    paragraph.AddText(lines[i]);
                }
            }

            return paragraph;
        }

        /// <summary>
        /// The text as runs separated by forced line breaks: a single run, the text itself, when every
        /// space-delimited token fits the width (mm). A token that does not fit is cut to pieces a little narrower
        /// than the width (<see cref="MeasureTolerance"/>); a token that fits is never touched.
        /// </summary>
        private List<string> BreakLongTokens(string text, double width, double size, bool bold)
        {
            string[] tokens = text.Split(' ');
            if (width <= MeasureTolerance || tokens.All(x => Fits(x, width, size, bold)))
            {
                return new List<string>() { text };
            }

            List<string> result = new List<string>();
            System.Text.StringBuilder current = new System.Text.StringBuilder();
            for (int i = 0; i < tokens.Length; i++)
            {
                if (i != 0)
                {
                    current.Append(' ');
                }

                List<string> pieces = Fits(tokens[i], width, size, bold) ? new List<string>() { tokens[i] } : Pieces(tokens[i], width - MeasureTolerance, size, bold);
                for (int j = 0; j < pieces.Count; j++)
                {
                    if (j != 0)
                    {
                        result.Add(current.ToString());
                        current.Clear();
                    }

                    current.Append(pieces[j]);
                }
            }

            result.Add(current.ToString());
            return result;
        }

        /// <summary>
        /// True when the token fits the width; the small allowance absorbs rounding in widths that were sized to this
        /// same measurement (a column exactly as wide as its widest unit).
        /// </summary>
        private bool Fits(string token, double width, double size, bool bold)
        {
            return textMeasure.Width(token, size, bold) <= width + 0.01;
        }

        /// <summary>
        /// A token cut into pieces no wider than the width: each piece is as long as fits, ending after the last
        /// separator that fits when there is one, else after the last character that fits (at least one character).
        /// </summary>
        private List<string> Pieces(string token, double width, double size, bool bold)
        {
            List<string> result = new List<string>();
            int start = 0;
            while (start < token.Length)
            {
                int end = start + 1;
                int lastBreak = -1;
                while (end <= token.Length && textMeasure.Width(token.Substring(start, end - start), size, bold) <= width)
                {
                    if (end < token.Length && BreakAfter.IndexOf(token[end - 1]) >= 0)
                    {
                        lastBreak = end;
                    }

                    end++;
                }

                int fit = end - 1;
                if (fit >= token.Length)
                {
                    result.Add(token.Substring(start));
                    break;
                }

                int cut = lastBreak > start ? lastBreak : Math.Max(fit, start + 1);
                result.Add(token.Substring(start, cut - start));
                start = cut;
            }

            return result;
        }

        // ------------------------------------------------------------------ helpers

        private static ParagraphAlignment Alignment(ColumnAlignment columnAlignment, int index)
        {
            if (index == 0)
            {
                return ParagraphAlignment.Left;
            }

            switch (columnAlignment)
            {
                case ColumnAlignment.Left:
                    return ParagraphAlignment.Left;

                case ColumnAlignment.Center:
                    return ParagraphAlignment.Center;
            }

            return ParagraphAlignment.Right;
        }

        private static void HeaderStyle(Row row, bool rule)
        {
            row.Format.Font.Size = Unit.FromPoint(PdfLayout.TableHeader);
            row.Format.Font.Bold = true;
            row.Format.Font.Color = PdfLayout.Muted;
            if (rule)
            {
                row.Borders.Bottom.Width = Unit.FromPoint(0.5);
                row.Borders.Bottom.Color = PdfLayout.HeaderRule;
            }
        }

        private static void RowRule(Row row)
        {
            row.Borders.Bottom.Width = Unit.FromPoint(0.4);
            row.Borders.Bottom.Color = PdfLayout.Rule;
        }

        private static void SetPadding(Table table, double horizontal, double vertical)
        {
            table.LeftPadding = Unit.FromMillimeter(horizontal);
            table.RightPadding = Unit.FromMillimeter(horizontal);
            table.TopPadding = Unit.FromMillimeter(vertical);
            table.BottomPadding = Unit.FromMillimeter(vertical);
            table.Rows.LeftIndent = 0;
        }

        private static void AddSpacer(DocumentElements elements, double millimeters)
        {
            Paragraph paragraph = elements.AddParagraph();
            paragraph.Format.Font.Size = Unit.FromPoint(1);
            paragraph.Format.LineSpacingRule = LineSpacingRule.Exactly;
            paragraph.Format.LineSpacing = Unit.FromMillimeter(millimeters);
        }

        private static string ImageSource(byte[] png)
        {
            return "base64:" + System.Convert.ToBase64String(png);
        }

        /// <summary>
        /// Size in mm of a PNG scaled to fit the box, keeping its aspect ratio.
        /// </summary>
        private static (double Width, double Height) Fit(byte[] png, double maxWidth, double maxHeight)
        {
            (int pixelWidth, int pixelHeight) = PngSize(png);
            double width = maxWidth;
            double height = maxWidth * pixelHeight / pixelWidth;
            if (height > maxHeight)
            {
                height = maxHeight;
                width = maxHeight * pixelWidth / pixelHeight;
            }

            return (width, height);
        }

        /// <summary>
        /// Pixel size from the PNG header; throws for data that is not a PNG.
        /// </summary>
        internal static (int Width, int Height) PngSize(byte[] png)
        {
            byte[] signature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
            if (png == null || png.Length < 24 || !signature.SequenceEqual(png.Take(8)))
            {
                throw new ArgumentException("Image data is not a PNG.", nameof(png));
            }

            int width = (png[16] << 24) | (png[17] << 16) | (png[18] << 8) | png[19];
            int height = (png[20] << 24) | (png[21] << 16) | (png[22] << 8) | png[23];
            if (width <= 0 || height <= 0)
            {
                throw new ArgumentException("PNG has no size.", nameof(png));
            }

            return (width, height);
        }
    }
}
