# Batch 1 — Critical Bug 修復

> **前置**：[Batch 0](./00-tests-foundation.md) 的 **T0.2**（Critical Regression Tests）已完成。
> **目標**：修復 5 個 Critical bug，使 T0.2 的 failing test 轉為通過。
> **驗證**：每項修完跑 `dotnet test`，對應 test 轉綠且不破壞其他測試。

---

## C1 — AxisLabel 序列化 round-trip 損毀

- [ ] **C1.1** 修正序列化欄位錯位
  - **檔案**：`source\ZedGraph\AxisLabel.cs`
  - **待修改 class/method**：
    - `AxisLabel.GetObjectData(SerializationInfo, StreamingContext)` — line 168-175
    - `AxisLabel(SerializationInfo, StreamingContext)` 反序列化建構子 — line 153-161
    - `const int schema3 = 10` — line 144
  - **問題**：line 172 寫 `info.AddValue("schema3", schema2)`（key/value 名稱不符）；line 173 寫 `info.AddValue("isOmitMag", _isVisible)`（誤用 `_isVisible` 取代 `_isOmitMag`）。round-trip 後 `_isOmitMag` 被父類別 `IsVisible` 汙染。
  - **修法**：
    ```csharp
    info.AddValue( "schema3", schema3 );          // 用 schema3 常數
    info.AddValue( "isOmitMag", _isOmitMag );     // 正確欄位
    ```
    反序列化端補 schema 版本判斷（參考 `PolyObj.cs:203` 模式）。
  - **對應測試**：T0.2.1（轉綠）

---

## C2 — Scale.SetScaleMag 在 0 邊界產生 -Infinity 污染

- [ ] **C2.1** 防護 `Log10(0)` 產生 `-Infinity`
  - **檔案**：`source\ZedGraph\Scale.cs`
  - **待修改 method**：`Scale.SetScaleMag(double min, double max, double step)` — line 2541-2559
  - **問題**：line 2547-2548 `Math.Log10(Math.Abs(_min))`，當 `_min` 或 `_max` 為 0 時回傳 `-Infinity`，導致 `_mag = int.MinValue`，下游 `ToString("f" + 負值)` 丟 `FormatException`。
  - **修法**：
    ```csharp
    double minMag = this._min != 0 ? Math.Floor(Math.Log10(Math.Abs(this._min))) : 0;
    double maxMag = this._max != 0 ? Math.Floor(Math.Log10(Math.Abs(this._max))) : 0;
    ```
  - **對應測試**：T0.2.2（轉綠）
  - **注意**：檢查 `Scale.MakeLabel`（line 1755+）使用 `_mag` 處，確認修復後 format 計算正確

---

## C3 — ZedGraphWeb.RenderMode getter 永遠回傳預設值

- [ ] **C3.1** 修正 enum Parse 呼叫
  - **檔案**：`web\ZedGraph.Web\ZedGraphWeb.cs`
  - **待修改 class/property**：`ZedGraphWeb.RenderMode` getter — line 978-990
  - **問題**：line 983 `RenderModeType.Parse(typeof(RenderModeType), ...)` — `RenderModeType` 是 enum，無此靜態方法；應為 `Enum.Parse`。導致設定值被忽略或編譯錯誤。
  - **修法**：
    ```csharp
    retVal = (RenderModeType)Enum.Parse( typeof( RenderModeType ), ViewState["RenderMode"].ToString() );
    ```
  - **對應測試**：T0.2.3（轉綠）
  - **注意**：檢查同檔其他 enum 屬性是否有相同 anti-pattern（`grep -n "\.Parse(typeof" web\`)

---

## C4 — ZedGraphWeb.MakeAreaTag 未編碼屬性值（XSS）

- [ ] **C4.1** 對 Href/target/title 屬性值編碼
  - **檔案**：`web\ZedGraph.Web\ZedGraphWeb.cs`
  - **待修改 method**：`ZedGraphWeb.MakeAreaTag(string shape, string coords, string url, string target, string title, object tag, HtmlTextWriter output)` — line 1750-1773
  - **問題**：line 1761 `url + "&" + tag`（tag 為 object，未 URL encode）；line 1766 `target`、line 1768 `title` 未 HTML 編碼。`HtmlTextWriter.AddAttribute` 不自動編碼任意值 → 反射型 XSS。
  - **修法**：
    ```csharp
    string safeUrl = Uri.EscapeDataString( url );
    string safeTag = tag is string s ? Uri.EscapeDataString( s ) : string.Empty;
    output.AddAttribute( HtmlTextWriterAttribute.Href, safeUrl + (safeTag != string.Empty ? "&" + safeTag : "") );
    output.AddAttribute( HtmlTextWriterAttribute.Target, HttpUtility.HtmlAttributeEncode( target ) );
    output.AddAttribute( HtmlTextWriterAttribute.Title, HttpUtility.HtmlAttributeEncode( title ) );
    ```
  - **對應測試**：T0.2.4（轉綠）

---

## C5 — HandlePointValues 未檢查 curve.IsVisible（ToDo.txt 已知未修）

- [ ] **C5.1** 加入可見性與邊界防護
  - **檔案**：`source\ZedGraph\ZedGraphControl.Events.cs`
  - **待修改 method**：`ZedGraphControl.HandlePointValues(Point mousePt, GraphPane pane)` — line 702-771（主邏輯 line 711-738）
  - **問題**：line 711 `if (nearestObj is CurveItem && iPt >= 0)` 缺 `curve.IsVisible` 檢查；line 738 `curve.Points[iPt]` 缺 `iPt < Points.Count` 與 `Points == null` 防護；line 757 `(string)pt.Tag` 可能丟 `InvalidCastException`。對應 ToDo.txt:14-18 已知問題。
  - **修法**：
    ```csharp
    if ( nearestObj is CurveItem && iPt >= 0 )
    {
        CurveItem curve = (CurveItem)nearestObj;
        if ( !curve.IsVisible || curve.Points == null || iPt >= curve.Points.Count )
        {
            this.DisableToolTip();
            return mousePt;
        }
        // ... 後續 tag 處理改用 pt.Tag as string ?? pt.ToString()，避免 InvalidCastException
    }
    ```
  - **對應測試**：T0.2.5（轉綠）
  - **注意**：`MasterPane.FindNearestPaneObject` 與 `GraphPane.FindNearestObject` 是否也需過濾 `IsVisible`？一併評估（可能需在 FindNearestObject 內即跳過隱藏曲線）

---

## Batch 1 完成檢查

- [x] **B1.0** T0.2.1–T0.2.5 五個 failing test 全部轉綠（含隔離測試覆蓋 C3/C4 不可直接測的部分）
- [x] **B1.1** `dotnet test unittest\ZedGraph.XUnitTests\ZedGraph.XUnitTests.csproj` 全綠（**32/32 通過**）
- [x] **B1.2** `dotnet build source\ZedGraph\ZedGraph.csproj` 成功（核心專案無錯誤，72 個既有警告）
- [x] **B1.3** 每項獨立 commit，訊息格式 `[B1-Cx] 描述`

### 已修復的 Critical bugs
| 編號 | 摘要 | 對應測試檔案 | 修法檔案 |
|------|------|--------------|----------|
| C1 | AxisLabel 序列化 round-trip 損毀 | AxisLabelSerializationTests.cs | source/ZedGraph/AxisLabel.cs |
| C2 | Scale.SetScaleMag 0 邊界 -Infinity 污染 | ScaleSetScaleMagTests.cs | source/ZedGraph/Scale.cs |
| C3 | ZedGraphWeb.RenderMode Parse 邏輯錯誤 | RenderModeTypeParseTests.cs | web/ZedGraph.Web/ZedGraphWeb.cs |
| C4 | ZedGraphWeb.MakeAreaTag XSS | WebSafeEncodingContractTests.cs | web/ZedGraph.Web/ZedGraphWeb.cs |
| C5 | HandlePointValues 缺 null/邊界防護 | HandlePointValuesEdgeCaseTests.cs | source/ZedGraph/ZedGraphControl.Events.cs |

### 測試累積
- T0.1 helper smoke tests：11 個
- T0.2.1 C1 regression：4 個
- T0.2.2 C2 regression：3 個
- T0.2.3 C3 邏輯：4 個
- T0.2.4 C4 BCL 契約：7 個
- T0.2.5 C5 防護：3 個
- **總計 32 個測試全綠**
