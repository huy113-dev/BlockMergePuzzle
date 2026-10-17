# 💎 Block Merge Puzzle (Gemstone 2048) - Unity Project

Dự án game giải đố xếp khối kết hợp đá quý phong cách **Drop & Merge 2048** hoàn chỉnh với bộ đồ họa **Đá Quý / Pha Lê giác cắt (Gemstones)** tùy chỉnh trực tiếp từ bạn, sẵn sàng chạy ngay trên **Unity** tại thư mục `D:\BlockMergePuzzle`.

---

## 🎨 Bộ Đồ Họa Khối Đá Quý (Gemstone Assets)

Các khối trong game đã được thay thế hoàn toàn bằng 5 khối đá quý giác cắt đa giác sắc nét:

| Cấp Khối | Giá Trị Số | Loại Đá Quý | File Sprite | Màu Sắc |
|:---:|:---:|:---:|:---:|:---:|
| **Tier 1** | **2** | **Ngọc Lục Bảo (Emerald)** | `gem_green.png` | Xanh lá |
| **Tier 2** | **4** | **Ngọc Hoàng Ngọc (Topaz / Amber)** | `gem_yellow.png` | Vàng cam |
| **Tier 3** | **8** | **Ngọc Hồng Ngọc (Ruby)** | `gem_red.png` | Đỏ thẫm |
| **Tier 4** | **16** | **Thạch Anh Hồng (Rose Quartz)** | `gem_magenta.png` | Hồng tím |
| **Tier 5** | **32** | **Ngọc Tử Thạch (Amethyst / Tanzanite)** | `gem_purple.png` | Xanh tím than |
| **Tier 6+** | **64 ➔ 2048+** | **Pha Lê Ánh Kim Tối Thượng** | Các biến thể ánh kim lấp lánh (Cyan, Lam Sapphire, Kim Cương, Hoàng Kim) | Hiệu ứng hào quang |

> [!TIP]
> - Vị trí lưu trữ sprite: `Assets/Resources/Sprites/Blocks/` và `Assets/Sprites/Blocks/`.
> - Bạn có thể bật / tắt hiển thị số trên đá quý bằng biến `ShowNumbersOnBlocks` trong `Assets/Scripts/Core/GameConfig.cs`.

---

## 🚀 Hướng Dẫn Mở & Chạy Trên Unity

1. **Mở Unity Hub**:
   - Nhấn **Add** (hoặc chọn **Add project from disk**).
   - Trỏ tới thư mục: `D:\BlockMergePuzzle`.

2. **Khởi động dự án**:
   - Chọn mở bằng phiên bản Unity trên máy bạn (Unity 2021, 2022, 6...).
   - Đợi Unity import assets lần đầu.

3. **Chơi game**:
   - Mở scene: [MainScene.unity](file:///D:/BlockMergePuzzle/Assets/Scenes/MainScene.unity).
   - Nhấn nút **Play (▶)** để bắt đầu chơi ngay!

---

## 🕹️ Cơ Chế Gameplay Đá Quý

- **Thả ngọc (Aim & Drop)**: Rê chuột / chạm ngón tay để chọn cột. Viên ngọc xem trước (Ghost Gem) sẽ dao động nhẹ ở đầu cột báo vị trí thả.
- **Hợp nhất (Merge Gemstones)**: Khi 2 hoặc nhiều viên ngọc cùng loại nằm liền kề, chúng sẽ hút vào nhau và thăng cấp lên loại ngọc quý hiếm hơn với âm thanh chuông pha lê vui tai.
- **Dây chuyền Combo & Trọng lực**: Ngọc rơi xuống lấp đầy khoảng trống và tự động kích hoạt chuỗi hợp nhất liên hoàn (Combo x2, x3, x4...).
- **Lưu kỷ lục**: Điểm cao nhất được lưu tự động.

---

## 📁 Cấu Trúc Thư Mục

```
D:\BlockMergePuzzle\
├── Assets\
│   ├── Resources\
│   │   └── Sprites\
│   │       └── Blocks\
│   │           ├── gem_green.png       # Ngọc xanh lá (2)
│   │           ├── gem_yellow.png      # Ngọc vàng (4)
│   │           ├── gem_red.png         # Ngọc đỏ (8)
│   │           ├── gem_magenta.png     # Ngọc hồng (16)
│   │           └── gem_purple.png      # Ngọc tím (32)
│   ├── Scenes\
│   │   └── MainScene.unity             # Scene chính của game
│   └── Scripts\
│       ├── Audio\SoundManager.cs       # Bộ tổng hợp âm thanh chuông pha lê procedural
│       ├── Core\
│       │   ├── GameConfig.cs           # Cấu hình lưới, màu sắc, chế độ hiện số
│       │   ├── GameManager.cs          # Quản lý ván chơi, tính điểm, lưu điểm cao
│       │   ├── GridManager.cs          # Lưới ma trận, tìm khối hợp nhất, trọng lực rơi
│       │   └── GameBootstrapper.cs     # Tự động khởi tạo hệ thống khi ấn Play
│       ├── Gameplay\
│       │   ├── BlockView.cs            # Hiển thị ngọc giác cắt, đổ bóng chữ số, hiệu ứng nảy
│       │   └── PlayerController.cs     # Cảm ứng/chuột, ngắm & thả ngọc
│       ├── UI\GameUI.cs                # Giao diện điểm số, ô xem trước ngọc tiếp theo, combo, game over
│       └── Utils\
│           ├── GemSpriteManager.cs     # Trình quản lý nạp và gán sprite đá quý
│           └── SpriteGenerator.cs      # Tạo ô khay socket chứa ngọc
```
