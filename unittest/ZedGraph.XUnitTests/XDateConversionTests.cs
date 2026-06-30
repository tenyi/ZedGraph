// <copyright file="XDateConversionTests.cs" company="ZedGraph Project">
//   This library is free software; you can redistribute it and/or
//   modify it under the terms of the GNU Lesser General Public
//   License as published by the Free Software Foundation; either
//   version 2.1 of the License, or (at your option) any later version.
// </copyright>

// <summary>
//   Characterization tests for XDate 雙向轉換（T0.3.10）。
//   XDate 是純函式模組（無 GDI+ 依賴），透過 Julian Day 實作 Excel-style XL Date。
//   本測試鎖定「現有」的雙向轉換行為：
//     - DateTime ↔ XLDate round-trip 必須還原
//     - 建構子與 DateTimeToXLDate 的一致性
//     - 閏年、DayOfWeek 對應
//   Batch 3/4 若重構 XDate 內部，這些外部契約不可改變。
// </summary>

namespace ZedGraph.XUnitTests
{
    using System;

    using Xunit;

    public class XDateConversionTests
    {
        /// <summary>
        /// DateTime → XLDate → DateTime 的 round-trip 必須精確還原。
        /// 涵蓋跨年度、跨世紀、閏日等代表性日期。
        /// </summary>
        [Theory]
        [InlineData(1970, 1, 1, 0, 0, 0)]     // Unix epoch 附近
        [InlineData(2000, 1, 1, 12, 30, 45)]  // 跨世紀、含時分秒
        [InlineData(2000, 2, 29, 0, 0, 0)]    // 閏日（2000 可被 400 整除）
        [InlineData(2024, 2, 29, 23, 59, 59)] // 閏日（2024 可被 4 整除）
        [InlineData(1900, 3, 1, 6, 0, 0)]     // 含毫秒的晨間時刻
        public void DateTime_XLDate_RoundTrip_RestoresOriginal(
            int year, int month, int day, int hour, int minute, int second)
        {
            var original = new DateTime(year, month, day, hour, minute, second);
            double xl = XDate.DateTimeToXLDate(original);
            DateTime restored = XDate.XLDateToDateTime(xl);

            // 回來的日期時間應與原始一致（精確到秒，毫秒因 double 精度可能誤差，故比對到秒）
            Assert.Equal(
                new DateTime(year, month, day, hour, minute, second),
                new DateTime(restored.Year, restored.Month, restored.Day,
                             restored.Hour, restored.Minute, restored.Second));
        }

        /// <summary>
        /// 建構子 new XDate(year,month,day) 產生的 XLDate 值，
        /// 應與 DateTimeToXLDate(new DateTime(...)) 一致。
        /// 鎖定兩條轉換路徑的內部一致性。
        /// </summary>
        [Fact]
        public void Constructor_XLDate_Matches_DateTimeToXLDate()
        {
            var xDate = new XDate(2000, 1, 1);
            var dateTime = new DateTime(2000, 1, 1);

            Assert.Equal(XDate.DateTimeToXLDate(dateTime), xDate.XLDate);
        }

        /// <summary>
        /// XLDateToDayOfWeek 的回傳值應與 .NET DateTime.DayOfWeek 的整數值一致
        ///（0 = Sunday … 6 = Saturday）。
        /// 依實作 (int)(JulianDay + 1.5) % 7 推導：Monday→1，與 DayOfWeek 對齊。
        /// 以 2024-01-01（星期一）為錨點固定值，確保推導正確。
        /// </summary>
        [Fact]
        public void XLDateToDayOfWeek_2024_01_01_Returns1_ForMonday()
        {
            var monday = new DateTime(2024, 1, 1);
            double xl = XDate.DateTimeToXLDate(monday);

            Assert.Equal(1, XDate.XLDateToDayOfWeek(xl));
        }

        /// <summary>
        /// 對連續七天，XLDateToDayOfWeek 應與 (int)DateTime.DayOfWeek 完全對應。
        /// 鎖定「星期編號與 .NET 一致」的現有契約。
        /// </summary>
        [Fact]
        public void XLDateToDayOfWeek_Matches_DateTimeDayOfWeek_ForSevenDays()
        {
            var start = new DateTime(2024, 1, 1); // Monday
            for (int i = 0; i < 7; i++)
            {
                var dt = start.AddDays(i);
                double xl = XDate.DateTimeToXLDate(dt);

                Assert.Equal((int)dt.DayOfWeek, XDate.XLDateToDayOfWeek(xl));
            }
        }

        /// <summary>
        /// AddDays(1.0) 應使 XLDate 恰好增加 1.0（一天）。
        /// 鎖定 AddDays 的線性語意，供 Batch 3 重構時驗證。
        /// </summary>
        [Fact]
        public void AddDays_IncrementsXLDate_ByExactlyOne()
        {
            var xDate = new XDate(2000, 6, 15);
            double before = xDate.XLDate;

            xDate.AddDays(1.0);

            Assert.Equal(before + 1.0, xDate.XLDate);
        }

        /// <summary>
        /// AddMinutes(60*24)（即一天的分鐘數）應等同於加一天。
        /// 鎖定 AddMinutes 與 AddDays 的換算一致性。
        /// </summary>
        [Fact]
        public void AddMinutes_OneDay_Worth_EqualsAddOneDay()
        {
            var byMinutes = new XDate(2010, 1, 1);
            var byDays = new XDate(2010, 1, 1);

            byMinutes.AddMinutes(60.0 * 24.0);
            byDays.AddDays(1.0);

            Assert.Equal(byDays.XLDate, byMinutes.XLDate);
        }
    }
}
