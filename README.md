# SpotGamma Levels for ATAS

A custom indicator for the **ATAS Platform** that draws SpotGamma options-flow levels parsed from a TradingView-style CSV string. Port of the closed-source TradingView indicator *"Levels by SpotGamma"* — same data, same look, native in ATAS.

The indicator does **not** compute the levels itself. It parses a string you paste into the indicator settings (the same string you already use on TradingView), picks the block for the chosen symbol, and draws the levels as horizontal lines plus a summary table in the bottom-right corner of the chart.

---

## Features

### Levels drawn on the chart
- **Call Wall** — strongest positive gamma strike
- **Put Wall** — strongest negative gamma strike
- **Vol Trigger** — implied volatility regime threshold
- **L1 – L4** — additional support/resistance levels
- **C1 – C4** — additional call levels
- **Zero Gamma** — gamma flip point (default: dotted line)

### Table-only entries (rendered in the bottom-right table, not plotted)
- **1D Est** — 1-day estimated move (%)
- **5D Est** — 5-day estimated move (%)

### UI / customization
- **Per-level styling**: each level has its own color, line width, dash style, label size, and label alignment (left / right of the chart)
- **Bottom-right info table** showing every level with its current value
  - Toggle on/off
  - Adjustable font size
- **Multi-symbol** support via dropdown — the same data string can contain blocks for multiple symbols, the indicator picks the one you select
- **Live updates**: changing the data string, symbol or any styling refreshes the chart immediately

### Supported symbols
`SPX`, `SPY`, `NDX`, `QQQ`, `RUT`, `IWM`, `ES1!`, `MES1!`, `NQ1!`, `MNQ1!`, `RTY1!`, `M2K1!`

---

## Installation

### Prerequisites
- **ATAS Platform** (developed against version `8.0.13.384`, installed at `C:\Program Files (x86)\ATAS Platform\`)
- **.NET 10 SDK** — only required if you build from source

### Option A — Use the precompiled DLL (recommended)
1. Download [`dist/SpotGammaLevels.dll`](dist/SpotGammaLevels.dll)
   (direct download: <https://github.com/xentres86/SpotGammaATAS/raw/main/dist/SpotGammaLevels.dll>)
2. Copy it into `%APPDATA%\ATAS\Indicators\`
   (i.e. `C:\Users\<you>\AppData\Roaming\ATAS\Indicators\`)
3. Restart ATAS (or reload indicators)

> **Important:** ATAS scans `%APPDATA%\ATAS\Indicators\` — **not** `Documents\ATAS\Indicators\`. Dropping the DLL into the wrong folder will silently do nothing.

### Option B — Build from source
```bash
git clone https://github.com/xentres86/SpotGammaATAS.git
cd SpotGammaATAS
dotnet build SpotGammaLevels.csproj -c Release
```
The build automatically deploys the compiled DLL into `%APPDATA%\ATAS\Indicators\`. Restart ATAS afterwards.

If your ATAS install path is different from `C:\Program Files (x86)\ATAS Platform\`, edit the `<AtasDir>` property at the top of [`SpotGammaLevels.csproj`](SpotGammaLevels.csproj).

---

## Usage

1. In ATAS, open a chart for one of the supported symbols.
2. Add the indicator: **Indicators → Custom → SpotGamma Levels**.
3. Open the indicator's settings (the `…` menu next to the indicator name).
4. **Input → Data String**: paste the full SpotGamma CSV string.
5. **Input → Symbol**: choose the symbol whose block should be drawn.
6. (Optional) Tweak per-level colors, line styles and label placement.

The levels appear immediately on the chart, and the summary table populates in the bottom-right corner.

---

## Data string format

The data string is comma-separated and may contain multiple symbol blocks back-to-back. Each block consists of **16 tokens**:

```
$SYM, SYM, CallWall, PutWall, VolTrigger, L1, L2, L3, L4, C1, C2, C3, C4, 1DayEstMove, 5DayEstMove, ZeroGamma
```

| Position | Field         | Notes                                            |
|---------:|---------------|--------------------------------------------------|
|  0       | `$SYM`        | Marker — `$` followed by the symbol token        |
|  1       | `SYM`         | Symbol token (e.g. `ES1!`, `SPX`)                |
|  2       | Call Wall     | Price                                            |
|  3       | Put Wall      | Price                                            |
|  4       | Vol Trigger   | Price                                            |
|  5 – 8   | L1 – L4       | Prices                                           |
|  9 – 12  | C1 – C4       | Prices                                           |
| 13       | 1D Est Move   | Decimal fraction (`0.0066` = `0.66 %`)           |
| 14       | 5D Est Move   | Decimal fraction                                 |
| 15       | Zero Gamma    | Price                                            |

Multiple blocks in one string are detected by their `$SYM` markers — the indicator searches for `$<selectedSymbol>` and reads the following 15 tokens.

---

## Default colors

| Level         | Color          | Notes                |
|---------------|----------------|----------------------|
| Call Wall     | LimeGreen      | width 2              |
| Put Wall      | Crimson        | width 2              |
| Vol Trigger   | Orange         | width 2              |
| L1            | DeepSkyBlue    |                      |
| L2            | DodgerBlue     |                      |
| L3            | RoyalBlue      |                      |
| L4            | SlateBlue      |                      |
| C1            | Violet         |                      |
| C2            | MediumOrchid   |                      |
| C3            | MediumPurple   |                      |
| C4            | DarkViolet     |                      |
| Zero Gamma    | Gold           | dotted, width 2      |
| 1D Est (table)| Aquamarine     | label-only color     |
| 5D Est (table)| Khaki          | label-only color     |

All colors and line styles are user-configurable in the indicator settings via ATAS' standard `PenSettings` editor.

---

## Build configuration

- **Target framework**: `net10.0-windows`, `x64`, WPF enabled
- **ATAS SDK**: referenced as local DLLs (not on NuGet)
  - `ATAS.Indicators.dll`, `ATAS.DataFeedsCore.dll`, `ATAS.Types.dll`
  - `OFT.Attributes.dll`, `OFT.Core.dll`, `OFT.Rendering.dll`, `OFT.Localization.dll`
  - `Utils.Common.dll`
  - All marked `<Private>false</Private>` so they are not copied into the output
- **Post-build**: compiled DLL is copied to `%APPDATA%\ATAS\Indicators\` automatically

---

## Project structure

```
.
├── SpotGamma.sln              # Visual Studio solution
├── SpotGammaLevels.csproj     # Build configuration + ATAS SDK references
├── SpotGammaLevels.cs         # All indicator code in a single file
├── .gitignore
└── README.md
```

---

## Troubleshooting

- **Indicator doesn't show up in ATAS** — check the DLL is in `%APPDATA%\ATAS\Indicators\` (not `Documents\…`) and restart ATAS.
- **Build error `CS1705` about `System.Runtime`** — your `<TargetFramework>` is wrong. ATAS 8.x DLLs target .NET 10; use `net10.0-windows`.
- **Build error `NU1101` about `ATAS.Indicators`** — ATAS is not on NuGet. Reference the DLLs locally via `<HintPath>` (already configured in this project).
- **Levels not drawn** — verify your data string contains a `$SYM` marker matching the selected symbol, and that the block has all 16 tokens.

---

## License

This project is provided as-is for personal use. The SpotGamma data itself and its source levels are property of their respective owners — this repository only contains the ATAS-side rendering logic.
