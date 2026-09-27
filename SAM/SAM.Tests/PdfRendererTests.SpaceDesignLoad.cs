// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using PdfSharp.Pdf;
using SAM.Core.Reporting;
using SAM.Core.Reporting.Pdf;
using SAM.Tests.Helpers;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Document = SAM.Core.Reporting.Document;
using MigraDocument = MigraDoc.DocumentObjectModel.Document;

namespace SAM.Tests
{
    /// <summary>
    /// The Space Design Load Summary through the production PDF renderer (PR2C visual gate): every case renders to
    /// valid A4 pages with natural pagination, prints every formatted value as supplied, and keeps every word inside its
    /// column. The PDFs are written to ReportingPdf/ (or SAM_REPORTING_PDF_DIR) for visual review.
    /// </summary>
    public partial class PdfRendererTests
    {
        public static IEnumerable<object[]> SpaceDesignLoadCases()
        {
            return SpaceDesignLoadFixture.Cases().Select(x => new object[] { x.Name });
        }

        [Theory]
        [MemberData(nameof(SpaceDesignLoadCases))]
        public void SpaceDesignLoad_RendersA4Pages(string name)
        {
            byte[] bytes = new PdfRenderer().Render(SpaceDesignLoadFixture.Document(name));
            Save(string.Format("SpaceDesignLoad_{0}.pdf", name), bytes);

            PdfDocument pdfDocument = Open(bytes);
            output.WriteLine("{0}: {1} page(s)", name, pdfDocument.PageCount);
            Assert.Equal(ExpectedPages(name), pdfDocument.PageCount);
            Assert.All(pdfDocument.Pages.Cast<PdfPage>(), AssertA4);
        }

        [Theory]
        [MemberData(nameof(SpaceDesignLoadCases))]
        public void SpaceDesignLoad_PrintsEveryFormattedValueVerbatim(string name)
        {
            Document document = SpaceDesignLoadFixture.Document(name);
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

        [Theory]
        [MemberData(nameof(SpaceDesignLoadCases))]
        public void SpaceDesignLoad_NothingEscapesItsColumn(string name)
        {
            MigraDocument migraDocument = MigraDocBuilder.Build(SpaceDesignLoadFixture.Document(name));

            AssertNoLineEscapes(migraDocument);
        }

        /// <summary>
        /// An untitled key/value block that introduces a table is placed with it as one unit (a text frame), so a
        /// page break cannot leave it alone at the foot of a page; a titled one, or one not followed by a table, is not.
        /// </summary>
        [Fact]
        public void Renderer_KeepsAnUntitledKeyValueLeadWithItsTable()
        {
            KeyValueBlock lead = new KeyValueBlock("lead", null, new[] { new KeyValueRow("Lead key", FormattedValue.Label("Lead value")) });
            TableBlock tableBlock = new TableBlock("table", null, new[] { new TableColumn("Item", alignment: ColumnAlignment.Left), new TableColumn("Value") }, new[] { new TableRow(FormattedValue.Label("Row 1"), FormattedValue.Label("1")) });
            KeyValueBlock titled = new KeyValueBlock("titled", "Titled", new[] { new KeyValueRow("Titled key", FormattedValue.Label("Titled value")) });
            KeyValueBlock last = new KeyValueBlock("last", null, new[] { new KeyValueRow("Last key", FormattedValue.Label("Last value")) });

            Document document = new Document("test", Metadata(), new[]
            {
                new DocumentSection("kept", "Kept", new DocumentBlock[] { lead, tableBlock }),
                new DocumentSection("not-kept", "Not kept", new DocumentBlock[] { titled, tableBlock, last }),
            }, null);

            List<TextFrame> textFrames = Objects(MigraDocBuilder.Build(document)).OfType<TextFrame>().Where(x => !Objects(x).OfType<Paragraph>().Any(y => ParagraphText(y) == "SAM")).ToList();
            TextFrame textFrame = Assert.Single(textFrames);
            List<string> texts = Objects(textFrame).OfType<Paragraph>().Select(ParagraphText).ToList();
            Assert.Contains("Lead key", texts);
            Assert.Contains("Row 1", texts);
            Assert.DoesNotContain("Titled key", texts);
            Assert.DoesNotContain("Last key", texts);

            Assert.Equal(1, Open(new PdfRenderer().Render(document)).PageCount);
        }

        /// <summary>
        /// A lead and table too tall to place as one unit stay in the page flow, so the table breaks across pages.
        /// </summary>
        [Fact]
        public void Renderer_LongTableAfterALead_StillBreaksAcrossPages()
        {
            KeyValueBlock lead = new KeyValueBlock("lead", null, new[] { new KeyValueRow("Lead key", FormattedValue.Label("Lead value")) });
            TableBlock tableBlock = new TableBlock("table", null, new[] { new TableColumn("Item", alignment: ColumnAlignment.Left), new TableColumn("Value") },
                Enumerable.Range(0, 150).Select(x => new TableRow(FormattedValue.Label("Row " + x), FormattedValue.Label(x.ToString()))));

            Document document = new Document("test", Metadata(), new[] { new DocumentSection("long", "Long", new DocumentBlock[] { lead, tableBlock }) }, null);

            Assert.DoesNotContain(Objects(MigraDocBuilder.Build(document)).OfType<TextFrame>(), x => Objects(x).OfType<Paragraph>().Any(y => ParagraphText(y) == "Row 90"));
            Assert.True(Open(new PdfRenderer().Render(document)).PageCount > 1);
        }

        /// <summary>
        /// The accepted page counts of the visual gate: one page for a space with at most one populated load type
        /// (Bathroom_2, and every notice case); two when both heating and cooling carry full peaks and breakdowns
        /// (Studio 1_0, and the invented stress case), cooling then starting page 2 whole.
        /// </summary>
        private static int ExpectedPages(string name)
        {
            return name.StartsWith("B_") || name.StartsWith("G_") ? 2 : 1;
        }
    }
}
