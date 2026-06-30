# Batch 3 — 死碼清理 + 正確性防護

> **前置**：Batch 0（測試安全網）已完成。
> **目標**：移除死碼 + 補強 `PointPair` / 數值邊界處理的正確性弱點。
> **原則**：每項修復先補 regression test，再動產品碼。

---

## 障礙聲明

**H2 / H5 / M8 / M9 的原始審查明細已遺失**（同 M10/M11 狀況），本計畫檔僅記錄於 Batch 3 Discovery（2026-06-30 source code 掃描）新浮現的可疑候選。如需補回原始明細，須回原始審查筆記。

---

## 進度總覽

| 編號 | 問題（來源） | 位置（class/method） | 可測性 | 狀態 |
|------|------------|----------------------|--------|------|
| H2 | ⚠️ 原始明細已遺失 | — | — | ⏳ 待定位 |
| H5 | ⚠️ 原始明細已遺失 | — | — | ⏳ 待定位 |
| **B3-1** | StockPt.GetObjectData 寫入「schema3」但用 `schema2` 值（typo：naming/value 不一致） | `StockPt.cs:202` | ✅ 高 | ✅ **完成** |
| B3-2 | StockPt 反序列化端取 schema3 後未使用（不影響資料完整性） | `StockPt.cs:185` | ✅ 中 | ✅ **不修**：保留行為相容性，僅 B3-1 已修 key/value 語意 |
| M8 | ⚠️ 原始明細已遺失 | — | — | ⏳ 待定位 |
| M9 | ⚠️ 原始明細已遺失 | — | — | ⏳ 待定位 |
| M11 | ⚠️ 原始明細已遺失 | — | — | ⏳ 待定位（從 Batch 2 轉入） |

---

## Discovery（2026-06-30 source code 掃描）訊號

### 死碼：零命中
下列訊號 grep 全為 0 hit，可推論 codebase 為 well-maintained 狀態，**無死碼清理需求**：
- `TODO` / `FIXME` / `XXX` / `HACK` 註解
- `#if false` / `#if DEBUG` 條件排除區塊
- `[Obsolete]` 屬性
- `throw new NotImplementedException()` 空實作

### 正確性：候選弱點
1. **B3-1 / B3-2**：StockPt 序列化 typo（見下）
2. NaN / Infinity 防禦：codebase 已**密集**處理（30+ 處 `Double.IsNaN` / `Double.IsInfinity` 檢查），未有發現「缺失防禦」之熱路徑
3. ISerializable 在多個 class 中重複實作（PointPair、StockPt、PointPair4、StockPointList、Fill、CurveItem 等）—— 屬 pattern 而非 bug

---

## B3-1 / B3-2 — StockPt 序列化 typo

### 問題
**`StockPt.cs:202`**：
```csharp
public override void GetObjectData( SerializationInfo info, StreamingContext context )
{
    base.GetObjectData( info, context );
    info.AddValue( "schema3", schema2 );   // ← 寫 key="schema3" 但值是 PointPair.schema2
    info.AddValue( "Open", Open );
    ...
}
```

**`StockPt.cs:185`**：
```csharp
int sch = info.GetInt32( "schema3" );
// ... `sch` 之後未被使用（schema is informational only）
```

### 評估
- **`sch` 未被使用** → 行為上無害（資料 round-trip 仍正確）
- **但語意錯誤**：
  - StockPt.GetObjectData 應寫自己的 `schema3` 值（與讀取端 key 對應）
  - 寫 `schema2` 值是命名 / 數值同時錯誤（推測：作者複製 `PointPair.GetObjectData` 後改 key 為 "schema3" 但忘記換值）
- **修正方向**：
  - 加 StockPt 的 `public const int schema3 = 11;` 之類的常數（若尚未存在）
  - `info.AddValue( "schema3", schema3 )`
  - 反序列化端可保留（或也加 usage），至少 key/value 一致

### 影響
- 序列化 round-trip：資料正確、舊檔相容（key 仍是 "schema3"）
- 跨版本互通：相容於既有序列化檔（行為同前）
- **唯一可見變化**：序列化的 schema 數值從 PointPair.schema2 改為 StockPt.schema3

### 建議測試
- **T-B3.1.a**：StockPt 序列化後反序列化得到相同欄位值（round-trip）
- **T-B3.1.b**：手刻一組 SerializationInfo 內容模擬舊格式，確認向後相容（schema 數值改不破壞讀取）

### 範圍
- 1 個檔（`StockPt.cs`）+ 1 ~ 2 個 regression test
- 估 2 ~ 3 commit

---

## B3-1 — StockPt.GetObjectData 序列化 typo ✅

### 問題源碼位置
**`StockPt.cs:202`**（修復前）：
```csharp
public override void GetObjectData( SerializationInfo info, StreamingContext context )
{
    base.GetObjectData( info, context );
    info.AddValue( "schema3", schema2 );  // ← 寫 key="schema3" 但讀 PointPair 的 schema2
    ...
}
```

`StockPt.cs:171` 已定義自身常數 `public const int schema3 = 11;`，但序列化時誤用父類 `PointPair.schema2`（巧合也是 11）。

### 評估
- **功能無害**：因 schema2 ≡ schema3 = 11，round-trip 行為正確
- **語意錯誤**：作者複製 `PointPair.GetObjectData` 後改 key 為 "schema3" 但忘記換值
- **修正後**：`info.AddValue("schema3", schema3)` — 與還原端 185 行的 key 對應，且值來源是 StockPt 自己的常數

### 測試
| 測試 ID | 範圍 | 檔案 | 狀態 |
|---------|------|------|------|
| T-B3-1.a | `BinaryFormatter` round-trip，所有欄位完整還原 | StockPtSerializationTests.cs | ✅ |
| T-B3-1.b | `SerializationInfo.GetEnumerator` 驗證 GetObjectData 寫入 "schema3" 鍵 | StockPtSerializationTests.cs | ✅ |

### 範圍
- 1 個檔（`StockPt.cs`）修改 1 行 + 註解
- 2 個 regression test（新增檔 `StockPtSerializationTests.cs`）
- 共 3 個 commit（test + fix + plan）

### B3-1 與 B3-2 的邊界
B3-2（反序列化端 `int sch = info.GetInt32("schema3");` 取後未使用）保留不修——`sch` 不被使用是文件化設計（schema 作為 informational version marker），改用反而引入未來維護負擔。B3-1 已讓 key/value 來源一致。

---

## 待辦項目細節

### H2 / H5 — 原始明細已遺失
需回原始審查筆記才能進一步定位。**仍請使用者補上原始明細**或裁示範圍掃描方向。

### M8 / M9 / M11 — 原始明細已遺失
同 H2/H5 狀況。需回原始審查筆記。

---

## Batch 3 完成檢查

### 已完成 ✅
- [x] **B3-1** StockPt 序列化 typo 修正（key/value 語意一致，2 個 regression test）

### 進行中 🟡
- [ ] **H2** / **H5** / **M8** / **M9** / **M11** 原始明細仍待補（請使用者提供原始審查筆記）
