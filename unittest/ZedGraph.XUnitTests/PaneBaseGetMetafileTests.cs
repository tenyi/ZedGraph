// <copyright file="PaneBaseGetMetafileTests.cs" company="ZedGraph Project">
//   This library is free software; you can redistribute it and/or
//   modify it under the terms of the GNU Lesser General Public
//   License as published by the Free Software Foundation; either
//   version 2.1 of the License, or (at your option) any later version.
// </copyright>

// <summary>
//   Characterization tests for PaneBase.GetMetafile（M12 資源管理前置）。
//   PaneBase.GetMetafile(width, height) 與 GetMetafile(width, height, isAntiAlias)
//   內部以 MemoryStream 作為 Metafile 建構的 placeholder（M12: PaneBase.cs:974, 1027）。
//   本測試鎖定「呼叫不拋例外、回傳非 null Metafile」的最低契約，作為把 stream
//   改為 using 後仍可正常運作的安全網。
//
//   ⚠ Sanity 等級：GDI+ Metafile 在測試環境受限，無法精確驗證繪製內容。
//   僅守護「重構後 API 仍可呼叫」與「資源釋放順序未中斷渲染流程」。
// </summary>

namespace ZedGraph.XUnitTests
{
    using System.Drawing;
    using System.Drawing.Imaging;

    using ZedGraph.XUnitTests.TestHelpers;

    using Xunit;

    public class PaneBaseGetMetafileTests
    {
        /// <summary>
        /// PaneBase.GetMetafile(int, int) 不應拋例外，且回傳非 null 的 Metafile。
        /// 對應 M12 修復前的 PaneBase.cs:1027 MemoryStream placeholder。
        /// </summary>
        [Fact]
        public void GetMetafile_WidthHeight_ReturnsNonNullMetafile()
        {
            using var fx = new GraphicsFixture(640, 480);
            var pane = new GraphPane(new RectangleF(0, 0, 640, 480),
                "title", "X", "Y");
            pane.AddCurve("c", new[] { 1.0, 2.0, 3.0, 4.0 },
                new[] { 10.0, 20.0, 15.0, 30.0 }, Color.Blue);
            pane.AxisChange(fx.Graphics);

            Metafile metafile = null;
            var ex = Record.Exception(() => metafile = pane.GetMetafile(320, 240));

            Assert.Null(ex);
            Assert.NotNull(metafile);
            metafile?.Dispose();
        }

        /// <summary>
        /// PaneBase.GetMetafile(int, int, bool) 不應拋例外，且回傳非 null 的 Metafile。
        /// 對應 M12 修復前的 PaneBase.cs:974 MemoryStream placeholder（isAntiAlias=true 路徑）。
        /// </summary>
        [Fact]
        public void GetMetafile_WidthHeightAntiAlias_ReturnsNonNullMetafile()
        {
            using var fx = new GraphicsFixture(640, 480);
            var pane = new GraphPane(new RectangleF(0, 0, 640, 480),
                "title", "X", "Y");
            pane.AddCurve("c", new[] { 1.0, 2.0, 3.0, 4.0 },
                new[] { 10.0, 20.0, 15.0, 30.0 }, Color.Blue);
            pane.AxisChange(fx.Graphics);

            Metafile metafile = null;
            var ex = Record.Exception(() => metafile = pane.GetMetafile(320, 240, true));

            Assert.Null(ex);
            Assert.NotNull(metafile);
            metafile?.Dispose();
        }
    }
}