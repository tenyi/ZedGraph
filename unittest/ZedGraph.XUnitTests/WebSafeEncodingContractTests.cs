// <copyright file="WebSafeEncodingContractTests.cs" company="ZedGraph Project">
//   This library is free software; you can redistribute it and/or
//   modify it under the terms of the GNU Lesser General Public
//   License as published by the Free Software Foundation; either
//   version 2.1 of the License, or (at your option) any later version.
// </copyright>

// <summary>
//   隔離契約測試：守護 C4 修復所用的 .NET 內建編碼 API 的語意契約。
//   因 ZedGraphWeb 專案仍是 .NET 3.5 風格無法直接從 xUnit 參考，
//   採用「契約測試」驗證 BCL 的編碼方法對特殊字元的處理行為。
//   修復後的產品碼 (ZedGraphWeb.MakeAreaTag) 呼叫這些 BCL API，
//   只要這些測試通過，XSS 防護鏈中「編碼這一步」的語意即被驗證。
// </summary>

namespace ZedGraph.XUnitTests
{
    using System;
    using System.Web;

    using Xunit;

    public class WebSafeEncodingContractTests
    {
        // ===== Uri.EscapeDataString 契約 =====

        [Fact]
        public void UriEscapeDataString_Quotes_AreEncoded()
        {
            // 用來防止 href 屬性中的雙引號突破字串邊界。
            string result = Uri.EscapeDataString("javascript:alert(\"xss\")");
            Assert.DoesNotContain("\"", result);
        }

        [Fact]
        public void UriEscapeDataString_Ampersand_Encoded()
        {
            // 用來防止 href 屬性中額外的 & 注入額外參數。
            string result = Uri.EscapeDataString("a&b=c&evil=1");
            Assert.Contains("%26", result); // & 應被編碼為 %26
        }

        [Fact]
        public void UriEscapeDataString_PlainText_Unchanged()
        {
            string result = Uri.EscapeDataString("http://example.com/path");
            Assert.Equal("http%3A%2F%2Fexample.com%2Fpath", result);
        }

        // ===== HttpUtility.HtmlAttributeEncode 契約 =====

        [Fact]
        public void HtmlAttributeEncode_DoubleQuote_Encoded()
        {
            // 用來防止屬性值中的雙引號突破屬性邊界。
            string result = HttpUtility.HtmlAttributeEncode("evil\"onmouseover=\"alert(1)");
            Assert.DoesNotContain("\"", result);
        }

        [Fact]
        public void HtmlAttributeEncode_SingleQuote_Encoded()
        {
            string result = HttpUtility.HtmlAttributeEncode("evil'onmouseover='alert(1)");
            Assert.DoesNotContain("'", result);
        }

        [Fact]
        public void HtmlAttributeEncode_Ampersand_Encoded()
        {
            string result = HttpUtility.HtmlAttributeEncode("Tom & Jerry");
            Assert.Contains("&amp;", result);
        }

        [Fact]
        public void HtmlAttributeEncode_PlainText_Unchanged()
        {
            string result = HttpUtility.HtmlAttributeEncode("Hello World");
            Assert.Equal("Hello World", result);
        }
    }
}
