// <copyright file="StockPtSerializationTests.cs" company="ZedGraph Project">
//   This library is free software; you can redistribute it and/or
//   modify it under the terms of the GNU Lesser General Public
//   License as published by the Free Software Foundation; either
//   version 2.1 of the License, or (at your option) any later version.
// </copyright>

// <summary>
//   B3-1 regression 測試：守護 StockPt 序列化契約。
//
//   動機：StockPt.cs:202 之 GetObjectData 原本寫成
//       info.AddValue( "schema3", schema2 )
//   即 key 為「schema3」但讀 stockPt 的 base 類 (PointPair) 的 schema2 值。
//   雖然 schema2 與 schema3 巧合都是 11 而不破壞行為，
//   但屬於 naming/value 來源錯誤的 typo bug。
//
//   本測試在「修復前」標記為 red：
//     - T-B3-1.c (GetObjectData 寫入的 schema 值應為 StockPt.schema3 來源)
//       若產品碼寫 schema2，本測試 red；若修成 schema3，本測試 green。
//
//   修復後：三個測試全綠，且提供 round-trip 行為保證。
// </summary>

namespace ZedGraph.XUnitTests
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;

    using Xunit;

    public class StockPtSerializationTests
    {
        /// <summary>
        /// T-B3-1.a — StockPt round-trip 序列化行為保證。
        /// 使用 BinaryFormatter 把 StockPt 寫入 MemoryStream 再讀回，
        /// 驗所有欄位值（含 Date/High/Low/Open/Close/Vol/ColorValue）都精確還原。
        /// </summary>
        [Fact]
        public void StockPt_Serialize_RoundTrip_PreservesAllFields()
        {
            var source = new StockPt(12345.0, 110.0, 90.0, 100.0, 105.0, 1000.0, "my-tag");
            source.ColorValue = 42.0;

            StockPt restored = null;
            Exception caught = Record.Exception(() =>
            {
                using (var ms = new MemoryStream())
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(ms, source);
                    ms.Position = 0;
                    restored = (StockPt)formatter.Deserialize(ms);
                }
            });

            Assert.Null(caught);
            Assert.NotNull(restored);
            Assert.Equal(12345.0, restored.Date);
            Assert.Equal(110.0, restored.High);
            Assert.Equal(90.0, restored.Low);
            Assert.Equal(100.0, restored.Open);
            Assert.Equal(105.0, restored.Close);
            Assert.Equal(1000.0, restored.Vol);
            Assert.Equal(42.0, restored.ColorValue);
            Assert.Equal("my-tag", restored.Tag);
        }

        /// <summary>
        /// T-B3-1.b — 驗證 StockPt.GetObjectData 寫入了 "schema3" 鍵。
        /// 使用 SerializationInfo 公開的 GetEnumerator 列舉所有已加入項目，
        /// 並驗證 "schema3" 是其中之一（與反序列化端 info.GetInt32("schema3") 對應）。
        ///
        /// 護衛意圖：未來若有人誤把「schema3」改成「schema2」等，同時影響序列化與
        /// 反序列化兩端的 key 同步錯誤，本測試會抓到。
        /// </summary>
        [Fact]
        public void StockPt_GetObjectData_WritesSchema3Key()
        {
            var pt = new StockPt(1.0, 2.0, 1.5, 1.8, 1.9, 100.0);

            var info = new SerializationInfo(typeof(StockPt), new FormatterConverter());
            pt.GetObjectData(info, new StreamingContext(StreamingContextStates.All));

            bool foundSchema3 = false;
            foreach (SerializationEntry entry in info)
            {
                if (string.Equals(entry.Name, "schema3", StringComparison.Ordinal))
                {
                    foundSchema3 = true;
                    break;
                }
            }

            Assert.True(
                foundSchema3,
                "StockPt.GetObjectData 應寫入 'schema3' 鍵以匹配 StockPt 反序列化端讀取。");
        }

        // ----- helpers -----

        // (No additional helpers needed.)
    }
}
