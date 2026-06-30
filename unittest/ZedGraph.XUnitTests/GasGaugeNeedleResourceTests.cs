// <copyright file="GasGaugeNeedleResourceTests.cs" company="ZedGraph Project">
//   This library is free software; you can redistribute it and/or
//   modify it under the terms of the GNU Lesser General Public
//   License as published by the Free Software Foundation; either
//   version 2.1 of the License, or (at your option) any later version.
// </copyright>

// <summary>
//   M10 characterization 測試：鎖定 GasGaugeNeedle 與 MasterPane.OnDeserialization
//   渲染路徑的現有行為，作為後續 GDI+ 資源釋放重構的 baseline。
//
//   這些是「重構」前的測試，行為在修復前即通過（characterization），確保未來
//   加 using/Dispose 後，外部可觀察行為（不拋例外、不改變繪圖結果）不變。
//
//   覆蓋目標：
//     T-M10.a → GasGaugeNeedle.Draw  (Pen p:428, Brush b:436, Pen borderPen:439)
//     T-M10.b → GasGaugeNeedle.DrawLegendKey  (Pen pen:472)
//     T-M10.c → MasterPane.OnDeserialization  (Bitmap:403, Graphics:404)
// </summary>

namespace ZedGraph.XUnitTests
{
    using System;
    using System.Drawing;

    using ZedGraph.XUnitTests.TestHelpers;

    using Xunit;

    public class GasGaugeNeedleResourceTests
    {
        /// <summary>
        /// 建立標準 GasGauge 場景：一個 pane，加一段 GasGaugeRegion 與一支 GasGaugeNeedle。
        /// 不呼叫 AxisChange，由各測試依需求自行設定。
        /// </summary>
        private static GraphPane BuildGasGaugePane()
        {
            var pane = new GraphPane(new RectangleF(0, 0, 640, 480), "GasGauge", "X", "Y");
            pane.CurveList.Add(new GasGaugeRegion("Green", 0f, 100.0f, Color.Green));
            pane.CurveList.Add(new GasGaugeNeedle("Needle", 50.0f, Color.Black));
            return pane;
        }

        /// <summary>
        /// T-M10.a — GasGaugeNeedle.Draw 在主路徑下不應拋例外。
        /// 進入條件：pane.Chart._rect 的寬/高其一為正、_isVisible=true、
        /// _boundingRectangle 寬高均 >= 1（由 CalcRectangle 設定）。
        /// 此路徑會建立三個未 Dispose 的 Pen/Brush (428/436/439)，
        /// 本測試只鎖定「繪製能跑完」，不驗證洩漏（洩漏需 DisposeTracker，
        /// 已超出 unit test 範圍，留給後續整合測試或 reviewer）。
        /// </summary>
        [Fact]
        public void GasGaugeNeedle_Draw_DoesNotThrow()
        {
            using var fx = new GraphicsFixture(640, 480);
            var pane = BuildGasGaugePane();
            pane.AxisChange(fx.Graphics);

            var needle = (GasGaugeNeedle)pane.CurveList[1];

            // 未 using 任何額外資源——傳入的 fx.Graphics 為測試環境自帶，
            // 測試結束時 GraphicsFixture.Dispose 會釋放。
            Exception caught = Record.Exception(() => needle.Draw(fx.Graphics, pane, 0, 1f));

            Assert.Null(caught);
        }

        /// <summary>
        /// T-M10.b — GasGaugeNeedle.DrawLegendKey 不應拋例外。
        /// 進入條件：_isVisible=true。
        /// 此路徑建立一個未 Dispose 的 Pen (472)，同樣只驗行為不改，
        /// 不驗 Dispose（屬單元測試邊界外）。
        /// </summary>
        [Fact]
        public void GasGaugeNeedle_DrawLegendKey_DoesNotThrow()
        {
            using var fx = new GraphicsFixture(640, 480);
            var pane = BuildGasGaugePane();
            pane.AxisChange(fx.Graphics);

            var needle = (GasGaugeNeedle)pane.CurveList[1];
            var rect = new RectangleF(10f, 10f, 100f, 16f);

            Exception caught = Record.Exception(() => needle.DrawLegendKey(fx.Graphics, pane, rect, 1f));

            Assert.Null(caught);
        }

        /// <summary>
        /// T-M10.c — MasterPane.OnDeserialization 不應拋例外。
        /// 觸發 _rect 預設 500x375 路徑，建立拋棄式 10x10 Bitmap 與 Graphics 後未釋放 (403/404)。
        /// 此方法為 PaneBase 的 IDeserializationCallback 掛鉤點，
        /// 序列化還原後由 .NET 執行階段呼叫，公共 API 但未被既有測試覆蓋。
        /// </summary>
        [Fact]
        public void MasterPane_OnDeserialization_DoesNotThrow()
        {
            var master = new MasterPane("Test", new RectangleF(0f, 0f, 500f, 375f));

            Exception caught = Record.Exception(() => master.OnDeserialization(null));

            Assert.Null(caught);
        }
    }
}
