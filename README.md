# 💎 Block Merge Puzzle - Unity Project

Dự án game giải đố trượt khối đá quý (**Sliding Block Merge & Escape Puzzle**) hoàn chỉnh, xây dựng trên **Unity** tại thư mục `D:\BlockMergePuzzle`.

---

## 🕹️ Cơ Chế Gameplay Chính

1. **Trượt đa hướng linh hoạt (Multi-Directional Drag & Snap)**:
   - Chạm và rê ngón tay/chuột để dẫn hướng khối đá quý lướt qua các hành lang và khoảng trống trên bàn cờ.
   - Cơ chế dịch chuyển tâm ô động (`_currentOrigin`) cho phép quẹo góc, rẽ hành lang trong một lần kéo duy nhất mà không bị khựng hay giật.
   - Hiệu ứng **Lift & Tilt**: Khối tự động nhấc nổi lên (`sortingOrder = 50`, scale `1.075x`), hơi nghiêng theo hướng kéo và phát âm thanh tương tác sinh động.

2. **Hợp nhất thẳng hàng (Collinear Straight-Line Merge)**:
   - Hai khối cùng **màu sắc**, cùng **hình dạng** và cùng **giá trị số** khi va chạm thẳng hàng trên cùng hàng/cột sẽ gộp thành một khối có giá trị tăng thêm 1 (`+50 điểm`).
   - Khóa va chạm lệch trục và chéo để đảm bảo tính giải đố chuẩn xác.

3. **Thoát cổng theo màu và điều kiện (Gate Escape & Cell Blocking)**:
   - **Khóa cổng vật lý (Gate Cell Blocking)**: Các khối **tuyệt đối không được dẫm lên hoặc dừng trên ô cổng** khi chưa đủ điều kiện. Cổng đóng vai trò như bức tường chắn chặn đứng chuyển động nếu khối sai màu hoặc chưa đủ số yêu cầu, đồng thời hiển thị thông báo lỗi cụ thể và phát âm thanh cảnh báo.
   - Mỗi cổng thoát yêu cầu một **màu sắc** và **giá trị số tối thiểu** nhất định.
   - Khi đưa khối đủ điều kiện vào đúng hướng cổng, khối sẽ căn giữa tâm cổng và bay ra ngoài (`+150 điểm`).
   - Giải phóng toàn bộ khối trên bàn cờ để kích hoạt trạng thái chiến thắng.

---

## 🗺️ Hệ Thống Màn Chơi (Multi-Level Architecture)

Game hỗ trợ chuyển đổi linh hoạt giữa các màn chơi cả trong **Edit Mode** (Bake hiển thị tĩnh) và **Runtime** (Play Mode):

### 1. Level 1: Tutorial Cross Map
- **Bản đồ**: Hình chữ thập 7 cột × 6 hàng từ asset viền đá `Map lv1.png`, giữ nền tím nguyên bản.
- **Tâm bàn cờ**: Giao điểm hành lang `(col 3, row 2)` đặt chuẩn xác tại gốc tọa độ `(0, 0, 0)`.
- **Cổng thoát (2 cổng)**:
  - Cổng trên (Top): **Xanh lá (Green)** yêu cầu số **2**.
  - Cổng trái (Left): **Đỏ (Red)** yêu cầu số **3**.
- **Khối khởi đầu (5 khối 1x1)**:
  - 1 khối Đỏ [2] tại tâm `(3, 2)`.
  - 2 khối Đỏ [1] tại hai đầu hành lang ngang `(0, 2)` và `(6, 2)`.
  - 2 khối Xanh lá [1] tại hai đầu hành lang dọc `(3, 0)` và `(3, 5)`.
- **Độ giải đố**: Đã kiểm chứng 100% tính giải được (Red 2 trượt mở đường -> Red 1 gộp thành Red 2 -> Red 2 gộp Red 2 thành Red 3 -> thoát cổng đỏ -> Green 1 gộp Green 2 -> thoát cổng xanh).
- **Asset riêng**: Scene `Assets/Scenes/Level1.unity` và Prefab `Assets/Prefabs/Levels/Level1_Board.prefab`.

### 2. Level 2: Cross Map (`map_lv2.png`) - [ĐÃ CHỐT HOÀN THIỆN]
- **Bản đồ**: Chữ thập lệch tâm 6 cột × 6 hàng từ asset khung đá `map_lv2.png` / `map_lv2 (3).png` (11 ô hành lang: cột 2 dọc [row 0..5] và hàng 3 ngang [col 0..5]).
- **Căn gốc tọa độ**: Giao điểm `(col 2, row 3)`, `GridOffset = (0.617f, 0.426f, 0f)`, `CellWidth = 1.16f`, `CellHeight = 1.03f`. Tích hợp bảng ánh xạ tọa độ pixel chuẩn `Level2CellPositions` giúp căn giữa hoàn hảo từng khối đá quý trong hành lang vẽ tay bất đối xứng (khắc phục hiện tượng khối lệch vạch chia ô).
- **Cổng thoát công nghệ cao (Tech Hologram - 2 cổng)**:
  - Cổng trên (Top): **Xanh lá (Green)** yêu cầu số **2**, hướng lên `Vector2.up`.
  - Cổng phải (Right): **Đỏ (Red)** yêu cầu số **1**, hướng sang phải `Vector2.right`.
- **Khối khởi đầu (5 khối 1x1 theo [Image #12])**:
  - 3 khối Đỏ: Đỏ [2] tại `(2, 1)`, Đỏ [1] tại `(2, 3)`, Đỏ [2] tại `(2, 5)`.
  - 2 khối Xanh lá: Xanh lá [1] tại `(0, 3)`, Xanh lá [1] tại `(4, 3)`.
- **Độ giải đố**: Đã kiểm chứng 100% tính giải được (Đỏ 1 trượt xuống mở giao điểm -> Xanh 1 trượt sang phải gộp thành Xanh 2 -> Đỏ 2 trượt qua cổng Đỏ 1 -> các khối Đỏ lần lượt thoát -> Xanh 2 thoát cổng Xanh 2).
- **Asset riêng**: Scene `Assets/Scenes/Level2.unity`, Prefab `Assets/Prefabs/Levels/Level2_Board.prefab` và ảnh nghiệm thu `Assets/Screenshots/level2_current_saved.png`.

### 3. Level 3: Branching Map (`Map_lv3.png`) - [ĐÃ CHỐT HOÀN THIỆN]
- **Bản đồ**: Cấu trúc phân nhánh 5 cột × 7 hàng từ asset viền đá `Map_lv3.png` (15 ô hợp lệ: hàng 0 có 3 ô [0..2], cột 2 dọc [hàng 0..6], nhánh phải hàng 3 có 3 ô [2..4], hàng đáy 6 có 5 ô [0..4]).
- **Căn gốc tọa độ**: Tích hợp từ điển pixel mapping chuẩn `Level3CellPositions` căn giữa tuyệt đối từng khối trong lòng các ô đá vẽ tay bất đối xứng.
- **Cổng thoát công nghệ cao (Tech Hologram - 2 cổng)**:
  - Cổng trái trên (Top-Left): **Xanh lá (Green)** yêu cầu số **4**, hướng sang trái `Vector2.left` tại `(col 0, row 0)`.
  - Cổng phải giữa (Middle-Right): **Đỏ (Red)** yêu cầu số **4**, hướng sang phải `Vector2.right` tại `(col 4, row 3)`.
- **Khối khởi đầu (8 khối 1x1 theo thiết kế [Image #17])**:
  - 4 khối Đỏ: Đỏ [1] tại `(0, 6)`, Đỏ [1] tại `(2, 4)`, Đỏ [2] tại `(2, 5)`, Đỏ [3] tại `(2, 6)` (gộp đủ thành Đỏ 4).
  - 4 khối Xanh lá: Xanh [1] tại `(3, 3)`, Xanh [1] tại `(3, 6)`, Xanh [2] tại `(1, 6)`, Xanh [3] tại `(4, 6)` (gộp đủ thành Xanh 4).
- **Độ giải đố**: Đã kiểm chứng 100% giải được qua các bước trượt, dồn hàng và gộp thẳng hàng thành Đỏ 4 và Xanh 4 để thoát cổng.
- **Asset riêng**: Scene `Assets/Scenes/Level3.unity`, Prefab `Assets/Prefabs/Levels/Level3_Board.prefab` và ảnh nghiệm thu `Assets/Screenshots/verify_level3.png`.

### 4. Cơ Chế Bảo Vệ Độc Lập Scene (Scene Protection Mechanism)
- **Nạp Scene trực tiếp qua `EditorSceneManager.OpenScene`**: `SceneSetupHelper.LoadLevel(LevelId level)` kiểm tra file `Assets/Scenes/{level}.unity`, nếu đã tồn tại thì mở trực tiếp scene đó, **tuyệt đối không chạy `BakeLevelToScene()` nướng đè** lên scene đang mở.
- **Cách ly tuyệt đối Level 1, Level 2 & Level 3**: Khi tác giả tạo và cấu hình các level sau (Level 4..10), hệ thống chỉ ghi vào scene/prefab của level mới, **bảo toàn 100% Level 1, Level 2 và Level 3** không bị thay đổi layout UI, vị trí khối hay thông số cổng.

---

## 📱 Tối Ưu Màn Hình Dọc 1440×3200 & Hệ Thống UI

- **Thanh Header UI (`HeaderPanel`)**:
  - **Nút Replay (`Btn_Replay`)**: Khối đá cổ bên trái, bấm để load lại đúng trạng thái đồ họa, bố cục bàn cờ và bộ đếm thời gian ban đầu của màn chơi (`SceneManager.LoadScene` + snapshot fallback).
  - **Banner Level (`Banner_Level`)**: Hiển thị cấp độ hiện tại (`LEVEL 1`, `LEVEL 2`, đồng bộ tông màu phía trên banner thời gian).
  - **Banner Time (`Banner_Time` + `TimeVal`)**: Hộp thời gian với icon đồng hồ và chữ số `05:00` (Level 1 & Level 2 = 300 giây / 5 phút), hiển thị sẵn cả ở **Edit Mode** lẫn đếm ngược **Play Mode**, chuyển đỏ cảnh báo khi còn dưới 20 giây.
  - **Nút Pause (`Btn_Pause`)**: Khối đá cổ bên phải, mở modal Settings/Pause.
- **Thanh Booster (`BoosterBar`)**:
  - 3 ô booster ở đáy màn hình (Freeze, Magnet, Wand), hiển thị mờ + ổ khóa khi chưa mở khóa (mở lần lượt ở Level 6, Level 12, Level 16).
- **Hướng dẫn trực quan (`TutorialHandGuide`)**:
  - Tay chỉ dẫn động và chữ hướng dẫn *Swipe block to merge* trên Level 1 -> tự động ẩn trên Level 2.
- **Hộp thoại Setting / Pause Panel (`PausePanel`)**:
  - Phủ mờ khung cảnh game phía sau (`#03050ECC`), giữ trọn vẹn bàn cờ.
  - Công tắc **Sound**, **Music**, **Vibration** tự động lưu vào `PlayerPrefs`, nút đóng `X (1)` mới, nút **No Ads**, **Return**, **Home**.
- **Âm thanh & Hiệu ứng (Audio & VFX)**:
  - **SoundManager (`DontDestroyOnLoad`)**: Âm thanh 2D (`preloadAudioData: 1`, `3D: 0`), phát âm thanh nhấc/đặt block (`block_move`), âm thanh merge tăng dần âm lượng theo combo tối đa 4 lần (timeout 1.5s), âm thanh biến mất khi qua cổng (`block_vanish`).
  - **VFX**: Số khắc chìm trên đá, hiệu ứng dập slam + flash khi merge, hiệu ứng tan biến dần qua cổng.
- **Background vũ trụ (`background_main scene .png`)**:
  - Tự động phủ kín màn hình với Aspect-Fill, làm dịu sắc độ (`Color(0.42, 0.42, 0.48, 1)`) để làm nổi bật bàn cờ và các khối đá quý.
- **Camera Auto-Fit**:
  - Tự động tính toán khung nhìn camera theo cả chiều rộng và chiều cao (`Mathf.Max(sizeByWidth, sizeByHeight)`), ép tỷ lệ 9:20 (`0.45`) trong Edit Mode để không bị cắt 2 bên viền.

---

## 🚀 Hướng Dẫn Sử Dụng & Quy Trình Làm Level Mới

### 1. Mở dự án trong Unity
- Mở **Unity Hub** -> chọn **Add project from disk** -> `D:\BlockMergePuzzle`.
- Mở scene chính: `Assets/Scenes/MainScene.unity`.

### 2. Công cụ Editor (`Tools/Block Merge Puzzle/...`)
- `Load Level 1` .. `Load Level 10`: Nạp nhanh bàn cờ từng level.
- `Bake Level to Scene (Idle Display)`: Bake bàn cờ hiện tại vào scene để hiển thị ở Edit Mode.
- `Ensure Time Text on Banner`: Tạo/khôi phục `TimeVal` (`05:00`) trên `Banner_Time` và lưu scene mà không làm lệch vị trí UI đã xếp.

### 3. Quy trình chuẩn khi tạo / chỉnh Level mới (Không ảnh hưởng Level 1 & Level 2)
1. **Định nghĩa map**: Thêm dữ liệu lưới, cổng thoát và khối khởi đầu trong `LevelManager.cs` + hằng số/thời lượng trong `GameConfig.cs` / `GameUI.GetLevelDurationSeconds()`.
2. **Cô lập Scene & Prefab**: Mỗi level mới tạo một scene riêng `Assets/Scenes/Level{n}.unity` và prefab `Assets/Prefabs/Levels/Level{n}_Board.prefab`. **Tuyệt đối không chạy nướng đè hay sửa đổi `Level1.unity`, `Level2.unity` và `Level3.unity`**.
3. **UI chuẩn đồng bộ**: Dùng nguyên vẹn hierarchy GameUI chuẩn đã chốt từ Level 1, 2 & 3 (`HeaderPanel` pos `(0, -50)` với `Btn_Replay` pos `(-378, 5.5)`, `Banner_Time` pos `(16, -80)`, `Btn_Pause` pos `(374, 0)`, `Banner_Level` pos `(21, 32)`; `InstructionBanner` pos `(-24.76, 375.68)`; `BoosterBar` pos `(0, 60)`). Khi chỉnh sửa, chỉ lưu scene riêng của level đó.
4. **Nền & Ánh sáng**: Giữ tint background tối dịu `(0.42, 0.42, 0.48, 1)` qua `LevelManager.BuildBackground()`/`FitBackgroundToCamera()` để khối nổi bật.
5. **Âm thanh**: Mọi file audio mới phải để `preloadAudioData: 1` và `3D: 0` trong `.meta`, phát qua `SoundManager.Instance`.
6. **Kiểm tra Replay & Chuyển Level**: Chạy Play Mode thử kéo, merge, thoát cổng và bấm `Btn_Replay`; kiểm tra nạp qua menu `Tools/Block Merge Puzzle/Load Level {n}` chỉ mở đúng file scene tương ứng mà không làm thay đổi các level đã chốt.

### 3. Chạy game
- Nhấn nút **Play (▶)** trên Editor để trải nghiệm trực tiếp.
- Điều khiển: Dùng chuột/cảm ứng kéo khối, hoặc dùng phím mũi tên `W/A/S/D` để dịch chuyển từng bước.

---

## 📁 Cấu Trúc Thư Mục Quan Trọng

```
D:\BlockMergePuzzle\
├── Assets\
│   ├── Prefabs\
│   │   └── Levels\
│   │       ├── Level1_Board.prefab          # Prefab nguyên khối Level 1
│   │       ├── Level2_Board.prefab          # Prefab nguyên khối Level 2
│   │       └── Level3_Board.prefab          # Prefab nguyên khối Level 3
│   ├── Resources\
│   │   └── Sprites\LevelAssets\             # Sprites nạp động (map_lv1, gems, frame)
│   ├── Scenes\
│   │   ├── MainScene.unity                  # Scene mặc định đang hoạt động
│   │   ├── Level1.unity                     # Scene riêng cho Level 1 (Đã chốt)
│   │   ├── Level2.unity                     # Scene riêng cho Level 2 (Đã chốt)
│   │   └── Level3.unity                     # Scene riêng cho Level 3 (Đã chốt)
│   ├── Screenshots\                         # Ảnh chụp nghiệm thu tiến độ
│   ├── Scripts\
│   │   ├── Audio\SoundManager.cs            # Âm thanh tương tác procedural
│   │   ├── Core\
│   │   │   ├── GameConfig.cs                # Hằng số, bảng màu và nhãn tiếng Việt
│   │   │   ├── LevelManager.cs              # Quản lý grid, luật gộp, thoát cổng, auto-fit
│   │   │   └── GameBootstrapper.cs          # Tự khởi tạo hệ thống khi Play
│   │   ├── Editor\
│   │   │   └── SceneSetupHelper.cs          # Công cụ menu Bake & MCP Auto-Starter
│   │   ├── Gameplay\
│   │   │   ├── ExitGateView.cs              # Quản lý cổng thoát, badge tương phản
│   │   │   ├── PlayerPieceController.cs     # Điều khiển kéo thả đa hướng, snap
│   │   │   └── PuzzlePiece.cs               # Hiển thị khối, nhấc khối, animation EaseOutQuad
│   │   ├── UI\GameUI.cs                     # Điểm số, floating text, victory popup
│   │   └── Utils\GemSpriteManager.cs        # Nạp sprite động từ Resources/Disk
│   └── Sprites\
│       ├── UI B1 (1)\Map lv1.png            # Asset bản đồ gốc Level 1
│       └── LevelAssets\                     # Các sprite khung viền và khối đá
├── PROJECT_HANDOVER.md                      # Báo cáo chi tiết bàn giao kỹ thuật
└── README.md                                # Tài liệu tổng quan dự án
```
