// <copyright file="SerializationSchemaSourceTests.cs" company="ZedGraph Project">
//   This library is free software; you can redistribute it and/or
//   modify it under the terms of the GNU Lesser General Public
//   License as published by the Free Software Foundation; either
//   version 2.1 of the License, or (at your option) any later version.
// </copyright>

// <summary>
//   B6 Phase B — schema 鍵/值來源錯置護衛測試（同 B3-1 StockPt 模式）：
//
//   C1 / C4 兩個 class 的 GetObjectData 寫的是「自身的鍵名 (schema3)」
//   但用「父類別的 schema 值 (PointPair.schema2)」。巧合的是：所有 schema
//   版本常數都同時被升級到 11（PointPair.schema2 == 11 == 子類 schema3 == 11），
//   所以 round-trip 行為目前完全正確。
//
//   但這是 source-level 命名/值來源錯誤：作者從父類複製 GetObjectData 後改了
//   鍵但忘了換值來源。B6-B 把值來源統一到自身常數。
//
//   由於 schema2 與 schema3 同值，無法從 round-trip 結果或
//   SerializationInfo 列舉的值區分來源是 schema2 還是 schema3。
//   因此本測試採取與 B3-1 一致的策略：
//     - 守護「round-trip 不拋例外」── 守住「不能改更糟」
//     - 守護「'schema3' 鍵存在」── 若有人誤改為 "schema2" 等，本測試 red
//
//   真正的 source-level 修正（值來源統一）靠 code review 守護，
//   無法從外部單元測驗區分──已於 04 計畫檔 B6-B 段明確註記。
// </summary>

namespace ZedGraph.XUnitTests
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;

    using Xunit;

    public class SerializationSchemaSourceTests
    {
        /// <summary>
        /// T-B6-B.1.a — ArrowObj round-trip 不拋例外的 baseline 護衛。
        /// </summary>
        [Fact]
        public void ArrowObj_RoundTrip_DoesNotThrow()
        {
            var source = new ArrowObj(1.0, 2.0, 3.0, 4.0);

            ArrowObj restored = null;
            Exception caught = Record.Exception(() =>
            {
                using (var ms = new MemoryStream())
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(ms, source);
                    ms.Position = 0;
                    restored = (ArrowObj)formatter.Deserialize(ms);
                }
            });

            Assert.Null(caught);
            Assert.NotNull(restored);
        }

        /// <summary>
        /// T-B6-B.1.b — ArrowObj.GetObjectData 寫入 "schema3" 鍵（與反序列化端讀端對應）。
        /// 修補前/後皆過；守護若有人誤改鍵名為 "schema2" 等會 red。
        /// </summary>
        [Fact]
        public void ArrowObj_GetObjectData_WritesSchema3Key()
        {
            var obj = new ArrowObj(1.0, 2.0, 3.0, 4.0);
            var info = new SerializationInfo(typeof(ArrowObj), new FormatterConverter());
            obj.GetObjectData(info, new StreamingContext(StreamingContextStates.All));

            bool foundSchema3 = false;
            foreach (SerializationEntry entry in info)
            {
                if (string.Equals(entry.Name, "schema3", StringComparison.Ordinal))
                {
                    foundSchema3 = true;
                    break;
                }
            }

            Assert.True(foundSchema3, "ArrowObj.GetObjectData 應寫入 'schema3' 鍵");
        }

        /// <summary>
        /// T-B6-B.4 — PointPairCV.GetObjectData 寫入 "schema3" 鍵。
        /// 注意：PointPairCV 未直接標記 [Serializable]（僅靠 PointPair 繼承），
        ///       BinaryFormatter 對繼承 [Serializable] 的檢驗不一定吃——故只用
        ///       直接呼叫 GetObjectData + SerializationInfo 列舉的方式護衛鍵。
        /// </summary>
        [Fact]
        public void PointPairCV_GetObjectData_WritesSchema3Key()
        {
            var obj = new PointPairCV(1.0, 2.0, 3.0);
            var info = new SerializationInfo(typeof(PointPairCV), new FormatterConverter());
            obj.GetObjectData(info, new StreamingContext(StreamingContextStates.All));

            bool foundSchema3 = false;
            foreach (SerializationEntry entry in info)
            {
                if (string.Equals(entry.Name, "schema3", StringComparison.Ordinal))
                {
                    foundSchema3 = true;
                    break;
                }
            }

            Assert.True(foundSchema3, "PointPairCV.GetObjectData 應寫入 'schema3' 鍵");
        }
    }
}
