// <copyright file="PointListBuilder.cs" company="ZedGraph Project">
//   This library is free software; you can redistribute it and/or
//   modify it under the terms of the GNU Lesser General Public
//   License as published by the Free Software Foundation; either
//   version 2.1 of the License, or (at your option) any later version.
// </copyright>

// <summary>
//   為測試快速建立常見的 <see cref="PointPairList"/> 與 <see cref="IPointList"/> 實作。
//   涵蓋：空集合、單點、線性序列、含 NaN/Infinity、含 PointPair.Missing 等邊界情境。
// </summary>

namespace ZedGraph.XUnitTests.TestHelpers
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// 測試用 <see cref="PointPairList"/> 工廠。
    /// 為常見測試場景（空集合、NaN、極值等）提供一鍵建構。
    /// </summary>
    public static class PointListBuilder
    {
        /// <summary>建立空的 <see cref="PointPairList"/>。</summary>
        public static PointPairList Empty() => new PointPairList();

        /// <summary>
        /// 建立 <paramref name="count"/> 個線性遞增資料點：X = i, Y = i, Z = 0, Tag = null。
        /// </summary>
        /// <param name="count">點數，預設 5。</param>
        /// <param name="xStart">X 起始值，預設 0。</param>
        /// <param name="yStart">Y 起始值，預設 0。</param>
        /// <param name="xStep">X 步進，預設 1。</param>
        /// <param name="yStep">Y 步進，預設 1。</param>
        public static PointPairList LinearSequence(
            int count = 5,
            double xStart = 0,
            double yStart = 0,
            double xStep = 1,
            double yStep = 1)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "點數不可為負。");
            }

            var list = new PointPairList();
            for (int i = 0; i < count; i++)
            {
                list.Add(xStart + (i * xStep), yStart + (i * yStep));
            }

            return list;
        }

        /// <summary>
        /// 建立包含 NaN 與 Infinity 的邊界測試資料。
        /// 點序：0, NaN, 1, +Inf, 2, -Inf, 3。
        /// </summary>
        public static PointPairList WithSpecialDoubles()
        {
            var list = new PointPairList();
            list.Add(0.0, 0.0);
            list.Add(double.NaN, double.NaN);
            list.Add(1.0, 1.0);
            list.Add(2.0, double.PositiveInfinity);
            list.Add(3.0, 2.0);
            list.Add(4.0, double.NegativeInfinity);
            list.Add(5.0, 3.0);
            return list;
        }

        /// <summary>
        /// 建立指定範圍的 <see cref="StockPointList"/>，每點 OHLC 略為隨機。
        /// 沿用 <c>OHLCBarItemTests.CreateStockPointList</c> 的產生邏輯。
        /// </summary>
        /// <param name="count">點數，預設 50。</param>
        /// <param name="randomSeed">隨機種子，預設 20130101（與既有測試一致）。</param>
        /// <param name="valueStepSizeMinutes">每點之間的分鐘數，預設 1。</param>
        public static StockPointList StockSequence(
            int count = 50,
            int randomSeed = 20130101,
            double valueStepSizeMinutes = 1.0)
        {
            var spl = new StockPointList();
            var rand = new Random(randomSeed);
            var xDate = new XDate(2013, 1, 1);
            double open = 50.0;

            for (int i = 0; i < count; i++)
            {
                double x = xDate.XLDate;
                double close = open + (rand.NextDouble() * 10.0) - 5.0;
                double hi = Math.Max(open, close) + (rand.NextDouble() * 5.0);
                double low = Math.Min(open, close) - (rand.NextDouble() * 5.0);
                spl.Add(new StockPt(x, hi, low, open, close, 100000));
                open = close;
                xDate.AddMinutes(valueStepSizeMinutes);
                if (XDate.XLDateToDayOfWeek(xDate.XLDate) == 6)
                {
                    xDate.AddDays(2.0);
                }
            }

            return spl;
        }
    }
}
