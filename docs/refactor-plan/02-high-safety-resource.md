# Batch 2 — 安全性 + 資源管理

> **前置**：[Batch 0](./00-tests-foundation.md) 已完成（測試安全網就緒）。
> **目標**：修補安全性漏洞（XSS、URL scheme、反序列化）與 GDI+ 資源洩漏（Dispose）。
> **原則**：每項修復先補 regression / characterization test，再動產品碼。

---

## 進度總覽

| 編號 | 問題 | 位置（class/method） | 可測性 | 狀態 |
|------|------|----------------------|--------|------|
| H1 | Fill 持 GDI+ Brush 未 Dispose，缺 IDisposable | `Fill.cs`（介面/setter/Dispose） | 中（C 方案，不動 caller） | ✅ **完成**（C 方案） |
| H3 | 空 catch 吞例外 | `Line.cs:1154`、`ZedGraphControl.cs:606` | 中（契約式 catch，行為不變） | ✅ **完成**（H3.1 + H3.2） |
| H4 | stack 寫入 HTML | web（待定位） | 🔴 低（web 不可直接測） | ⏳ 待定位 |
| H7 | Process.Start 無 scheme 白名單 | `ZedGraphControl.Events.cs:446` | ✅ 高 | ✅ **完成** |
| H8 | ~~ImageObj Path Traversal~~ | — | — | ❌ **已確認誤判，移除** |
| H9 | ViewState Activator 無型別限制 | `ZedGraphWebTools.cs`（7 處） | 🔴 低（web） | ⏳ 待辦（web） |
| M10 | GasGaugeNeedle 與 MasterPane.OnDeserialization 6 處局部 GDI+ 洩漏 | `GasGaugeNeedle.cs:428/436/439/472`、`MasterPane.cs:403-404` | ✅ 高 | ✅ **完成** |
| M11 | ⚠️ 屬 Batch 3（原始計畫分類錯誤：見 README.md:40） | — | — | ⏳ 已轉交 Batch 3 |
| M12 | `GetMetafile` MemoryStream 未 Dispose | `PaneBase.cs:974, 1027` | ✅ 高 | ✅ **完成** |
| M13 | RenderedImagePath | `web/ZedGraph.Web/ZedGraphWeb.cs` | 🔴 低（web-only） | ⏳ 待辦（**web 路徑**） |
| M14 | TempFileDestructor | `web/ZedGraph.Web/TempFileDestructor.cs` | 🔴 低（web-only） | ⏳ 待辦（**web 路徑**） |
| L5 | BinaryFormatter（CWE-502） | `controltest/`、`libtest/`（範例） | — | ⏳ 範例專案，低優先 |

---

## H7 — Process.Start URL scheme 驗證 ✅

- [x] **H7.1** 新增 `Link.IsSafeUrl(string)` scheme 白名單（http/https/mailto）
  - **檔案**：`source\ZedGraph\Link.cs`（鄰近 `MakeCurveItemUrl`）
  - **邏輯**：null/空 → false；非絕對 URI → false；scheme 白名單比對
- [x] **H7.2** 在 `Process.Start` 前套用閘道
  - **檔案**：`source\ZedGraph\ZedGraphControl.Events.cs:444`
  - **修法**：`if ( !Link.IsSafeUrl( url ) ) return;` 後才 `Process.Start`
- [x] **H7.3** Regression tests（14 個）：`unittest\ZedGraph.XUnitTests\LinkUrlSchemeTests.cs`
  - 安全 scheme（http/https/mailto/大小寫）→ true
  - 危險 scheme（file/ftp/javascript/vbscript/data）→ false
  - 非 URL（null/空/相對/純路徑）→ false

---

## 待辦項目細節

### H1 — Fill/Brush GDI+ 洩漏（架構級 → 採 C 方案 ✅）
- **問題**：`Fill._brush`（TextureBrush / LinearGradientBrush）為 GDI+ 資源，但 `Fill` 與整個核心專案**未實作 IDisposable**，Brush 永不顯式 Dispose → handle 洩漏。
- **最初評估（A 方案）**：在 `Fill` 與所有 owner（11 個 instance Fill owner + `PaneBase` 基底）實作 `IDisposable` 鏈 → **變動面 12+ classes**，估 ~15 commit。
- **重新評估（C 方案，最終採行）**：限定在 `Fill.cs` 自身，不動任何 caller：
  - Fill 加 `IDisposable`，`Dispose()` 釋放 `_brush` / `_gradientBM`。
  - `_disposed` 旗標防重複 Dispose。
  - `Brush` 屬性 setter 重複指派時自動 Dispose 舊值——這是主要修補場景。
  - **不寫 finalizer**：`Brush`/`Bitmap`/`Image` 為 SafeHandle，GC 自然回收；寫 finalizer 反而引 finalization queue 成本救不回幾個 handle。
  - **不釋放 `_image`**：序列化專用欄位，caller 端負責，避免雙 Dispose。
- **A 方案設計藍圖保留**（供未來 N 個 commit 後的重啟使用）：
  14 個 owner 清單已記錄：`Chart`、`BoxObj`、`LineBase`、`Bar`、`Line`、`PieItem`、`Legend`、`GasGaugeRegion`、`GasGaugeNeedle`、`JapaneseCandleStick`、`FontSpec`、`Selection`、`PaneBase`（基底）、`Symbol`。

#### H1 變動面
| 檔案 | 變更 |
|------|------|
| `source/ZedGraph/Fill.cs` | 介面加 IDisposable、加 `_disposed`、Brush setter 重複 Dispose、加 `#region IDisposable` 的 `Dispose()` 方法 |

#### H1 測試
| 測試 ID | 範圍 | 狀態 |
|---------|------|------|
| T-H1.c.a | Fill 實作 IDisposable，重複 Dispose 不拋例外 | ✅ |
| T-H1.c.b | Brush setter 重複指派，最終持有的物件是後者 | ✅ |
| T-H1.c.c | Dispose 後純資料屬性（Color、Type）仍可讀 | ✅ |
| T-H1.c.d | Clone 後兩個 Fill 各自可獨立 Dispose | ✅ |

### H3 — 空 catch ✅
- **位置**：`Line.cs:1154`（`InterpolatePoint` 最外層 `catch { }`）、`ZedGraphControl.cs:606`（`OnPaint` 內 `_masterPane.Draw` 外層 `catch { }`）
- **評估結論**：兩個空 catch **皆為契約式設計**（刻意吞例外以保護渲染管線），非 bug。
  - `Line.cs:1154`：GDI+ 在極端座標下拋例外的最後防線（1025-1032 行註解已說明）。
  - `ZedGraphControl.cs:606`：WinForms Paint 事件由訊息迴圈觸發，使用者無法 catch，否則會拖垮 AppDomain。
- **H3.1**（`Line.cs`）：保留吞例外契約，改為 `catch (Exception ex) { Debug.WriteLine(...) }` 輸出診斷資訊。
- **H3.2**（`ZedGraphControl.cs`）：同上，加 `using System.Diagnostics;` 與說明註解。

#### 測試覆蓋狀態

| 測試 ID | 範圍 | 檔案 | 狀態 |
|---------|------|------|------|
| T-H3.1.a | 反射呼叫 `Line.InterpolatePoint` 傳 null Graphics → 驗證例外被吞 | `LineInterpolatePointCatchTests.cs` | ✅ 完成 |
| T-H3.1.b | 透過 `DebugTraceListener` 攔截 Debug.WriteLine → 驗證訊息含 "swallowed exception" | `LineInterpolatePointCatchTests.cs` | ✅ 完成 |
| T-H3.1.c | Baseline：正常輸入路徑不拋例外 | `LineInterpolatePointCatchTests.cs` | ✅ 完成 |
| T-H3.2.a | Form-level OnPaint 觸發例外 → 驗證例外被吞 + Debug 輸出 | — | ⏳ **未覆蓋**（需 Form-level test host） |

#### 未覆蓋路徑說明（T-H3.2.a）
- **原因**：`ZedGraphControl.OnPaint` 是 WinForms Control 方法，Paint 事件由 Form/Control 容器觸發，需要訊息迴圈；xUnit v3 net48 測試主機無此環境。
- **可參考的現有模式**：舊 NUnit 專案 `unittest/ZedGraph.UnitTest/ZGTest.cs:115` 使用 `form = new Form(); control = new ZedGraphControl();` 模式可觸發 OnPaint。
- **建議補測路徑**：於 NUnit 專案（已升級 3.14）或另建 WinForms test host 專案補 T-H3.2.a；本批次不做。

### M12 — `PaneBase.GetMetafile` MemoryStream 未 Dispose ✅
- **位置**：`PaneBase.cs:974` 與 `PaneBase.cs:1027`，兩處皆為 `Stream stream = new MemoryStream();` 作為 Metafile 建構的 placeholder。
- **問題**：stream 變數在 method 結束時失去引用，呼叫端依 .NET 契約應負責 Dispose 傳入 Metafile 的 stream。
- **M12 修復**：兩處皆改為 `using ( Stream stream = new MemoryStream() ) { ... }`，確保例外路徑下也釋放。
- **測試**：`unittest/ZedGraph.XUnitTests/PaneBaseGetMetafileTests.cs`（2 個 sanity 測試，守護 `GetMetafile(w,h)` 與 `GetMetafile(w,h,antiAlias)` 仍回傳非 null Metafile）。
- **範圍**：核心中僅此兩處 stream；M13/M14 屬 web。

### H4 / H9 / M13 / M14 — web 專案（ZedGraph.Web）
- **H9**：`ZedGraphWebTools.cs` 有 7 處 `Activator.CreateInstance`（ViewState 還原），無型別白名單 → 潛在反序列化風險。
- **H4**：stack trace 寫入 HTML（位置待定位）。
- **M13**：`ZedGraphWeb.cs` 的 `RenderedImagePath` 路徑處理（web-only）。
- **M14**：`TempFileDestructor.cs` 的暫存檔清理（web-only）。
- **阻礙**：`ZedGraph.Web` 是 .NET 3.5 風格專案，**無法從現有 xUnit v3 (net481) 直接參考測試**（先前 C3/C4 即因此用隔離測試）。
- **建議**：另建 web 測試專案，或持續用隔離/BCL 契約測試；M13/M14 必須走獨立 web 路徑，不在「核心小修」範圍內。

### H8 — 疑誤判
- `ImageObj` 只透過建構子接收 `System.Drawing.Image` 物件，**不從檔案路徑載入** → 無 Path Traversal 面。
- **建議**：與使用者確認是否為 code review 誤判；若是則從計畫移除。

### L5 — BinaryFormatter（範例專案）
- 僅見於 `controltest/Form1.cs`、`libtest/Form1.cs`（demo/test 用），非核心。
- 核心專案使用 `ISerializable` 自訂序列化（非 BinaryFormatter 直接還原外部資料），風險較低。
- **建議**：低優先，範例專案可改用其他序列化示範。

---

## Batch 2 完成檢查

### 已完成 ✅
- [x] **H7** Process.Start URL scheme 白名單（14 個回歸測試）
- [x] **H3.1** `Line.cs:1154` 空 catch 改為 Debug 記錄（契約保留）
- [x] **H3.2** `ZedGraphControl.cs:606` 空 catch 改為 Debug 記錄（契約保留）
- [x] **H8** 確認為誤判（ImageObj 不從路徑載入），已移除
- [x] **H1** Fill 實作 IDisposable + Brush setter 重複 Dispose（C 方案，4 個 characterization 測試）
- [x] **M10** GasGaugeNeedle + MasterPane.OnDeserialization 共 6 處 GDI+ 洩漏改為 using-statement（3 個 characterization 測試）
- [x] **M11** 從本計畫移除（已轉交 Batch 3）
- [x] **M12** `PaneBase.GetMetafile` MemoryStream 改為 using（2 個 characterization 測試）
- [x] **M13/M14** 確認位於 web 專案，標註為「獨立 web 路徑」

### 待辦（區分範圍）
- [ ] **H1-A 完整版**（未來重啟用）：14 個 owner 鋪 Dispose 鏈（見 H1 細節段藍圖）
- [ ] **H4** stack 寫入 HTML（web）
- [ ] **H9** ViewState Activator 反序列化（web，7 處）
- [ ] **L5** BinaryFormatter（範例專案，低優先）

### 測試狀態
- [x] `dotnet test` 全綠（**106 個測試**全通過；H1 加入後 102 → 106）
- [x] H1 獨立三 commit（test + product + plan）；M10 / M12 / H3.1 / H3.2 / H7 已先 commit
