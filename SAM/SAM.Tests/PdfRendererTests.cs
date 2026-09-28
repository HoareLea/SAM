// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Fields;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.IO;
using SAM.Analytical.Reporting;
using SAM.Core.Reporting;
using SAM.Core.Reporting.Pdf;
using SAM.Tests.Helpers;
using SAM.Units;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using Document = SAM.Core.Reporting.Document;
using MigraDocument = MigraDoc.DocumentObjectModel.Document;

namespace SAM.Tests
{
    /// <summary>
    /// The MigraDoc/PDFsharp renderer (SAM.Core.Reporting.Pdf): valid A4 output for the design-gate documents, content
    /// printed exactly as the builders formatted it, every generic block type, branding options and natural
    /// pagination. Structure is checked on the PDF (pages, size) and on the MigraDoc model the renderer builds (text),
    /// never byte for byte: a PDF carries a creation time and a random file id.
    /// </summary>
    public partial class PdfRendererTests
    {
        private const double A4Width = 595.28;
        private const double A4Height = 841.89;

        private readonly ITestOutputHelper output;

        public PdfRendererTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        public static IEnumerable<object[]> DesignGateCases()
        {
            return ReportingDesignGateFixture.Cases().Select(x => new object[] { x.Name });
        }

        // ---------- design-gate documents ----------

        /// <summary>
        /// The four representative design-gate cases render to one A4 page, the approved v2 target. The PDFs are
        /// written to the test output (ReportingPdf/) for visual review, or to SAM_REPORTING_PDF_DIR when set.
        /// </summary>
        [Theory]
        [MemberData(nameof(DesignGateCases))]
        public void DesignGate_RendersOneA4Page(string name)
        {
            byte[] bytes = new PdfRenderer().Render(ReportingDesignGateFixture.Document(name));
            Save(string.Format("SpaceAssumptions_{0}.pdf", name), bytes);

            PdfDocument pdfDocument = Open(bytes);
            Assert.Equal(1, pdfDocument.PageCount);
            AssertA4(pdfDocument.Pages[0]);
        }

        [Theory]
        [MemberData(nameof(DesignGateCases))]
        public void DesignGate_PrintsEveryFormattedValueVerbatim(string name)
        {
            Document document = ReportingDesignGateFixture.Document(name);
            string text = Text(document);

            foreach (FormattedValue formattedValue in document.FormattedValues())
            {
                Assert.Contains(formattedValue.Text, text);
            }

            foreach (string line in document.Footer.Lines.Concat(document.Footer.Legend))
            {
                Assert.Contains(line, text);
            }
        }

        [Fact]
        public void DesignGate_Imperial_HasNoSIUnits()
        {
            Document document = ReportingDesignGateFixture.Document("office_IP");
            string text = Text(document);

            foreach (string symbol in new[] { "m²", "m³", "°C", "L/s", "W/m²", "lx", "kW" })
            {
                Assert.DoesNotContain(symbol, text);
            }

            Assert.Contains("°F", text);
            Assert.Contains("ft²", text);
        }

        [Fact]
        public void DesignGate_Office_KeepsMarkersDistinct()
        {
            Document document = ReportingDesignGateFixture.Document("office_SI");
            List<string> paragraphs = Paragraphs(document);

            // Humidification disabled (none), cooling multiplier set nowhere (not set), equipment latent authored 0.
            Assert.Contains(SpaceDesignCriteriaSectionBuilder.ControlOffText, paragraphs);
            Assert.Contains(SpaceSizingSectionBuilder.SizingMultiplierNotSetText, paragraphs);
            Assert.Contains("0", paragraphs);
            Assert.Contains("lower RH limit", paragraphs);
            Assert.Contains("upper RH limit", paragraphs);
            Assert.Contains("Humidification set point", paragraphs);
            Assert.Contains("Dehumidification set point", paragraphs);

            // The legend lists only what is printed: n/a, but no "—" in this document.
            Assert.DoesNotContain("—", document.FormattedValues().Select(x => x.Text));
            // (The footer is built twice: for the first page and for the pages after it.)
            Assert.Equal(document.Footer.Legend, paragraphs.Where(x => x.Contains("not applicable") || x.Contains("not available")).Distinct());
        }

        [Fact]
        public void DesignGate_Sparse_PrintsNotAvailableMarker()
        {
            Document document = ReportingDesignGateFixture.Document("sparse_SI");
            List<string> paragraphs = Paragraphs(document);

            Assert.Contains("—", paragraphs);
            Assert.Contains(document.Footer.Legend, x => x.StartsWith("—"));
            Assert.Contains(SpaceSizingSectionBuilder.DesignLoadsNoneNotice, paragraphs);
        }

        [Fact]
        public void DesignGate_FabricNote_IsPrintedAsSuppliedWithoutUnit()
        {
            Document document = ReportingDesignGateFixture.Document("office_SI");
            NoticeBlock noticeBlock = document.Sections.Single(x => x.Id == "fabric").Blocks.OfType<NoticeBlock>().Single(x => x.Id == "fabric-not-present");

            Assert.Equal(NoticeLevel.Note, noticeBlock.Level);
            Assert.Contains(noticeBlock.Text, Paragraphs(document));
            Assert.DoesNotContain("m²", noticeBlock.Text);
        }

        [Fact]
        public void DesignGate_IdentityIsInTheHeaderBand()
        {
            Document document = ReportingDesignGateFixture.Document("atrium_SI");
            List<string> paragraphs = Paragraphs(document);

            Assert.Contains("SPACE ASSUMPTIONS", paragraphs);
            Assert.Contains(ReportingDesignGateFixture.AtriumName, paragraphs);
            Assert.Contains(paragraphs, x => x.Contains("Level: Level 00 (Ground)") && x.Contains("Internal condition: S12_Reception_Atrium_NCM_CirculationArea"));

            // The identity section is not repeated as a body section.
            Assert.DoesNotContain("SPACE", paragraphs);
        }

        // ---------- renderer contract ----------

        [Fact]
        public void Renderer_ImplementsIDocumentRenderer()
        {
            IDocumentRenderer documentRenderer = new PdfRenderer();
            Assert.Equal(".pdf", documentRenderer.FileExtension);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                documentRenderer.Render(ReportingDesignGateFixture.Document("office_SI"), memoryStream);
                Assert.StartsWith("%PDF-", Encoding.ASCII.GetString(memoryStream.ToArray(), 0, 5));
                Assert.True(memoryStream.CanWrite, "The renderer must not close the caller's stream.");
            }
        }

        [Fact]
        public void Renderer_RejectsInvalidArguments()
        {
            PdfRenderer pdfRenderer = new PdfRenderer();
            Assert.Throws<ArgumentNullException>(() => pdfRenderer.Render(null!, new MemoryStream()));
            Assert.Throws<ArgumentNullException>(() => pdfRenderer.Render(Minimal(), null!));
            Assert.Throws<ArgumentException>(() => pdfRenderer.Render(Minimal(), new MemoryStream(new byte[16], false)));
        }

        [Fact]
        public void Renderer_EmbedsNotoSans()
        {
            PdfDocument pdfDocument = Open(new PdfRenderer().Render(ReportingDesignGateFixture.Document("office_SI")));
            PdfDictionary? fonts = pdfDocument.Pages[0].Resources.Elements.GetDictionary("/Font");
            Assert.NotNull(fonts);
            List<string> baseFonts = fonts.Elements.Values.Select(x => ((PdfDictionary)((PdfReference)x!).Value).Elements.GetName("/BaseFont")).ToList();

            Assert.NotEmpty(baseFonts);
            Assert.All(baseFonts, x => Assert.Contains("Noto Sans", x));
        }

        [Fact]
        public void Renderer_CompanyLogoIsOptional()
        {
            AnalyticalModelCase(out Analytical.AnalyticalModel analyticalModel);

            Document withLogo = ReportingDesignGateFixture.Document(analyticalModel, ReportingDesignGateFixture.OfficeName, UnitStyle.SI, new DocumentStyle(companyLogoPng: Png(240, 80)));
            Document withoutMark = ReportingDesignGateFixture.Document(analyticalModel, ReportingDesignGateFixture.OfficeName, UnitStyle.SI, new DocumentStyle(showSamLogo: false));

            byte[] bytes = new PdfRenderer().Render(withLogo);
            Save("SpaceAssumptions_office_SI_logo.pdf", bytes);
            PdfDocument pdfDocument = Open(bytes);
            Assert.Equal(1, pdfDocument.PageCount);
            Assert.Single(Objects(withLogo).OfType<Image>());

            Assert.Equal(1, Open(new PdfRenderer().Render(withoutMark)).PageCount);
            Assert.DoesNotContain("SAM", Paragraphs(withoutMark));
            Assert.Contains("SAM", Paragraphs(withLogo));
        }

        [Fact]
        public void Renderer_RejectsALogoThatIsNotPng()
        {
            Document document = new Document("test", Metadata(), null!, null!, new DocumentStyle(companyLogoPng: new byte[] { 1, 2, 3, 4 }));
            Assert.Throws<ArgumentException>(() => new PdfRenderer().Render(document));
        }

        // ---------- generic blocks and layout ----------

        [Fact]
        public void Renderer_RendersEveryBlockType()
        {
            FormattedValue number = new FormattedValue("12.5", "m²", Availability.Available);
            FormattedValue missing = new FormattedValue("—", null, Availability.NotAvailable);
            FormattedValue outOfDate = new FormattedValue("3.2", "kW", Availability.Available, Freshness.OutOfDate);

            Document document = new Document("test", Metadata(), new[]
            {
                new DocumentSection(PdfRenderer.HeaderSectionId, "Identity", new DocumentBlock[] { new KeyValueBlock("identity", null, new[] { new KeyValueRow("Name", FormattedValue.Label("Subject")), new KeyValueRow("Level", FormattedValue.Label("L1")) }) }),
                new DocumentSection("kv", "Key values", new DocumentBlock[] { new KeyValueBlock("kv", "Block title", new[] { new KeyValueRow("Area", number, "sub label"), new KeyValueRow("Missing", missing), new KeyValueRow("Stale", outOfDate), new KeyValueRow("Name", FormattedValue.Label("A name")) }) }, SectionWidth.Half),
                new DocumentSection("table", "Table", new DocumentBlock[]
                {
                    new TableBlock("grouped", "Grouped", new[] { new TableColumn("Item", alignment: ColumnAlignment.Left), new TableColumn("A", group: "Group"), new TableColumn("B", "m²", group: "Group"), new TableColumn("C", alignment: ColumnAlignment.Center) },
                        new[] { new TableRow(FormattedValue.Label("Row"), number, number, FormattedValue.Label("x")) }),
                }, SectionWidth.Half),
                new DocumentSection("notices", "Notices", new DocumentBlock[]
                {
                    new NoticeBlock("info", "Information notice"),
                    new NoticeBlock("warning", "Warning notice", NoticeLevel.Warning),
                    new NoticeBlock("note", "A note", NoticeLevel.Note),
                    new TextBlock("text", "A paragraph of text."),
                    new ImageBlock("image", Png(400, 200), "An image caption", 0.5),
                    new ImageBlock("no-image", null!, "Caption without an image"),
                }),
                new DocumentSection("empty", "Empty section", null!),
                new DocumentSection("empty-blocks", "Empty blocks", new DocumentBlock[] { new KeyValueBlock("kv-empty", null!, null!), new TableBlock("table-empty", null!, null!, null!) }, SectionWidth.Half),
            }, new DocumentFooter(new[] { "Footer line" }, new[] { "— not available", "† out of date" }));

            byte[] bytes = new PdfRenderer().Render(document);
            Save("BlockTypes.pdf", bytes);
            PdfDocument pdfDocument = Open(bytes);
            Assert.True(pdfDocument.PageCount >= 1);

            List<string> paragraphs = Paragraphs(document);
            foreach (string expected in new[] { "12.5", "m²", "—", "3.2 †", "A name", "sub label", "BLOCK TITLE", "Group", "B (m²)", "Information notice", "Warning notice", "A note", "A paragraph of text.", "An image caption", "Caption without an image", "EMPTY SECTION", "Footer line", "† out of date" })
            {
                Assert.Contains(expected, paragraphs);
            }

            Assert.Contains(paragraphs, x => x.Contains("Level: L1"));
            Assert.Single(Objects(document).OfType<Image>());
        }

        [Fact]
        public void Renderer_LongContentContinuesOnMorePagesWithoutClipping()
        {
            List<TableRow> tableRows = Enumerable.Range(1, 150).Select(x => new TableRow(FormattedValue.Label("Row " + x), new FormattedValue(x.ToString(), "m²", Availability.Available))).ToList();
            List<KeyValueRow> keyValueRows = Enumerable.Range(1, 90).Select(x => new KeyValueRow("Long key " + x, new FormattedValue(x.ToString(), "W", Availability.Available))).ToList();

            Document document = new Document("test", Metadata(), new[]
            {
                new DocumentSection("left", "Left", new DocumentBlock[] { new KeyValueBlock("left", null, keyValueRows) }, SectionWidth.Half),
                new DocumentSection("right", "Right", new DocumentBlock[] { new KeyValueBlock("right", null, keyValueRows.Take(3)) }, SectionWidth.Half),
                new DocumentSection("long", "Long table", new DocumentBlock[] { new TableBlock("long", null, new[] { new TableColumn("Element", alignment: ColumnAlignment.Left), new TableColumn("Area", "m²") }, tableRows) }),
            }, new DocumentFooter(new[] { "Footer" }));

            byte[] bytes = new PdfRenderer().Render(document);
            Save("LongContent.pdf", bytes);
            PdfDocument pdfDocument = Open(bytes);
            Assert.True(pdfDocument.PageCount > 2, pdfDocument.PageCount.ToString());
            Assert.All(pdfDocument.Pages.Cast<PdfPage>(), AssertA4);

            // A half section too tall to set side by side as one unit is stacked in the page flow, so it can break
            // across pages: no text frame holds its rows.
            MigraDocument migraDocument = MigraDocBuilder.Build(document);
            List<TextFrame> textFrames = Objects(migraDocument).OfType<TextFrame>().ToList();
            Assert.DoesNotContain(textFrames, x => Objects(x).OfType<Paragraph>().Any(y => ParagraphText(y) == "Long key 90"));
            Assert.Contains("Long key 90", Paragraphs(document));

            // Every row of the long table is present, with its header marked to repeat on each page.
            Table table = Objects(migraDocument).OfType<Table>().Single(x => x.Rows.Count == 151);
            Assert.True(table.Rows[0].HeadingFormat);
        }

        [Fact]
        public void Renderer_SetsShortHalfSectionsSideBySide()
        {
            Document document = ReportingDesignGateFixture.Document("office_SI");
            MigraDocument migraDocument = MigraDocBuilder.Build(document);

            // Geometry | Design criteria, Ventilation | Systems, Fabric | Sizing, and the three internal-condition
            // key/value blocks: nine frames, plus the SAM mark.
            List<TextFrame> textFrames = Objects(migraDocument).OfType<TextFrame>().ToList();
            Assert.Equal(10, textFrames.Count);
        }

        // ---------- long unbroken text ----------

        private const string LongUnderscoreName = "S12_Reception_Atrium_NCM_CirculationArea_Level02_ZoneB_NorthWing_DoubleHeight_Extended";

        /// <summary>
        /// 120 characters with no space and no separator: only the character-level fallback can break it.
        /// </summary>
        private static readonly string LongUnbrokenToken = string.Concat(Enumerable.Repeat("ABCDEFGHIJ", 12));

        /// <summary>
        /// A document that puts both long tokens everywhere text can go: the header band, key/value labels and
        /// values in a half section (a side-by-side frame), a table cell, a notice, a note, a text paragraph and
        /// the footer.
        /// </summary>
        private static Document LongTokenDocument()
        {
            DocumentMetadata documentMetadata = Metadata();
            documentMetadata.Subject = LongUnderscoreName;

            return new Document("test", documentMetadata, new[]
            {
                new DocumentSection(PdfRenderer.HeaderSectionId, "Identity", new DocumentBlock[] { new KeyValueBlock("identity", null, new[] { new KeyValueRow("Internal condition", FormattedValue.Label(LongUnbrokenToken)) }) }),
                new DocumentSection("left", "Left", new DocumentBlock[]
                {
                    new KeyValueBlock("kv", null, new[]
                    {
                        new KeyValueRow("System", FormattedValue.Label(LongUnderscoreName)),
                        new KeyValueRow(LongUnbrokenToken, new FormattedValue("12.5", "m²", Availability.Available)),
                        new KeyValueRow("Riser", FormattedValue.Label(LongUnbrokenToken)),
                    }),
                }, SectionWidth.Half),
                new DocumentSection("right", "Right", new DocumentBlock[]
                {
                    new NoticeBlock("notice", "Notice about " + LongUnbrokenToken),
                    new NoticeBlock("note", "Not present: " + LongUnderscoreName, NoticeLevel.Note),
                }, SectionWidth.Half),
                new DocumentSection("table", "Table", new DocumentBlock[]
                {
                    new TableBlock("table", null, new[] { new TableColumn("Element", alignment: ColumnAlignment.Left), new TableColumn("Profile", alignment: ColumnAlignment.Left), new TableColumn("Area", "m²") }, new[]
                    {
                        new TableRow(FormattedValue.Label("Walls"), FormattedValue.Label(LongUnderscoreName), new FormattedValue("50.9", "m²", Availability.Available)),
                        new TableRow(FormattedValue.Label(LongUnbrokenToken), FormattedValue.Label("Short"), new FormattedValue("5.3", "m²", Availability.Available)),
                    }),
                    new TextBlock("text", "Before " + LongUnbrokenToken + LongUnbrokenToken + " after."),
                }),
            }, new DocumentFooter(new[] { "Footer " + LongUnderscoreName }));
        }

        /// <summary>
        /// Long tokens are broken so that no line escapes its column or frame, and the full text is kept: the
        /// paragraph text (line breaks removed) is exactly the supplied text.
        /// </summary>
        [Fact]
        public void LongTokens_WrapWithinTheirColumns_AndKeepTheFullText()
        {
            Document document = LongTokenDocument();
            MigraDocument migraDocument = MigraDocBuilder.Build(document);

            AssertNoLineEscapes(migraDocument);

            List<string> paragraphs = Paragraphs(document);
            foreach (string expected in new[] { LongUnderscoreName, LongUnbrokenToken, "Notice about " + LongUnbrokenToken, "Not present: " + LongUnderscoreName, "Before " + LongUnbrokenToken + LongUnbrokenToken + " after.", "Footer " + LongUnderscoreName })
            {
                Assert.Contains(expected, paragraphs);
            }

            byte[] bytes = new PdfRenderer().Render(document);
            Save("LongTokens.pdf", bytes);
            PdfDocument pdfDocument = Open(bytes);
            Assert.All(pdfDocument.Pages.Cast<PdfPage>(), AssertA4);
        }

        /// <summary>
        /// An underscore-delimited name breaks after an underscore, never inside a part; a token with no separator
        /// is broken by the character fallback, and its pieces join back to the whole token.
        /// </summary>
        [Fact]
        public void LongTokens_BreakAfterSeparators_ElseByCharacter()
        {
            MigraDocument migraDocument = MigraDocBuilder.Build(LongTokenDocument());
            List<Paragraph> paragraphs = Objects(migraDocument).OfType<Paragraph>().ToList();

            List<List<string>> underscore = paragraphs.Where(x => ParagraphText(x) == LongUnderscoreName).Select(Lines).ToList();
            Assert.NotEmpty(underscore);
            Assert.Contains(underscore, x => x.Count > 1);
            Assert.All(underscore, x => Assert.All(x.Take(x.Count - 1), y => Assert.EndsWith("_", y)));

            List<List<string>> unbroken = paragraphs.Where(x => ParagraphText(x) == LongUnbrokenToken).Select(Lines).ToList();
            Assert.NotEmpty(unbroken);
            Assert.All(unbroken, x => Assert.True(x.Count > 1));
            Assert.All(unbroken, x => Assert.Equal(LongUnbrokenToken, string.Concat(x)));
        }

        /// <summary>
        /// Normal documents are unchanged: every design-gate text fits, so no line break is inserted anywhere, and
        /// nothing escapes its column.
        /// </summary>
        [Theory]
        [MemberData(nameof(DesignGateCases))]
        public void DesignGate_NoForcedBreaks_AndNothingEscapes(string name)
        {
            MigraDocument migraDocument = MigraDocBuilder.Build(ReportingDesignGateFixture.Document(name));

            AssertNoLineEscapes(migraDocument);
            Assert.All(Objects(migraDocument).OfType<Paragraph>(), x => Assert.True(Lines(x).Count == 1, string.Join(" | ", Lines(x))));
        }

        /// <summary>
        /// Every word of every line (MigraDoc breaks only at spaces and forced breaks) fits the width of the cell,
        /// frame or page column that holds its paragraph, measured with the paragraph's own font.
        /// </summary>
        private static void AssertNoLineEscapes(MigraDocument migraDocument)
        {
            NotoSansFontResolver.Register();
            XGraphics xGraphics = XGraphics.CreateMeasureContext(new XSize(2000, 2000), XGraphicsUnit.Point, XPageDirection.Downwards);

            int checkedWords = 0;
            foreach ((Paragraph paragraph, double width, double size, bool bold) in Placed(migraDocument))
            {
                XFont xFont = new XFont(NotoSansFontResolver.FamilyName, size, bold ? XFontStyleEx.Bold : XFontStyleEx.Regular);
                double available = width - paragraph.Format.LeftIndent.Millimeter - paragraph.Format.RightIndent.Millimeter;
                foreach (string word in Lines(paragraph).SelectMany(x => x.Split(' ')).Where(x => x.Length != 0))
                {
                    double wordWidth = xGraphics.MeasureString(word, xFont).Width * 25.4 / 72.0;
                    Assert.True(wordWidth <= available + 0.01, string.Format("\"{0}\" is {1:0.00} mm wide in {2:0.00} mm", word, wordWidth, available));
                    checkedWords++;
                }
            }

            Assert.True(checkedWords > 0);
        }

        /// <summary>
        /// Each paragraph with the width it is laid out in (mm) and its effective font size (pt) and weight.
        /// </summary>
        private static IEnumerable<(Paragraph Paragraph, double Width, double Size, bool Bold)> Placed(MigraDocument migraDocument)
        {
            const double contentWidth = 180;
            foreach (Section section in migraDocument.Sections.OfType<Section>())
            {
                foreach (DocumentElements documentElements in new[] { section.Headers.Primary.Elements, section.Headers.FirstPage.Elements, section.Footers.Primary.Elements, section.Footers.FirstPage.Elements, section.Elements })
                {
                    foreach ((Paragraph, double, double, bool) placed in Placed(documentElements, contentWidth, 9, false))
                    {
                        yield return placed;
                    }
                }
            }
        }

        private static IEnumerable<(Paragraph Paragraph, double Width, double Size, bool Bold)> Placed(DocumentElements documentElements, double width, double size, bool bold)
        {
            foreach (DocumentObject documentObject in documentElements.OfType<DocumentObject>())
            {
                switch (documentObject)
                {
                    case Paragraph paragraph:
                        yield return (paragraph, width, Size(paragraph.Format, size), Bold(paragraph.Format, bold));
                        break;

                    case TextFrame textFrame:
                        foreach ((Paragraph, double, double, bool) placed in Placed(textFrame.Elements, FrameWidth(textFrame), size, bold))
                        {
                            yield return placed;
                        }

                        break;

                    case Table table:
                        double table_Size = Size(table.Format, size);
                        bool table_Bold = Bold(table.Format, bold);
                        foreach (Row row in table.Rows.OfType<Row>())
                        {
                            double row_Size = Size(row.Format, table_Size);
                            bool row_Bold = Bold(row.Format, table_Bold);
                            foreach (Cell cell in row.Cells.OfType<Cell>())
                            {
                                double cell_Width = Enumerable.Range(cell.Column!.Index, cell.MergeRight + 1).Sum(x => table.Columns[x]!.Width.Millimeter) - table.LeftPadding.Millimeter - table.RightPadding.Millimeter;
                                foreach (DocumentObject child in cell.Elements.OfType<DocumentObject>())
                                {
                                    if (child is Paragraph paragraph)
                                    {
                                        yield return (paragraph, cell_Width, Size(paragraph.Format, Size(cell.Format, row_Size)), Bold(paragraph.Format, Bold(cell.Format, row_Bold)));
                                    }
                                    else if (child is TextFrame textFrame)
                                    {
                                        foreach ((Paragraph, double, double, bool) placed in Placed(textFrame.Elements, FrameWidth(textFrame), row_Size, row_Bold))
                                        {
                                            yield return placed;
                                        }
                                    }
                                }
                            }
                        }

                        break;
                }
            }
        }

        private static double FrameWidth(TextFrame textFrame)
        {
            return textFrame.Width.Millimeter - textFrame.MarginLeft.Millimeter - textFrame.MarginRight.Millimeter;
        }

        private static double Size(ParagraphFormat paragraphFormat, double inherited)
        {
            Unit? unit = paragraphFormat.Font.Values.Size;
            return unit.HasValue && !unit.Value.IsEmpty ? unit.Value.Point : inherited;
        }

        private static bool Bold(ParagraphFormat paragraphFormat, bool inherited)
        {
            return paragraphFormat.Font.Values.Bold ?? inherited;
        }

        /// <summary>
        /// The lines of a paragraph as set by forced line breaks.
        /// </summary>
        private static List<string> Lines(Paragraph paragraph)
        {
            List<string> result = new List<string>() { string.Empty };
            foreach (DocumentObject documentObject in paragraph.Elements.OfType<DocumentObject>())
            {
                if (documentObject is Character character && character.SymbolName == SymbolName.LineBreak)
                {
                    result.Add(string.Empty);
                    continue;
                }

                StringBuilder stringBuilder = new StringBuilder();
                AppendText(stringBuilder, new[] { documentObject });
                result[result.Count - 1] += stringBuilder.ToString();
            }

            return result;
        }

        // ---------- helpers ----------

        /// <summary>
        /// Keeps a rendered PDF for visual review: in ReportingPdf/ under the test output, or in SAM_REPORTING_PDF_DIR.
        /// </summary>
        private void Save(string fileName, byte[] bytes)
        {
            string? directory = Environment.GetEnvironmentVariable("SAM_REPORTING_PDF_DIR");
            directory = string.IsNullOrWhiteSpace(directory) ? Path.Combine(AppContext.BaseDirectory, "ReportingPdf") : directory;
            Directory.CreateDirectory(directory);

            string path = Path.Combine(directory, fileName);
            File.WriteAllBytes(path, bytes);
            output.WriteLine(path);
        }

        private static void AnalyticalModelCase(out Analytical.AnalyticalModel analyticalModel)
        {
            analyticalModel = ReportingDesignGateFixture.Office();
        }

        private static DocumentMetadata Metadata()
        {
            return new DocumentMetadata() { Title = "Test Document", Subject = "Subject", ProjectName = "Project", Date = new DateTime(2026, 9, 25) };
        }

        private static Document Minimal()
        {
            return new Document("test", Metadata(), null!, null!);
        }

        private static PdfDocument Open(byte[] bytes)
        {
            return PdfReader.Open(new MemoryStream(bytes), PdfDocumentOpenMode.Import);
        }

        private static void AssertA4(PdfPage pdfPage)
        {
            Assert.Equal(A4Width, pdfPage.Width.Point, 0.5);
            Assert.Equal(A4Height, pdfPage.Height.Point, 0.5);
        }

        /// <summary>
        /// A valid PNG of the given size (one grey pixel row repeated), built without an imaging library.
        /// </summary>
        private static byte[] Png(int width, int height)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                memoryStream.Write(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, 0, 8);

                byte[] header = new byte[13];
                WriteInt(header, 0, width);
                WriteInt(header, 4, height);
                header[8] = 8; // bit depth
                header[9] = 0; // greyscale
                Chunk(memoryStream, "IHDR", header);

                byte[] raw = new byte[height * (width + 1)];
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        raw[(y * (width + 1)) + 1 + x] = (byte)(x * 255 / width);
                    }
                }

                using (MemoryStream compressed = new MemoryStream())
                {
                    using (System.IO.Compression.ZLibStream zLibStream = new System.IO.Compression.ZLibStream(compressed, System.IO.Compression.CompressionLevel.Optimal, true))
                    {
                        zLibStream.Write(raw, 0, raw.Length);
                    }

                    Chunk(memoryStream, "IDAT", compressed.ToArray());
                }

                Chunk(memoryStream, "IEND", new byte[0]);
                return memoryStream.ToArray();
            }
        }

        private static void Chunk(Stream stream, string type, byte[] data)
        {
            byte[] length = new byte[4];
            WriteInt(length, 0, data.Length);
            stream.Write(length, 0, 4);

            byte[] typeAndData = Encoding.ASCII.GetBytes(type).Concat(data).ToArray();
            stream.Write(typeAndData, 0, typeAndData.Length);

            byte[] crc = new byte[4];
            WriteInt(crc, 0, (int)Crc32(typeAndData));
            stream.Write(crc, 0, 4);
        }

        private static uint Crc32(byte[] bytes)
        {
            uint crc = 0xFFFFFFFF;
            foreach (byte b in bytes)
            {
                crc ^= b;
                for (int k = 0; k < 8; k++)
                {
                    crc = (crc & 1) != 0 ? 0xEDB88320 ^ (crc >> 1) : crc >> 1;
                }
            }

            return crc ^ 0xFFFFFFFF;
        }

        private static void WriteInt(byte[] bytes, int offset, int value)
        {
            bytes[offset] = (byte)(value >> 24);
            bytes[offset + 1] = (byte)(value >> 16);
            bytes[offset + 2] = (byte)(value >> 8);
            bytes[offset + 3] = (byte)value;
        }

        private static string Text(Document document)
        {
            return string.Join("\n", Paragraphs(document));
        }

        /// <summary>
        /// Text of every paragraph the renderer builds: body, header band, frames, table cells, headers and footers.
        /// </summary>
        private static List<string> Paragraphs(Document document)
        {
            return Objects(MigraDocBuilder.Build(document)).OfType<Paragraph>().Select(ParagraphText).Where(x => x.Length != 0).ToList();
        }

        private static IEnumerable<DocumentObject> Objects(Document document)
        {
            return Objects(MigraDocBuilder.Build(document));
        }

        private static IEnumerable<DocumentObject> Objects(DocumentObject? documentObject)
        {
            if (documentObject == null)
            {
                yield break;
            }

            yield return documentObject;

            IEnumerable<DocumentObject> children = Enumerable.Empty<DocumentObject>();
            switch (documentObject)
            {
                case MigraDocument migraDocument:
                    children = migraDocument.Sections.Cast<DocumentObject>();
                    break;

                case Section section:
                    children = new DocumentObject[] { section.Headers.Primary, section.Headers.FirstPage, section.Footers.Primary, section.Footers.FirstPage }.Concat(section.Elements.Cast<DocumentObject>());
                    break;

                case HeaderFooter headerFooter:
                    children = headerFooter.Elements.Cast<DocumentObject>();
                    break;

                case Table table:
                    children = table.Rows.Cast<Row>().SelectMany(x => x.Cells.Cast<Cell>()).SelectMany(x => x.Elements.Cast<DocumentObject>());
                    break;

                case TextFrame textFrame:
                    children = textFrame.Elements.Cast<DocumentObject>();
                    break;

                case Paragraph paragraph:
                    children = paragraph.Elements.Cast<DocumentObject>().Where(x => x is Image);
                    break;
            }

            foreach (DocumentObject child in children)
            {
                foreach (DocumentObject descendant in Objects(child))
                {
                    yield return descendant;
                }
            }
        }

        private static string ParagraphText(Paragraph paragraph)
        {
            StringBuilder stringBuilder = new StringBuilder();
            AppendText(stringBuilder, paragraph.Elements.Cast<DocumentObject>());
            return stringBuilder.ToString();
        }

        private static void AppendText(StringBuilder stringBuilder, IEnumerable<DocumentObject> paragraphElements)
        {
            foreach (DocumentObject documentObject in paragraphElements)
            {
                switch (documentObject)
                {
                    case Text text:
                        stringBuilder.Append(text.Content);
                        break;

                    case FormattedText formattedText:
                        AppendText(stringBuilder, formattedText.Elements.Cast<DocumentObject>());
                        break;

                    case Character character when character.SymbolName == SymbolName.LineBreak:
                        break;

                    case Character character:
                        stringBuilder.Append(character.SymbolName == SymbolName.Blank ? " " : character.Char.ToString());
                        break;

                    case PageField _:
                    case NumPagesField _:
                        stringBuilder.Append('#');
                        break;
                }
            }
        }
    }
}
