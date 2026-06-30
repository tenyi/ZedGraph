# Batch 5 — 品質 / 現代化 / 測試衝刺

> **前置**：Batch 0–4 已完成（115 測試全綠，working tree 乾淨）。
> **目標**：coverage 衝刺（line ~10% → ≥30%）+ 低風險現代化補強 + 補強品質測試。
> **原則**：純重構 / 現代化項目須先補 characterization test（鎖定現有行為），再動產品碼。
> **來源**：原審查 6 個 Low（L1–L6）+ README 第 57 行指定之「補 XDate/Scale 全系列/ValueHandler 系統性測試」。

---

## 障礙聲明

**L1–L6 原始審查明細已遺失**（同 H2/H5/M8/M9/M11 狀況，見 03 計畫檔）：

| 編號 | 已知資訊 | 狀態 |
|------|----------|------|
| L1 | 無記載 | ⚠️ 遺失 |
| L2 | 無記載 | ⚠️ 遺失 |
| L3 | 無記載 | ⚠️ 遺失 |
| L4 | 無記載 | ⚠️ 遺失 |
| L5 | BinaryFormatter（範例專案 `controltest/`、`libtest/`） | ✅ 已歸 **Batch 2**（02 計畫檔），核心風險低 |
| L6 | 無記載 | ⚠️ 遺失 |

> **決策（2026-07-01）**：使用者裁示採「先產計畫再執行」。
> 因 L1/L2/L3/L4/L6 無法還原，本批次實作主體改為 **Part A（coverage 衝刺，README 明確指定）+ Part B（現代化 discovery 候選）**。
> L1–L4/L6 標註「遺失待補」，待使用者提供原始審查筆記後再續。

---

## 現代化 Discovery（2026-07-01 核心掃描）

比照 B6 模式，對 `source/ZedGraph/` 做現代化訊號掃描：

| 維度 | 命中 | 結論 |
|------|------|------|
| `[Obsolete]` 屬性 | 0 | codebase 無標記為過時的 API |
| `string.Format(` | 1（`LogScale.cs:402`） | 幾乎無插值字串化空間 |
| `StringComparison`（顯式） | 0 | 見 MOD-1，存在隱含文化敏感比較 |
| `#nullable`（NRT） | 0 | 未啟用可空參考型別（見 MOD-NRT） |
| 非泛型集合 `ArrayList` | 3（2 檔） | 見 MOD-3，架構級 |

**總結**：codebase 已相當現代化，現代化補強空間小。Batch 5 主體為 **coverage 衝刺**。

---

## 進度總覽

| 編號 | 項目 | 範圍 | 可測性 | 狀態 |
|------|------|------|--------|------|
| **Part A — Coverage 衝刺** | | | | |
| B5-A1 | XDate 系統性測試（擴充 `XDateConversionTests`） | `XDate.cs` | ✅ 高 | ⏳ 待辦 |
| B5-A2 | Scale 全系列測試（Text/Log/Date/Ordinal/Exponent） | `Scale.cs` 各子類 | ✅ 中 | ⏳ 待辦 |
| B5-A3 | ValueHandler 系統性測試（擴充 `ValueHandlerTests`） | `ValueHandler.cs` | ✅ 高 | ⏳ 待辦 |
| B5-A4 | coverage 量測（FineCodeCoverage）與記錄 | — | — | ⏳ 待辦 |
| **Part B — 現代化補強** | | | | |
| B5-B1 (MOD-1) | `XDate.cs:1574-1605` 9 處 `IndexOf` 加 `StringComparison.Ordinal` | `XDate.cs` | ✅ 高（純重構） | ⏳ 待辦 |
| B5-B2 (MOD-2) | `LogScale.cs:402` `string.Format` → `ToString` | `LogScale.cs` | ✅ 高（純重構） | ⏳ 待辦 |
| B5-B3 (MOD-3) | `SamplePointList` / `SampleMultiPointList` `ArrayList` → `List<T>` | 2 檔 | 🟡 中（架構級，待評估） | ⏳ 待辦 |
| **Part C — 遺失項目** | | | | |
| L1–L4/L6 | 原始明細遺失 | — | — | ⏳ 待使用者補 |

---

## Part A — Coverage 衝刺（主要工作量）

> README 第 57 行明確指定：「補 XDate/Scale 全系列/ValueHandler 系統性測試」。
> 目標：line coverage ~10% → ≥30%。硬性門檻為「每個被觸碰 method 皆有測試」。

### A1 — XDate 系統性測試（擴充 `XDateConversionTests.cs`）

既有骨架：雙向轉換 sanity。擴充面向：

| 測試面向 | 對應 method | 性質 |
|----------|------------|------|
| XLDate ↔ DateTime 雙向（含閏年、跨世紀） | `XDate(double)` / `DateTime` / `XLDateToDateTime` | 有意義 |
| Julian Day 邊界（`JulDayMin` / `JulDayMax`） | `JulDayToXLDate` / `XLDateToJulianDay` | 有意義 |
| 無效日期例外（NaN、範圍外、year<1） | 建構函式 | 有意義（例外） |
| 格式化 token 解析（`[d][h][m][s][f]`） | `ToString(double, string)` | 有意義（與 B5-B1 相關） |
| 取值器（Year/Month/Day/Hour/Minute/Second） | 各 getter | 有意義 |

### A2 — Scale 全系列測試

既有骨架：`ScaleTransformTests`（Transform/ReverseTransform）、`LinearScalePickScaleTests`、`ScaleMyModTests`。擴充：

| Scale 子類 | 擴充面向 |
|-----------|----------|
| `LogScale` | 對數座標 Transform/ReverseTransform、零/負值處理、PickScale 刻度 |
| `DateScale` | 日期刻度 Transform、`CalcNumLabels`、`PickScale` |
| `TextScale` | 序數對應、`PickScale` |
| `OrdinalScale` / `LinearAsOrdinalScale` | 序數轉換 |
| `ExponentScale` | 指數刻度 |

### A3 — ValueHandler 系統性測試（擴充 `ValueHandlerTests.cs`）

既有骨架：`GetValues` sanity。擴充面向：

| 測試面向 | 對應 method |
|----------|------------|
| `NearestPoint`（找最近資料點） | `NearestPoint` |
| `DataRange`（曲線資料範圍） | `DataRange` |
| `GetValues` 各類曲線型（Line/Bar 等） | `GetValues` |

### A4 — Coverage 量測

- 命令列 coverlet 對 net48+Exe 不相容（00 計畫檔 T0.1.3 已驗證）。
- 採 fallback：FineCodeCoverage（使用者 VS 環境已裝）量測，記錄 line/branch-rate。
- 若 FCC 量測後 <30%，補強熱路徑（`PointPairList`、`CurveList`）測試。

---

## Part B — 現代化補強

### B5-B1（MOD-1）— `XDate.cs` IndexOf 加 `StringComparison.Ordinal`

**問題源碼**（`source\ZedGraph\XDate.cs:1574-1605`，9 處）：
```csharp
if ( fmtStr.IndexOf("[d]") >= 0 )
if ( fmtStr.IndexOf("[h]") >= 0 || fmtStr.IndexOf("[hh]") >= 0 )
// ... [m][s][f][ff][fff][ffff][fffff]
```

**評估**：
- net48 上 `string.IndexOf(string)` 採當前文化特性比較；某些文化（如土耳其）對 ASCII 括號雖無歧義，但顯式 `StringComparison.Ordinal` 為正確慣例。
- 純重構（外部行為不變）→ 先補 characterization test 鎖定格式化輸出，再改。

**範圍**：1 檔（`XDate.cs`）9 行 + characterization test。

### B5-B2（MOD-2）— `LogScale.cs` string.Format → ToString

**問題源碼**（`source\ZedGraph\LogScale.cs:402`）：
```csharp
return string.Format( "{0:F0}", dVal );   // → return dVal.ToString( "F0" );
```

**評估**：純風格，行為等價。極低價值，但可一併現代化。先補 characterization test。

**範圍**：1 檔 1 行 + characterization test。

### B5-B3（MOD-3）— ArrayList → List&lt;T&gt;（架構級，待評估）

**位置**：
- `source\ZedGraph\SamplePointList.cs:85,177`（`private ArrayList list;`）
- `source\ZedGraph\SampleMultiPointList.cs:174`（`DataCollection = new ArrayList();`）

**評估**：
- `SamplePointList` / `SampleMultiPointList` 為 `IPointList` 範例實作（非渲染熱路徑）。
- 改 `List<T>` 涉及型別參數確認、序列化相容、下游 `foreach` 行為。
- **風險中等**，需完整 characterization test 護航。
- **建議**：若評估顯示變更面過大，則標註「保留 ArrayList 以維持序列化相容」不修。

**範圍**：2 檔 + characterization test。

### MOD-NRT — 可空參考型別（不建議本批次）

- 全專案 `#nullable` 0 hit。
- net48 + 舊式 csproj 啟用 NRT 需 `<Nullable>enable</Nullable>`，將產生大量 warning，範圍過大。
- **決策**：本批次不啟用，標註為「未來評估」。

---

## Part C — L1–L4 / L6（遺失，待補）

原始審查明細遺失，無法還原。待使用者提供原始審查筆記後續辦。

---

## Batch 5 完成檢查

### 已完成 ✅
- [ ] **B5-A1** XDate 系統性測試擴充
- [ ] **B5-A2** Scale 全系列測試
- [ ] **B5-A3** ValueHandler 系統性測試擴充
- [ ] **B5-A4** Coverage 量測（FCC）line ≥ 30%
- [ ] **B5-B1** XDate IndexOf 加 StringComparison.Ordinal（含 characterization test）
- [ ] **B5-B2** LogScale string.Format → ToString（含 characterization test）
- [ ] **B5-B3** ArrayList → List&lt;T&gt;（或評估後標註不修）

### 進行中 🟡
- [ ] **L1–L4 / L6** 原始明細待補（請使用者提供原始審查筆記）

### 每批結束驗證
```powershell
dotnet test unittest\ZedGraph.XUnitTests\ZedGraph.XUnitTests.csproj
```
全綠才進下一子項。
