// <copyright file="ScaleSetScaleMagTests.cs" company="ZedGraph Project">
//   This library is free software; you can redistribute it and/or
//   modify it under the terms of the GNU Lesser General Public
//   License as published by the Free Software Foundation; either
//   version 2.1 of the License, or (at your option) any later version.
// </copyright>

// <summary>
//   Regression tests for Scale.SetScaleMag 0-boundary NaN/Infinity pollution.
//   對應 C2 critical bug：當 _min 或 _max 為 0 時 Math.Log10(Math.Abs(0)) 為 -Infinity，
//   導致 _mag 被設為 int.MinValue，下游 format 計算丟 FormatException 或產生巨大負值字串。
//   預期：修復前測試失敗（_mag 變 int.MinValue 或丟例外），Batch 1 修復後測試通過。
// </summary>

namespace ZedGraph.XUnitTests
{
    using System;
    using System.Reflection;

    using Xunit;

    /// <summary>
    /// SetScaleMag 是 internal 方法。為避免動 ZedGraph.csproj 加入 InternalsVisibleTo，
    /// 採用 reflection 直接呼叫，並透過公開 Scale.Mag 屬性驗證結果。
    /// </summary>
    public class ScaleSetScaleMagTests
    {
        private const BindingFlags InstanceNonPublic = BindingFlags.Instance | BindingFlags.NonPublic;
        private const BindingFlags InstancePublic = BindingFlags.Instance | BindingFlags.Public;

        /// <summary>
        /// C2 真正觸發場景：_min = 0, _max = 0。兩個 Math.Log10(0) 都回傳 -Infinity，
        /// Max(-Inf, -Inf) = -Inf，Math.Abs(-Inf)=Inf > 3 → 不觸發 reset → _mag = int.MinValue。
        /// </summary>
        [Fact]
        public void SetScaleMag_BothMinAndMaxZero_DoesNotProduceIntMinValue()
        {
            var scale = CreateLinearScale();
            SetField(scale, "_min", 0.0);
            SetField(scale, "_max", 0.0);
            SetField(scale, "_magAuto", true);
            SetField(scale, "_formatAuto", true);

            InvokeSetScaleMag(scale, 0.0, 0.0, 1.0);

            int mag = (int)GetField(scale, "_mag");
            Assert.NotEqual(int.MinValue, mag);
        }

        /// <summary>
        /// 衍生測試：驗證 Scale.Mag 公開屬性反映 _mag 狀態。
        /// 修復前 Scale.Mag 會是 int.MinValue。
        /// </summary>
        [Fact]
        public void Mag_BothMinAndMaxZero_IsNotIntMinValue()
        {
            var scale = CreateLinearScale();
            SetField(scale, "_min", 0.0);
            SetField(scale, "_max", 0.0);
            SetField(scale, "_magAuto", true);
            SetField(scale, "_formatAuto", true);

            InvokeSetScaleMag(scale, 0.0, 0.0, 1.0);

            Assert.NotEqual(int.MinValue, scale.Mag);
        }

        /// <summary>
        /// 邊界案例：_min=0, _max=10（單邊為 0，另一邊正常）。
        /// 此情境下 Max(minMag=-Inf, maxMag=1) = 1，正常運作。
        /// 用來確認測試只在「雙 0」邊界 fail，單邊 0 應正常。
        /// </summary>
        [Fact]
        public void SetScaleMag_OneSideZero_WorksCorrectly()
        {
            var scale = CreateLinearScale();
            SetField(scale, "_min", 0.0);
            SetField(scale, "_max", 10.0);
            SetField(scale, "_magAuto", true);
            SetField(scale, "_formatAuto", true);

            InvokeSetScaleMag(scale, 0.0, 10.0, 1.0);

            int mag = (int)GetField(scale, "_mag");
            Assert.NotEqual(int.MinValue, mag);
            Assert.InRange(mag, -3, 3);
        }

        /// <summary>
        /// 透過 reflection 建立 LinearScale 實例（避免動 InternalsVisibleTo）。
        /// LinearScale 建構子：public LinearScale(Axis owner)。
        /// </summary>
        private static Scale CreateLinearScale()
        {
            var linearScaleType = Type.GetType("ZedGraph.LinearScale, ZedGraph")
                ?? throw new InvalidOperationException("找不到 ZedGraph.LinearScale 型別。");
            var ctor = linearScaleType.GetConstructor(
                InstancePublic,
                null,
                new[] { typeof(Axis) },
                null);
            Assert.NotNull(ctor);
            return (Scale)ctor.Invoke(new object[] { null });
        }

        private static void SetField(object instance, string name, object value)
        {
            var field = instance.GetType().GetField(name, InstanceNonPublic);
            Assert.NotNull(field);
            field.SetValue(instance, value);
        }

        private static object GetField(object instance, string name)
        {
            var field = instance.GetType().GetField(name, InstanceNonPublic);
            Assert.NotNull(field);
            return field.GetValue(instance);
        }

        private static void InvokeSetScaleMag(Scale scale, double min, double max, double step)
        {
            var method = typeof(Scale).GetMethod("SetScaleMag", InstanceNonPublic);
            Assert.NotNull(method);
            method.Invoke(scale, new object[] { min, max, step });
        }
    }
}
