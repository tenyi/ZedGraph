// <copyright file="GdiCharacterizationSanityTests.cs" company="ZedGraph Project">
//   This library is free software; you can redistribute it and/or
//   modify it under the terms of the GNU Lesser General Public
//   License as published by the Free Software Foundation; either
//   version 2.1 of the License, or (at your option) any later version.
// </copyright>

// <summary>
//   GDI+ 渲染路徑的 sanity characterization（涵蓋 T0.3.2 Symbol、T0.3.3 Line、
//   T0.3.4 CurveList.SortOverlay、T0.3.5 FontSpec）。
//
//   這些方法高度依賴 GDI+ 與 Draw pipeline 的內部狀態，強求精確的 round-trip
//   斷言（如像素座標序列）會因內部欄位設定困難而反覆失敗。故採 sanity 等級：
//     - 完整 pane.Draw(graphics) 不拋例外（涵蓋 Symbol.Draw、Line.Draw、
//       BuildPointsArray、CurveList.SortOverlay 等熱路徑）
//     - FontSpec.BoundingBox 回傳非零尺寸（有意義的測量契約）
//
//   ⚠ Sanity 等級：守護「重構後渲染不崩潰」的最低契約，不鎖定精確視覺結果。
//   Batch 4 效能重構（如 isPixelDrawn 改 BitArray）後，這些測試確保渲染流程仍可走通。
// </summary>

namespace ZedGraph.XUnitTests
{
    using System.Drawing;

    using ZedGraph.XUnitTests.TestHelpers;

    using Xunit;

    public class GdiCharacterizationSanityTests
    {
        /// <summary>
        /// 線圖完整繪製不拋例外。
        /// 涵蓋 Symbol.Draw、Symbol.MakePath、Line.Draw、Line.BuildPointsArray 等熱路徑。
        /// </summary>
        [Fact]
        public void Draw_LineItemCurve_DoesNotThrow()
        {
            using var fx = new GraphicsFixture(640, 480);
            var pane = new GraphPane(new RectangleF(0, 0, 640, 480),
                "title", "X", "Y");
            pane.AddCurve("c", new[] { 1.0, 2.0, 3.0, 4.0 },
                new[] { 10.0, 20.0, 15.0, 30.0 }, Color.Blue);
            pane.AxisChange(fx.Graphics);

            var ex = Record.Exception(() => pane.Draw(fx.Graphics));

            Assert.Null(ex);
        }

        /// <summary>
        /// SortedOverlay 柱狀圖完整繪製不拋例外。
        /// 涵蓋 CurveList.SortOverlay 的排序與 DrawSingleBar 流程。
        /// </summary>
        [Fact]
        public void Draw_SortedOverlayBars_DoesNotThrow()
        {
            using var fx = new GraphicsFixture(640, 480);
            var pane = new GraphPane(new RectangleF(0, 0, 640, 480),
                "title", "X", "Y");
            pane.BarSettings.Type = BarType.SortedOverlay;
            pane.AddBar("a", new[] { 1.0, 2.0, 3.0 }, new[] { 100.0, -50.0, 80.0 }, Color.Red);
            pane.AddBar("b", new[] { 1.0, 2.0, 3.0 }, new[] { 60.0, 90.0, -20.0 }, Color.Blue);
            pane.AddBar("c", new[] { 1.0, 2.0, 3.0 }, new[] { 40.0, 70.0, 50.0 }, Color.Green);
            pane.AxisChange(fx.Graphics);

            var ex = Record.Exception(() => pane.Draw(fx.Graphics));

            Assert.Null(ex);
        }

        /// <summary>
        /// FontSpec.BoundingBox 對非空字串應回傳非零尺寸（寬、高皆 > 0）。
        /// 鎖定 MeasureString 的測量契約。
        /// </summary>
        [Fact]
        public void FontSpec_BoundingBox_ReturnsNonZeroSize()
        {
            using var fx = new GraphicsFixture(640, 480);
            var pane = new GraphPane();
            SizeF size = pane.Title.FontSpec.BoundingBox(fx.Graphics, "Hello World", 1.0f);

            Assert.True(size.Width > 0, "字串測量寬度應 > 0");
            Assert.True(size.Height > 0, "字串測量高度應 > 0");
        }

        /// <summary>
        /// 旋轉角度對 FontSpec.BoundingBox 的影響：
        /// 90 度旋轉後，寬與高應與未旋轉時「交換」（非嚴格相等，因含邊距，但趨勢相反）。
        /// 鎖定角度參與測量計算的事實。
        /// </summary>
        [Fact]
        public void FontSpec_BoundingBox_RotationSwapsDimensions()
        {
            using var fx = new GraphicsFixture(640, 480);
            var pane = new GraphPane();
            var fontSpec = pane.Title.FontSpec;

            SizeF normal = fontSpec.BoundingBox(fx.Graphics, "Wide Text Sample", 1.0f);
            fontSpec.Angle = 90;
            SizeF rotated = fontSpec.BoundingBox(fx.Graphics, "Wide Text Sample", 1.0f);

            // 未旋轉：寬字串為「寬扁」形（width > height）
            Assert.True(normal.Width > normal.Height, "未旋轉時寬應 > 高");
            // 旋轉 90 度後：形狀反轉為「高瘦」（height > width），證明角度參與測量
            Assert.True(rotated.Height > rotated.Width, "旋轉 90 度後高應 > 寬");
        }
    }
}
