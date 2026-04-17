using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Windows.Media;

using ATAS.Indicators;
using OFT.Rendering.Context;
using OFT.Rendering.Settings;
using OFT.Rendering.Tools;

using DrawColor = System.Drawing.Color;
using Rect      = System.Drawing.Rectangle;

namespace SpotGammaLevels
{
    [DisplayName("SpotGamma Levels")]
    [Category("Custom")]
    public class SpotGammaLevels : Indicator
    {
        #region Nested types

        public enum SymbolKey
        {
            [Display(Name = "SPX")]   SPX,
            [Display(Name = "SPY")]   SPY,
            [Display(Name = "NDX")]   NDX,
            [Display(Name = "QQQ")]   QQQ,
            [Display(Name = "RUT")]   RUT,
            [Display(Name = "IWM")]   IWM,
            [Display(Name = "ES1!")]  ES1,
            [Display(Name = "MES1!")] MES1,
            [Display(Name = "NQ1!")]  NQ1,
            [Display(Name = "MNQ1!")] MNQ1,
            [Display(Name = "RTY1!")] RTY1,
            [Display(Name = "M2K1!")] M2K1,
        }

        public enum LabelSide
        {
            [Display(Name = "Left")]  Left,
            [Display(Name = "Right")] Right,
        }

        private static readonly Dictionary<SymbolKey, string> SymbolTokens = new()
        {
            { SymbolKey.SPX,  "SPX"   },
            { SymbolKey.SPY,  "SPY"   },
            { SymbolKey.NDX,  "NDX"   },
            { SymbolKey.QQQ,  "QQQ"   },
            { SymbolKey.RUT,  "RUT"   },
            { SymbolKey.IWM,  "IWM"   },
            { SymbolKey.ES1,  "ES1!"  },
            { SymbolKey.MES1, "MES1!" },
            { SymbolKey.NQ1,  "NQ1!"  },
            { SymbolKey.MNQ1, "MNQ1!" },
            { SymbolKey.RTY1, "RTY1!" },
            { SymbolKey.M2K1, "M2K1!" },
        };

        #endregion

        #region Line series (12 SpotGamma levels)

        private readonly LineSeries _callWall   = MakeLine("Call Wall");
        private readonly LineSeries _putWall    = MakeLine("Put Wall");
        private readonly LineSeries _volTrigger = MakeLine("Vol Trigger");
        private readonly LineSeries _l1         = MakeLine("L1");
        private readonly LineSeries _l2         = MakeLine("L2");
        private readonly LineSeries _l3         = MakeLine("L3");
        private readonly LineSeries _l4         = MakeLine("L4");
        private readonly LineSeries _c1         = MakeLine("C1");
        private readonly LineSeries _c2         = MakeLine("C2");
        private readonly LineSeries _c3         = MakeLine("C3");
        private readonly LineSeries _c4         = MakeLine("C4");
        private readonly LineSeries _zeroGamma  = MakeLine("Zero Gamma");

        #endregion

        #region Input settings

        private string _data = string.Empty;
        private SymbolKey _symbol = SymbolKey.ES1;

        [Display(Name = "Data String", GroupName = "Input", Order = 10,
                 Description = "Den kompletten SpotGamma-CSV-String hier einfügen.")]
        public string Data
        {
            get => _data;
            set
            {
                _data = value ?? string.Empty;
                ApplyValues();
                RedrawChart();
            }
        }

        [Display(Name = "Symbol", GroupName = "Input", Order = 20,
                 Description = "Welchen Datenblock aus dem String verwenden.")]
        public SymbolKey Symbol
        {
            get => _symbol;
            set
            {
                _symbol = value;
                ApplyValues();
                RedrawChart();
            }
        }

        #endregion

        #region Per-level settings (flat — each level owns a PenSettings + LabelSize + LabelAlign)

        [Display(Name = "Style",       GroupName = "Call Wall", Order = 1)]
        public PenSettings CallWallPen       { get; set; } = new() { Color = Colors.LimeGreen,    Width = 2 };
        [Display(Name = "Label Size",  GroupName = "Call Wall", Order = 2)]
        public int         CallWallLabelSize  { get; set; } = 11;
        [Display(Name = "Label Align", GroupName = "Call Wall", Order = 3)]
        public LabelSide   CallWallLabelAlign { get; set; } = LabelSide.Right;

        [Display(Name = "Style",       GroupName = "Put Wall", Order = 1)]
        public PenSettings PutWallPen        { get; set; } = new() { Color = Colors.Crimson,      Width = 2 };
        [Display(Name = "Label Size",  GroupName = "Put Wall", Order = 2)]
        public int         PutWallLabelSize  { get; set; } = 11;
        [Display(Name = "Label Align", GroupName = "Put Wall", Order = 3)]
        public LabelSide   PutWallLabelAlign { get; set; } = LabelSide.Right;

        [Display(Name = "Style",       GroupName = "Vol Trigger", Order = 1)]
        public PenSettings VolTriggerPen        { get; set; } = new() { Color = Colors.Orange,       Width = 2 };
        [Display(Name = "Label Size",  GroupName = "Vol Trigger", Order = 2)]
        public int         VolTriggerLabelSize  { get; set; } = 11;
        [Display(Name = "Label Align", GroupName = "Vol Trigger", Order = 3)]
        public LabelSide   VolTriggerLabelAlign { get; set; } = LabelSide.Right;

        [Display(Name = "Style",       GroupName = "L1", Order = 1)]
        public PenSettings L1Pen        { get; set; } = new() { Color = Colors.DeepSkyBlue };
        [Display(Name = "Label Size",  GroupName = "L1", Order = 2)]
        public int         L1LabelSize  { get; set; } = 11;
        [Display(Name = "Label Align", GroupName = "L1", Order = 3)]
        public LabelSide   L1LabelAlign { get; set; } = LabelSide.Right;

        [Display(Name = "Style",       GroupName = "L2", Order = 1)]
        public PenSettings L2Pen        { get; set; } = new() { Color = Colors.DodgerBlue };
        [Display(Name = "Label Size",  GroupName = "L2", Order = 2)]
        public int         L2LabelSize  { get; set; } = 11;
        [Display(Name = "Label Align", GroupName = "L2", Order = 3)]
        public LabelSide   L2LabelAlign { get; set; } = LabelSide.Right;

        [Display(Name = "Style",       GroupName = "L3", Order = 1)]
        public PenSettings L3Pen        { get; set; } = new() { Color = Colors.RoyalBlue };
        [Display(Name = "Label Size",  GroupName = "L3", Order = 2)]
        public int         L3LabelSize  { get; set; } = 11;
        [Display(Name = "Label Align", GroupName = "L3", Order = 3)]
        public LabelSide   L3LabelAlign { get; set; } = LabelSide.Right;

        [Display(Name = "Style",       GroupName = "L4", Order = 1)]
        public PenSettings L4Pen        { get; set; } = new() { Color = Colors.SlateBlue };
        [Display(Name = "Label Size",  GroupName = "L4", Order = 2)]
        public int         L4LabelSize  { get; set; } = 11;
        [Display(Name = "Label Align", GroupName = "L4", Order = 3)]
        public LabelSide   L4LabelAlign { get; set; } = LabelSide.Right;

        [Display(Name = "Style",       GroupName = "C1", Order = 1)]
        public PenSettings C1Pen        { get; set; } = new() { Color = Colors.Violet };
        [Display(Name = "Label Size",  GroupName = "C1", Order = 2)]
        public int         C1LabelSize  { get; set; } = 11;
        [Display(Name = "Label Align", GroupName = "C1", Order = 3)]
        public LabelSide   C1LabelAlign { get; set; } = LabelSide.Right;

        [Display(Name = "Style",       GroupName = "C2", Order = 1)]
        public PenSettings C2Pen        { get; set; } = new() { Color = Colors.MediumOrchid };
        [Display(Name = "Label Size",  GroupName = "C2", Order = 2)]
        public int         C2LabelSize  { get; set; } = 11;
        [Display(Name = "Label Align", GroupName = "C2", Order = 3)]
        public LabelSide   C2LabelAlign { get; set; } = LabelSide.Right;

        [Display(Name = "Style",       GroupName = "C3", Order = 1)]
        public PenSettings C3Pen        { get; set; } = new() { Color = Colors.MediumPurple };
        [Display(Name = "Label Size",  GroupName = "C3", Order = 2)]
        public int         C3LabelSize  { get; set; } = 11;
        [Display(Name = "Label Align", GroupName = "C3", Order = 3)]
        public LabelSide   C3LabelAlign { get; set; } = LabelSide.Right;

        [Display(Name = "Style",       GroupName = "C4", Order = 1)]
        public PenSettings C4Pen        { get; set; } = new() { Color = Colors.DarkViolet };
        [Display(Name = "Label Size",  GroupName = "C4", Order = 2)]
        public int         C4LabelSize  { get; set; } = 11;
        [Display(Name = "Label Align", GroupName = "C4", Order = 3)]
        public LabelSide   C4LabelAlign { get; set; } = LabelSide.Right;

        [Display(Name = "Style",       GroupName = "Zero Gamma", Order = 1)]
        public PenSettings ZeroGammaPen        { get; set; } = new() { Color = Colors.Gold, LineDashStyle = LineDashStyle.Dot, Width = 2 };
        [Display(Name = "Label Size",  GroupName = "Zero Gamma", Order = 2)]
        public int         ZeroGammaLabelSize  { get; set; } = 11;
        [Display(Name = "Label Align", GroupName = "Zero Gamma", Order = 3)]
        public LabelSide   ZeroGammaLabelAlign { get; set; } = LabelSide.Right;

        #endregion

        #region Table-only entries (Estimated Moves – not plotted on the chart)

        private decimal _est1Day;
        private decimal _est5Day;

        // Hardcoded label colors for the two Est-Move table cells.
        // Aquamarine = 1D, Khaki = 5D — visually distinct from the level colors above.
        private static readonly Color Est1DayLabelColor = Colors.Aquamarine;
        private static readonly Color Est5DayLabelColor = Colors.Khaki;

        #endregion

        #region Table settings

        private bool _showTable = true;
        private int _tableFontSize = 11;

        [Display(Name = "Show Table", GroupName = "Table", Order = 200)]
        public bool ShowTable
        {
            get => _showTable;
            set { _showTable = value; RedrawChart(); }
        }

        [Display(Name = "Table Font Size", GroupName = "Table", Order = 210)]
        public int TableFontSize
        {
            get => _tableFontSize;
            set { _tableFontSize = Math.Max(8, Math.Min(24, value)); RedrawChart(); }
        }

        #endregion

        public SpotGammaLevels() : base(true)
        {
            DenyToChangePanel = true;
            EnableCustomDrawing = true;
            SubscribeToDrawingEvents(DrawingLayouts.Final);

            ((ValueDataSeries)DataSeries[0]).VisualType = VisualMode.Hide;
            ((ValueDataSeries)DataSeries[0]).IsHidden = true;

            foreach (var line in AllLines())
                LineSeries.Add(line);
        }

        protected override void OnCalculate(int bar, decimal value)
        {
            if (bar == 0)
                ApplyValues();
        }

        protected override void OnRender(RenderContext context, DrawingLayouts layout)
        {
            if (ChartInfo == null)
                return;

            SyncLineStyles();

            var chartRegion = ChartInfo.PriceChartContainer.Region;

            foreach (var lv in LevelConfigs())
            {
                if (lv.Line.IsHidden || lv.Line.Value <= 0m)
                    continue;

                var y = ChartInfo.PriceChartContainer.GetYByPrice(lv.Line.Value, false);
                if (y < chartRegion.Top || y > chartRegion.Bottom)
                    continue;

                var font = new RenderFont("Arial", lv.LabelSize);
                var size = context.MeasureString(lv.Line.Name, font);
                int textW = (int)size.Width;
                int textH = (int)size.Height;

                int x = lv.LabelAlign == LabelSide.Right
                    ? chartRegion.Right - textW - 6
                    : chartRegion.Left + 6;
                int textY = (int)y - textH - 1;

                var bg = DrawColor.FromArgb(160, 0, 0, 0);
                context.FillRectangle(bg, new Rect(x - 3, textY - 1, textW + 6, textH + 2));
                context.DrawString(lv.Line.Name, font, ToDrawing(lv.Pen.Color), x, textY);
            }

            if (_showTable)
                RenderTable(context, chartRegion);
        }

        private void RenderTable(RenderContext context, Rect chartRegion)
        {
            var rows = new List<(string Label, string ValueText, DrawColor Color)>();

            foreach (var lv in LevelConfigs())
                rows.Add((lv.Line.Name, FormatValue(lv.Line.Value), ToDrawing(lv.Pen.Color)));

            rows.Add(("1D Est",  FormatPercent(_est1Day), ToDrawing(Est1DayLabelColor)));
            rows.Add(("5D Est",  FormatPercent(_est5Day), ToDrawing(Est5DayLabelColor)));

            var font = new RenderFont("Arial", _tableFontSize);

            int pad    = 6;
            int cellY  = 4;
            int rowH   = 0;
            int[] colW = new int[rows.Count];

            for (int i = 0; i < rows.Count; i++)
            {
                var labelSize = context.MeasureString(rows[i].Label, font);
                var valueSize = context.MeasureString(rows[i].ValueText, font);
                colW[i] = Math.Max((int)labelSize.Width, (int)valueSize.Width) + pad * 2;
                rowH = Math.Max(rowH, Math.Max((int)labelSize.Height, (int)valueSize.Height));
            }

            int totalW = colW.Sum();
            int totalH = rowH * 2 + cellY * 3;

            int right  = chartRegion.Right  - 10;
            int bottom = chartRegion.Bottom - 10;
            int left   = right - totalW;
            int top    = bottom - totalH;

            var bg = DrawColor.FromArgb(210, 15, 15, 25);
            context.FillRectangle(bg, new Rect(left, top, totalW, totalH));

            var divider = DrawColor.FromArgb(70, 255, 255, 255);
            int rowDivY = top + cellY + rowH + cellY / 2;
            context.DrawLine(new RenderPen(divider, 1), left + 1, rowDivY, left + totalW - 1, rowDivY);

            int x = left;
            for (int i = 0; i < rows.Count; i++)
            {
                int cellLeft = x + pad;
                int headerY  = top + cellY;
                int valueY   = rowDivY + cellY / 2;

                context.DrawString(rows[i].Label, font, rows[i].Color, cellLeft, headerY);
                context.DrawString(rows[i].ValueText, font, DrawColor.White, cellLeft, valueY);

                if (i > 0)
                    context.DrawLine(new RenderPen(divider, 1), x, top + 1, x, top + totalH - 1);

                x += colW[i];
            }
        }

        private static string FormatValue(decimal v) =>
            v <= 0m ? "—" : v.ToString("0.##", CultureInfo.InvariantCulture);

        private static string FormatPercent(decimal v) =>
            v == 0m ? "—" : (v * 100m).ToString("0.##", CultureInfo.InvariantCulture) + "%";

        private void ApplyValues()
        {
            var parsed = ParseForSymbol(_data, SymbolTokens[_symbol]);

            if (parsed == null)
            {
                HideAll();
                _est1Day = 0m;
                _est5Day = 0m;
                return;
            }

            _est1Day = parsed.Est1Day;
            _est5Day = parsed.Est5Day;

            _callWall.Value   = parsed.CallWall;
            _putWall.Value    = parsed.PutWall;
            _volTrigger.Value = parsed.VolTrigger;
            _l1.Value = parsed.L1;
            _l2.Value = parsed.L2;
            _l3.Value = parsed.L3;
            _l4.Value = parsed.L4;
            _c1.Value = parsed.C1;
            _c2.Value = parsed.C2;
            _c3.Value = parsed.C3;
            _c4.Value = parsed.C4;
            _zeroGamma.Value = parsed.ZeroGamma;

            foreach (var line in AllLines())
                line.IsHidden = false;
        }

        private void HideAll()
        {
            foreach (var line in AllLines())
                line.IsHidden = true;
        }

        private void SyncLineStyles()
        {
            foreach (var lv in LevelConfigs())
            {
                lv.Line.Color = lv.Pen.Color;
                lv.Line.Width = Math.Max(1, lv.Pen.Width);
                lv.Line.LineDashStyle = lv.Pen.LineDashStyle;
            }
        }

        private IEnumerable<LineSeries> AllLines()
        {
            yield return _callWall;
            yield return _putWall;
            yield return _volTrigger;
            yield return _l1;
            yield return _l2;
            yield return _l3;
            yield return _l4;
            yield return _c1;
            yield return _c2;
            yield return _c3;
            yield return _c4;
            yield return _zeroGamma;
        }

        private readonly struct LevelConfig
        {
            public LineSeries Line     { get; }
            public PenSettings Pen     { get; }
            public int LabelSize       { get; }
            public LabelSide LabelAlign { get; }
            public LevelConfig(LineSeries line, PenSettings pen, int labelSize, LabelSide labelAlign)
            { Line = line; Pen = pen; LabelSize = labelSize; LabelAlign = labelAlign; }
        }

        private IEnumerable<LevelConfig> LevelConfigs()
        {
            yield return new LevelConfig(_callWall,   CallWallPen,   CallWallLabelSize,   CallWallLabelAlign);
            yield return new LevelConfig(_putWall,    PutWallPen,    PutWallLabelSize,    PutWallLabelAlign);
            yield return new LevelConfig(_volTrigger, VolTriggerPen, VolTriggerLabelSize, VolTriggerLabelAlign);
            yield return new LevelConfig(_l1,         L1Pen,         L1LabelSize,         L1LabelAlign);
            yield return new LevelConfig(_l2,         L2Pen,         L2LabelSize,         L2LabelAlign);
            yield return new LevelConfig(_l3,         L3Pen,         L3LabelSize,         L3LabelAlign);
            yield return new LevelConfig(_l4,         L4Pen,         L4LabelSize,         L4LabelAlign);
            yield return new LevelConfig(_c1,         C1Pen,         C1LabelSize,         C1LabelAlign);
            yield return new LevelConfig(_c2,         C2Pen,         C2LabelSize,         C2LabelAlign);
            yield return new LevelConfig(_c3,         C3Pen,         C3LabelSize,         C3LabelAlign);
            yield return new LevelConfig(_c4,         C4Pen,         C4LabelSize,         C4LabelAlign);
            yield return new LevelConfig(_zeroGamma,  ZeroGammaPen,  ZeroGammaLabelSize,  ZeroGammaLabelAlign);
        }

        private static LineSeries MakeLine(string name)
        {
            return new LineSeries(name, name)
            {
                Color = Colors.White,
                Width = 1,
                LineDashStyle = LineDashStyle.Solid,
                UseScale = true,
                IsHidden = true,
                Value = 0m,
            };
        }

        private static DrawColor ToDrawing(Color c) =>
            DrawColor.FromArgb(c.A, c.R, c.G, c.B);

        #region Parser

        private sealed class Parsed
        {
            public decimal CallWall, PutWall, VolTrigger;
            public decimal L1, L2, L3, L4;
            public decimal C1, C2, C3, C4;
            public decimal Est1Day, Est5Day;
            public decimal ZeroGamma;
        }

        private static Parsed ParseForSymbol(string data, string symbol)
        {
            if (string.IsNullOrWhiteSpace(data) || string.IsNullOrEmpty(symbol))
                return null;

            var tokens = data.Split(',');
            for (int i = 0; i < tokens.Length; i++)
                tokens[i] = tokens[i].Trim();

            var marker = "$" + symbol;

            for (int i = 0; i < tokens.Length; i++)
            {
                if (!string.Equals(tokens[i], marker, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (i + 15 >= tokens.Length)
                    return null;

                decimal V(int offset) =>
                    decimal.TryParse(tokens[i + offset], NumberStyles.Any,
                                     CultureInfo.InvariantCulture, out var v) ? v : 0m;

                return new Parsed
                {
                    CallWall   = V(2),
                    PutWall    = V(3),
                    VolTrigger = V(4),
                    L1         = V(5),
                    L2         = V(6),
                    L3         = V(7),
                    L4         = V(8),
                    C1         = V(9),
                    C2         = V(10),
                    C3         = V(11),
                    C4         = V(12),
                    Est1Day    = V(13),
                    Est5Day    = V(14),
                    ZeroGamma  = V(15),
                };
            }

            return null;
        }

        #endregion
    }
}
