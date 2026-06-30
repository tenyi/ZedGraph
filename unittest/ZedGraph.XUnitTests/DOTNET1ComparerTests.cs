// <copyright file="DOTNET1ComparerTests.cs" company="ZedGraph Project">
//   This library is free software; you can redistribute it and/or
//   modify it under the terms of the GNU Lesser General Public
//   License as published by the Free Software Foundation; either
//   version 2.1 of the License, or (at your option) any later version.
// </copyright>

// <summary>
//   Characterization tests for CurveItem.Comparer 泛型版（T0.3.9）。
//   CurveItem.Comparer 有兩個條件編譯版本：
//     - DOTNET1 版（非泛型 IComparer）：目前建置未定義 DOTNET1，為死碼
//     - 泛型版（IComparer<CurveItem>）：實際使用版本
//   本測試鎖定「泛型版」的排序結果，確保未來刪除 DOTNET1 死碼區塊後行為不變。
//
//   現有行為重點（皆需鎖定）：
//     - 降序排序（大值排前）
//     - 比對值的絕對值 |X| 或 |Y|
//     - 缺失/NaN/Infinity/越界(NPts<=index) 視為 null
//     - 差距 < 1e-10 視為相等
// </summary>

namespace ZedGraph.XUnitTests
{
    using Xunit;

    public class DOTNET1ComparerTests
    {
        /// <summary>
        /// SortType.XValues：依指定 index 的 |X| 降序比較。
        /// l.X 絕對值較大 → Compare 回傳 -1（l 排前）。
        /// </summary>
        [Fact]
        public void Compare_XValues_Descending_LargerFirst()
        {
            var pane = new GraphPane();
            var larger = pane.AddBar("a", new[] { 10.0 }, new[] { 1.0 }, System.Drawing.Color.Red);
            var smaller = pane.AddBar("b", new[] { 5.0 }, new[] { 1.0 }, System.Drawing.Color.Blue);

            var comparer = new CurveItem.Comparer(SortType.XValues, 0);

            // 10 > 5（降序）→ larger 在前 → 回傳 -1
            Assert.Equal(-1, comparer.Compare(larger, smaller));
            // 反向 → 回傳 1
            Assert.Equal(1, comparer.Compare(smaller, larger));
        }

        /// <summary>
        /// SortType.YValues：依 |Y| 降序比較（X 相同時驗證 Y 分支）。
        /// </summary>
        [Fact]
        public void Compare_YValues_Descending_LargerFirst()
        {
            var pane = new GraphPane();
            var larger = pane.AddBar("a", new[] { 1.0 }, new[] { 20.0 }, System.Drawing.Color.Red);
            var smaller = pane.AddBar("b", new[] { 1.0 }, new[] { 3.0 }, System.Drawing.Color.Blue);

            var comparer = new CurveItem.Comparer(SortType.YValues, 0);

            Assert.Equal(-1, comparer.Compare(larger, smaller));
        }

        /// <summary>
        /// 兩曲線值差距 < 1e-10 應視為相等，回傳 0。
        /// </summary>
        [Fact]
        public void Compare_NearlyEqualValues_ReturnsZero()
        {
            var pane = new GraphPane();
            var a = pane.AddBar("a", new[] { 5.0 }, new[] { 1.0 }, System.Drawing.Color.Red);
            var b = pane.AddBar("b", new[] { 5.0 + 1e-11 }, new[] { 1.0 }, System.Drawing.Color.Blue);

            var comparer = new CurveItem.Comparer(SortType.XValues, 0);

            Assert.Equal(0, comparer.Compare(a, b));
        }

        /// <summary>
        /// null 處理契約：
        ///   Compare(null, null) = 0
        ///   Compare(null, curve) = -1
        ///   Compare(curve, null) = 1
        /// </summary>
        [Fact]
        public void Compare_NullHandling_ReturnsExpectedSigns()
        {
            var pane = new GraphPane();
            var curve = pane.AddBar("a", new[] { 1.0 }, new[] { 1.0 }, System.Drawing.Color.Red);
            var comparer = new CurveItem.Comparer(SortType.XValues, 0);

            Assert.Equal(0, comparer.Compare(null, null));
            Assert.Equal(-1, comparer.Compare(null, curve));
            Assert.Equal(1, comparer.Compare(curve, null));
        }

        /// <summary>
        /// index 越界（NPts <= index）時，該曲線視為 null。
        /// 兩條單點曲線在 index=1 比較 → 兩者皆視為 null → 回傳 0。
        /// </summary>
        [Fact]
        public void Compare_IndexExceedsNPts_TreatedAsNull()
        {
            var pane = new GraphPane();
            var a = pane.AddBar("a", new[] { 1.0 }, new[] { 1.0 }, System.Drawing.Color.Red);
            var b = pane.AddBar("b", new[] { 2.0 }, new[] { 2.0 }, System.Drawing.Color.Blue);

            // index=1，但兩曲線 NPts=1（1 <= 1）→ 皆視為 null
            var comparer = new CurveItem.Comparer(SortType.XValues, 1);

            Assert.Equal(0, comparer.Compare(a, b));
        }

        /// <summary>
        /// 負值取絕對值比較：-20 的絕對值 20 大於 5 → 排前。
        /// 鎖定 Math.Abs 行為。
        /// </summary>
        [Fact]
        public void Compare_UsesAbsoluteValue_OfX()
        {
            var pane = new GraphPane();
            var neg = pane.AddBar("a", new[] { -20.0 }, new[] { 1.0 }, System.Drawing.Color.Red);
            var pos = pane.AddBar("b", new[] { 5.0 }, new[] { 1.0 }, System.Drawing.Color.Blue);

            var comparer = new CurveItem.Comparer(SortType.XValues, 0);

            // |-20|=20 > |5|=5 → neg 排前 → -1
            Assert.Equal(-1, comparer.Compare(neg, pos));
        }
    }
}
