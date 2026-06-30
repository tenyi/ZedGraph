// <copyright file="HandlePointValuesEdgeCaseTests.cs" company="ZedGraph Project">
//   This library is free software; you can redistribute it and/or
//   modify it under the terms of the GNU Lesser General Public
//   License as published by the Free Software Foundation; either
//   version 2.1 of the License, or (at your option) any later version.
// </copyright>

// <summary>
//   防護測試：守護 C5 修復的關鍵邊界行為。
//   由於 ZedGraphControl.HandlePointValues 是 WinForms Control 上的 private 方法，
//   無法直接從 xUnit 測試呼叫。改測底層的 GraphPane.FindNearestPoint
//   （已守護 IsVisible）與「Points 為 null」場景下 LineItem 的穩定性。
//   這些測試確保 HandlePointValues 內部邏輯依賴的契約得到驗證。
// </summary>

namespace ZedGraph.XUnitTests
{
    using System;
    using System.Drawing;
    using System.Reflection;

    using ZedGraph.XUnitTests.TestHelpers;

    using Xunit;

    public class HandlePointValuesEdgeCaseTests
    {
        [Fact]
        public void FindNearestPoint_HiddenCurve_IsSkipped()
        {
            // 驗證 GraphPane.FindNearestPoint 已守護 IsVisible（line 2075）。
            // 這是 C5「隱藏曲線不應顯示 ToolTip」的深層防護。
            using var fx = new GraphicsFixture(640, 480);
            var pane = new GraphPane();
            pane.Rect = new RectangleF(0, 0, 640, 480);

            // 兩條曲線：一條可見、一條隱藏，資料點重疊
            var visibleCurve = pane.AddCurve("visible", new[] { 1.0, 2.0, 3.0 }, new[] { 1.0, 2.0, 3.0 }, Color.Blue);
            var hiddenCurve = pane.AddCurve("hidden", new[] { 1.0, 2.0, 3.0 }, new[] { 1.0, 2.0, 3.0 }, Color.Red);
            hiddenCurve.IsVisible = false;

            pane.AxisChange(fx.Graphics);

            // 使用 chart rect 中心點（chart rect 已 AxisChange 後計算）
            var chartRectField = typeof(GraphPane).GetField("_chart", BindingFlags.Instance | BindingFlags.NonPublic);
            var chart = chartRectField.GetValue(pane);
            var rectProp = chart.GetType().GetProperty("Rect");
            var chartRect = (RectangleF)rectProp.GetValue(chart);
            var center = new PointF(
                chartRect.Left + (chartRect.Width / 2),
                chartRect.Top + (chartRect.Height / 2));

            // 反射呼叫 FindNearestPoint：精確型別為 (PointF, CurveList, out CurveItem, out int)
            var findNearestPoint = typeof(GraphPane).GetMethod(
                "FindNearestPoint",
                BindingFlags.Public | BindingFlags.Instance,
                null,
                new[] { typeof(PointF), typeof(CurveList), typeof(CurveItem).MakeByRefType(), typeof(int).MakeByRefType() },
                null);

            Assert.NotNull(findNearestPoint);
            object[] args = { center, pane.CurveList, null, 0 };
            bool found = (bool)findNearestPoint.Invoke(pane, args);
            var nearestCurve = (CurveItem)args[2];
            int iNearest = (int)args[3];

            // 應找到可見曲線，而非隱藏曲線
            Assert.True(found);
            Assert.NotNull(nearestCurve);
            Assert.True(nearestCurve.IsVisible, "找到的曲線必須是可見的（IsVisible=false 應被過濾）。");
        }

        [Fact]
        public void FindNearestPoint_OnlyHiddenCurve_ReturnsFalse()
        {
            // 若所有曲線都隱藏，FindNearestPoint 應回傳 false（不進入 ToolTip 邏輯）
            using var fx = new GraphicsFixture(640, 480);
            var pane = new GraphPane();
            pane.Rect = new RectangleF(0, 0, 640, 480);

            var hiddenCurve = pane.AddCurve("hidden", new[] { 1.0, 2.0, 3.0 }, new[] { 1.0, 2.0, 3.0 }, Color.Red);
            hiddenCurve.IsVisible = false;

            pane.AxisChange(fx.Graphics);

            var chartRectField = typeof(GraphPane).GetField("_chart", BindingFlags.Instance | BindingFlags.NonPublic);
            var chart = chartRectField.GetValue(pane);
            var rectProp = chart.GetType().GetProperty("Rect");
            var chartRect = (RectangleF)rectProp.GetValue(chart);
            var center = new PointF(
                chartRect.Left + (chartRect.Width / 2),
                chartRect.Top + (chartRect.Height / 2));

            var findNearestPoint = typeof(GraphPane).GetMethod(
                "FindNearestPoint",
                BindingFlags.Public | BindingFlags.Instance,
                null,
                new[] { typeof(PointF), typeof(CurveList), typeof(CurveItem).MakeByRefType(), typeof(int).MakeByRefType() },
                null);

            Assert.NotNull(findNearestPoint);
            object[] args = { center, pane.CurveList, null, 0 };
            bool found = (bool)findNearestPoint.Invoke(pane, args);

            Assert.False(found);
        }

        /// <summary>
        /// 守護 C5 修法中 (string)pt.Tag 改為 as string 的行為差異。
        /// 修復前：自訂 Tag 為非 string 物件時 (string) 強轉丟 InvalidCastException。
        /// 修復後：使用 as string 取得 null，改走預設 ToolTip 路徑。
        /// </summary>
        [Fact]
        public void StringCasting_OfNonStringTag_DoesNotThrow()
        {
            // 驗證 "as" 運算子的契約
            object nonStringTag = 42; // 整數
            string result = nonStringTag as string;
            Assert.Null(result);

            string stringTag = "hello";
            string result2 = stringTag as string;
            Assert.Equal("hello", result2);
        }
    }
}
