# BÁO CÁO TIẾN ĐỘ & BÀN GIAO DỰ ÁN BLOCK MERGE PUZZLE

## 1. Trạng Thái Hiện Tại (Ready for Next Session - Level 1, Level 2, Level 3 & Level 4 ĐÃ CHỐT)
- **Hệ thống màn chơi hoàn chỉnh & Đã khóa nghiệm thu (Finalized & Frozen)**:
  - **Level 1 (Tutorial Cross Map - ĐÃ CHỐT)**: Bàn cờ chữ thập 7×6, giữ trọn vẹn nền tím tự nhiên của asset `Map lv1.png`. Hiển thị đầy đủ chữ hướng dẫn `InstructionBanner` ("Ghép 2 khối cùng màu và số") tại pos `(-24.76, 375.68)`, banner `LEVEL 1`, đồng bộ 100% bố cục UI với Level 2.
  - **Level 2 (Cross Map - map_lv2.png - ĐÃ CHỐT)**: Bàn cờ chữ thập lệch tâm 6×6 (11 ô hành lang: cột 2 dọc [row 0..5] và hàng 3 ngang [col 0..5]) xây dựng theo asset `map_lv2.png` / `map_lv2 (3).png`. Cổng thoát công nghệ cao (Tech Hologram) và 5 khối 1x1 (3 Đỏ, 2 Xanh lá) sắp xếp chuẩn xác theo thiết kế [Image #12]. Tích hợp bảng ánh xạ tọa độ pixel chuẩn `Level2CellPositions` căn giữa tuyệt đối trong lòng ô đá.
  - **Level 3 (Branching Map - Map_lv3.png - ĐÃ CHỐT)**: Bàn cờ phân nhánh 5×7 (15 ô hợp lệ) xây dựng từ asset `Map_lv3.png`. 2 cổng thoát công nghệ cao (Tech Hologram): Cổng trên-trái (Xanh lá [4], hướng sang trái `Vector2.left`) và Cổng giữa-phải (Đỏ [4], hướng sang phải `Vector2.right`). 8 khối 1x1 (4 Đỏ: [1], [1], [2], [3] và 4 Xanh lá: [1], [1], [2], [3]) sắp xếp chuẩn xác 100% theo mẫu thiết kế [Image #17]. Tích hợp bảng ánh xạ tọa độ pixel chuẩn `Level3CellPositions` căn giữa tuyệt đối trong lòng các ô đá vẽ tay. Thời gian 5:00 (`300s`), banner `LEVEL 3`, đồng bộ 100% bố cục UI từ Level 2.
  - **Level 4 (E-Shape Map - map_lv4.png - ĐÃ CHỐT)**: Bàn cờ chữ E 4×5 (14 ô hợp lệ: 3 hành lang ngang hàng 0, 2, 4 mỗi hàng 4 ô [col 0..3], nối qua hành lang dọc cột 0). 3 cổng thoát công nghệ cao (Tech Hologram) hướng phải: Đỏ [3] tại `(3, 0)`, Xanh lá [3] tại `(3, 2)`, Hồng [2] tại `(3, 4)`. 7 khối 1x1 (3 Đỏ, 3 Xanh lá, 1 Hồng) sắp xếp chuẩn xác 100% theo thiết kế [Image #18] và được người dùng tinh chỉnh trực tiếp trên scene. Tích hợp từ điển pixel mapping chuẩn `Level4CellPositions` căn giữa tuyệt đối từng khối và cổng vào tâm các lỗ đá. Thời gian 5:00 (`300s`), banner `LEVEL 4`, bảo toàn 100% layout UI chuẩn Level 2.
- **Bảo Vệ Độc Lập Scene (Scene Protection Mechanism)**:
  - `SceneSetupHelper.LoadLevel(LevelId level)` đã chuyển hoàn toàn sang nạp trực tiếp file `Assets/Scenes/{level}.unity` qua `EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single)`, **tuyệt đối không chạy procedural bake đè lên scene đang mở**.
  - Đã loại bỏ hoàn toàn các hook tự động ghi đè scene, đảm bảo khi tác giả làm các level sau (Level 5..10) thì **Level 1, Level 2, Level 3 và Level 4 được cô lập 100%, không bị sửa đổi, không bị giật lệch UI, không bị reset khối hay cổng**.
- **Bộ Hợp Đồng Tọa Độ UI Chuẩn (UI Layout Contract)**:
  - `HeaderPanel`: pos `(0, -50)`, size `(1000, 190)`
    - `Btn_Replay`: pos `(-378.00, 5.50)`, size `(215.94, 201.01)`
    - `Banner_Time`: pos `(16.00, -80.00)`, size `(430.00, 154.00)` (thời gian 5:00)
    - `Btn_Pause`: pos `(374.00, 0.00)`, size `(171.29, 190.00)`
    - `Banner_Level`: pos `(21.00, 32.00)`, size `(276.00, 64.00)` (`LEVEL 1` / `LEVEL 2` / `LEVEL 3` / `LEVEL 4`)
  - `TutorialLayer`:
    - `InstructionBanner`: pos `(-24.76, 375.68)`, size `(600.47, 92.64)`
  - `BoosterBar`: pos `(0.00, 60.00)`, size `(900.00, 260.00)`
    - `Booster_Freeze`: pos `(-318.00, 101.00)`, size `(321.29, 339.44)`
    - `Booster_Magnet`: pos `(0.00, 104.76)`, size `(366.55, 331.93)`
    - `Booster_Wand`: pos `(311.00, 93.38)`, size `(323.64, 354.69)`
  - `PausePanel`: pos `(0, 0)`, `DialogBox`: pos `(0, 0)`, size `(880, 756)`
  - `VictoryPanel`: pos `(0, 0)`, `DialogBox`: pos `(0, 0)`, size `(760, 580)`
  - `FeedbackBanner`: pos `(0, 140)`, size `(960, 110)`
- **Lưu trữ Scene & Prefab độc lập**:
  - `Assets/Scenes/MainScene.unity`: Scene làm việc mặc định.
  - `Assets/Scenes/Level1.unity`: Scene độc lập cho Level 1 (Đã khóa nghiệm thu).
  - `Assets/Scenes/Level2.unity`: Scene độc lập cho Level 2 (Đã khóa nghiệm thu).
  - `Assets/Scenes/Level3.unity`: Scene độc lập cho Level 3 (Đã khóa nghiệm thu).
  - `Assets/Scenes/Level4.unity`: Scene độc lập cho Level 4 (Đã khóa nghiệm thu).
  - `Assets/Prefabs/Levels/Level1_Board.prefab`: Prefab nguyên bản bàn cờ Level 1.
  - `Assets/Prefabs/Levels/Level2_Board.prefab`: Prefab nguyên bản bàn cờ Level 2.
  - `Assets/Prefabs/Levels/Level3_Board.prefab`: Prefab nguyên bản bàn cờ Level 3.
  - `Assets/Prefabs/Levels/Level4_Board.prefab`: Prefab nguyên bản bàn cờ Level 4.
- **Git Repository**: `https://github.com/huy113-dev/BlockMergePuzzle.git`
  - Nhánh hiện tại: `feat/level-4-map` (chuẩn bị commit và push đồng bộ).
- **Build Settings Scenes**:
  - `[0] Assets/Scenes/MainScene.unity`
  - `[1] Assets/Scenes/Level1.unity`
  - `[2] Assets/Scenes/Level2.unity`
  - `[3] Assets/Scenes/Level3.unity`
  - `[4] Assets/Scenes/Level4.unity`
- **Hệ thống kết nối Unity MCP**: FastMCP HTTP bridge hoạt động ổn định trên `http://127.0.0.1:8080/mcp`. Tự động kết nối lại khi domain reload nhờ `AutoMcpStarter` trong `SceneSetupHelper.cs`.
- **Screenshot kiểm chứng**:
  - `Assets/Screenshots/verify_level4.png`: Ảnh chụp Game View Level 4 hoàn chỉnh chuẩn 100% mẫu [Image #18] với 7 khối, 3 cổng hologram, thời gian 05:00 và banner LEVEL 4.
  - `Assets/Screenshots/verify_level3.png`: Ảnh chụp Game View Level 3 hoàn chỉnh chuẩn 100% mẫu [Image #17] với 8 khối, 2 cổng hologram, thời gian 05:00 và banner LEVEL 3.
  - `Assets/Screenshots/level2_current_saved.png`: Ảnh chụp Game View chính thức của Level 2 được lưu trực tiếp từ bố cục của người dùng (khối xanh lá căn giữa chuẩn xác, 2 cổng hologram, thanh Header và 3 booster).
  - `Assets/Screenshots/level2_final_preview.png`: Ảnh chụp Game View hoàn thiện Level 2 với map_lv2, 2 cổng hologram, 5 khối xếp chuẩn Image #12, thời gian 05:00 và banner LEVEL 2.
  - `Assets/Screenshots/level1_final_preview.png`: Ảnh chụp Game View Level 1 hoàn chỉnh do user xếp: nút Replay, LEVEL 1, thời gian 05:00, nút Pause, bàn cờ Level 1 và thanh 3 Booster khóa dưới đáy.
  - `Assets/Screenshots/pause_panel_preview.png`: Ảnh chụp popup Pause Game hoàn thiện chuẩn xác theo Image #7 với nền game tối mờ phía sau.
  - `Assets/Screenshots/pause_panel_toggled.png`: Ảnh chụp trạng thái toggle OFF của Sound & Music (icon xám + switch xám).
  - `Assets/Screenshots/normal_gameplay_preview.png`: Ảnh chụp màn chơi bình thường với thanh Header UI và nền vũ trụ.
  - `Assets/Screenshots/screenshot-20261102-143623.png`: Ảnh chụp Level 1 hoàn thiện với Header UI và nền vũ trụ.
  - `Assets/Screenshots/screenshot-20261102-143627.png`: Ảnh chụp Level 2 hoàn thiện với Header UI và nền vũ trụ.

---

## 2. Các Tính Năng & Lỗi Kỹ Thuật Đã Xử Lý

### a. Hệ thống Level 1 Tutorial Cross Map
- **Hệ tọa độ chữ thập**: Grid 7 cột (0..6) × 6 hàng (0..5). Hành lang hợp lệ: `(col == 3 && row >= 0 && row < 6) || (row == 2 && col >= 0 && col < 7)`.
- **Căn gốc tọa độ**: Giao điểm `(3, 2)` cố định tại `Vector3.zero`. Tọa độ chuyển đổi:
  - `x = (col - 3) * CurrentCellWidth`
  - `y = (2 - row) * CurrentCellHeight`
- **Bố cục cổng & khối**:
  - Cổng trên: Xanh lá [2] tại `(0, 3.15, 0)`.
  - Cổng trái: Đỏ [3] tại `(-3.90, 0, 0)`.
  - Khối: 1 Đỏ [2] tại tâm, 2 Đỏ [1] ở 2 đầu cánh ngang, 2 Xanh lá [1] ở 2 đầu cánh dọc.
- **Badge tương phản cổng (`ExitGateView.cs`)**: Tạo đĩa tròn tối màu `#141724E0` phía sau số cổng (`sortingOrder = 4`) giúp chữ số hiển thị rõ ràng trên nền mũi tên trắng.
- **Lược bỏ gạch sàn**: Đã loại bỏ các GameObject `FloorTile` trong hành lang, bảo toàn nền tím nguyên bản của asset `Map lv1.png`.

### b. Chuyển động giật giật (Movement Jitter / Teleport)
- **Đã sửa**:
  - `PlayerPieceController.cs`: Loại bỏ reset vị trí khi nhả chuột; tính toán `targetSteps` theo ngưỡng 35% khoảng cách; thêm rubber-band khi kéo vướng chướng ngại vật; tự động lướt mượt về vị trí cũ nếu hủy kéo.
  - `PuzzlePiece.cs`: Dùng đường cong vận tốc liên tục `EaseOutQuad` (`1f - (1f - t) * (1f - t)`); tính `duration` động theo khoảng cách thực tế `Mathf.Clamp(dist / 9.5f, 0.08f, 0.25f)`.

### c. Merge thẳng hàng (Collinear Straight-Line Merge)
- **Đã sửa** trong `LevelManager.cs`:
  - Ép điều kiện thẳng hàng tuyệt đối:
    - Di chuyển ngang: `piece.GridOrigin.y == hitPiece.GridOrigin.y`
    - Di chuyển dọc: `piece.GridOrigin.x == hitPiece.GridOrigin.x`
  - Khóa điều kiện va chạm đa điểm (`multipleHits == false`).
  - Kiểm tra đủ 3 yếu tố: cùng hình dạng (`Shape`), cùng màu (`PieceColor`), cùng giá trị (`Value`).

### d. Căn giữa cổng khi thoát (Centered Gate Exit)
- **Đã sửa** trong `LevelManager.cs`:
  - `SlideActionType.Escape`: Gán `targetWorldPos = result.TargetGate.transform.position`. Khối trượt vào chính giữa cổng trước khi chạy hiệu ứng bay ra ngoài theo hướng `gate.ExitDirection`.

### e. Tối ưu vừa vặn màn hình tỷ lệ hẹp dài 1440x3200 (Portrait 0.45)
- **Đã sửa**: `SceneSetupHelper.cs` và `FitCameraToBoard()` trong `LevelManager.cs`. Camera tính toán theo cả width và height: `Mathf.Max(sizeByWidth, sizeByHeight)` với `orthographicSize = 10.45f` và vị trí `(0, 0, -10)`. Kèm clamp chống vỡ khung hình trong Edit Mode khi máy tính ở dạng landscape (`if (screenRatio > 0.75f) screenRatio = 1440f / 3200f;`).

### f. Game Feel: Cảm giác nhấc khối (Lift on Touch & Responsive Drag)
- **Tính năng**: Khi chạm hoặc kéo khối, khối phóng to nhẹ (1.075x), nhấc nổi lên `Z = -0.1f` và `Y +0.08f`, nâng `sortingOrder = 50` để nổi trên các khối khác; phát âm thanh click nhẹ (`SoundManager.PlayPickup`); nghiêng động (`tilt`) theo vận tốc di chuột và hạ mượt mà về kích thước gốc khi thả.

### h. Bảo Toàn Layout UI Thủ Công của Người Dùng & Lưu Màn Hình Game
- **Đã xử lý**:
  - Người dùng chỉnh sửa thủ công layout UI trên `MainScene.unity` để căn chỉnh trực quan (Nút Replay, Đồng hồ thời gian đếm ngược 5:00, Nút Pause, Banner Level, Booster Bar và Camera `orthoSize = 9.155556f`).
  - Cập nhật `GameUI.cs` và `SceneSetupHelper.cs`: Bổ sung `BindExistingUI()` để liên kết các thành phần UI sẵn có mà không gọi `RebuildUI()`, bảo toàn 100% tọa độ RectTransform thủ công của người dùng trong mọi lần nạp scene hoặc chuyển đổi chế độ.
  - Lưu và đồng bộ trạng thái `MainScene.unity`, `Assets/Scenes/Level2.unity` và `Assets/Prefabs/Levels/Level2_Board.prefab`.

### i. Cơ Chế Khóa Cổng: Khối Không Được Dẫm Lên Gate Khi Chưa Đủ Điều Kiện
- **Vấn đề**:
  - Trong các bản đồ chữ thập như Level 2 (`map_lv2.png`), cổng thoát nằm trực tiếp trên tọa độ grid vật lý `(col 2, row 0)` (Cổng trên Xanh lá) và `(col 5, row 3)` (Cổng phải Đỏ). Trước đây các ô này được coi như ô hành lang trống thông thường, dẫn tới khối bất kỳ có thể trượt vào hoặc dẫm lên cổng.
- **Giải pháp & Triển khai**:
  - `ExitGateView.cs`: Thêm trường `GateCoords`, phương thức `ContainsGateCell(Vector2Int coord)` và `EnsureGateCoords()` tự động nội suy tọa độ ô bàn cờ chứa cổng (`TriggerCoords - TriggerDirection`).
  - `LevelManager.cs`:
    - Bổ sung `GetGateAt(Vector2Int coord)` để tra cứu nhanh cổng tại bất kỳ tọa độ nào.
    - Cập nhật `CanPieceFitAt(piece, origin)`: Trả về `false` ngay lập tức nếu bất kỳ ô chiếm dụng nào của khối trùng với ô cổng (`GetGateAt(c) != null`). Điều này đảm bảo không khối nào có thể đặt vị trí nghỉ hoặc snap lên ô cổng.
    - Cập nhật `CheckEscapeOpportunity`: Kiểm tra cổng tại các ô ứng viên trước khi cho phép thoát.
    - Cập nhật `RaycastSlide`: Đưa kiểm tra cổng lên trước kiểm tra tường ngoài. Nếu va chạm cổng và khối cùng hướng:
      - Nếu khác màu hoặc chưa đủ giá trị: Cổng đóng vai trò như bức tường chắn vững chắc (`result.Action = Blocked`, `result.Steps = 0` hoặc dừng ngay trước cổng), phát âm thanh lỗi và hiển thị thông báo: *"Sai màu cổng!"* hoặc *"Chưa đủ số yêu cầu!"*.
      - Chỉ khi khối cùng màu VÀ `Value >= RequiredValue`: Cho phép thoát (`result.Action = Escape`), khối trượt vào giữa cổng và biến mất.
  - `PlayerPieceController.cs`:
    - Trong khi kéo ngón tay: Khối bị giới hạn rung nhẹ tối đa `0.08 units` nếu cố đẩy vào cổng không hợp lệ.
    - Khi thả ngón tay: Nếu người dùng cố đẩy khối vào cổng không hợp lệ, hệ thống tự động phát âm thanh lỗi, hiển thị thông báo lỗi cụ thể qua `GameUI.ShowFeedbackError` và snap khối an toàn về ô đứng ngay trước cổng, tuyệt đối không dẫm lên cổng.

### h. Tích hợp Background vũ trụ pha lê (`background_main scene .png`)
- **Asset**: `Assets/Sprites/Background/background_main scene .png` (kích thước gốc 1441×3200, TextureType = Sprite 2D and UI, maxTextureSize = 4096px bảo toàn độ phân giải cao).
- **Auto Aspect-Fill**: Tự động scale theo camera (`Mathf.Max(camH / nativeH, camW / nativeW)`) đảm bảo phủ kín 100% màn hình dọc 1440×3200 và mọi tỷ lệ hiển thị khác, không bị méo tỉ lệ hay hở viền đen.
- **Layering**: Đặt tại `sortingOrder = -10`, nằm sau khung bàn cờ (`sortingOrder = 2`), badge cổng (`4`), chữ số cổng (`5-6`) và các khối đá quý (`10-50`).
- **Tự động áp dụng & Bake**: Nạp tự động trong `GemSpriteManager.GetBackgroundSprite()`, `LevelManager.BuildBackground()` và `FitBackgroundToCamera()` cho mọi level (Level 1, Level 2, MainScene) và các prefab tương ứng (`Level1_Board.prefab`, `Level2_Board.prefab`).

### i. Thiết kế thanh Header UI (Return, Time 1:40, Pause)
- **Thiết kế theo mẫu [Image #5]**:
  - **Nút Return (Trái)**: Sprite `Return.png` (khối đá cổ có mũi tên quay lại từ `Sprites/LevelAssets/Return.png`), kích thước 145×145, hiệu ứng punch scale khi bấm, gọi `RestartLevel()` reset bàn cờ và bộ đếm thời gian.
  - **Banner Time (Giữa)**: Sprite `Time.png` (hộp kim loại viền neon xanh cyan và icon đồng hồ bấm giờ), kích thước 360×157, tích hợp text đếm ngược động `1:40` (100 giây), cảnh báo chuyển đỏ khi còn dưới 20 giây.
  - **Nút Pause (Phải)**: Sprite `Pause.png` (khối đá cổ có biểu tượng ||), kích thước 145×145, hiệu ứng punch scale khi bấm, mở modal Tạm Dừng (`Time.timeScale = 0f`).

### j. Thiết kế hoàn chỉnh popup Pause Game Panel ([Image #7])
- **Background Game Darkened Overlay**: Khi mở Pause panel, màn hình được phủ lớp bán trong suốt `#03050ECC` (`alpha = 0.82`), làm mờ và tối khung cảnh game (bàn cờ, khối đá, background vũ trụ, thanh header) nhưng vẫn thấy rõ trạng thái game bên dưới theo đúng yêu cầu.
- **Cấu trúc 14 Assets Sci-Fi chuyên biệt** từ `D:\BlockMergePuzzle\Assets\Sprites\Main Scene\Pause game`:
  - **Khung hộp thoại chính (`panel.png`)**: Khung kim loại viền xanh neon kích thước 820×1040, đặt cân đối giữa màn hình.
  - **Tiêu đề vương miện băng (`Panel game pause.png`)**: "Game Pause" với các tinh thể pha lê băng viền neon xanh, gắn tại đỉnh notch trên cùng.
  - **Nút đóng X (`X.png`)**: Nút tròn đỏ X kích thước 105×105 nằm tại góc vai phải, bấm để tiếp tục game (`ResumeGame()`).
  - **Thẻ cài đặt màu tối (SettingsCard)**: Hộp chứa màu xanh navy sâu `#11152d` với viền xanh neon, chứa 3 hàng tùy chỉnh ngăn cách bởi đường kẻ ngang:
    - **Sound**: Icon loa xanh `Group 308.png` (ON) / loa xám `Group 314.png` (OFF) + text "Sound" + công tắc switch `on.png` / `off.png`. Kết nối trực tiếp `SoundManager.Instance.SoundEnabled`.
    - **Music**: Icon nốt nhạc cam `Group 312.png` (ON) / nốt nhạc xám `Group 315.png` (OFF) + text "Music" + công tắc switch `on.png` / `off.png`.
    - **Vibration**: Icon rung điện thoại cyan `Group 313.png` (ON) / xám `Group 316.png` (OFF) + text "Vibration" + công tắc switch `on.png` / `off.png`.
    - Trạng thái tự động lưu vào `PlayerPrefs` (`BMP_SoundEnabled`, `BMP_MusicEnabled`, `BMP_VibrationEnabled`).
  - **Nút No Ads (`No Ads.png`)**: Nút vàng ánh kim rộng 620×156 có biểu tượng giỏ hàng & cấm quảng cáo.
  - **Cụm nút điều hướng chân trang**:
    - **Nút Return (`Return.png` trong Pause game)**: Nút tròn xanh cyan 150×150 với mũi tên quay tròn, bấm để chơi lại màn chơi hiện tại.
    - **Nút Home (`Home.png`)**: Nút lục giác xanh lá 390×141 có chữ "Home", bấm để về trang chủ / reset bàn cờ.
- **Tách biệt Asset Namespace (Disambiguation)**: Dùng `GemSpriteManager.GetHeaderReturnSprite()` cho nút Return trên thanh Header (khối đá 170×171) và `GemSpriteManager.GetPauseSprite("Return")` cho nút Return trong Pause panel (nút tròn cyan 92×93), tránh triệt để xung đột trùng tên file.
- **Đồng bộ bố cục người dùng chỉnh tay (User Custom Layout Sync)**: Tọa độ tinh chỉnh trực tiếp trên scene của người dùng đã được trích xuất và cập nhật vào `GameUI.BuildPausePanel()`, đồng thời bake đồng nhất trên toàn bộ các scene `Level1.unity`, `Level2.unity` và `MainScene.unity`.
- **Render Mode**: Sử dụng `RenderMode.ScreenSpaceCamera` với `Camera.main`, `planeDistance = 5f`, `sortingOrder = 100`, hiển thị hoàn hảo và ổn định cả trong Edit Mode bake lẫn Runtime.

### k. Hệ thống Level 2 Cross Map (`map_lv2.png` / `map_lv2 (3).png`)
- **Phân tích hình học asset & Khớp tọa độ (Pixel Alignment)**:
  - Khung đá kích thước gốc 1303×1183, viền đá xám cổ với 2 cổng thoát (Xanh lá phía trên, Đỏ bên phải).
  - Phân tích tọa độ tâm cổng theo pixel: Green pad tại `(655.5, 107.0)`, Red pad tại `(1215.5, 613.0)`.
  - Cấu hình kích thước ô: `CellWidth = 1.16f`, `CellHeight = 1.03f`.
  - Frame scale: `FrameWidth = 8.000f`, `FrameHeight = 7.263f`.
  - GridOffset: `Vector3(0.617f, 0.426f, 0f)` căn chỉnh chuẩn xác sub-pixel toàn bộ 11 ô đá hành lang vào khung đá.
- **Hành lang hợp lệ (Corridor Layout)**:
  - Grid logic 6 cột (0..5) × 6 hàng (0..5).
  - Tọa độ hợp lệ: `(col == 2 && row >= 0 && row < 6) || (row == 3 && col >= 0 && col < 6)`.
  - Giao điểm hành lang tại `(col 2, row 3)`.
- **Cổng thoát công nghệ cao (Tech Hologram Exit Gates)**:
  - Áp dụng `ExitGateView.Create(..., isTechHologram: true)` giống phong cách Level 1: vòng xoay cyber ngoài, vòng xoay ngược trong, bóng đổ, chữ số 3D phát sáng và đế holographic.
  - Cổng trên: Xanh lá (Green), yêu cầu số 2, hướng lên `Vector2.up`, trigger `(2, -1)`.
  - Cổng phải: Đỏ (Red), yêu cầu số 1, hướng sang phải `Vector2.right`, trigger `(6, 3)`.
- **Bố cục 5 khối khởi đầu theo [Image #12]**:
  - Khối Đỏ [2] tại `(2, 1)`.
  - Khối Đỏ [1] tại `(2, 3)` (ngay giao điểm hành lang).
  - Khối Đỏ [2] tại `(2, 5)`.
  - Khối Xanh lá [1] tại `(0, 3)`.
  - Khối Xanh lá [1] tại `(4, 3)`.
- **Tính giải được (Solvability Check)**:
  1. Trượt khối Đỏ [1] từ `(2, 3)` xuống `(2, 4)` để mở giao điểm.
  2. Trượt khối Xanh lá [1] từ `(0, 3)` sang phải qua giao điểm gộp với Xanh lá [1] tại `(4, 3)` -> tạo thành Xanh lá [2].
  3. Trượt khối Đỏ [2] tại `(2, 1)` xuống `(2, 3)` rồi rẽ phải qua cổng Đỏ [1] -> thoát cổng.
  4. Lần lượt trượt khối Đỏ [1] và Đỏ [2] còn lại ra cổng Đỏ.
  5. Đưa khối Xanh lá [2] về giao điểm và trượt lên cổng Xanh lá [2] -> chiến thắng.
- **Tự động hóa UI & Tutorial**:
  - Thời gian màn: 5 phút (`300s` / `05:00`).
  - Banner hiển thị chuẩn `LEVEL 2`.
  - `TutorialHandGuide` tự động ẩn (chỉ kích hoạt ở Level 1).

### l. Căn Chỉnh Vị Trí Ô Chuẩn Pixel Cho Bản Đồ Vẽ Tay Level 2 (Non-Linear Grid Mapping)
- **Vấn đề [Image #13]**: Asset `map_lv2.png` là bản vẽ tay bất đối xứng (nhánh hành lang trái 2 ô chiếm 430 px, nhánh phải 3 ô chiếm 526 px). Khi dùng công thức bước nhảy tuyến tính `col * cw`, khối xanh lá ngoài cùng bên trái `(0, 3)` bị lệch sang phải ~0.40 units, đè lên vạch ngăn ô của hành lang.
- **Giải pháp & Triển khai**:
  - Dùng Python đo tọa độ pixel các vạch ngăn thực tế trên ảnh gốc `map_lv2.png`.
  - Bổ sung bảng ánh xạ vị trí pixel chuẩn xác `Level2CellPositions` trong `LevelManager.cs` cho toàn bộ 11 ô của Level 2.
  - Cập nhật `GridToWorld(col, row)`: Đối với Level 2, tra cứu trực tiếp từ `Level2CellPositions` (tọa độ ô `(0, 3)` chuẩn là `(-2.672f, -0.047f)`).
  - Cập nhật lại vị trí khối và cổng trong `MainScene.unity`, `Assets/Scenes/Level2.unity` và `Assets/Prefabs/Levels/Level2_Board.prefab`. Khối xanh lá [1] nằm chính giữa ô hành lang với lề đều 2 bên viền đá.

### m. Hệ Thống Level 3 Branching Map (`Map_lv3.png`) - [ĐÃ CHỐT HOÀN THIỆN]
- **Bản đồ 5 cột × 7 hàng**:
  - Xây dựng từ asset khung đá viền xanh `Map_lv3.png` kích thước 1144×1456 px.
  - 15 ô hợp lệ chia thành 3 nhánh nối qua cột trung tâm `col 2`:
    - Hàng 0: 3 ô `(0, 0)`, `(1, 0)`, `(2, 0)` nối thẳng tới Cổng Xanh lá bên trái.
    - Cột 2 (trục dọc): 7 ô từ `row 0` đến `row 6`.
    - Hàng 3 (nhánh phải): 3 ô `(2, 3)`, `(3, 3)`, `(4, 3)` nối thẳng tới Cổng Đỏ bên phải.
    - Hàng 6 (đáy ngang): 5 ô `(0, 6)` đến `(4, 6)`.
- **Ánh xạ tọa độ chuẩn xác (`Level3CellPositions`)**:
  - Phân tích pixel và tính toán bảng tra cứu vị trí thực tế cho toàn bộ 15 ô, giúp các khối đá quý và cổng holographic nằm chuẩn xác 100% tại tâm các hốc đá vẽ tay.
- **Cổng thoát Hologram công nghệ cao**:
  - Cổng trên-trái: Xanh lá [4], hướng thoát `Vector2.left` tại `(0, 0)`.
  - Cổng giữa-phải: Đỏ [4], hướng thoát `Vector2.right` tại `(4, 3)`.
- **Khối khởi đầu (8 khối 1x1 chuẩn mẫu [Image #17])**:
  - 4 khối Đỏ: Đỏ [1] tại `(0, 6)`, Đỏ [1] tại `(2, 4)`, Đỏ [2] tại `(2, 5)`, Đỏ [3] tại `(2, 6)` (gộp đủ thành Đỏ 4).
  - 4 khối Xanh lá: Xanh [1] tại `(3, 3)`, Xanh [1] tại `(3, 6)`, Xanh [2] tại `(1, 6)`, Xanh [3] tại `(4, 6)` (gộp đủ thành Xanh 4).
- **Đồng bộ UI & Độc lập Scene**:
  - Kế thừa toàn bộ hợp đồng UI từ Level 2 (Replay, Pause, Timer 05:00, 3 Booster bar).
  - Tự động hiển thị `LEVEL 3` trên `Banner_Level`.
  - Lưu khép kín trong `Assets/Scenes/Level3.unity` và `Assets/Prefabs/Levels/Level3_Board.prefab`.

### n. Hệ Thống Level 4 E-Shape Map (`map_lv4.png`) - [ĐÃ CHỐT HOÀN THIỆN]
- **Bản đồ chữ E 4 cột × 5 hàng**:
  - Xây dựng từ asset khung đá viền nâu xám `map_lv4.png` (kích thước frame `7.088 × 8.390`, offset `(0.07f, 0.29f, 0f)`).
  - 14 ô hợp lệ chia thành 3 hành lang ngang và 1 trục dọc phía tây:
    - Hàng 0 (Hành lang trên): 4 ô `(0, 0)`, `(1, 0)`, `(2, 0)`, `(3, 0)` nối tới Cổng Đỏ bên phải.
    - Hàng 1 (Trục dọc kết nối): ô `(0, 1)`.
    - Hàng 2 (Hành lang giữa): 4 ô `(0, 2)`, `(1, 2)`, `(2, 2)`, `(3, 2)` nối tới Cổng Xanh lá bên phải.
    - Hàng 3 (Trục dọc kết nối): ô `(0, 3)`.
    - Hàng 4 (Hành lang dưới): 4 ô `(0, 4)`, `(1, 4)`, `(2, 4)`, `(3, 4)` nối tới Cổng Hồng bên phải.
- **Đồng bộ tọa độ tinh chỉnh thủ công (`Level4CellPositions`)**:
  - Người dùng trực tiếp căn chỉnh vị trí khung, khối và cổng trên scene để đạt độ thẩm mỹ tối đa.
  - Trích xuất toàn bộ 14 vị trí tâm ô thực tế và cập nhật vào `Level4CellPositions` trong `LevelManager.cs`, đảm bảo khi kéo thả, snap, replay hoặc nạp scene không bị nhảy về vị trí cũ:
    - Hàng 0: Y = `3.215f`
    - Hàng 1: Y = `1.770f`
    - Hàng 2: Y = `0.325f`
    - Hàng 3: Y = `-1.068f`
    - Hàng 4: Y = `-2.460f`
    - Cổng thoát X = `2.460f`
- **Căn chỉnh Gate Transform Root & Colliders**:
  - Chuẩn hóa vị trí gốc (root transform) của 3 cổng thoát tại `X = 2.460f` và reset local position các thành phần con (`LabelRoot`, `FloatingDigit`, `GateTechRing`) về `(0, 0, 0)`, đảm bảo collider vật lý chặn khối và đường ray trượt thoát cổng đồng trục 100% với lòng hốc đá.
- **Cổng thoát Hologram công nghệ cao (3 cổng hướng phải `Vector2.right`)**:
  - Cổng trên (Row 0): Đỏ [3] tại `(3, 0)`.
  - Cổng giữa (Row 2): Xanh lá [3] tại `(3, 2)`.
  - Cổng dưới (Row 4): Hồng [2] tại `(3, 4)`.
- **Bố cục 7 khối khởi đầu theo thiết kế [Image #18]**:
  - 3 khối Đỏ: Đỏ [2] tại `(0, 0)`, Đỏ [1] tại `(0, 4)`, Đỏ [1] tại `(2, 4)` (gộp đủ thành Đỏ 3).
  - 3 khối Xanh lá: Xanh lá [2] tại `(1, 0)`, Xanh lá [1] tại `(2, 0)`, Xanh lá [1] tại `(1, 4)` (gộp đủ thành Xanh lá 3).
  - 1 khối Hồng: Hồng [2] tại `(2, 2)` (thoát cổng Hồng 2).
- **Kiểm chứng tính giải được bằng thuật toán BFS**:
  - Chạy mô phỏng không gian trạng thái đầy đủ, xác nhận giải được 100% trong 22 bước trượt thẳng hàng và thoát cổng.
- **Đồng bộ UI & Độc lập Scene**:
  - Kế thừa toàn bộ hợp đồng UI từ Level 2 & 3 (Replay, Pause, Timer 05:00, 3 Booster bar).
  - Hiển thị tự động `LEVEL 4` trên `Banner_Level`.
  - Lưu trữ khép kín trong `Assets/Scenes/Level4.unity` và `Assets/Prefabs/Levels/Level4_Board.prefab`.

---

## 3. Bản Đồ File Quan Trọng

| Đường dẫn | Vai trò | Trạng thái |
|---|---|---|
| `Assets/Scenes/MainScene.unity` | Scene chính của game | Hoàn thiện |
| `Assets/Scenes/Level1.unity` | Scene riêng biệt nạp sẵn Level 1 Tutorial | Hoàn thiện (Khóa) |
| `Assets/Scenes/Level2.unity` | Scene riêng biệt nạp sẵn Level 2 Cross Map | Hoàn thiện (Khóa) |
| `Assets/Scenes/Level3.unity` | Scene riêng biệt nạp sẵn Level 3 Branching Map | Hoàn thiện (Khóa) |
| `Assets/Scenes/Level4.unity` | Scene riêng biệt nạp sẵn Level 4 E-Shape Map | Hoàn thiện (Khóa) |
| `Assets/Prefabs/Levels/Level1_Board.prefab` | Prefab trọn bộ bàn cờ, cổng, khối Level 1 | Hoàn thiện (Khóa) |
| `Assets/Prefabs/Levels/Level2_Board.prefab` | Prefab trọn bộ bàn cờ, cổng, khối Level 2 | Hoàn thiện (Khóa) |
| `Assets/Prefabs/Levels/Level3_Board.prefab` | Prefab trọn bộ bàn cờ, cổng, khối Level 3 | Hoàn thiện (Khóa) |
| `Assets/Prefabs/Levels/Level4_Board.prefab` | Prefab trọn bộ bàn cờ, cổng, khối Level 4 | Hoàn thiện (Khóa) |
| `Assets/Scripts/Core/LevelManager.cs` | Logic bàn cờ đa level, raycast, merge thẳng hàng, thoát cổng, auto-fit camera | Hoàn thiện |
| `Assets/Scripts/Gameplay/PlayerPieceController.cs` | Kéo thả đa hướng trực quan Drag & Snap, dynamic tilt | Hoàn thiện |
| `Assets/Scripts/Gameplay/PuzzlePiece.cs` | Visual khối, render mảnh con, nhãn số, animation EaseOutQuad | Hoàn thiện |
| `Assets/Scripts/Gameplay/ExitGateView.cs` | Render cổng thoát, đĩa badge tương phản số, tech hologram | Hoàn thiện |
| `Assets/Scripts/Editor/SceneSetupHelper.cs` | Menu items chuyển đổi level + AutoMcpStarter tự reconnect | Hoàn thiện |
| `Assets/Scripts/Utils/GemSpriteManager.cs` | Nạp sprite động từ Resources/Disk cho mọi level (hỗ trợ pattern matching map) | Hoàn thiện |

---

## 4. Hướng Dẫn Thao Tác Nhanh

1. **Chuyển đổi màn chơi trên Unity Editor**:
   - `Tools/Block Merge Puzzle/Load Level 1`: Nạp Level 1.
   - `Tools/Block Merge Puzzle/Load Level 2`: Nạp Level 2.
   - `Tools/Block Merge Puzzle/Load Level 3`: Nạp Level 3.
   - `Tools/Block Merge Puzzle/Load Level 4`: Nạp Level 4.
   - `Tools/Block Merge Puzzle/Bake Level to Scene (Idle Display)`: Cập nhật lại màn đang chọn.
2. **Kiểm tra tự động bằng Python qua FastMCP**:
   - Chạy các script test trong `C:\Users\OS\.claude\jobs\fb04934c\tmp\`:
     - `uv run --with mcp python test_play_mode_level3.py`: Kiểm tra runtime Play Mode Level 3.
     - `uv run --with mcp python verify_scenes_integrity.py`: Kiểm tra tính toàn vẹn và cô lập giữa các scene Level 1, 2, 3, 4.
     - `uv run --with mcp python capture_level3.py`: Render và chụp ảnh Game View Level 3.
     - `python solve_level4.py`: Thuật toán BFS kiểm chứng tính giải được 100% của Level 4.

---

## 5. Quy Trình Các Bước Chuẩn Để Tạo & Hoàn Thiện Các Level Sau (Bảo Vệ Tuyệt Đối Level 1, Level 2, Level 3 & Level 4)

Áp dụng đúng các bước chuẩn dưới đây để mở rộng cho Level 5..10+ mà **tuyệt đối không sửa đổi, không nướng đè và không làm ảnh hưởng đến Level 1, Level 2, Level 3 và Level 4 đã chốt**:

1. **Cô lập Scene & Prefab riêng biệt**:
   - Mỗi level tạo file scene riêng: `Assets/Scenes/Level{n}.unity` và board prefab riêng: `Assets/Prefabs/Levels/Level{n}_Board.prefab`.
   - **Tuyệt đối KHÔNG chạy các lệnh bake hàng loạt đè lên `Level1.unity`, `Level2.unity`, `Level3.unity` và `Level4.unity`**.
   - Dùng menu `Tools/Block Merge Puzzle/Load Level {n}`: Hệ thống tự động mở trực tiếp scene file nếu đã tồn tại, tránh ghi đè scene đang mở.
2. **Thiết kế dữ liệu bàn cờ (`LevelManager.cs` + `GameConfig.cs`)**:
   - Khai báo `LevelId` mới trong enum `LevelId` (ví dụ `Level5`), cấu trúc lưới/hình dạng map, danh sách cổng thoát (`ExitGateView`) và khối khởi đầu (`PuzzlePiece`).
   - Thiết lập thời gian cho màn trong `GameUI.GetLevelDurationSeconds()` (ví dụ Level 5 = `300f` hoặc `DefaultLevelTime`).
3. **Áp dụng Bộ Hợp Đồng UI Chuẩn từ Level 1, 2, 3 & 4**:
   - Sử dụng đúng hierarchy UI chuẩn đã chốt:
     - `HeaderPanel`: pos `(0, -50)` với `Btn_Replay` pos `(-378, 5.5)`, `Banner_Time` pos `(16, -80)`, `Btn_Pause` pos `(374, 0)`, `Banner_Level` pos `(21, 32)` hiển thị `LEVEL {n}`.
     - `BoosterBar`: pos `(0, 60)` với 3 nút Freeze `(-318, 101)`, Magnet `(0, 104.76)`, Wand `(311, 93.38)`.
     - `InstructionBanner`: pos `(-24.76, 375.68)` (tự động bật nếu có hướng dẫn hoặc tắt ở các level nâng cao).
   - Khi chỉnh sửa scene mới, chỉ lưu file scene đó (`File -> Save`), không gọi `RebuildUI()`.
4. **Background & Camera Fit (`LevelManager.cs`)**:
   - Giữ màu tint dịu `new Color(0.42f, 0.42f, 0.48f, 1f)` cho `BackgroundSprite` để bàn cờ và khối đá nổi bật.
   - Giữ `FitCameraToBoard()` với `Mathf.Max(sizeByWidth, sizeByHeight)` và ép ratio `0.45` khi Edit Mode để không bị crop 2 bên.
5. **Cơ chế Replay & Chuyển Level**:
   - Thêm scene mới vào `EditorBuildSettings` để `GameUI.RestartLevel()` gọi `SceneManager.LoadScene(activeScene.buildIndex)`.
   - Chuyển level bằng cách mở scene tương ứng, đảm bảo mỗi level hoàn toàn độc lập và khép kín.
6. **Âm thanh (2D & Preload)**:
   - Mọi file audio mới phải có `.meta` thiết lập `preloadAudioData: 1` và `3D: 0`. Phát toàn bộ SFX qua `SoundManager.Instance`.
7. **Lưu Scene & Kiểm Thử**:
   - Kiểm tra Play Mode, chụp ảnh nghiệm thu Game View và commit lên git theo branch riêng cho level đó.
   - Chuyển level bằng cách mở scene tương ứng, đảm bảo mỗi level hoàn toàn độc lập và khép kín.
6. **Âm thanh (2D & Preload)**:
   - Mọi file audio mới phải có `.meta` thiết lập `preloadAudioData: 1` và `3D: 0`. Phát toàn bộ SFX qua `SoundManager.Instance`.
7. **Lưu Scene & Kiểm Thử**:
   - Kiểm tra Play Mode, chụp ảnh nghiệm thu Game View và commit lên git theo branch riêng cho level đó.
