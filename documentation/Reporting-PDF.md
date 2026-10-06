# Reporting: the PDF renderer (`SAM.Core.Reporting.Pdf`)

Status: PR2 of the *SAM Documentation Framework* (SAM#141). The renderer implements the approved v2 layout in
[Reporting-LAYOUT.md](Reporting-LAYOUT.md). SAM_UI calls it (SAM_UI#121); Phase 1 is complete (see below).

## Phase 1 status (closeout 2026-09-26)

```text
SAM Documentation Framework — Phase 1
Status: COMPLETE
```

**Phase 1 = the Space Assumptions PDF.** The production chain is merged and deployed:

```text
AnalyticalModel
    ↓  SAM.Analytical.Reporting (collector)
SpaceDocumentData            typed ReportValue<T> + Quantity
    ↓  builders + QuantityFormatter (SI/IP)
generic Document             SAM.Core.Reporting
    ↓
SAM.Core.Reporting.Pdf       MigraDoc/PDFsharp 6.2.0
    ↓
SAM_UI                       Edit › Reports › Space Assumptions PDF   (SAM_UI#121, 7e7de033)
    ↓
SAM_Deploy / installed       installer payload gate + installed smoke test   (SAM_Deploy#51, 8e6740af)
```

SAM PRs: #135 (SAM.Units), #136 (domain and analytical collector), #139 (occupancy missing-gain correction), #140
(visual polish), #141 (this renderer), #143 (airflow `L/s`, `22f9c743`).

**Delivered.**
- One selected Space gives one PDF; SAM_UI reports SI.
- The default SI airflow symbol is `L/s`. It is defined once, in `QuantityFormatter`.
- Reporting data is typed (`ReportValue<T>`, `Quantity`), and the reporting core can produce SI or IP.
- MigraDoc/PDFsharp renderer: Noto Sans embedded, SAM branding, A4, natural pagination with repeated table headers.
- Missing / zero / `n/a` / `not set` semantics are preserved end to end. The renderer never reinterprets them.
- Production installer packaging: SAM_Deploy's *Assert reporting/PDF payload* gate, and licences
  `licenses\NotoSans\OFL.txt` and `licenses\PDFsharp-MigraDoc\LICENSE.txt`.
- The real installed `SAM Analytical.exe` produced a valid 1-page PDF showing `L/s`. With PdfSharp removed, it fails
  cleanly and writes no file.

**Intentionally outside Phase 1.**
- Space Design Load Summary / simulation results.
- TSD peak load, time and gain breakdown.
- Building Summary.
- HVAC/AHU reports.
- Batch Space reporting.
- HTML and Excel renderers.
- A report preview UI.
- Removing legacy Print RDS.

**Kept visible.**
- [SAM#138](https://github.com/SAM-BIM/SAM/issues/138), **open**, was deliberately kept out of the Phase-1 PRs:
  - RH query naming/mapping;
  - composite `Profile.MinValue`;
  - gain queries returning 0 when nothing is authored.
- Legacy **Print RDS** stays in SAM_UI. Decision: *retain legacy Print RDS until Phase 2 reaches sufficient parity*.
- Unit systems: the core supports SI and IP. SAM_UI exposes SI only, because it has no global unit-system
  preference yet. No IP selector was added.

## Pipeline and responsibility

```text
AnalyticalModel → typed data (SAM.Analytical.Reporting) → ReportValue<T> + Quantity → QuantityFormatter (SI/IP)
    → generic Document blocks (SAM.Core.Reporting) → PdfRenderer (SAM.Core.Reporting.Pdf)
```

The renderer prints the `FormattedValue.Text` and `Unit` it is given. It never converts units, recomputes a value,
reads engineering data, or reinterprets a placeholder:
- `—` means not available;
- `n/a` means not applicable;
- `not set` is text supplied by the builder;
- an explicit `0` is a number.

All of these print exactly as supplied. The same holds for the footer lines, the legend (only the markers the
document actually uses) and notices such as the fabric "Not present (zero area): …" note.

## Project and dependencies
- `SAM/SAM.Core.Reporting.Pdf`: netstandard2.0. It outputs to `build/` and sits in `SAM.sln` under the SAM folder.
- Dependencies are one-way: `SAM.Core.Reporting.Pdf → SAM.Core.Reporting`. The reporting domain has no reference to
  PDFsharp or MigraDoc.
- Package: `PDFsharp-MigraDoc` **6.2.0** (MIT). It is pinned to the PDFsharp 6.2.0 that SAM_Revit already uses
  (`SAM.Core.Pdf.Revit`), so both can load in one process. Its transitive dependencies are PDFsharp 6.2.0,
  Microsoft.Extensions.Logging 8.0.1 and System.Security.Cryptography.Pkcs 8.0.1. This is the cross-platform
  "Core" build: it uses no GDI+ or WPF.
- Public API:
  - `PdfRenderer : IDocumentRenderer`, with `FileExtension` ".pdf".
  - `Render(Document, Stream)` leaves the stream open.
  - `Render(Document)` returns the bytes.
  - `PdfRenderer.HeaderSectionId` is "identity".
  - `NotoSansFontResolver`.
- Internal classes:
  - `MigraDocBuilder` turns a `Document` into a MigraDoc document. The tests inspect its output.
  - `PdfLayout` holds the page geometry, type sizes and colours.
  - `TextMeasure` measures text for column sizing.

## Layout rules (generic; no engineering logic)
The renderer knows no section or field meanings. It follows the hints already in the `Document`:

| Hint | Layout |
|---|---|
| Section id `identity` (`PdfRenderer.HeaderSectionId`) | Printed in the header band under the title and subject, as "Label: value · …". The row that repeats the subject is left out. |
| Consecutive `SectionWidth.Half` sections | Set side by side, two to a row, with a 6 mm gap. A lone half section keeps its half. |
| `SectionWidth.Full` | Full width. Within it, consecutive **titled** `KeyValueBlock`s are set side by side, up to three, each width in proportion to its content. |
| `KeyValueBlock` | Label, then value (right-aligned), then unit (a fixed column of 15 mm, or sized to the widest unit when side by side). Numbers align down a block. A text value spans the value and unit columns. `SubLabel` is printed under the label in 7 pt grey. |
| `TableBlock` | Columns are sized to their content. Spare width goes to the left-aligned (text) columns. A unit shared by the whole column (`TableColumn.Unit`) is printed once in the header ("External (m²)"). A right-aligned column whose cells carry their own units is split into value and unit sub-columns. `Group` gives a merged heading row. `RepeatHeader` repeats the header rows on each page. |
| `NoticeBlock` | `Information` is a light callout with an accent bar. `Warning` is an amber callout. `Note` (new in PR2) is a small grey line, for example the fabric "Not present" note. |
| `TextBlock` | A 9 pt paragraph. |
| `ImageBlock` | The PNG at `WidthFraction` of the available width, capped at 80 % of the body height, plus a grey caption. A block with no image prints its caption only. |
| Missing or not-applicable values | Printed in grey, text unchanged. `FormattedValue.IsOutOfDate` appends " †". |

To make these rules give the v2 grid, the Space Assumptions definition (`SAM.Analytical.Reporting`) was adjusted in
PR2. No value changed; the goldens were verified semantically:
- the section order is identity, geometry, design criteria, internal condition, ventilation, systems, fabric, sizing;
- fabric and sizing are `Half`;
- the internal-condition blocks are ordered gains table, occupancy, lighting, infiltration;
- the fabric note uses `NoticeLevel.Note`.

## Page, header, footer
- A4 portrait (210 × 297 mm).
- Margins: 15 mm on the left, right and top; 26 mm at the bottom, which includes the footer.
- **Header band (page 1)**, left to right:
  - the SAM mark;
  - the title (16 pt bold, capitals), the subject (13 pt bold) and the identity line;
  - on the right: the optional company logo, the project name, "Project no.", "Prepared by" and the date (ISO).
  - A 1 pt accent rule runs under the band.
- **Pages 2+:** a small running header, "Title · Subject".
- **Footer (every page):** the `DocumentFooter.Lines` on the left; on the right, the `Legend` then "Page n / N".
  Both are 7 pt grey.

## Fonts
- Noto Sans Regular and Bold (SIL OFL 1.1) are embedded as resources in the assembly. They were downloaded from the
  official Noto repository; see `THIRD_PARTY.md` for the source and hashes. Italic is simulated.
- `NotoSansFontResolver` is installed as PDFsharp's global font resolver on first render. Output therefore never
  depends on the fonts installed on the machine, and no machine-specific font paths are used.
- A family other than Noto Sans goes to PDFsharp's platform resolver, so other PDFsharp code in the process keeps its
  fonts. If the platform cannot supply the family, Noto Sans is used. This matters because MigraDoc asks for Courier New,
  its predefined error font, on every render.
- PDFsharp allows **one** global resolver per process, set before the first font is used. If another resolver is
  already installed, it is kept when it can resolve "Noto Sans"; otherwise `Render` throws a clear
  `InvalidOperationException`. Only SAM_Revit uses PDFsharp today, and it installs no resolver.
- PDFs embed a subset of each font; the OFL permits this. SAM_UI ships `OFL.txt` with the DLL, and SAM_Deploy's
  payload gate checks for it.

## Images and branding
- **SAM mark:** drawn as vector shapes, a 13 mm accent square with "SAM" in white bold. It is crisp at any zoom and
  needs no asset. `DocumentStyle.ShowSamLogo = false` removes it.
- **Company logo:** `DocumentStyle.CompanyLogoPng`, optional. It is fitted into 45 × 14 mm with its aspect ratio kept.
  The layout is unchanged without it. PNG data is required; other data throws `ArgumentException`.
- No Tas or EDSL branding. The renderer has no dependency on SAM_UI resources.

## Long unbroken text
MigraDoc breaks lines only at spaces, so on its own a token wider than its column would run past the column edge.
Examples are a long underscore-delimited name, a path, or a run of characters with no break. Every text the renderer
prints therefore goes through one generic step (`MigraDocBuilder.AddWrappedText`):
- each space-delimited token is measured with the paragraph's font against the width it is printed in (a cell, a
  merged cell, a frame or the page column);
- a token that fits is left alone, so normal text is added exactly as before;
- a token that does not fit is split into pieces that fit (with a 0.3 mm margin). A piece ends preferably after
  `_ - / \ . , ; : ) ] } | + &`, the separator staying on the first line; with no separator it ends after the last
  character that fits;
- a forced line break is placed between the pieces.

No character is removed or added, no hyphen is inserted, and the font size is unchanged. The text read back from the
paragraph, with the line breaks removed, is exactly the supplied text.

## Pagination
- Content flows naturally onto further pages. Tables break between rows, and their headers repeat.
- A side-by-side row (a half-section pair, or the three-column block) is placed as one unit, each part in a frame of
  its measured height. MigraDoc cannot break a frame across pages, so when the tallest part exceeds **half the body
  height** the parts are **stacked** at full width in the normal flow instead. That way long content paginates and is
  never clipped.
- Text is never shrunk and content is never truncated to hold one page. One page is the design target for normal
  spaces, not an invariant.

## Tests (`SAM.Tests/PdfRendererTests.cs`)
The fixture is `SAM.Tests/Helpers/ReportingDesignGateFixture.cs`. It holds the design-gate models, built with the SAM
API and with fixed GUIDs, and gives the office in SI and IP, the atrium in SI and the sparse store in SI.
- The four design-gate cases render to **one A4 page**. This is a regression guard for the design target.
- Every `FormattedValue.Text`, footer line and legend entry is printed. Text is read from the MigraDoc model the
  renderer builds; PDFsharp has no text extraction.
- The Imperial output contains no SI unit symbol.
- `n/a`, `not set`, `0` and `—` stay distinct. The humidity sub-labels are present. The legend lists only the markers
  used.
- The identity line is in the header band. The fabric note is printed as supplied, without a unit.
- The renderer contract is checked: a valid `%PDF`; the caller's stream is left open; argument checks; the embedded
  fonts are all Noto Sans.
- The company logo is optional and the SAM mark can be turned off. A logo that is not a PNG is rejected.
- Every block type renders: key/value with a sub-label, a placeholder and a dagger; a table with a group; the three
  notice levels; text; an image with and without data; an empty section; empty blocks.
- Long content runs to several A4 pages. An over-tall half pair is stacked rather than framed. A long table keeps
  every row, and its header repeats.
- The office sets its short half sections side by side.
- Long tokens:
  - a long underscore name and a 120-character unbroken token are placed in the header band, labels, values, a
    table cell, a notice, a note, text and the footer;
  - every word of every line fits its cell, frame or page width, measured with the paragraph's own font;
  - the full text is kept;
  - underscore names break after `_`.
- The design-gate documents get no forced break, and nothing in them escapes its column.

The tests write the PDFs to `SAM.Tests/bin/<config>/net8.0/ReportingPdf/`, or to `SAM_REPORTING_PDF_DIR` when that is
set, for visual review. PDFs are not compared byte for byte: PDFsharp writes a creation time and a random file id.

## Known limitations
- A half section or column block that breaks across pages (the stacked fallback) is printed full width.
- Table header rows repeat only for tables in the normal flow, not inside a side-by-side frame; frames never break.
- A token broken by the fallback is split between two characters with no hyphen, so the break is not marked.
- Only PNG images are accepted, matching `ImageBlock`/`DocumentStyle`.
- The fixed renderer text ("Project no.", "Prepared by", "Page n / N") is English.
