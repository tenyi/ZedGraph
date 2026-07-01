// <copyright file="SamplePointListTests.cs" company="ZedGraph Project">
//   This library is free software; you can redistribute it and/or
//   modify it under the terms of the GNU Lesser General Public
//   License as published by the Free Software Foundation; either
//   version 2.1 of the License, or (at your option) any later version.
// </copyright>

// <summary>
//   Batch 5 (B5-B3) characterization 測試：鎖定 SamplePointList 的 IPointList 行為，
//   作為 ArrayList→List&lt;Sample&gt; 現代化重構之護航。
//   SamplePointList 為 IPointList 範例實作，indexer 依 XType/YType 從 Sample 產生 PointPair。
// </summary>

namespace ZedGraph.XUnitTests
{
    using System;

    using Xunit;

    public class SamplePointListTests
    {
        /// <summary>
        /// 新建的 SamplePointList 應為空（Count=0），且預設 XType=Time、YType=Position。
        /// 鎖定初始狀態與預設型別。
        /// </summary>
        [Fact]
        public void NewInstance_IsEmpty_WithDefaultTypes()
        {
            var spl = new SamplePointList();

            Assert.Equal( 0, spl.Count );
            Assert.Equal( SampleType.Time, spl.XType );
            Assert.Equal( SampleType.Position, spl.YType );
        }

        /// <summary>
        /// Add 應回傳新元素的索引（ArrayList.Add 語意：末尾加入，回傳其索引），
        /// 且 Count 遞增。鎖定 Add 的回傳值契約（List&lt;T&gt;.Add 為 void，重構須補此回傳）。
        /// </summary>
        [Fact]
        public void Add_ReturnsIndex_And_IncrementsCount()
        {
            var spl = new SamplePointList();
            var s0 = new Sample { Time = new DateTime( 2000, 1, 1 ), Position = 5.0 };
            var s1 = new Sample { Time = new DateTime( 2000, 1, 2 ), Position = 8.0 };

            int idx0 = spl.Add( s0 );
            int idx1 = spl.Add( s1 );

            Assert.Equal( 0, idx0 );
            Assert.Equal( 1, idx1 );
            Assert.Equal( 2, spl.Count );
        }

        /// <summary>
        /// 預設型別（XType=Time, YType=Position）下，indexer 應回傳 PointPair，
        /// 其 X = sample.Time.ToOADate()，Y = sample.Position。
        /// 鎖定 indexer 的座標對應。
        /// </summary>
        [Fact]
        public void Indexer_DefaultTypes_ReturnsTimeAndPosition()
        {
            var spl = new SamplePointList();
            var t = new DateTime( 2000, 6, 15 );
            var s = new Sample { Time = t, Position = 42.0 };
            spl.Add( s );

            PointPair pt = spl[0];

            Assert.Equal( t.ToOADate(), pt.X );
            Assert.Equal( 42.0, pt.Y );
        }

        /// <summary>
        /// TimeDiff 型別應回傳「該樣本時間相對於第一個樣本時間」的差（OADate 差）。
        /// 第一個樣本的 TimeDiff 應為 0（自己減自己）。鎖定 GetValue 的 list[0] 相對計算。
        /// </summary>
        [Fact]
        public void GetValue_TimeDiff_IsRelativeToFirstSample()
        {
            var spl = new SamplePointList();
            var first = new Sample { Time = new DateTime( 2000, 1, 1 ), Position = 0.0 };
            var second = new Sample { Time = new DateTime( 2000, 1, 3 ), Position = 10.0 };
            spl.Add( first );
            spl.Add( second );

            // 第一個樣本：TimeDiff = 0
            Assert.Equal( 0.0, spl.GetValue( first, SampleType.TimeDiff ) );
            // 第二個樣本：TimeDiff = 2 天（OADate 單位為天）
            Assert.Equal( 2.0, spl.GetValue( second, SampleType.TimeDiff ) );
        }
    }
}
