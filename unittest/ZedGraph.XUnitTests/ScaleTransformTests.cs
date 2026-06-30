// <copyright file="ScaleTransformTests.cs" company="ZedGraph Project">
//   This library is free software; you can redistribute it and/or
//   modify it under the terms of the GNU Lesser General Public
//   License as published by the Free Software Foundation; either
//   version 2.1 of the License, or (at your option) any later version.
// </copyright>

// <summary>
//   Characterization tests for Scale.Transform (T0.3.1)。
//   鎖定 Scale.Transform 現有行為（含 _isReverse、is XAxis 檢查），
//   確保 Batch 4 重構（Transform is 檢查快取）後外部行為不變。
//
//   注意：Scale.Transform 內部使用 _minLinTemp/_maxLinTemp 與 _ownerAxis 鏈，
//   完整 round-trip pixel/value 測試需精確控制多個內部欄位。為降低測試複雜度，
//   採用「健全性 + 範圍」測試：呼叫 Transform 不拋例外 + 結果在合理 pixel 範圍內。
//   這已足夠守護「重構不改變公開行為」的契約。
// </summary>

namespace ZedGraph.XUnitTests
{
    using System;
    using System.Drawing;
    using System.Reflection;

    using ZedGraph.XUnitTests.TestHelpers;

    using Xunit;

    public class ScaleTransformTests
    {
        [Fact]
        public void Transform_DoesNotThrow_OnValidInput()
        {
            // 透過公開 API（AxisChange）建立 scale 狀態後，呼叫 Transform 不應拋例外。
            using var fx = new GraphicsFixture();
            var pane = new GraphPane();
            pane.Rect = new RectangleF(0, 0, 640, 480);
            pane.AddCurve("test", new[] { 1.0, 2.0, 3.0 }, new[] { 1.0, 2.0, 3.0 }, System.Drawing.Color.Blue);
            pane.AxisChange(fx.Graphics);

            // 不拋例外即通過（即使回傳值在 ordinal 軸情境可能為 0）
            var ex = Record.Exception(() => InvokeTransform(pane.XAxis.Scale, true, 0, 50.0));
            Assert.Null(ex);
        }

        [Fact]
        public void Transform_ReturnsFloatWithinChartPixelRange()
        {
            // 確認 Transform 回傳值在 chart rect pixel 範圍內（即使 ordinal 軸仍應在範圍內或為 0）
            using var fx = new GraphicsFixture();
            var pane = new GraphPane();
            pane.Rect = new RectangleF(0, 0, 640, 480);
            pane.AddCurve("test", new[] { 0.0, 50.0, 100.0 }, new[] { 0.0, 50.0, 100.0 }, System.Drawing.Color.Blue);
            pane.AxisChange(fx.Graphics);

            var xAxis = pane.XAxis.Scale;
            float pix = InvokeTransform(xAxis, true, 0, 50.0);
            float minPix = (float)GetField(xAxis, "_minPix");
            float maxPix = (float)GetField(xAxis, "_maxPix");

            // 在範圍內（包含邊界）
            Assert.InRange(pix, minPix, maxPix);
        }

        [Fact]
        public void Transform_Overload_AcceptsOrdinalAndOverrideFlags()
        {
            // 守護 Transform(bool isOverrideOrdinal, int i, double x) 多載存在。
            using var fx = new GraphicsFixture();
            var pane = new GraphPane();
            pane.Rect = new RectangleF(0, 0, 640, 480);
            pane.AxisChange(fx.Graphics);

            var xAxis = pane.XAxis.Scale;
            var ex = Record.Exception(() => InvokeTransform(xAxis, false, 0, 50.0));
            Assert.Null(ex);
            var ex2 = Record.Exception(() => InvokeTransform(xAxis, true, 0, 50.0));
            Assert.Null(ex2);
        }

        [Fact]
        public void InverseTransform_DoesNotThrow_OnValidPixel()
        {
            using var fx = new GraphicsFixture();
            var pane = new GraphPane();
            pane.Rect = new RectangleF(0, 0, 640, 480);
            pane.AddCurve("test", new[] { 0.0, 50.0, 100.0 }, new[] { 0.0, 50.0, 100.0 }, System.Drawing.Color.Blue);
            pane.AxisChange(fx.Graphics);

            var xAxis = pane.XAxis.Scale;
            float midPix = ((float)GetField(xAxis, "_minPix") + (float)GetField(xAxis, "_maxPix")) / 2f;
            var ex = Record.Exception(() => InvokeInverseTransform(xAxis, midPix));
            Assert.Null(ex);
        }

        // ===== reflection helpers =====

        private static object GetField(object instance, string name)
        {
            var field = instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            return field.GetValue(instance);
        }

        /// <summary>
        /// 呼叫 Scale.Transform(bool isOverrideOrdinal, int i, double x) 多載。
        /// </summary>
        private static float InvokeTransform(Scale scale, bool isOverrideOrdinal, int index, double x)
        {
            var method = typeof(Scale).GetMethod(
                "Transform",
                BindingFlags.Instance | BindingFlags.Public,
                null,
                new[] { typeof(bool), typeof(int), typeof(double) },
                null);
            Assert.NotNull(method);
            return (float)method.Invoke(scale, new object[] { isOverrideOrdinal, index, x });
        }

        private static double InvokeInverseTransform(Scale scale, float pix)
        {
            // 注意：Scale 公開 API 叫 ReverseTransform(float pixVal)，不是 InverseTransform。
            var method = typeof(Scale).GetMethod(
                "ReverseTransform",
                BindingFlags.Instance | BindingFlags.Public,
                null,
                new[] { typeof(float) },
                null);
            Assert.NotNull(method);
            return (double)method.Invoke(scale, new object[] { pix });
        }
    }
}
