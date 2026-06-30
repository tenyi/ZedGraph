// <copyright file="AxisCalcCrossFractionTests.cs" company="ZedGraph Project">
//   This library is free software; you can redistribute it and/or
//   modify it under the terms of the GNU Lesser General Public
//   License as published by the Free Software Foundation; either
//   version 2.1 of the License, or (at your option) any later version.
// </copyright>

// <summary>
//   B6-C-1 (C6) — Axis.CalcCrossFraction 的 min/max 對調 bug 護衛測試。
//
//   背景：CalcCrossFraction (Axis.cs:981-982) 原本把變數對調：
//       double max = Linearize(_min);   // 變數叫 max，值來自 _min
//       double min = Linearize(_max);   // 變數叫 min，值來自 _max
//   但同檔 EffectiveCrossValue (Axis.cs:908-909) 是正確的對照組：
//       double min = Linearize(_min);
//       double max = Linearize(_max);
//
//   對調會使 (max - min) 變號，frac 在兩個分支的行為互換 → 軸交叉位置方向反相。
//
//   測試場景（分支 A：XAxis 且 _isLabelsInside == crossAxis.IsReverse）：
//       crossAxis (YAxis) _min=0, _max=100；XAxis _cross=25。
//       修正前 frac = (25 - 100) / (0 - 100) = 0.75
//       修正後 frac = (25 - 0)   / (100 - 0) = 0.25
//
//   因 CalcCrossFraction 是 internal，本測試以 reflection 呼叫。
// </summary>

namespace ZedGraph.XUnitTests
{
    using System.Reflection;
    using Xunit;

    public class AxisCalcCrossFractionTests
    {
        /// <summary>
        /// T-B6-C.1 — 守護 CalcCrossFraction 的 min/max 不再對調。
        /// 修正前：frac == 0.75（min/max 對調導致分支 A 行為反相）。
        /// 修正後：frac == 0.25（與 EffectiveCrossValue 的 min/max 定義一致）。
        /// </summary>
        [Fact]
        public void CalcCrossFraction_BranchA_FracMatchesEffCrossPosition()
        {
            var pane = new GraphPane();
            Axis xAxis = pane.XAxis;   // public property
            Axis yAxis = pane.YAxis;   // public property（crossAxis）

            // 透過 reflection 設定 internal 欄位（Scale._min/_max/_isLabelsInside、Axis._cross/_crossAuto）
            SetField(yAxis.Scale, "_min", 0.0);
            SetField(yAxis.Scale, "_max", 100.0);
            SetField(xAxis, "_cross", 25.0);
            SetField(xAxis, "_crossAuto", false);
            SetField(xAxis.Scale, "_isLabelsInside", false);
            // crossAxis.IsReverse 預設 false，不需設定

            // 呼叫 internal CalcCrossFraction(GraphPane)
            float frac = (float)InvokeInternal(xAxis, "CalcCrossFraction", pane);

            // 修正後：frac 應為 0.25（effCross=25 落在 [0,100] 的四分之一處）
            Assert.Equal(0.25f, frac, 3);
        }

        // ----- reflection helpers（internal 欄位/方法存取）-----

        private static void SetField(object obj, string name, object value)
        {
            FieldInfo f = obj.GetType().GetField(
                name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.NotNull(f);
            f.SetValue(obj, value);
        }

        private static object InvokeInternal(object obj, string method, params object[] args)
        {
            MethodInfo m = obj.GetType().GetMethod(
                method, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.NotNull(m);
            return m.Invoke(obj, args);
        }
    }
}
