// <copyright file="SerializationFunctionalRoundTripTests.cs" company="ZedGraph Project">
//   This library is free software; you can redistribute it and/or
//   modify it under the terms of the GNU Lesser General Public
//   License as published by the Free Software Foundation; either
//   version 2.1 of the License, or (at your option) any later version.
// </copyright>

// <summary>
//   B6 Phase A — 三個 round-trip 會拋例外的 functional bug 護衛測試。
//
//   這三個 bug 都是寫端與讀端「鍵/值」不一致或「鍵漏寫」，導致
//   BinaryFormatter 序列化後反序列化失敗。修補前測試為紅，修補後為綠。
//
//   - C2: HiLowBarItem.GetObjectData 完全沒寫 schema3 鍵（讀端期待）
//   - C3: GasGaugeRegion 鍵名 minVal/maxVal 對應不到讀端的 minValue/maxValue
//   - C5: PointPair4 寫 schema2 鍵但讀端期待 schema3 鍵
//
//   測試僅守護「round-trip 不拋例外 + 鍵值還原」這個最低契約。
// </summary>

namespace ZedGraph.XUnitTests
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;

    using Xunit;

    public class SerializationFunctionalRoundTripTests
    {
        /// <summary>
        /// T-B6-A.2 — C2 護衛：HiLowBarItem round-trip 不能因漏寫 schema3 而炸例外。
        /// 修補前：base.GetObjectData 之後沒 AddValue("schema3", schema3)，
        ///         讀端 <c>info.GetInt32("schema3")</c> 拿不到 → SerializationException。
        /// 修補後：Round-trip 順利完成且 Label 文字與資料點都還原。
        /// </summary>
        [Fact]
        public void HiLowBarItem_RoundTrip_DoesNotThrow()
        {
            // 建一個簡單的 HiLowBarItem（label / 雙軸資料 / 顏色）
            double[] xs = { 1.0, 2.0, 3.0 };
            double[] ys = { 10.0, 20.0, 15.0 };
            double[] bases = { 5.0, 5.0, 5.0 };
            var source = new HiLowBarItem("test-hilow", xs, ys, bases, Color.Red);

            HiLowBarItem restored = null;
            Exception caught = Record.Exception(() =>
            {
                using (var ms = new MemoryStream())
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(ms, source);
                    ms.Position = 0;
                    restored = (HiLowBarItem)formatter.Deserialize(ms);
                }
            });

            Assert.Null(caught);
            Assert.NotNull(restored);
            // Label 是 CurveItem 的 public 屬性；Text 是 Label 的 public 屬性
            Assert.Equal("test-hilow", restored.Label.Text);
        }

        /// <summary>
        /// T-B6-A.3 — C3 護衛：GasGaugeRegion round-trip 不能因 minVal/maxVal 鍵名錯而炸例外。
        /// 修補前：寫端 AddValue("minVal", _minValue) / AddValue("maxVal", _maxValue)
        ///         讀端 GetDouble("minValue") / GetDouble("maxValue") → 鍵不存在，
        ///         fallback 預設 0.0 或擲 SerializationException。
        /// 修補後：minValue / maxValue 兩個 double 都精確還原。
        /// 注意：GasGaugeRegion.MinValue setter 把 ≤0 值改為 0，
        ///       所以測試用正值（10 / 200）以避免觸碰這個無關 guard。
        /// </summary>
        [Fact]
        public void GasGaugeRegion_RoundTrip_RestoresMinMaxValues()
        {
            // 建構子簽名：GasGaugeRegion(string label, double minVal, double maxVal, Color color)
            var source = new GasGaugeRegion("test-region", 10.0, 200.0, Color.Blue);
            // 確認初始值
            Assert.Equal(10.0, source.MinValue);
            Assert.Equal(200.0, source.MaxValue);

            GasGaugeRegion restored = null;
            Exception caught = Record.Exception(() =>
            {
                using (var ms = new MemoryStream())
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(ms, source);
                    ms.Position = 0;
                    restored = (GasGaugeRegion)formatter.Deserialize(ms);
                }
            });

            Assert.Null(caught);
            Assert.NotNull(restored);
            // 修補前：restored.MinValue == 0.0（讀不到鍵）；修補後：== 10.0
            Assert.Equal(10.0, restored.MinValue);
            Assert.Equal(200.0, restored.MaxValue);
        }

        /// <summary>
        /// T-B6-A.5 — C5 護衛：PointPair4 round-trip 不能因 schema2/schema3 鍵錯位而炸例外。
        /// 修補前：寫端 AddValue("schema2", schema3)——鍵錯（應為 "schema3" 鍵），
        ///         子 ctor GetInt32("schema3") 讀不到 → SerializationException。
        /// 修補後：Round-trip 不拋例外，且 T 屬性還原。
        /// </summary>
        [Fact]
        public void PointPair4_RoundTrip_DoesNotThrow()
        {
            // PointPair4(double x, double y, double z, double t)
            var source = new PointPair4(1.0, 2.0, 3.0, 7.5);

            PointPair4 restored = null;
            Exception caught = Record.Exception(() =>
            {
                using (var ms = new MemoryStream())
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(ms, source);
                    ms.Position = 0;
                    restored = (PointPair4)formatter.Deserialize(ms);
                }
            });

            Assert.Null(caught);
            Assert.NotNull(restored);
            Assert.Equal(7.5, restored.T);
            Assert.Equal(1.0, restored.X);
            Assert.Equal(2.0, restored.Y);
            Assert.Equal(3.0, restored.Z);
        }
    }
}
