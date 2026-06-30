// <copyright file="LinkUrlSchemeTests.cs" company="ZedGraph Project">
//   This library is free software; you can redistribute it and/or
//   modify it under the terms of the GNU Lesser General Public
//   License as published by the Free Software Foundation; either
//   version 2.1 of the License, or (at your option) any later version.
// </copyright>

// <summary>
//   Regression tests for Link.IsSafeUrl（H7）。
//   ZedGraphControl 在點擊連結時呼叫 Process.Start(url)，url 來自 Link._url 或
//   曲線標籤（可能由資料/外部提供）。若不限制 scheme，惡意的 file:///、javascript:
//   或任意已註冊的 protocol handler 皆可能被觸發（CWE-601 / URL scheme 注入）。
//
//   本測試驗證 Link.IsSafeUrl 的 scheme 白名單（http/https/mailto），
//   作為 Process.Start 前的閘道。修復前此方法不存在；修復後應守護所有安全邊界。
// </summary>

namespace ZedGraph.XUnitTests
{
    using Xunit;

    public class LinkUrlSchemeTests
    {
        /// <summary>
        /// 安全 scheme（http/https/mailto）應被允許導航。
        /// </summary>
        [Theory]
        [InlineData("http://example.com/path")]
        [InlineData("https://example.com/path?q=1")]
        [InlineData("HTTP://EXAMPLE.COM")]      // 大小寫不拘
        [InlineData("mailto:foo@bar.com")]
        public void IsSafeUrl_AllowsSafeSchemes(string url)
        {
            Assert.True(Link.IsSafeUrl(url));
        }

        /// <summary>
        /// 危險 scheme 應被拒絕：file（本地檔案）、ftp、javascript、任意 protocol。
        /// </summary>
        [Theory]
        [InlineData("file:///C:/Windows/System32/secret.txt")]
        [InlineData("ftp://example.com/file")]
        [InlineData("javascript:alert(1)")]
        [InlineData("vbscript:msgbox(1)")]
        [InlineData("data:text/html,<script>alert(1)</script>")]
        public void IsSafeUrl_RejectsDangerousSchemes(string url)
        {
            Assert.False(Link.IsSafeUrl(url));
        }

        /// <summary>
        /// 非 URL 輸入應被拒絕：null、空字串、相對路徑、純檔案路徑。
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("not a url")]
        [InlineData("foo/bar")]                 // 相對路徑，無 scheme
        [InlineData("C:\\Windows\\system32")]   // 純檔案路徑
        public void IsSafeUrl_RejectsNonAbsoluteUrls(string url)
        {
            Assert.False(Link.IsSafeUrl(url));
        }
    }
}
