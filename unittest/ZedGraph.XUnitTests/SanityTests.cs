// <copyright file="SanityTests.cs" company="ZedGraph Project">
//   This library is free software; you can redistribute it and/or
//   modify it under the terms of the GNU Lesser General Public
//   License as published by the Free Software Foundation; either
//   version 2.1 of the License, or (at your option) any later version.
// </copyright>

// <summary>
//   Prototype 健全性測試：確認 xUnit v3 測試基礎設施在 net481 上能建置並執行。
//   若此測試可被 dotnet test 發現並通過，代表 prototype 成功。
// </summary>

namespace ZedGraph.XUnitTests
{
    using Xunit;

    public class SanityTests
    {
        [Fact]
        public void SanityCheck_TrueIsTrue()
        {
            Assert.True(true);
        }

        [Fact]
        public void SanityCheck_GraphPaneInstantiable()
        {
            // 確認 ZedGraph 核心可從測試專案載入並實例化。
            var pane = new GraphPane();
            Assert.NotNull(pane);
        }
    }
}
