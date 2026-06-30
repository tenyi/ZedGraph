// <copyright file="ValueHandlerTests.cs" company="ZedGraph Project">
//   This library is free software; you can redistribute it and/or
//   modify it under the terms of the GNU Lesser General Public
//   License as published by the Free Software Foundation; either
//   version 2.1 of the License, or (at your option) any later version.
// </copyright>

// <summary>
//   Characterization tests for ValueHandler（T0.3.11）。
//   ValueHandler 僅有兩個公開方法：靜態 GetValues、實例 BarCenterValue。
//   （原計畫文件誤載有 Average/Cum/Low/High，已在此修正為實際 API。）
//   本測試鎖定 GetValues 的防護契約與 baseVal 指派，供 Batch 3/4 重構時守護。
// </summary>

namespace ZedGraph.XUnitTests
{
    using ZedGraph.XUnitTests.TestHelpers;

    using Xunit;

    public class ValueHandlerTests
    {
        /// <summary>
        /// null 曲線：GetValues 應回傳 false，三個 out 值皆為 PointPair.Missing。
        /// 鎖定空值防護契約。
        /// </summary>
        [Fact]
        public void GetValues_NullCurve_ReturnsFalse_AndMissingOutValues()
        {
            using var fx = new GraphicsFixture();
            var pane = new GraphPane();
            pane.AxisChange(fx.Graphics);

            bool result = ValueHandler.GetValues(pane, null, 0,
                out double baseVal, out double lowVal, out double hiVal);

            Assert.False(result);
            Assert.Equal(PointPair.Missing, baseVal);
            Assert.Equal(PointPair.Missing, lowVal);
            Assert.Equal(PointPair.Missing, hiVal);
        }

        /// <summary>
        /// iPt 越界：GetValues 應回傳 false（防護越界存取）。
        /// </summary>
        [Fact]
        public void GetValues_IndexOutOfBounds_ReturnsFalse()
        {
            using var fx = new GraphicsFixture();
            var pane = new GraphPane();
            pane.AddCurve("c", new[] { 1.0, 2.0 }, new[] { 10.0, 20.0 }, System.Drawing.Color.Blue);
            pane.AxisChange(fx.Graphics);
            var curve = pane.CurveList[0];

            bool result = ValueHandler.GetValues(pane, curve, 99,
                out _, out _, out _);

            Assert.False(result);
        }

        /// <summary>
        /// 隱藏曲線（IsVisible=false）：GetValues 應回傳 false。
        /// </summary>
        [Fact]
        public void GetValues_HiddenCurve_ReturnsFalse()
        {
            using var fx = new GraphicsFixture();
            var pane = new GraphPane();
            var curve = pane.AddCurve("c", new[] { 1.0 }, new[] { 10.0 }, System.Drawing.Color.Blue);
            curve.IsVisible = false;
            pane.AxisChange(fx.Graphics);

            bool result = ValueHandler.GetValues(pane, curve, 0, out _, out _, out _);

            Assert.False(result);
        }

        /// <summary>
        /// 合法可見曲線 + 合法索引：GetValues 應回傳 true，
        /// 且 baseVal 應等於該點的 X 值（預設 X 軸為底軸）。
        /// </summary>
        [Fact]
        public void GetValues_ValidPoint_ReturnsTrue_AndBaseValEqualsX()
        {
            using var fx = new GraphicsFixture();
            var pane = new GraphPane();
            pane.AddCurve("c", new[] { 5.0, 15.0 }, new[] { 10.0, 20.0 }, System.Drawing.Color.Blue);
            pane.AxisChange(fx.Graphics);
            var curve = pane.CurveList[0];

            bool result = ValueHandler.GetValues(pane, curve, 1,
                out double baseVal, out _, out _);

            Assert.True(result);
            Assert.Equal(15.0, baseVal);
        }
    }
}
