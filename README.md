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

3. **Thoát cổng theo màu và điều kiện (Gate Escape)**:
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

### 2. Level 2: Reference Board (Bàn cờ 6×8)
- **Bản đồ**: Khung đá kín 6 cột × 8 hàng.
- **Cổng thoát (7 cổng)**:
  - Cổng trên: Xanh dương [2], Xanh lá [3].
  - Cổng phải: Đỏ [2], Hồng [3].
  - Cổng dưới: Cam [2].
  - Cổng trái: Tím [4], Vàng [2].
- **Khối khởi đầu**: 21 khối đa hình dạng (1x1, 1x2 dọc, 2x1 ngang, chữ L) với nhiều cấp độ màu sắc.
- **Asset riêng**: Scene `Assets/Scenes/Level2.unity` và Prefab `Assets/Prefabs/Levels/Level2_Board.prefab`.

---

## 📱 Tối Ưu Màn Hình Dọc 1440×3200 (Portrait 0.45)

- Tự động tính toán khung nhìn camera theo cả chiều rộng và chiều cao:
  ```csharp
  float sizeByWidth = (CurrentFrameWidth / 2f + paddingX) / screenRatio;
  float sizeByHeight = CurrentFrameHeight / 2f + paddingY;
  cam.orthographicSize = Mathf.Max(sizeByWidth, sizeByHeight);
  ```
- Bộ lọc `screenRatio` bảo vệ trong Edit Mode: Luôn ép tỷ lệ 9:20 (0.45) để bàn cờ không bao giờ bị cắt 2 bên viền khi mở trên màn hình máy tính landscape.

---

## 🚀 Hướng Dẫn Sử Dụng

### 1. Mở dự án trong Unity
- Mở **Unity Hub** -> chọn **Add project from disk**.
- Trỏ tới thư mục `D:\BlockMergePuzzle`.
- Mở scene chính: `Assets/Scenes/MainScene.unity`.

### 2. Chuyển đổi màn chơi trên Unity Editor
Thanh menu trên cùng hỗ trợ thao tác nhanh:
- `Tools/Block Merge Puzzle/Load Level 1 (Tutorial Cross)`: Nạp và bake Level 1 vào scene.
- `Tools/Block Merge Puzzle/Load Level 2 (Reference 6x8)`: Nạp và bake Level 2 vào scene.
- `Tools/Block Merge Puzzle/Bake Level to Scene (Idle Display)`: Làm mới và lưu trạng thái bàn cờ hiện tại.

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
│   │       └── Level2_Board.prefab          # Prefab nguyên khối Level 2
│   ├── Resources\
│   │   └── Sprites\LevelAssets\             # Sprites nạp động (map_lv1, gems, frame)
│   ├── Scenes\
│   │   ├── MainScene.unity                  # Scene mặc định đang hoạt động
│   │   ├── Level1.unity                     # Scene riêng cho Level 1
│   │   └── Level2.unity                     # Scene riêng cho Level 2
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
