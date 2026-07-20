# Unity 曲面道具置放 Demo

以 Unity 6000.3.20f1（URP）製作的桌面互動 Demo：在一個高低起伏的有限範圍曲面上，透過右側道具下拉清單以拖曳方式置放 3D 道具，並可點選已置放的道具開啟互動選單刪除。

## 功能

- **可縮放視窗**：視窗模式可自由縮放，UI 以 Canvas Scaler 依螢幕比例自動適應。
- **程序化曲面**：以多層 Perlin noise 高度場產生的有限範圍曲面（含 MeshCollider）。
- **道具下拉清單**：畫面右側「道具」按鈕，點擊展開/收合六種不同形狀的三維道具（方塊、圓球、圓柱、膠囊、小樹、拱門）。
- **拖曳置放**：從清單拖曳道具會產生半透明預覽跟隨游標；在曲面上放開即於對應位置生成道具。放開時若游標位於 UI 上或不在曲面範圍內，該次置放自動取消。
- **置放指引線**：拖曳期間預覽物懸浮於落點上方，垂直指引線與圓形標記指出道具將掉落的準確 3D 位置。
- **道具互動選單**：點選場景中已置放的道具，會在其旁開啟選單，可刪除該道具或關閉選單。
- **桌面 / VR 切換架構**：互動邏輯僅依賴 `IPointerProvider` 指向抽象層；啟動時 `PlatformSwitcher` 偵測 XR 裝置狀態，自動在滑鼠射線（桌面）與控制器射線（XR）之間切換。專案已安裝 XR Plugin Management 與 OpenXR，啟用 loader 後即可建置 VR 版本。

## 操作方式

| 操作 | 說明 |
| --- | --- |
| 點擊右上「道具 ▼」 | 展開 / 收合道具清單 |
| 從清單項目往場景拖曳 | 產生預覽並跟隨游標（綠=可置放、紅=不可） |
| 在曲面上放開 | 於指引線標記的落點生成道具 |
| 在 UI 上或曲面外放開 | 取消置放 |
| 點選已置放道具 | 開啟互動選單（刪除 / 關閉） |

## 專案結構

```
Assets/
  Scripts/
    Core/        指向抽象層（IPointerProvider、PlatformSwitcher）與共用定義
    Surface/     程序化曲面（CurvedSurface）
    Props/       道具資料（PropDefinition / PropCatalog / PlacedProp）
    Placement/   拖曳置放與指引線（DragPlacementController / PlacementGuide）
    Interaction/ 場景點選（PropSelectionController）
    UI/          下拉清單與互動選單（PropDropdownPanel / PropContextMenu）
  Editor/        場景與資產產生器、建置腳本（SceneSetup / PropAssetGenerator / BuildScript）
  Prefabs/ Materials/ Data/  由產生器輸出的資產
```

場景與資產皆由 Editor 腳本程式化產生（`Tools > Setup`），流程完全可重現。

## 建置方式

於編輯器選單 `Tools > Build > Windows x64`，或使用命令列：

```
Unity.exe -batchmode -quit -projectPath <專案路徑> -executeMethod BuildScript.BuildWindows
```

輸出於 `Build/Windows/UnityTest.exe`。

## VR 切換設計

- 所有場景指向（置放射線、點選射線）都經由 `PointerService.Current`（`IPointerProvider`）取得，桌面實作為 `MousePointerProvider`，XR 實作為 `XRRayPointerProvider`（右手控制器姿態射線）。
- `PlatformSwitcher` 於啟動時檢查 `XRSettings.isDeviceActive`：偵測到 XR 裝置即切換為控制器射線，否則維持滑鼠。桌面版建置不啟用任何 XR loader，行為不受影響。
- 要輸出 VR 版本：於 `Project Settings > XR Plug-in Management` 勾選 OpenXR loader 後重新建置即可，遊戲程式碼無需修改。
