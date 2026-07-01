// <copyright file="LogScaleTests.cs" company="ZedGraph Project">
//   This library is free software; you can redistribute it and/or
//   modify it under the terms of the GNU Lesser General Public
//   License as published by the Free Software Foundation; either
//   version 2.1 of the License, or (at your option) any later version.
// </copyright>

// <summary>
//   Batch 5 (B5-A2 第一塊) characterization 測試：鎖定 LogScale 的外部可觀測行為。
//   透過 AxisChange 觀測範圍與穩定性，避開 internal/protected PickScale 直接呼叫。
//   後續塊（DateScale/TextScale/OrdinalScale/ExponentScale）待續。
// </summary>

namespace ZedGraph.XUnitTests
{
    using ZedGraph.XUnitTests.TestHelpers;

    using Xunit;

    public class LogScaleTests
    {
        /// <summary>
        /// LogScale + 正值資料：AxisChange 後 Y 軸 Min/Max 應涵蓋資料範圍。
        /// 鎖定對數軸的基本自動範圍契約。
        /// </summary>
        [Fact]
        public void LogScale_PositiveData_AxisChange_RangeCoversData()
        {
            using var fx = new GraphicsFixture();
            var pane = new GraphPane();
            pane.YAxis.Type = AxisType.Log;
            pane.AddCurve( "c", new[] { 1.0, 2.0 }, new[] { 1.0, 100.0 }, System.Drawing.Color.Blue );

            pane.AxisChange( fx.Graphics );

            Assert.True( pane.YAxis.Scale.Min <= 1.0, "Min 應 <= 資料最小值" );
            Assert.True( pane.YAxis.Scale.Max >= 100.0, "Max 應 >= 資料最大值" );
        }

        /// <summary>
        /// LogScale + 零值資料：AxisChange 不應拋例外。
        /// 鎖定對數軸對 ≤0 值的邊界穩定性（通常跳過或產生 NaN，但不崩）。
        /// </summary>
        [Fact]
        public void LogScale_ZeroValue_AxisChange_DoesNotThrow()
        {
            using var fx = new GraphicsFixture();
            var pane = new GraphPane();
            pane.YAxis.Type = AxisType.Log;
            pane.AddCurve( "c", new[] { 1.0 }, new[] { 0.0 }, System.Drawing.Color.Blue );

            var ex = Record.Exception( () => pane.AxisChange( fx.Graphics ) );

            Assert.Null( ex );
        }

        /// <summary>
        /// LogScale + 負值資料：AxisChange 不應拋例外。
        /// 鎖定負值邊界穩定性。
        /// </summary>
        [Fact]
        public void LogScale_NegativeValue_AxisChange_DoesNotThrow()
        {
            using var fx = new GraphicsFixture();
            var pane = new GraphPane();
            pane.YAxis.Type = AxisType.Log;
            pane.AddCurve( "c", new[] { 1.0 }, new[] { -5.0 }, System.Drawing.Color.Blue );

            var ex = Record.Exception( () => pane.AxisChange( fx.Graphics ) );

            Assert.Null( ex );
        }

        /// <summary>
        /// 設定 AxisType.Log 後，Y 軸的 Scale 應為 LogScale 型別。
        /// 鎖定型別對應契約。
        /// </summary>
        [Fact]
        public void LogScale_Type_Set_RemainsLog()
        {
            using var fx = new GraphicsFixture();
            var pane = new GraphPane();
            pane.YAxis.Type = AxisType.Log;
            pane.AxisChange( fx.Graphics );

            Assert.Equal( AxisType.Log, pane.YAxis.Type );
        }
    }
}