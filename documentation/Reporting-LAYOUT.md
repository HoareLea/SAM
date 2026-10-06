# Space Assumptions — approved Phase-1 layout (v2)

Status: **approved** at the visual design gate (2026-09-25). This supersedes the layout in §10 of the *SAM
Documentation Framework* plan (Revision 3). That plan is kept outside git. The typography, missing-value rules and
branding in §10 still apply, except where this page changes them.

The builders (`SAM.Analytical.Reporting`) produce a renderer-neutral `Document`. This page specifies how the PR2
renderer lays it out. The renderer never converts units or reinterprets values.

## Page
- A4 portrait with 15 mm margins.
- Noto Sans, the one embedded family (OFL). Sizes:
  - title 16 pt bold;
  - space name 13 pt bold;
  - section headings 8.5 pt bold caps in the accent colour, with a 0.5 pt rule;
  - body about 9 pt;
  - sub-labels 7 pt grey;
  - footer 7 pt grey.
- **Normal target: one page.** Every Phase-1 section has a bounded row count: the fabric table has at most 8 rows,
  and the gains table 5. With realistic data, all measured cases fit one page with about 35 mm to spare.
- **One page is not a renderer invariant.** Exceptionally long content (long names, many risers, future rows) must
  paginate naturally, with table headers repeated (`TableBlock.RepeatHeader`). The renderer never shrinks text,
  truncates or clips engineering information to stay on one page.

## Grid
The identity section is shown in the header band: space name, level and internal condition. The sections below it
are laid out in this order:

| Row | Left (half width) | Right (half width) |
|---|---|---|
| 1 | Geometry | Design criteria (heating/cooling table + room humidity) |
| 2 | Internal condition, full width: the gains table, then **three compact columns** — Occupancy, Lighting, Infiltration | |
| 3 | Ventilation | Systems |
| 4 | Fabric / exposure | Sizing (Tas design loads) |

- Paired sections sit side by side with a 6 mm gap. A section that has no data shows its one-line `NoticeBlock`.
- **Fixed unit column:** key/value units sit in their own left-aligned column of fixed width (about 15 mm; 12 mm in
  the three-column block), so numbers align down a section. Table units go in the column header when the column
  shares one unit (`TableColumn.Unit`). Otherwise they go in the cell.
- Values are right-aligned with tabular figures.

## Content rules the builders enforce (renderer just prints)
- **Occupancy:** the profile is shown only in the gains table, not repeated as an Occupancy row.
- **Fabric:** a display row whose external and internal areas are both *explicitly* zero is left out. The omitted
  rows are named in one grey note, `Not present (zero area): …` (`NoticeBlock` id `fabric-not-present`):
  - when every row of a category is zero, the note names the category ("Windows");
  - otherwise it names the row ("Doors (frame)").

  A missing area ("—") is never treated as zero. The note carries no unit, and the table keeps the selected SI/IP
  area unit.
- **Sizing:** there is one notice.
  - "No Tas design loads in model"; or
  - "Design loads: from Tas sizing — date/currency not recorded", extended with "; whether they include the Sizing
    multiplier is not recorded" when a multiplier is set.

  The Sizing multiplier row prints:
  - `1.20` for a set value;
  - `not set` when set neither on the space nor on the model (SAM_Tas then applies no multiplier);
  - `—` when the stored value is invalid.

  Design-load freshness stays `Unknown` in Phase 1.
- **Room humidity:** the primary labels are "Humidification set point" and "Dehumidification set point". The
  `KeyValueRow.SubLabel` gives "lower RH limit" / "upper RH limit", which the renderer prints under the label in the
  sub-label style. Disabled control (a 0 % lower limit or a 100 % upper limit) prints `n/a`.
- **Placeholders:** `—` means not available, and `n/a` means not applicable. An authored zero prints as a number
  ("0" or "0.0", at the unit's decimals), never as a placeholder. The footer legend lists only the markers the page
  actually prints.

## Implementation (PR2)
The production renderer is `SAM.Core.Reporting.Pdf` (MigraDoc + PDFsharp); see [Reporting-PDF.md](Reporting-PDF.md).
It derives the grid above from generic hints only:
- the section order and `SectionWidth` (consecutive `Half` sections pair up);
- titled key/value blocks side by side in a full-width section;
- the `identity` section in the header band.

The Space Assumptions definition therefore orders its sections as the grid reads: geometry, design criteria,
internal condition, ventilation, systems, fabric, sizing, with fabric and sizing marked `Half`. The internal-condition
blocks come gains first. The fabric note is `NoticeLevel.Note`.

All four reference cases print on **one A4 page**. The spare space is about 15 mm for the office and the atrium,
against about 35 mm in the HTML mock-up: MigraDoc sets Noto Sans with its full line height.

Differences from the static mock-up:
- The identity line keeps its labels ("Level: … · Internal condition: …"). It wraps to a second line for long
  condition names.
- The SAM mark has square corners.
- The gains-table profile column is printed in the body colour.
- In the three-column block, the unit column is sized to its widest unit rather than a fixed 12 mm.

## Reference mock-ups
The static mock-ups are kept outside git, in `Documents\SAM_daily\2026-09-25-Reporting\mockup\`. They are
`render_mockup_v2.py` with HTML, PDF and PNG output, rendered from the builder JSON. The v2 renderer applies no
transforms; it only asserts that the builders already apply the rules above.
