// <copyright file="DateScaleTests.cs" company="ZedGraph Project">
//   This library is free software; you can redistribute it and/or
//   modify it under the terms of the GNU Lesser General Public
//   License as published by the Free Software Foundation; either
//   version 2.1 of the License, or (at your option) any later version.
// </copyright>

// <summary>
//   Batch 5 (B5-A2 第二塊) characterization 測試：鎖定 DateScale 的外部可觀測行為。
//   日期軸通常設於 X 軸，資料 X 值為 OADate（double）。
//   後續塊（TextScale/OrdinalScale/ExponentScale）待續。
// </summary>

namespace ZedGraph.XUnitTests
{
    using ZedGraph.XUnitTests.TestHelpers;

    using Xunit;

    public class DateScaleTests
    {
        /// <summary>
        /// DateScale + 日期資料（X 為 OADate）：AxisChange 後 X 軸 Min/Max 應涵蓋資料範圍。
        /// 鎖定日期軸的基本自動範圍契約。
        /// </summary>
        [Fact]
        public void DateScale_PositiveData_AxisChange_RangeCoversData()
        {
            using var fx = new GraphicsFixture();
            var pane = new GraphPane();
            pane.XAxis.Type = AxisType.Date;
            // X 為 OADate：DateTime(2000,1,1).ToOADate() ≈ 36526
            pane.AddCurve( "c",
                new[] { 36526.0, 36892.0 },  // 2000-01-01, 2001-01-01
                new[] { 10.0, 20.0 },
                System.Drawing.Color.Blue );

            pane.AxisChange( fx.Graphics );

            Assert.True( pane.XAxis.Scale.Min <= 36526.0, "Min 應 <= 第一個 OADate" );
            Assert.True( pane.XAxis.Scale.Max >= 36892.0, "Max 應 >= 第二個 OADate" );
        }

        /// <summary>
        /// DateScale + 單點資料：AxisChange 不應拋例外，且應產生有效範圍。
        /// 鎖定單點邊界穩定性。
        /// </summary>
        [Fact]
        public void DateScale_SinglePoint_AxisChange_DoesNotThrow()
        {
            using var fx = new GraphicsFixture();
            var pane = new GraphPane();
            pane.XAxis.Type = AxisType.Date;
            pane.AddCurve( "c", new[] { 36526.0 }, new[] { 42.0 }, System.Drawing.Color.Blue );

            var ex = Record.Exception( () => pane.AxisChange( fx.Graphics ) );

            Assert.Null( ex );
        }

        /// <summary>
        /// 設定 AxisType.Date 後，X 軸的 Type 應保持 Date。
        /// 鎖定型別對應契約。
        /// </summary>
        [Fact]
        public void DateScale_Type_Set_RemainsDate()
        {
            using var fx = new GraphicsFixture();
            var pane = new GraphPane();
            pane.XAxis.Type = AxisType.Date;
            pane.AxisChange( fx.Graphics );

            Assert.Equal( AxisType.Date, pane.XAxis.Type );
        }
    }
}