// <copyright file="PointListBuilderSmokeTests.cs" company="ZedGraph Project">
//   This library is free software; you can redistribute it and/or
//   modify it under the terms of the GNU Lesser General Public
//   License as published by the Free Software Foundation; either
//   version 2.1 of the License, or (at your option) any later version.
// </copyright>

// <summary>
//   驗證 PointListBuilder 各工廠方法的行為。
// </summary>

namespace ZedGraph.XUnitTests
{
    using System;

    using ZedGraph.XUnitTests.TestHelpers;

    using Xunit;

    public class PointListBuilderSmokeTests
    {
        [Fact]
        public void Empty_HasZeroCount()
        {
            var list = PointListBuilder.Empty();
            Assert.Empty(list);
        }

        [Fact]
        public void LinearSequence_DefaultCount_IsFive()
        {
            var list = PointListBuilder.LinearSequence();
            Assert.Equal(5, list.Count);
            Assert.Equal(0.0, list[0].X);
            Assert.Equal(0.0, list[0].Y);
            Assert.Equal(4.0, list[4].X);
            Assert.Equal(4.0, list[4].Y);
        }

        [Fact]
        public void LinearSequence_CustomRange_RespectsStep()
        {
            var list = PointListBuilder.LinearSequence(count: 3, xStart: 10, yStart: 100, xStep: 0.5, yStep: -10);
            Assert.Equal(3, list.Count);
            Assert.Equal(10.0, list[0].X);
            Assert.Equal(100.0, list[0].Y);
            Assert.Equal(10.5, list[1].X);
            Assert.Equal(90.0, list[1].Y);
            Assert.Equal(11.0, list[2].X);
            Assert.Equal(80.0, list[2].Y);
        }

        [Fact]
        public void LinearSequence_NegativeCount_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => PointListBuilder.LinearSequence(count: -1));
        }

        [Fact]
        public void WithSpecialDoubles_ContainsNaNAndInfinity()
        {
            var list = PointListBuilder.WithSpecialDoubles();
            Assert.Equal(7, list.Count);
            Assert.True(double.IsNaN(list[1].Y));
            Assert.True(double.IsPositiveInfinity(list[3].Y));
            Assert.True(double.IsNegativeInfinity(list[5].Y));
        }

        [Fact]
        public void StockSequence_DefaultCount_IsFifty()
        {
            var spl = PointListBuilder.StockSequence();
            Assert.Equal(50, spl.Count);

            // 確認 OHLC 都為有限數（.NET 4.8 無 double.IsFinite，用 !IsNaN && !IsInfinity）。
            // StockPointList 的索引子回傳 PointPair，需 GetAt 取得 StockPt。
            for (int i = 0; i < spl.Count; i++)
            {
                var pt = spl.GetAt(i);
                Assert.True(!double.IsNaN(pt.X) && !double.IsInfinity(pt.X));
                Assert.True(!double.IsNaN(pt.Open) && !double.IsInfinity(pt.Open));
                Assert.True(!double.IsNaN(pt.Close) && !double.IsInfinity(pt.Close));
                Assert.True(!double.IsNaN(pt.High) && !double.IsInfinity(pt.High));
                Assert.True(!double.IsNaN(pt.Low) && !double.IsInfinity(pt.Low));
            }
        }
    }
}
