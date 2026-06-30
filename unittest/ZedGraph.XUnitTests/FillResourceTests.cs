// <copyright file="FillResourceTests.cs" company="ZedGraph Project">
//   This library is free software; you can redistribute it and/or
//   modify it under the terms of the GNU Lesser General Public
//   License as published by the Free Software Foundation; either
//   version 2.1 of the License, or (at your option) any later version.
// </copyright>

// <summary>
//   H1 (C 方案) characterization 測試：守護 Fill 既有行為，並在 C 方案修復後
//   守護新行為（setter 重複 Dispose、IDisposable 釋放）。
//
//   C 方案設計：
//     - Fill 實作 IDisposable（釋放自有 _brush / _gradientBM / _image）
//     - Brush 屬性 setter 重複指派時，自動 Dispose 舊 Brush
//     - 不動任何 caller
//     - 重複 Dispose 不拋例外
//
//   這些測試在「修復前」鎖定現有契約，「修復後」守護新行為。
// </summary>

namespace ZedGraph.XUnitTests
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;

    using Xunit;

    public class FillResourceTests
    {
        /// <summary>
        /// T-H1.c.a — 驗證 Fill 實作 IDisposable 介面（修復後成立）。
        /// 同時驗證 Dispose() 可被重複呼叫而不拋例外。
        /// 實作 IDisposable 是 C 方案的核心契約。
        /// </summary>
        [Fact]
        public void Fill_ImplementsIDisposable_DisposeCanBeCalledTwice()
        {
            var fill = new Fill(Color.Red);

            // 修復前：Fill 不實 IDisposable，下列 cast 不成立 → 編譯失敗即契約
            // 修復後：(fill as IDisposable).Dispose() 釋放自有 GDI+ 資源
            Assert.IsAssignableFrom<IDisposable>(fill);

            var disposable = (IDisposable)fill;
            Exception first = Record.Exception(() => disposable.Dispose());
            Exception second = Record.Exception(() => disposable.Dispose());

            Assert.Null(first);
            Assert.Null(second);
        }

        /// <summary>
        /// T-H1.c.b — 鎖定「修復前」契約：Brush setter 重複指派時，最終持有的物件是後者。
        /// 修復後：除此外，前一個 Brush 還會被自動 Dispose（但不對 GDI+ 物件直接驗證以避免 flaky）。
        /// 用 using-statement 包外部 Brush 確保 GC 路徑下不洩漏（無論 setter 是否主動 Dispose）。
        /// </summary>
        [Fact]
        public void Fill_BrushSetter_HoldsLatestValue()
        {
            var fill = new Fill(Color.Red)
            {
                Type = FillType.Brush
            };

            using (var firstBrush = new SolidBrush(Color.Blue))
            {
                fill.Brush = firstBrush;
                Assert.Same(firstBrush, fill.Brush);

                using (var secondBrush = new SolidBrush(Color.Green))
                {
                    fill.Brush = secondBrush;
                    Assert.Same(secondBrush, fill.Brush);
                }
            }

            ((IDisposable)fill).Dispose();
        }

        /// <summary>
        /// T-H1.c.c — 修復後：Fill 內建構的 Brush（透過 ColorBlend / Image 建構子的內部路徑），
        /// 在 Dispose 時應一併釋放（透過「ReleaseInstance」呼叫後 SolidBrush 的後續存取可能例外）。
        /// 這裡用「取得 Brush 後、Dispose 後，純資料屬性仍可讀取」這個比較弱的契約，避免依賴
        /// GDI+ 內部實作行為。
        /// </summary>
        [Fact]
        public void Fill_Dispose_DoesNotAffectNonGdiPropertyAccess()
        {
            var fill = new Fill(Color.Red, Color.Blue, 45.0F);

            ((IDisposable)fill).Dispose();

            // 純資料屬性（不持有 GDI+ handle）在 Dispose 後仍應可讀——避免「破壞既有契約」。
            // 注意：Fill(Color c1, Color c2, float angle) 建構子把 _color 設為 color2（即終端色），
            // 詳見 Fill.cs:230。
            Assert.Equal(Color.Blue, fill.Color);
            Assert.Equal(FillType.Brush, fill.Type);
        }

        /// <summary>
        /// T-H1.c.d — Clone 後兩個 Fill 物件互不干擾，各自管理 GDI+ 資源。
        /// 鎖定「Clone 後可獨立 Dispose」這個 C 方案重要契約。
        /// </summary>
        [Fact]
        public void Fill_Clone_ThenEachCanBeDisposedIndependently()
        {
            var source = new Fill(Color.Red, Color.Blue, 0.0F);

            Fill clone = (Fill)source.Clone();

            ((IDisposable)source).Dispose();

            // 來源 Dispose 後，clone 仍應可用、再次 Dispose 不應拋例外。
            Exception cloneDisposeEx = Record.Exception(() => ((IDisposable)clone).Dispose());
            Assert.Null(cloneDisposeEx);
        }
    }
}
