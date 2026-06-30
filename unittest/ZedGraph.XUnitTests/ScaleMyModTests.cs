// <copyright file="ScaleMyModTests.cs" company="ZedGraph Project">
//   This library is free software; you can redistribute it and/or
//   modify it under the terms of the GNU Lesser General Public
//   License as published by the Free Software Foundation; either
//   version 2.1 of the License, or (at your option) any later version.
// </copyright>

// <summary>
//   Characterization tests for Scale.MyMod（T0.3.6）。
//   MyMod 是 protected 內部方法，實作「數學模數」：
//     y * ( x/y - floor(x/y) )
//   與 C# % 運算子關鍵差異：對負的 x，MyMod 回傳正值（落在 [0, y) 區間），
//   而 C# % 回傳與 x 同號的值。
//   Batch 3 計畫修正 MyMod 負數行為；修正前先鎖定「現狀」，確保行為變更被測試捕捉。
//
//   透過反射呼叫 protected MyMod，Scale 實例取自 GraphPane.XAxis.Scale。
// </summary>

namespace ZedGraph.XUnitTests
{
    using System;
    using System.Reflection;

    using ZedGraph.XUnitTests.TestHelpers;

    using Xunit;

    public class ScaleMyModTests
    {
        /// <summary>
        /// 正數被除數：MyMod 行為與 C# % 一致。
        /// </summary>
        [Theory]
        [InlineData(7.0, 10.0, 7.0)]
        [InlineData(17.0, 10.0, 7.0)]
        [InlineData(0.0, 10.0, 0.0)]
        [InlineData(2.5, 1.0, 0.5)]
        public void MyMod_PositiveDividend_MatchesCSharpModulus(double x, double y, double expected)
        {
            Assert.Equal(expected, InvokeMyMod(x, y));
        }

        /// <summary>
        /// 負數被除數：MyMod 回傳正值（數學模數），與 C# % 不同。
        /// 鎖定此關鍵現狀——MyMod(-1,10)=9、MyMod(-13,10)=7。
        /// </summary>
        [Theory]
        [InlineData(-1.0, 10.0, 9.0)]
        [InlineData(-13.0, 10.0, 7.0)]
        public void MyMod_NegativeDividend_ReturnsPositiveMathematicalModulus(double x, double y, double expected)
        {
            // 對照 C# % 的行為（負值），強調差異
            Assert.NotEqual(x % y, InvokeMyMod(x, y));
            Assert.Equal(expected, InvokeMyMod(x, y));
        }

        /// <summary>
        /// 除數為 0：MyMod 應回傳 0（除零防護，不拋例外）。
        /// </summary>
        [Fact]
        public void MyMod_ZeroDivisor_ReturnsZero()
        {
            var ex = Record.Exception(() => InvokeMyMod(5.0, 0.0));
            Assert.Null(ex);
            Assert.Equal(0.0, InvokeMyMod(5.0, 0.0));
        }

        /// <summary>
        /// 結果永遠落在 [0, |y|) 區間（數學模數的定義）。
        /// </summary>
        [Fact]
        public void MyMod_ResultAlwaysWithin_HalfOpenInterval()
        {
            for (int x = -20; x <= 20; x++)
            {
                double result = InvokeMyMod(x, 7.0);
                Assert.True(result >= 0.0 && result < 7.0,
                    $"x={x} 時 MyMod={result} 超出 [0,7)");
            }
        }

        // ===== 反射 helper =====

        /// <summary>
        /// 透過反射呼叫 protected Scale.MyMod(double, double)。
        /// Scale 實例取自一個臨時 GraphPane 的 XAxis（MyMod 不依賴 scale 內部狀態）。
        /// </summary>
        private static double InvokeMyMod(double x, double y)
        {
            using var fx = new GraphicsFixture();
            var pane = new GraphPane();
            pane.AxisChange(fx.Graphics); // 確保 Scale 實例可用
            Scale scale = pane.XAxis.Scale;

            var method = typeof(Scale).GetMethod(
                "MyMod",
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[] { typeof(double), typeof(double) },
                null);
            Assert.NotNull(method);
            return (double)method.Invoke(scale, new object[] { x, y });
        }
    }
}
