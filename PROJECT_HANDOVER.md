# BÁO CÁO TIẾN ĐỘ & BÀN GIAO DỰ ÁN BLOCK MERGE PUZZLE

> **Cập nhật lần cuối:** 21/11/2026 | **Unity:** `6000.0.74f1` (`WindowsEditor` · Build target: `Android`) | **Thư mục dự án:** `D:\BlockMergePuzzle`

---

## 1. Trạng Thái Hiện Tại (Hoàn Thiện 10/10 Level — Level 1 đến Level 10 ĐÃ CHỐT)

- **Tổng quan tiến độ**:
  - Đã xây dựng và đóng gói hoàn chỉnh **10/10 màn chơi** (`Level 1` đến `Level 10`), mỗi màn có file `.unity` và prefab `_Board.prefab` độc lập.
  - Hoàn thiện vòng lặp gameplay cốt lõi: trượt đa hướng mượt mà, hợp nhất thẳng hàng + hiệu ứng đập **Merge Slam**, kiểm tra điều kiện cổng chính xác + khóa ô cổng + hiệu ứng tan biến **Portal Dissolve Shader**, cơ chế **khối đóng băng (`IceCount`)**, hệ thống **Booster Bar**, **Settings/Pause Panel** phong cách pha lê tím, **Victory Popup** 3 sao có chuyển màn **Next Level**, **Tutorial Hand Guide** cho Level 1 và âm thanh 2D qua `SoundManager`.
  - Đã bổ sung bộ asset UI thắng/thua mới từ Figma tại `Assets/Sprites/Win&Lose scene/` để phục vụ đợt nâng cấp giao diện Win/Lose tiếp theo.

### Danh Sách Chi Tiết 10 Màn Chơi Đã Chốt

1. **Level 1 (`map_lv1.png` — Tutorial T-Shape Map — ĐÃ CHỐT)**:
   - **Bản đồ**: Grid 5×3 (7 ô hợp lệ: `(2,0), (2,1)` và hàng ngang `row 2`, `col 0..4`).
   - **Cổng thoát (1 cổng Tech Hologram)**: Cổng trên **Đỏ `[2]`** tại `(2,0)` (exit `(2,-1)`, hướng `Vector2.up`).
   - **Khối khởi đầu (2 khối `1x1`)**: Đỏ `[1]` tại `(0,2)` và Đỏ `[1]` tại `(4,2)`.
   - **Hướng dẫn & UI**: Kích hoạt `TutorialHandGuide` 2 giai đoạn (`Merge` -> `ExitGate`), timer `05:00`, banner `LEVEL 1`.
   - **Lưu trữ**: `Assets/Scenes/Level1.unity`, `Assets/Prefabs/Levels/Level1_Board.prefab`.

2. **Level 2 (`map_lv2.png` — Asymmetric Cross Map — ĐÃ CHỐT)**:
   - **Bản đồ**: Grid 6×6 (11 ô hành lang: cột 2 dọc `row 0..5` và hàng 3 ngang `col 0..5`), sử dụng bảng ánh xạ tọa độ phi tuyến `Level2CellPositions`.
   - **Cổng thoát (2 cổng Tech Hologram)**: Trên **Xanh lá `[2]`** tại `(2,0)` (`Vector2.up`), Phải **Đỏ `[3]`** tại `(5,3)` (`Vector2.right`).
   - **Khối khởi đầu (5 khối `1x1`)**: Đỏ `[1]@(2,1)`, Đỏ `[2]@(2,3)`, Đỏ `[1]@(2,5)`, Xanh lá `[1]@(0,3)`, Xanh lá `[1]@(4,3)`.
   - **UI**: Timer `05:00`, banner `LEVEL 2`.
   - **Lưu trữ**: `Assets/Scenes/Level2.unity`, `Assets/Prefabs/Levels/Level2_Board.prefab`.

3. **Level 3 (`Map_lv3.png` — Branching Map — ĐÃ CHỐT)**:
   - **Bản đồ**: Grid 5×7 (15 ô hợp lệ chia 3 nhánh nối qua trục `col 2`), ánh xạ chuẩn `Level3CellPositions`.
   - **Cổng thoát (2 cổng Tech Hologram)**: Trên-trái **Xanh lá `[4]`** tại `(0,0)` (`Vector2.left`), Giữa-phải **Đỏ `[4]`** tại `(4,3)` (`Vector2.right`).
   - **Khối khởi đầu (8 khối `1x1`)**: 4 Đỏ (`[1]@(0,6), [1]@(2,4), [2]@(2,5), [3]@(2,6)`) và 4 Xanh lá (`[1]@(3,3), [1]@(3,6), [2]@(1,6), [3]@(4,6)`).
   - **UI**: Timer `05:00`, banner `LEVEL 3`.
   - **Lưu trữ**: `Assets/Scenes/Level3.unity`, `Assets/Prefabs/Levels/Level3_Board.prefab`.

4. **Level 4 (`map_lv4.png` — E-Shape Map — ĐÃ CHỐT)**:
   - **Bản đồ**: Grid chữ E 4×5 (14 ô hợp lệ: 3 hành lang ngang `row 0, 2, 4` nối qua trục `col 0`), ánh xạ `Level4CellPositions`.
   - **Cổng thoát (3 cổng Tech Hologram hướng phải `Vector2.right`)**: Đỏ `[3]@(3,0)`, Xanh lá `[3]@(3,2)`, Hồng `[2]@(3,4)`.
   - **Khối khởi đầu (7 khối `1x1`)**: 3 Đỏ (`[2]@(0,0), [1]@(0,4), [1]@(2,4)`), 3 Xanh lá (`[2]@(1,0), [1]@(2,0), [1]@(1,4)`), 1 Hồng `[2]@(2,2)`. Đã kiểm chứng BFS giải được trong 22 bước.
   - **UI**: Timer `05:00`, banner `LEVEL 4`.
   - **Lưu trữ**: `Assets/Scenes/Level4.unity`, `Assets/Prefabs/Levels/Level4_Board.prefab`.

5. **Level 5 (`map_lv5.png` — Diamond Cross Multi-Shape Map — ĐÃ CHỐT)**:
   - **Bản đồ**: Grid đối xứng bậc thang 8×11 (40 ô bàn cờ + 4 ô cổng), ánh xạ `Level5CellPositions`.
   - **Cổng thoát (2 cổng Tech Hologram rộng 2 ô)**: Trên **Vàng `[5]`** tại `(3..4, 0)` (`Vector2.up`), Dưới **Xanh lá `[2]`** tại `(3..4, 10)` (`Vector2.down`).
   - **Khối khởi đầu (17 khối đa hình dạng)**:
     - 6 Vàng `Horizontal_2x1`: `[3]@(3,1), [2]@(2,2), [3]@(4,2), [3]@(3,3), [1]@(3,4), [1]@(3,7)`.
     - 6 Vàng `Vertical_1x2`: `[3]@(2,3), [3]@(5,3), [1]@(2,5), [2]@(3,5), [3]@(4,5), [1]@(5,5)`.
     - 4 Xanh lá chữ L: `ShapeL_TopRight [1]@(0,5)`, `ShapeL_TopLeft [1]@(6,5)`, `ShapeL_TopRight [1]@(1,7)`, `ShapeL_TopLeft [1]@(5,7)`.
     - 1 Xanh lá `Square_2x2 [2]` tại `(3,8)`.
   - **UI**: Timer `05:00`, banner `LEVEL 5`.
   - **Lưu trữ**: `Assets/Scenes/Level5.unity`, `Assets/Prefabs/Levels/Level5_Board.prefab`.

6. **Level 6 (`map_lv6 (1).png` — Cross Piece & 4-Gate Asymmetric Map — ĐÃ CHỐT)**:
   - **Bản đồ**: Grid 7×8 (26 ô bàn cờ + 8 ô cổng), ánh xạ `Level6CellPositions`.
   - **Cổng thoát (4 cổng Tech Hologram)**: Trên **Đỏ `[2]`** `(2..4, 0)`, Phải **Xanh lá `[2]`** `(6, 1..2)`, Trái giữa **Cam `[2]`** `(0, 3)`, Trái dưới **Hồng `[3]`** `(0, 5..6)`.
   - **Khối khởi đầu (10 khối đa hình dạng)**: 1 Đỏ chữ thập `Cross_3x3 [2]` tại `(2,2)`, 3 Hồng `Vertical_1x2` (`[2]@(2,1), [1]@(1,6), [1]@(5,6)`), 2 Xanh lá `Vertical_1x2` (`[1]@(4,1), [1]@(3,6)`), 2 Xanh lá `1x1` (`[1]@(1,1), [1]@(5,1)`), 2 Cam `1x1` (`[1]@(3,1), [1]@(1,5)`).
   - **UI**: Timer `01:45` (`105s`), banner `LEVEL 6`.
   - **Lưu trữ**: `Assets/Scenes/Level6.unity`, `Assets/Prefabs/Levels/Level6_Board.prefab`.

7. **Level 7 (`map_lv7.png` — Pyramid Stepped Map + Ice Mechanic — ĐÃ CHỐT)**:
   - **Bản đồ**: Grid tháp bậc thang 8×4 (12 ô bàn cờ + 4 ô cổng), ánh xạ `Level7CellPositions`.
   - **Cổng thoát (3 cổng Tech Hologram yêu cầu `[2]`)**: Trên **Xanh lá** `(3..4, 0)`, Trái dưới **Vàng** `(0, 3)`, Phải dưới **Hồng** `(7, 3)`.
   - **Khối khởi đầu (5 khối đa hình dạng)**:
     - 1 **Xanh lá `Square_2x2 [2]` tại `(3,1)` đóng băng `IceCount = 2`** (chỉ vỡ băng sau khi 2 khối khác thoát cổng).
     - 2 Hồng `Single_1x1 [1]` tại `(2,2)` & `(5,2)`.
     - 2 Vàng `Horizontal_2x1 [1]` tại `(1,3)` & `(5,3)`.
   - **UI**: Timer hiển thị trên scene `05:00` (mặc định code `01:45`), banner `LEVEL 7`.
   - **Lưu trữ**: `Assets/Scenes/Level7.unity`, `Assets/Prefabs/Levels/Level7_Board.prefab`.

8. **Level 8 (`map_lv8.png` — Asymmetric Inner-Obstacle Map + Dual Ice — ĐÃ CHỐT)**:
   - **Bản đồ**: Grid 8×9 với **2 ô chướng ngại vật bên trong** bị khoét lỗ tại `(3,3)` và `(4,4)`, ánh xạ `Level8CellPositions`.
   - **Cổng thoát (5 cổng Tech Hologram yêu cầu `[2]`)**: Trên **Xanh lá** `(4..5, 0)`, Phải **Cam** `(7,1)`, Trái **Tím** `(0, 3..4)`, Phải **Xanh dương** `(7, 3..4)`, Trái **Vàng** `(0,6)`.
   - **Khối khởi đầu (15 khối)**:
     - 2 Cam `1x1 [1]` tại `(1,0), (2,1)` & 2 Xanh lá `1x1 [1]` tại `(2,0), (1,1)`.
     - **2 khối Xanh lá `Square_2x2 [1]` đóng băng `IceCount = 4` (tinh chỉnh trực tiếp trong `Level8.unity`)** tại `(1,2)` và `(5,4)` *(Lưu ý: `SpawnLevelPieces` trong `LevelManager.cs` chưa gán `IceCount` mặc định cho Level 8)*.
     - 2 Xanh dương `1x2 [1]` tại `(3,1), (4,5)` & 2 Tím `1x2 [1]` tại `(4,1), (3,5)`.
     - 2 Vàng `1x1 [1]` tại `(5,6), (6,7)`, 2 Tím `1x1 [1]` tại `(6,6), (5,7)` & 1 Tím `2x1 [2]` tại `(5,8)`.
   - **UI**: Timer `05:00`, banner `LEVEL 8`.
   - **Lưu trữ**: `Assets/Scenes/Level8.unity`, `Assets/Prefabs/Levels/Level8_Board.prefab`.

9. **Level 9 (`map_lv9.png` — Vertical 5-Gate Map, 2D Restored — ĐÃ CHỐT)**:
   - **Bản đồ**: Grid 5×8, ánh xạ `Level9CellPositions`. Đã khôi phục hoàn toàn hiển thị **2D chuẩn** sau thử nghiệm 2.5D (`ApplyLevel25DTilt()` reset các root về phẳng).
   - **Cổng thoát (5 cổng Tech Hologram)**: Trên-trái **Đỏ `[2]`** `(0,0)`, Trên-phải **Tím `[3]`** `(3,0)`, Phải-trên **Vàng `[3]`** `(4,3)`, Phải-dưới **Xanh dương `[2]`** `(4,4)`, Dưới-giữa **Xanh lá `[3]`** `(1..2, 7)`.
   - **Khối khởi đầu (12 khối)**:
     - 2 Đỏ `1x2 [1]` tại `(0,1), (3,1)`.
     - 2 Xanh lá `Square_2x2 [2]` tại `(1,1)` và `(1,3)` — **khối tại `(1,3)` được cấu hình đóng băng `IceCount = 4` trong `Level9.unity`** (`_frozenBlockIceCount = 4`).
     - 3 Tím `1x2`: `[2]@(0,3), [1]@(3,3), [1]@(0,5)`.
     - 3 Vàng `1x1`: `[1]@(1,5), [2]@(3,5), [1]@(2,6)`.
     - 2 Xanh dương `1x1 [1]`: `(2,5), (1,6)`.
   - **Mở khóa tính năng**: **Booster Freeze (Đóng Băng)** chính thức mở khóa từ Level 9 (`BoosterFreezeUnlockLevel = 9`).
   - **UI**: Timer `05:00`, banner `LEVEL 9`.
   - **Lưu trữ**: `Assets/Scenes/Level9.unity`, `Assets/Prefabs/Levels/Level9_Board.prefab`.

10. **Level 10 (`map_lv10 (2).png` — Christmas-Tree Multi-Shape Map — ĐÃ CHỐT)**:
    - **Bản đồ**: Grid 9×11 (tài nguyên `map_lv10 (2).png` 500×500 ánh xạ lên khung 11×11 world units, `FrameOffset = (0, -0.650f, 0)`), sử dụng `ApplyLevel10GateVisualScale()` để thu gọn vòng hologram và chữ số vừa khít hốc cổng nhỏ.
    - **Cổng thoát (4 cổng Tech Hologram)**:
      - Trên **Đỏ** tại `(4,0)`: **yêu cầu `[5]` trên `Level10.unity`** *(tinh chỉnh trên scene; code mặc định trong `LevelManager.cs` là `4`)*.
      - Dưới **Cam `[2]`** tại `(4,10)`.
      - Trái **Xanh lá `[5]`** tại `(0, 6..8)`.
      - Phải **Xanh lá `[4]`** tại `(8, 6..8)`.
    - **Khối khởi đầu (26 khối đa hình dạng)**:
      - 7 Đỏ `1x1`, 1 Cam `1x1 [2]` tại `(4,9)`, 18 Xanh lá gồm `1x1`, `Horizontal_2x1`, `Vertical_1x2` và 2 `ShapeL_BottomLeft [3]` tại `(2,6), (5,6)`. *(Một số khối trên `Level10.unity` đã tinh chỉnh giá trị trực tiếp trên scene như `(3,2)` -> `[1]`, `(1,8)` -> `[2]`, `(7,8)` -> `[2]`)*.
    - **UI**: Timer `05:00`, banner `LEVEL 10`.
    - **Lưu trữ**: `Assets/Scenes/Level10.unity`, `Assets/Prefabs/Levels/Level10_Board.prefab`.

### Bảo Vệ Độc Lập Scene (Scene Protection Mechanism)
- `SceneSetupHelper.LoadLevel(LevelId level)` mở trực tiếp `Assets/Scenes/{level}.unity` qua `EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single)`, **tuyệt đối không chạy procedural bake đè lên scene đang mở**.
- `BakeAll10Levels()` bảo vệ toàn bộ 10 màn chơi đã chốt (`if (i >= 1 && i <= 10) continue;`).
- `SetupSceneHierarchy()` giữ nguyên các khối đã xếp trên scene cho Level 1..10.

### Bộ Hợp Đồng Tọa Độ UI Chuẩn (UI Layout Contract)
- `HeaderPanel`: pos `(0, -50)`, size `(1000, 190)`
  - `Btn_Replay`: pos `(-378.00, 5.50)`, size `(215.94, 201.01)`
  - `Banner_Time`: pos `(16.00, -80.00)`, size `(430.00, 154.00)` (mặc định `05:00` cho Level 1–5; `01:45` cho Level 6–7 trong code, chỉnh linh hoạt trên scene/Inspector)
  - `Btn_Pause`: pos `(374.00, 0.00)`, size `(171.29, 190.00)`
  - `Banner_Level`: pos `(21.00, 32.00)`, size `(276.00, 64.00)` (`LEVEL 1` .. `LEVEL 10`)
- `TutorialLayer`:
  - `InstructionBanner`: pos `(-24.76, 375.68)`, size `(600.47, 92.64)` (tự động bật ở Level 1, ẩn từ Level 2 trở đi)
- `BoosterBar`: pos `(0.00, 60.00)`, size `(900.00, 260.00)`
  - `Booster_Freeze`: pos `(-318.00, 101.00)`, size `(321.29, 339.44)` — Mở ở **Level 9**
  - `Booster_Magnet`: pos `(0.00, 104.76)`, size `(366.55, 331.93)` — Mở ở **Level 12**
  - `Booster_Wand`: pos `(311.00, 93.38)`, size `(323.64, 354.69)` — Mở ở **Level 16**
- `PausePanel` (Purple Crystal): pos `(0, 0)`, `DialogBox`: pos `(0, 0)`, size `(880, 756)`
- `VictoryPanel`: bản mới (`BuildVictoryPanel`) có `DialogBox` pos `(0, -20)`, size `(840, 760)` kèm 3 sao vàng, chip thời gian còn lại, nút `NextLevelBtn` và `PlayAgainBtn`. *(Các scene hiện lưu cấu trúc cũ sẽ tự động được `GameUI.BindVictoryPanelUI()` nâng cấp sang bản mới ngay khi Play hoặc chạy `RebuildVictoryPanel()`).*
- `FeedbackBanner`: pos `(0, 140)`, size `(960, 110)`

### Build Settings Scenes (`ProjectSettings/EditorBuildSettings.asset`)
- `[0] Assets/Scenes/MainScene.unity` *(hiện mở Level 2)*
- `[1] Assets/Scenes/Level1.unity`
- `[2] Assets/Scenes/Level2.unity`
- `[3] Assets/Scenes/Level3.unity`
- `[4] Assets/Scenes/Level4.unity`
- `[5] Assets/Scenes/Level5.unity`
- `[6] Assets/Scenes/Level6.unity`
- `[7] Assets/Scenes/Level7.unity`
- `[8] Assets/Scenes/Level8.unity`
- `[9] Assets/Scenes/Level9.unity`
- `[10] Assets/Scenes/Level10.unity`

### Lưu Trữ Scene, Prefab, Git & Kết Nối MCP
- **Scenes**: `Assets/Scenes/MainScene.unity` + `Assets/Scenes/Level1.unity` .. `Assets/Scenes/Level10.unity`.
- **Prefabs**: `Assets/Prefabs/Levels/Level1_Board.prefab` .. `Assets/Prefabs/Levels/Level10_Board.prefab`.
- **Git Repository**: `https://github.com/huy113-dev/BlockMergePuzzle.git` — nhánh hiện tại: `feat/level-4-map` (đồng bộ toàn bộ tiến độ Level 1–10).
- **Unity MCP**: FastMCP HTTP bridge (`http://127.0.0.1:8080/mcp`), tự động kết nối lại sau domain reload thông qua `AutoMcpStarter` trong `SceneSetupHelper.cs`.

### Screenshot Kiểm Chứng Tiêu Biểu (`Assets/Screenshots/`)
- `level1_updated_preview.png`: Game View Level 1 (map chữ T + bàn tay hướng dẫn).
- `level2_updated_preview.png` & `level2_clean_load_verified.png`: Game View Level 2 (khối & cổng chuẩn pixel, UI đồng bộ).
- `level3_updated_preview.png`: Game View Level 3 (8 khối, 2 cổng hologram).
- `verify_level4.png`: Game View Level 4 (7 khối, 3 cổng hướng phải).
- `verify_level5.png`: Game View Level 5 (17 khối đa hình dạng, 2 cổng rộng 2 ô).
- `verify_level6.png`: Game View Level 6 (khối chữ thập `Cross_3x3`, 4 cổng).
- `verify_level7.png`: Game View Level 7 (khối `2x2` đóng băng `IceCount = 2`, 3 cổng).
- `level8_enlarged_preview.png` & `level8_final_preview.png`: Game View Level 8 (2 ô lỗ nội bộ, 5 cổng, 15 khối).
- `level9_2d_restored.png`: Game View Level 9 sau khi khôi phục 2D chuẩn (12 khối, 5 cổng).
- `level10_final_preview.png`: Game View Level 10 bản đồ cây thông 26 khối, 4 cổng.
- `verify_pause_panel_final.png`: Popup Pause/Settings phong cách pha lê tím.

---

## 2. Các Hệ Thống, Tính Năng & Kỹ Thuật Đã Hoàn Thiện

### a. Cơ Chế Kéo Thả, Hợp Nhất Thẳng Hàng & Merge Slam VFX (`PlayerPieceController.cs`, `LevelManager.cs`, `PuzzlePiece.cs`)
- **Trượt đa hướng & quẹo hành lang mượt mà (`_currentOrigin`)**:
  - Người chơi chạm và rê chuột/ngón tay tự do qua các ngã rẽ mà không cần nhả tay; ngưỡng trượt `0.5f * cellWidth/cellHeight`; chống cắt góc chéo khi có vật cản; rubber-band `0.15x` (tối đa `0.08 units`) khi đụng tường/cổng khóa.
  - Hỗ trợ phím `W/A/S/D` và mũi tên di chuyển từng bước (`TrySlidePieceSteps`).
- **Lift & Tilt Game Feel**:
  - Khi chạm: phóng to `1.075x`, nhấc nổi `Y +0.08f, Z = -0.1f`, nâng `sortingOrder = 50`, phát `SoundManager.PlayPickup()`, nghiêng động (`SetTilt`) tối đa `±3.5°` theo hướng kéo ngang.
- **Hợp nhất thẳng hàng (`CheckMergeOpportunity` & `RaycastSlide`)**:
  - Kiểm tra đủ 4 điều kiện: `!IsFrozen`, cùng hình dạng (`AreShapesMergeable`), cùng màu (`PieceColor`), cùng giá trị (`Value`) và thẳng hàng/cột (`origin.y == hit.GridOrigin.y` hoặc `origin.x == hit.GridOrigin.x`), chặn va chạm đa điểm (`!multiple`).
  - Hiển thị thông báo lỗi cụ thể tiếng Việt qua `GameUI.ShowFeedbackError` khi khác số, khác màu, khác hình dạng hoặc lệch hàng/cột.
- **Merge Slam VFX (`PuzzlePiece.SlideAndSlamMerge` + `LevelManager.SpawnMergeFlash`)**:
  - Pha 1: Khối nhấc lên cao (`Y +0.24f, Z -0.40f, scale 1.16x`) lao tới vị trí mục tiêu.
  - Pha 2: Đập mạnh xuống với squash/stretch (`1.28x, 0.72y`), ép dẹt khối đích trước khi hủy.
  - Pha 3: Chớp silhouette trắng toàn bộ khối qua `GUI/Text Shader` (`CoFlashOverlay`) + `SpawnMergeFlash` (hào quang tròn + sao lóe sáng xoay) + rung camera nhẹ `KickCameraImpact(0.045f, 0.07f)` + cộng `piece.Value * 50` điểm.
  - Pha 4: Đàn hồi tắt dần mượt về scale `1.0`.

### b. Hệ Thống Cổng Thoát, Khóa Ô Cổng & Portal Dissolve Shader (`ExitGateView.cs`, `LevelManager.cs`, `SpritePortalDissolve.shader`)
- **Khóa ô cổng vật lý (`GateCoords` & `CanPieceFitAt`)**:
  - Cổng nằm trên tọa độ mép bàn cờ được quản lý qua `GateCoords`; `CanPieceFitAt()` trả `false` nếu bất kỳ ô nào của khối trùng với ô cổng, cấm khối dừng hoặc đè lên cổng khi chưa đủ điều kiện.
- **Khớp điều kiện & Bề rộng cổng đa ô**:
  - `CheckEscapeOpportunity` và `RaycastSlide` yêu cầu **khớp chính xác** `piece.Value == gate.RequiredValue` (báo lỗi riêng nếu chưa đủ số hoặc số quá lớn), đúng màu, đúng hướng `TriggerDirection`, và bề rộng khối nằm trọn trong phạm vi cổng (`IsPieceSpanAlignedWithGate`).
- **Tech Hologram Exit Gates (`ExitGateView.cs`)**:
  - Đế hologram `tech_gate_holo_pad.png`, vòng cyber ngoài `tech_gate_ring.png` (xoay `-18°/s`), vòng trong `tech_gate_inner_ring.png` (xoay `+25°/s` + nhịp thở), chữ số 3D nổi phát sáng theo màu cổng, tự co giãn theo cổng rộng/hẹp và qua `ApplyLevel10GateVisualScale()`.
- **Portal Dissolve VFX (`Assets/Resources/Shaders/SpritePortalDissolve.shader` + `EscapeVfxRunner`)**:
  - Khối trượt vào giữa tâm cổng (`gate.transform.position`), bật shader `Custom/SpritePortalDissolve` cắt tan biến theo mặt phẳng cổng (`_DissolveDir`, `_PlaneOrigin`, viền phát sáng cyan `_GlowColor`) kết hợp `EscapeVfxRunner` tạo màn ánh sáng cổng và các mảnh vỡ bay dọc theo hướng thoát, cộng `piece.Value * 150` điểm và kiểm tra chiến thắng.

### c. Cơ Chế Khối Đóng Băng (`IceCount` — Level 7, 8, 9)
- **Hiển thị băng (`PuzzlePiece.BuildIceOverlay`)**:
  - Sử dụng sprite `ice_block_overlay.png` (`sortingOrder = _baseSortingOrder + 6`) che phủ hoàn toàn khối đá và chữ số gốc bên dưới (`RefreshIceStateVisuals()` tắt `_renderers` và `_textMeshLayers` khi `IsFrozen`).
  - Render chữ số băng 3D nhiều lớp (hào quang cyan, viền băng trắng dày 3D, outline trắng, mặt số xanh dương `#0358FA`).
- **Tương tác & Rã băng (`LevelManager.ExecuteEscape` -> `DecrementIceCount`)**:
  - Khi người chơi chạm/kéo khối đang đóng băng: lắc ngang `PlayFrozenShake()`, phát `SoundManager.PlayError()` và báo *"Khối đang bị đóng băng! Hãy đưa {IceCount} khối khác thoát cổng để phá băng."*
  - Mỗi lần một khối khác thoát cổng thành công, tất cả khối đang đóng băng giảm `IceCount` đi `1`:
    - Nếu vẫn `> 0`: chạy `CoIceCrackPulse()` và bắn 10 mảnh băng (`SpawnIceShardsVFX`).
    - Nếu về `0`: phát âm thanh vỡ băng `SoundManager.PlayIceCrack()` (`icecrak.wav`), chạy `CoShatterIce()` bắn 26 mảnh băng và lộ rõ khối bên trong để chơi bình thường.
- **Tùy chỉnh trên Inspector**:
  - Chỉnh trực tiếp `LevelManager._frozenBlockIceCount` (`OnValidate`) hoặc `PuzzlePiece.IceCount` (hỗ trợ cả Edit Mode & Play Mode).

### d. Hệ Thống UI: Header, Timer Linh Hoạt, Booster Bar, Pause/Settings, Victory Popup (`GameUI.cs`, `SpriteGenerator.cs`)
- **Bảo toàn UI thủ công (`BindExistingUI`)**: Tự động liên kết cây UI có sẵn trong scene thay vì `RebuildUI()`, tránh làm lệch tọa độ do người dùng căn chỉnh tay.
- **Bộ đếm thời gian linh hoạt (`GetLevelDurationSeconds`, `SetPlayDurationSeconds`)**:
  - Ưu tiên lấy theo `_customDurationSeconds`, chuỗi `_customTimeMMSS` (`MM:SS`), giá trị ban đầu trong `TimeVal.text` trên Inspector (cho phép sửa trực tiếp trong Play Mode), rồi mới fallback về cấu hình mặc định (Level 1–5 = `300s`, Level 6–7 = `105s`, Level 8+ = `100s`).
  - Đổi màu xanh cyan khi đang kích hoạt Booster đóng băng thời gian, đỏ cảnh báo khi `< 20s`.
- **Thanh Booster (`BoosterBar`)**:
  - `Booster_Freeze` (`x3`, mở tại **Level 9**): đóng băng bộ đếm thời gian trong `15s` (`_freezeTimerRemaining = 15f`).
  - `Booster_Magnet` (`x2`, mở tại **Level 12**): tự động tìm và merge một cặp khối cùng màu/hình/số thông qua `TryMagnetAutoMerge()`.
  - `Booster_Wand` (`x1`, mở tại **Level 16**): hiển thị thông báo kích hoạt (hiệu ứng chuyên sâu sẽ triển khai tiếp).
  - Các ô chưa đủ level mở khóa tự động làm mờ icon, hiển thị ổ khóa `lock.png` và badge `Lv.{unlockLevel}`.
- **Pause / Settings Panel (`BuildPausePanel`)**:
  - Phong cách khung pha lê tím (`Assets/Sprites/Main Scene/UI/setting/` gồm `Group 331.png`, nút đóng `X (1).png`, switch `Group 333/334`, icon `Vector 389/390/392/394/395`, `Group 332`, nền hàng `SpriteGenerator.GetSettingRowSprite()`).
  - Lưu 3 tùy chọn Sound, Music, Vibration vào `PlayerPrefs` (`BMP_SoundEnabled`, `BMP_MusicEnabled`, `BMP_VibrationEnabled`).
- **Victory Popup & Chuyển Màn (`BuildVictoryPanel`, `NextLevel`)**:
  - Giao diện pha lê tím + ruy-băng vàng (`LEVEL X CLEAR!` / `ALL LEVELS CLEAR!`), 3 ngôi sao vàng hoạt ảnh nảy tuần tự (`CoAnimateVictoryBanner`), chip hiển thị thời gian còn lại, nút xanh ngọc `NEXT LEVEL {n+1} >` (load scene `Level{n+1}` hoặc quay lại `Level 1` sau Level 10) và nút `CHƠI LẠI`.

### e. Hệ Thống Hướng Dẫn Level 1 (`TutorialHandGuide.cs`)
- Chỉ bật khi `CurrentLevel == LevelId.Level1`.
- Gồm `InstructionBanner` ("Swipe block to merge" / "Swipe block to exit gate") và bàn tay thế giới `TutorialHand_World` (`UI_hand.png`, `sortingOrder = 95`).
- Tự ẩn khi người chơi đang kéo khối, hiện lại sau `0.7s` đứng yên, và tắt hẳn khi hoàn tất màn (`Completed`).

### f. Hệ Thống Âm Thanh (`SoundManager.cs`)
- Singleton `DontDestroyOnLoad`, ép 2D (`spatialBlend = 0f`), tự kiểm tra/gắn `AudioListener`, đồng bộ trạng thái `BMP_SoundEnabled`.
- Hỗ trợ 4 clip thực tế từ `Assets/Resources/Sound/` (`block_move.wav`, `merge_block.wav`, `block_vanish.mp3`, `icecrak.wav`) kèm fallback procedural đầy đủ (bao gồm `GenerateVictoryFanfare()` C5-E5-G5-C6).
- **Merge Combo Audio**: Tăng dần âm lượng qua 4 bậc (`0.40 -> 0.60 -> 0.80 -> 1.00`) trong cửa sổ timeout `1.5s`.

### g. Background, Camera Fit & Thử Nghiệm 2.5D
- **Background vũ trụ (`background_main scene .png`)**: Tự động scale Aspect-Fill phủ kín 100% khung hình, tint dịu `(0.42f, 0.42f, 0.48f, 1f)`, `sortingOrder = -10`.
- **Camera Auto-Fit (`FitCameraToBoard`)**: Tính `Mathf.Max(sizeByWidth, sizeByHeight)` theo tỷ lệ dọc `1440×3200` (`0.45`). Đối với Level 8–10 dùng kích thước khung tham chiếu cố định `8.000f × 8.000f` (`useFixedRef`) để giữ kích thước UI/bàn cờ cân đối trên màn hình dọc.
- **Hiển thị 2D vs 2.5D**: Đã viết helper `PuzzlePiece.Apply25DBlockExtrusion()` và `Update25DPerspectiveScale()` để thử nghiệm hiệu ứng khối 2.5D trên Level 9/10; sau đánh giá thẩm mỹ đã chốt giữ toàn bộ 10 màn chơi ở chế độ **2D sắc nét**.

### h. Asset Mới Chuẩn Bị Cho Giai Đoạn Tiếp Theo (WIP)
- Đã thêm bộ sprite Win Scene tại `Assets/Sprites/Win&Lose scene/win scene/UI B1 (15)/` gồm: chữ `W, E, L, L, D, O, N, E` (`E (2).png`, `N (2).png`), `Button1.png`, `Button2.png`, `ADS.png`, `Coin.png`, `Coins.png` và thư mục `Assets/Sprites/Win&Lose scene/lose scene/`. Các asset này mới import vào repo, chưa tích hợp vào `GameUI.cs`.

---

## 3. Bản Đồ File Quan Trọng

| Đường dẫn | Vai trò | Trạng thái |
|---|---|---|
| `Assets/Scenes/MainScene.unity` | Scene khởi đầu mặc định (đang cấu hình Level 2) | Hoàn thiện |
| `Assets/Scenes/Level1.unity` .. `Level10.unity` | 10 scene độc lập cho Level 1 đến Level 10 | Hoàn thiện (Khóa) |
| `Assets/Prefabs/Levels/Level1_Board.prefab` .. `Level10_Board.prefab` | 10 prefab bàn cờ, cổng và khối cho Level 1 đến Level 10 | Hoàn thiện (Khóa) |
| `Assets/Scripts/Core/LevelManager.cs` | Quản lý 10 màn chơi, cell mapping phi tuyến, raycast, merge, escape, băng, VFX | Hoàn thiện |
| `Assets/Scripts/Core/GameConfig.cs` | Enum `PieceColor`, `PieceShape` (8 hình), `GateSide`, bảng màu & tên tiếng Việt | Hoàn thiện |
| `Assets/Scripts/Core/GameBootstrapper.cs` | Tự khởi tạo singleton/camera/EventSystem khi chạy runtime | Hoàn thiện |
| `Assets/Scripts/Gameplay/PlayerPieceController.cs` | Điều khiển kéo thả đa hướng, nhấc/nghiêng khối, bàn phím WASD/mũi tên | Hoàn thiện |
| `Assets/Scripts/Gameplay/PuzzlePiece.cs` | Render khối đa hình, số khắc chìm, băng `IceCount`, slam merge, dissolve escape | Hoàn thiện |
| `Assets/Scripts/Gameplay/ExitGateView.cs` | Cổng thoát Tech Hologram, vòng xoay, khóa ô cổng `GateCoords` | Hoàn thiện |
| `Assets/Scripts/UI/GameUI.cs` | Header, Timer linh hoạt, BoosterBar, PausePanel tím, VictoryPanel & NextLevel | Hoàn thiện |
| `Assets/Scripts/UI/TutorialHandGuide.cs` | Bàn tay & banner hướng dẫn 2 pha cho Level 1 | Hoàn thiện |
| `Assets/Scripts/Audio/SoundManager.cs` | Phát SFX 2D (`DontDestroyOnLoad`), combo merge 4 bậc, fallback procedural | Hoàn thiện |
| `Assets/Scripts/Utils/GemSpriteManager.cs` | Nạp sprite động từ `Resources` / `AssetDatabase` / disk cho mọi level & UI | Hoàn thiện |
| `Assets/Scripts/Utils/SpriteGenerator.cs` | Tạo sprite procedural cho UI Victory, Settings row, Ice overlay, Glow/Flare/Shard | Hoàn thiện |
| `Assets/Scripts/Editor/SceneSetupHelper.cs` | Menu `Tools/Block Merge Puzzle/...`, bảo vệ Level 1–10 & `AutoMcpStarter` | Hoàn thiện |
| `Assets/Resources/Shaders/SpritePortalDissolve.shader` | Shader tan biến voxel/portal khi khối thoát cổng | Hoàn thiện |
| `Assets/Resources/Sound/` | `block_move.wav`, `merge_block.wav`, `block_vanish.mp3`, `icecrak.wav` | Hoàn thiện |
| `Assets/Sprites/Main Scene/Map/` | Bản đồ `map_lv1.png` .. `map_lv10 (2).png` | Hoàn thiện |
| `Assets/Sprites/Win&Lose scene/win scene/UI B1 (15)/` | Bộ sprite Win scene mới (`WELL DONE`, `Coin`, `Coins`, `ADS`, `Button1/2`) | Mới import (WIP) |

---

## 4. Hướng Dẫn Thao Tác Nhanh (`Tools/Block Merge Puzzle/...`)

1. **Chuyển đổi & kiểm tra màn chơi trên Unity Editor**:
   - `Tools/Block Merge Puzzle/Load Level 1` .. `Load Level 10`: Mở trực tiếp scene tương ứng mà không ghi đè layout.
   - `Tools/Block Merge Puzzle/Bake Level to Scene (Idle Display)` / `Setup Scene Now`: Bake lại màn đang chọn và lưu prefab tương ứng (Level 1..10 vẫn giữ nguyên khối khi gọi qua menu).
2. **Công cụ UI & kiểm chứng nhanh**:
   - `Tools/Block Merge Puzzle/Verify Merge Slam`: Assert toàn bộ shader, sprite VFX, booster, setting UI và audio clips trong `Resources`.
   - `Tools/Block Merge Puzzle/Upgrade Settings Panel to New Style`: Nâng cấp `PausePanel` sang phong cách pha lê tím.
   - `Tools/Block Merge Puzzle/Update Level Banner & Settings Close X`: Cập nhật `Banner_Level` và nút đóng `X (1)`.
   - `Tools/Block Merge Puzzle/Ensure Time Text on Banner`: Khôi phục/đảm bảo `TimeVal` trên `Banner_Time`.
   - `Tools/Block Merge Puzzle/Add Booster UI to Scene`: Thêm `BoosterBar` mà giữ nguyên bố cục UI có sẵn.
   - `Tools/Block Merge Puzzle/Step Tutorial Verification`: Test chuyển pha `TutorialHandGuide` trong Play Mode.

---

## 5. Quy Trình Chuẩn Mở Rộng Level 11+ (Bảo Vệ Tuyệt Đối Level 1..10)

Áp dụng đúng quy trình sau khi thêm Level 11 trở lên để **tuyệt đối không sửa đổi, nướng đè hay làm lệch layout của Level 1..10 đã chốt**:

1. **Cô lập Scene & Prefab riêng biệt**:
   - Mỗi level mới tạo file `Assets/Scenes/Level{n}.unity` và `Assets/Prefabs/Levels/Level{n}_Board.prefab` riêng.
   - Giữ nguyên guard bảo vệ trong `BakeAll10Levels()` (`if (i >= 1 && i <= 10) continue;`), không chạy bake hàng loạt đè lên Level 1..10.
2. **Định nghĩa dữ liệu bàn cờ (`LevelManager.cs`)**:
   - Thêm enum vào `LevelId`, khai báo kích thước lưới (`GetLevelColumns`, `GetLevelRows`, `GetLevelCellWidth/Height`, `GetLevelFrameWidth/Height`), luật ô hợp lệ (`CheckLevelValidCoord`), bảng ánh xạ tâm ô `Level{n}CellPositions`, danh sách cổng (`BuildLevelGates`) và khối (`SpawnLevelPieces`).
   - Nếu là scene có tinh chỉnh riêng (như `IceCount` ở Level 8/9 hay cổng `RequiredValue` ở Level 10), cập nhật đồng bộ cả trong scene lẫn code nếu muốn hỗ trợ rebake sạch.
3. **Đồng bộ `GameUI.cs`**:
   - Cập nhật `MaxLevelNumber` (hiện tại `= 10`) và thời gian màn trong `GetLevelDurationSeconds()` hoặc cấu hình trực tiếp trên `TimeVal` / `_customTimeMMSS`.
4. **Áp dụng Bộ Hợp Đồng UI Chuẩn**:
   - Tái sử dụng đúng cây UI từ `Level 2..10` (`HeaderPanel`, `BoosterBar`, `PausePanel`, `VictoryPanel`, `FeedbackBanner`), không gọi `RebuildUI()` trên các scene đã khóa.
5. **Background & Camera Fit (`LevelManager.cs`)**:
   - Giữ tint dịu `Color(0.42f, 0.42f, 0.48f, 1f)` cho `BackgroundSprite` và `FitCameraToBoard()` theo tỷ lệ portrait `0.45`.
6. **Âm thanh (2D & Preload)**:
   - Mọi audio clip mới đặt ở `Assets/Resources/Sound/` với `preloadAudioData: 1`, `3D: 0` trong `.meta` và phát qua `SoundManager.Instance`.
7. **Build Settings & Kiểm Thử**:
   - Thêm `Assets/Scenes/Level{n}.unity` vào `EditorBuildSettings`, kiểm thử Play Mode (kéo, merge, thoát cổng, Victory -> Next Level), chụp ảnh nghiệm thu vào `Assets/Screenshots/` và commit lên git.

---

## 6. Hạng Mục Tiếp Theo (Next Session Roadmap)

1. **Nâng cấp UI Win & Lose bằng bộ asset Figma mới (`Assets/Sprites/Win&Lose scene/`)**:
   - Tích hợp chữ `WELL DONE`, `Coin.png` / `Coins.png`, `ADS.png`, `Button1.png`, `Button2.png` vào `VictoryPanel` (hỗ trợ thưởng xu & nhân thưởng quảng cáo).
   - Thiết kế hoàn chỉnh **Lose Popup** khi hết giờ (`GameUI.OnTimeExpired()` hiện mới báo lỗi text) kèm nút Chơi Lại / Thêm Thời Gian.
2. **Hoàn thiện Booster & Kinh tế trong game**:
   - Bổ sung chức năng thực chiến cho **Booster_Wand** (`OnBoosterWandClicked` — ví dụ phá băng / xóa khối / nâng giá trị khối).
   - Kết nối hệ thống Coin, nút **No Ads** và hồi lượt Booster.
3. **Nhạc nền (BGM) & Rung phản hồi (Haptics)**:
   - Bổ sung kênh nhạc nền trong `SoundManager` gắn với toggle `Music`, và gọi rung phản hồi (`Handheld.Vibrate` / Android haptic) gắn với toggle `Vibration`.
4. **Đồng bộ Inspector & Mở rộng màn chơi**:
   - Đồng bộ lại các tinh chỉnh đang lưu trên scene vào `LevelManager.cs` (`Level8` & `Level9` frozen `IceCount = 4`, `Level10` cổng Đỏ `req = 5`, và thời lượng/giá trị khối tương ứng) để code và scene khớp 100%, đồng thời cập nhật `MaxLevelNumber` khi triển khai Level 11+.
