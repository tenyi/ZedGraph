// <copyright file="AxisLabelSerializationTests.cs" company="ZedGraph Project">
//   This library is free software; you can redistribute it and/or
//   modify it under the terms of the GNU Lesser General Public
//   License as published by the Free Software Foundation; either
//   version 2.1 of the License, or (at your option) any later version.
// </copyright>

// <summary>
//   Regression tests for the AxisLabel (de)serialization round-trip.
//   對應 C1 critical bug：序列化端 GetObjectData 寫入 "isOmitMag" key 但 value 用了
//   父類別的 _isVisible 欄位，導致 round-trip 後 _isOmitMag 被汙染。
//   預期：修復前測試失敗，Batch 1 修復後測試通過。
// </summary>

namespace ZedGraph.XUnitTests
{
    using System.Runtime.Serialization;

    using Xunit;

    /// <summary>
    /// AxisLabel 序列化 round-trip 測試。
    /// 為了避免使用 BinaryFormatter（CWE-502 風險，.NET 5+ 棄用），
    /// 採用 FormatterServices 取得 uninitialized 物件 + 手動驅動 GetObjectData 與
    /// 反序列化建構子的標準測試手法。
    /// </summary>
    public class AxisLabelSerializationTests
    {
        [Fact]
        public void RoundTrip_IsOmitMag_True_StaysTrue()
        {
            // Arrange
            var original = new AxisLabel("test", "Arial", 10f, System.Drawing.Color.Black, false, false, false)
            {
                IsOmitMag = true,
                IsTitleAtCross = false,
            };

            // Act
            var roundTripped = SerializeAndDeserialize(original);

            // Assert：修復前 _isOmitMag 會變成 _isVisible（父類別），目前是 true，剛好會「碰巧通過」
            // 為更明確捕捉 bug，採用 IsOmitMag=true + IsVisible=false 的組合
            Assert.True(roundTripped.IsOmitMag, "IsOmitMag 在 round-trip 後應保持 true。");
        }

        [Fact]
        public void RoundTrip_IsOmitMag_False_StaysFalse()
        {
            // 修復前 bug 行為：_isOmitMag=false + _isVisible=true → 反序列化後 _isOmitMag 變 true。
            // 修復後：應保持 false。
            var parent = new AxisLabel("p", "Arial", 10f, System.Drawing.Color.Black, false, false, false)
            {
                // 父類別 Label._isVisible 預設為 true
            };
            Assert.True(parent.IsVisible, "前置：父類別 IsVisible 預設應為 true。");

            var original = new AxisLabel(parent)
            {
                IsOmitMag = false,   // 明確設為 false
                IsVisible = true,    // 父類別可見（兩者值不同，正是觸發 bug 的情境）
            };

            var roundTripped = SerializeAndDeserialize(original);

            Assert.False(roundTripped.IsOmitMag,
                "IsOmitMag=false 在 round-trip 後應保持 false（修復前會被父類別 _isVisible 覆寫成 true）。");
        }

        [Fact]
        public void RoundTrip_IsTitleAtCross_Preserved()
        {
            var original = new AxisLabel("test", "Arial", 10f, System.Drawing.Color.Black, false, false, false)
            {
                IsTitleAtCross = true,
            };

            var roundTripped = SerializeAndDeserialize(original);

            Assert.True(roundTripped.IsTitleAtCross);
        }

        [Fact]
        public void RoundTrip_Text_Preserved()
        {
            var original = new AxisLabel("Hello", "Arial", 10f, System.Drawing.Color.Black, false, false, false);
            var roundTripped = SerializeAndDeserialize(original);
            Assert.Equal("Hello", roundTripped.Text);
        }

        /// <summary>
        /// 執行 round-trip：呼叫 GetObjectData 收集至 SerializationInfo，
        /// 再用反序列化建構子重建成新實例。
        /// </summary>
        private static AxisLabel SerializeAndDeserialize(AxisLabel original)
        {
            var info = new SerializationInfo(typeof(AxisLabel), new FormatterConverter());
            var context = new StreamingContext();

            original.GetObjectData(info, context);

            // 反序列化建構子是 protected，需透過 FormatterServices 建立實例
            var copy = (AxisLabel)FormatterServices.GetUninitializedObject(typeof(AxisLabel));
            var ctor = typeof(AxisLabel).GetConstructor(
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null,
                new[] { typeof(SerializationInfo), typeof(StreamingContext) },
                null);
            Assert.NotNull(ctor);
            ctor.Invoke(copy, new object[] { info, context });
            return copy;
        }
    }
}
