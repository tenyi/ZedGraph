// <copyright file="PointPairListEditTests.cs" company="ZedGraph Project">
//   This library is free software; you can redistribute it and/or
//   modify it under the terms of the GNU Lesser General Public
//   License as published by the Free Software Foundation; either
//   version 2.1 of the License, or (at your option) any later version.
// </copyright>

// <summary>
//   Characterization tests for PointPairList 編輯行為（T0.3.8）。
//   鎖定 PointPairList 的「現有」語意，重點是 Add 與 Insert 的拷貝行為不一致：
//     - Add(PointPair)     → 內部呼叫 point.Clone()（深拷貝）
//     - Insert(int, PointPair) → 直接加入參考（未拷貝）
//   這個不一致屬於 Batch 3「正確性防護」的潛在修正目標；在修正前先鎖定現狀，
//   確保任何「統一拷貝行為」的重構都會被本測試捕捉，需同步更新斷言。
// </summary>

namespace ZedGraph.XUnitTests
{
    using System;

    using Xunit;

    public class PointPairListEditTests
    {
        /// <summary>
        /// Add(PointPair) 應對傳入的 PointPair 做深拷貝（Clone）。
        /// 加入後修改原始 PointPair 的值，不應影響清單內已儲存的內容。
        /// </summary>
        [Fact]
        public void Add_ClonesPoint_ModifyingOriginalDoesNotAffectList()
        {
            var list = new PointPairList();
            var original = new PointPair(10.0, 20.0);

            list.Add(original);

            // 透過外部參考修改原始點
            original.Y = 999.0;

            // 清單內的點應保持加入時的值（未被外部修改污染）
            Assert.Equal(20.0, list[0].Y);
        }

        /// <summary>
        /// Insert(int, PointPair) 目前「不」做深拷貝（與 Add 不一致）。
        /// 鎖定此現狀：插入後修改原始 PointPair，清單內容會隨之改變。
        /// ⚠ 此為已知不一致；Batch 3 若統一為拷貝，需將本斷言改為「不影響」。
        /// </summary>
        [Fact]
        public void Insert_PointPair_DoesNotClone_ModifyingOriginalAffectsList()
        {
            var list = new PointPairList();
            list.Add(new PointPair(1.0, 1.0)); // 佔位

            var original = new PointPair(5.0, 5.0);
            list.Insert(0, original);

            // 修改原始參考
            original.Y = 888.0;

            // 現狀：清單內容被污染（因 Insert 未拷貝）
            Assert.Equal(888.0, list[0].Y);
        }

        /// <summary>
        /// RemoveAt 應移除指定索引的點，並使 Count 遞減。
        /// 鎖定標準集合編輯行為。
        /// </summary>
        [Fact]
        public void RemoveAt_RemovesElementAt_Index_DecrementsCount()
        {
            var list = new PointPairList(new[] { 1.0, 2.0, 3.0 }, new[] { 10.0, 20.0, 30.0 });

            list.RemoveAt(1); // 移除中間點 (2.0, 20.0)

            Assert.Equal(2, list.Count);
            Assert.Equal(3.0, list[1].X); // 原第三點前移
        }

        /// <summary>
        /// indexer set 應替換指定索引的 PointPair（標準 List 語意）。
        /// </summary>
        [Fact]
        public void Indexer_Set_ReplacesElementAt_Index()
        {
            var list = new PointPairList(new[] { 1.0, 2.0 }, new[] { 10.0, 20.0 });

            list[1] = new PointPair(99.0, 88.0);

            Assert.Equal(99.0, list[1].X);
            Assert.Equal(88.0, list[1].Y);
        }

        /// <summary>
        /// 無參數建構的 PointPairList，Sorted 屬性為 false。
        /// 注意：欄位初值 _sorted=true，但無參數建構子明設 _sorted=false（兩者矛盾）。
        /// 本測試鎖定「實際現狀」——無參數建構結果 Sorted=false。
        /// </summary>
        [Fact]
        public void Sorted_IsFalse_ForNewEmptyList()
        {
            var list = new PointPairList();

            Assert.False(list.Sorted);
        }

        /// <summary>
        /// 任何 Add/Insert 編輯後，Sorted 應變為 false（標記需要重新排序）。
        /// 鎖定 _sorted 的失效語意。
        /// </summary>
        [Fact]
        public void Sorted_BecomesFalse_AfterAddOrInsert()
        {
            var list = new PointPairList();

            list.Add(new PointPair(1.0, 1.0));
            Assert.False(list.Sorted);

            list.Insert(0, new PointPair(0.0, 0.0));
            Assert.False(list.Sorted);
        }

        /// <summary>
        /// Clone() 應產生獨立的深拷貝；修改複本不影響原始清單。
        /// </summary>
        [Fact]
        public void Clone_ProducesIndependentDeepCopy()
        {
            var list = new PointPairList(new[] { 1.0, 2.0 }, new[] { 10.0, 20.0 });
            var copy = list.Clone();

            // 修改複本中的點
            copy[0].Y = -1.0;

            Assert.Equal(2, copy.Count);
            // 原始清單不受影響
            Assert.Equal(10.0, list[0].Y);
        }

        /// <summary>
        /// 建構子 PointPairList(double[], double[]) 應依序填入點，Count 等於陣列長度。
        /// </summary>
        [Fact]
        public void Constructor_FromArrays_FillsInOrder()
        {
            var list = new PointPairList(new[] { 1.0, 2.0, 3.0 }, new[] { 10.0, 20.0, 30.0 });

            Assert.Equal(3, list.Count);
            Assert.Equal(20.0, list[1].Y);
        }
    }
}
