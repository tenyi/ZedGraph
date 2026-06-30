// <copyright file="LinearScalePickScaleTests.cs" company="ZedGraph Project">
//   This library is free software; you can redistribute it and/or
//   modify it under the terms of the GNU Lesser General Public
//   License as published by the Free Software Foundation; either
//   version 2.1 of the License, or (at your option) any later version.
// </copyright>

// <summary>
//   Characterization tests for LinearScale.PickScale（T0.3.7）。
//   PickScale 為 protected/internal，直接呼叫需精確控制大量內部狀態。
//   改測其「外部可觀測效果」：AxisChange 後軸範圍（Min/Max）應自動涵蓋資料、
//   並依 grace 擴張。鎖定此自動範圍契約，Batch 3 修正 _max==_min 邊界與全負資料
//   行為時，確保正常的自動範圍計算不被破壞。
// </summary>

namespace ZedGraph.XUnitTests
{
    using ZedGraph.XUnitTests.TestHelpers;

    using Xunit;

    public class LinearScalePickScaleTests
    {
        /// <summary>
        /// 正常資料範圍：AxisChange 後 Y 軸 Min/Max 應涵蓋資料最小/最大值。
        /// （自動範圍通常還會加 grace，故用 <= / >= 斷言）
        /// </summary>
        [Fact]
        public void AxisChange_YAxisRange_CoversDataMinMax()
        {
            using var fx = new GraphicsFixture();
            var pane = new GraphPane();
            pane.AddCurve("c", new[] { 1.0, 2.0, 3.0 }, new[] { 10.0, 20.0, 30.0 },
                System.Drawing.Color.Blue);
            pane.AxisChange(fx.Graphics);

            Assert.True(pane.YAxis.Scale.Min <= 10.0, "Min 應 <= 資料最小值");
            Assert.True(pane.YAxis.Scale.Max >= 30.0, "Max 應 >= 資料最大值");
        }

        /// <summary>
        /// 全負資料：AxisChange 後 Min/Max 仍應涵蓋資料範圍（含負值）。
        /// 鎖定負值範圍的正確處理（不因 Math.Log10 等而崩潰，呼應 C2 修復）。
        /// </summary>
        [Fact]
        public void AxisChange_AllNegativeData_RangeCoversData()
        {
            using var fx = new GraphicsFixture();
            var pane = new GraphPane();
            pane.AddCurve("c", new[] { 1.0, 2.0, 3.0 }, new[] { -30.0, -20.0, -10.0 },
                System.Drawing.Color.Blue);
            pane.AxisChange(fx.Graphics);

            Assert.True(pane.YAxis.Scale.Min <= -30.0);
            Assert.True(pane.YAxis.Scale.Max >= -10.0);
        }

        /// <summary>
        /// 單點資料（_max==_min 邊界）：AxisChange 不應拋例外，且應產生有效範圍。
        /// 鎖定邊界穩定性。
        /// </summary>
        [Fact]
        public void AxisChange_SinglePoint_DoesNotThrow_AndProducesValidRange()
        {
            using var fx = new GraphicsFixture();
            var pane = new GraphPane();
            pane.AddCurve("c", new[] { 5.0 }, new[] { 42.0 }, System.Drawing.Color.Blue);

            var ex = Record.Exception(() => pane.AxisChange(fx.Graphics));

            Assert.Null(ex);
            // 單點時範圍應被擴張為有效區間（Max > Min）
            Assert.True(pane.YAxis.Scale.Max > pane.YAxis.Scale.Min);
        }

        /// <summary>
        /// 預設 GraphPane 的 Y 軸應為 LinearScale（確認測試對象正確）。
        /// </summary>
        [Fact]
        public void DefaultYAxis_IsLinearScale()
        {
            using var fx = new GraphicsFixture();
            var pane = new GraphPane();
            pane.AxisChange(fx.Graphics);

            Assert.Equal(AxisType.Linear, pane.YAxis.Type);
        }
    }
}
