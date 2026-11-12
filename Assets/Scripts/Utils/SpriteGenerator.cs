using UnityEngine;

namespace BlockMergePuzzle.Utils
{
    public static class SpriteGenerator
    {
        private static Sprite _cachedBlockSprite;
        private static Sprite _cachedSlotSprite;
        private static Sprite _cachedCircleSprite;
        private static Sprite _cachedBannerSprite;
        private static Sprite _cachedRadialGlowSprite;
        private static Sprite _cachedStarFlareSprite;
        private static Sprite _cachedShardSprite;
        private static Sprite _cachedSettingRowSprite;
        private static Sprite _cachedLevelBannerSprite;

        public static Sprite GetLevelBannerSprite()
        {
            if (_cachedLevelBannerSprite != null) return _cachedLevelBannerSprite;
            _cachedLevelBannerSprite = CreateLevelBannerSprite(320, 72);
            return _cachedLevelBannerSprite;
        }

        public static Sprite GetSettingRowSprite()
        {
            if (_cachedSettingRowSprite != null) return _cachedSettingRowSprite;
            _cachedSettingRowSprite = CreateSettingRowSprite(640, 128, 28);
            return _cachedSettingRowSprite;
        }

        public static Sprite GetShardSprite()
        {
            if (_cachedShardSprite != null) return _cachedShardSprite;
            _cachedShardSprite = CreateSquareShardSprite(16);
            return _cachedShardSprite;
        }

        public static Sprite GetRadialGlowSprite()
        {
            if (_cachedRadialGlowSprite != null) return _cachedRadialGlowSprite;
            _cachedRadialGlowSprite = CreateRadialGlowSprite(128);
            return _cachedRadialGlowSprite;
        }

        public static Sprite GetStarFlareSprite()
        {
            if (_cachedStarFlareSprite != null) return _cachedStarFlareSprite;
            _cachedStarFlareSprite = CreateStarFlareSprite(128);
            return _cachedStarFlareSprite;
        }

        public static Sprite GetBannerSprite()
        {
            if (_cachedBannerSprite != null) return _cachedBannerSprite;
            _cachedBannerSprite = CreateRoundedRectSprite(256, 64, 28, Color.white);
            return _cachedBannerSprite;
        }

        public static Sprite GetBlockSprite()
        {
            if (_cachedBlockSprite != null) return _cachedBlockSprite;
            _cachedBlockSprite = CreateRoundedRectSprite(128, 128, 24, Color.white);
            return _cachedBlockSprite;
        }

        public static Sprite GetSlotSprite()
        {
            if (_cachedSlotSprite != null) return _cachedSlotSprite;
            _cachedSlotSprite = CreateRoundedSlotSocketSprite(128, 128, 24);
            return _cachedSlotSprite;
        }

        public static Sprite GetCircleSprite()
        {
            if (_cachedCircleSprite != null) return _cachedCircleSprite;
            _cachedCircleSprite = CreateCircleSprite(64, Color.white);
            return _cachedCircleSprite;
        }

        private static Sprite CreateRoundedSlotSocketSprite(int width, int height, int radius)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];
            float r = radius;
            int borderWidth = 4;

            Color socketColor = new Color(0.12f, 0.14f, 0.19f, 0.85f);
            Color rimColor = new Color(0.24f, 0.28f, 0.38f, 0.7f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dx = 0f;
                    float dy = 0f;

                    if (x < r) dx = r - x;
                    else if (x > width - r - 1) dx = x - (width - r - 1);

                    if (y < r) dy = r - y;
                    else if (y > height - r - 1) dy = y - (height - r - 1);

                    float distSq = dx * dx + dy * dy;
                    if (distSq <= r * r)
                    {
                        float dist = Mathf.Sqrt(distSq);
                        float alpha = Mathf.Clamp01(r + 0.5f - dist);

                        // Check if near border edge
                        bool isBorder = (x < borderWidth || x >= width - borderWidth ||
                                         y < borderWidth || y >= height - borderWidth ||
                                         dist >= r - borderWidth);

                        Color fill = isBorder ? rimColor : socketColor;
                        pixels[y * width + x] = new Color(fill.r, fill.g, fill.b, fill.a * alpha);
                    }
                    else if (dx > 0 && dy > 0)
                    {
                        pixels[y * width + x] = Color.clear;
                    }
                    else
                    {
                        bool isBorder = (x < borderWidth || x >= width - borderWidth ||
                                         y < borderWidth || y >= height - borderWidth);
                        Color fill = isBorder ? rimColor : socketColor;
                        pixels[y * width + x] = fill;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateRoundedRectSprite(int width, int height, int radius, Color fillColor)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];
            float r = radius;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dx = 0f;
                    float dy = 0f;

                    if (x < r) dx = r - x;
                    else if (x > width - r - 1) dx = x - (width - r - 1);

                    if (y < r) dy = r - y;
                    else if (y > height - r - 1) dy = y - (height - r - 1);

                    float distSq = dx * dx + dy * dy;
                    if (distSq <= r * r)
                    {
                        float dist = Mathf.Sqrt(distSq);
                        float alpha = Mathf.Clamp01(r + 0.5f - dist);
                        pixels[y * width + x] = new Color(fillColor.r, fillColor.g, fillColor.b, fillColor.a * alpha);
                    }
                    else if (dx > 0 && dy > 0)
                    {
                        pixels[y * width + x] = Color.clear;
                    }
                    else
                    {
                        pixels[y * width + x] = fillColor;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateCircleSprite(int size, Color color)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];
            float r = size * 0.5f;
            Vector2 center = new Vector2(r - 0.5f, r - 0.5f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist <= r)
                    {
                        float alpha = Mathf.Clamp01(r + 0.5f - dist);
                        pixels[y * size + x] = new Color(color.r, color.g, color.b, color.a * alpha);
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateRadialGlowSprite(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            Color[] pixels = new Color[size * size];
            float r = size * 0.5f;
            Vector2 center = new Vector2(r - 0.5f, r - 0.5f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    float t = Mathf.Clamp01(dist / r);
                    float alpha = Mathf.Pow(1f - t, 2.2f);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateStarFlareSprite(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            Color[] pixels = new Color[size * size];
            float r = size * 0.5f;
            Vector2 center = new Vector2(r - 0.5f, r - 0.5f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = Mathf.Abs(x - center.x) / r;
                    float ny = Mathf.Abs(y - center.y) / r;
                    float distN = Mathf.Sqrt(nx * nx + ny * ny);
                    float core = Mathf.Pow(Mathf.Clamp01(1f - distN * 2.2f), 2f);
                    float rayH = Mathf.Clamp01(1f - nx) * Mathf.Pow(Mathf.Clamp01(1f - ny * 6.5f), 2f);
                    float rayV = Mathf.Clamp01(1f - ny) * Mathf.Pow(Mathf.Clamp01(1f - nx * 6.5f), 2f);
                    float alpha = Mathf.Clamp01(core + (rayH + rayV) * 0.85f);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateSquareShardSprite(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            Color[] pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isBorder = (x == 0 || x == size - 1 || y == 0 || y == size - 1);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, isBorder ? 0.65f : 1f);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private static Sprite CreateSettingRowSprite(int width, int height, int radius)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];
            float r = radius;
            float borderThickness = 3.2f;

            Color fillTop = new Color(0.32f, 0.24f, 0.56f, 0.94f);
            Color fillBot = new Color(0.27f, 0.20f, 0.49f, 0.96f);
            Color borderColor = new Color(0.49f, 0.38f, 0.78f, 0.92f);

            for (int y = 0; y < height; y++)
            {
                float vt = (float)y / (height - 1);
                Color fill = Color.Lerp(fillBot, fillTop, vt);

                for (int x = 0; x < width; x++)
                {
                    float dx = 0f;
                    float dy = 0f;

                    if (x < r) dx = r - x;
                    else if (x > width - r - 1) dx = x - (width - r - 1);

                    if (y < r) dy = r - y;
                    else if (y > height - r - 1) dy = y - (height - r - 1);

                    float distToEdge;
                    if (dx > 0f && dy > 0f)
                    {
                        distToEdge = r - Mathf.Sqrt(dx * dx + dy * dy);
                    }
                    else
                    {
                        float dLeft = x;
                        float dRight = (width - 1) - x;
                        float dBot = y;
                        float dTop = (height - 1) - y;
                        distToEdge = Mathf.Min(Mathf.Min(dLeft, dRight), Mathf.Min(dBot, dTop));
                    }

                    if (distToEdge < -0.5f)
                    {
                        pixels[y * width + x] = Color.clear;
                    }
                    else
                    {
                        float aa = Mathf.Clamp01(distToEdge + 0.5f);
                        float borderBlend = 1f - Mathf.Clamp01((distToEdge - (borderThickness - 1f)) / 1.5f);
                        Color c = Color.Lerp(fill, borderColor, borderBlend);
                        pixels[y * width + x] = new Color(c.r, c.g, c.b, c.a * aa);
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        }

        private static Sprite CreateLevelBannerSprite(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];
            float chamfer = 15f;
            float borderOut = 2.0f;
            float bevel = 5.5f;

            // Matching UI_Bn Time.png stone tablet palette
            Color outerDark = new Color(0.09f, 0.09f, 0.22f, 0.96f);
            Color rimTop = new Color(0.38f, 0.43f, 0.72f, 1.0f);
            Color rimBot = new Color(0.22f, 0.20f, 0.44f, 1.0f);
            Color bodyTop = new Color(0.19f, 0.42f, 0.52f, 0.98f); // Teal slate
            Color bodyBot = new Color(0.17f, 0.20f, 0.42f, 0.98f); // Deep indigo slate

            for (int y = 0; y < height; y++)
            {
                float vt = (float)y / (height - 1);
                Color bodyCol = Color.Lerp(bodyBot, bodyTop, vt);
                Color rimCol = Color.Lerp(rimBot, rimTop, vt);

                for (int x = 0; x < width; x++)
                {
                    float dx = Mathf.Min(x, (width - 1) - x);
                    float dy = Mathf.Min(y, (height - 1) - y);
                    // Chamfered octagonal stone silhouette
                    float diag = (dx + dy - chamfer) * 0.7071f;
                    float distToEdge = Mathf.Min(Mathf.Min(dx, dy), diag);

                    // Subtle carved stone notch on top-left & bottom-right matching UI_Bn Time
                    float notch1 = Mathf.Abs((x - width * 0.14f) + (height - 1 - y) * 0.6f);
                    float notch2 = Mathf.Abs((x - width * 0.86f) - y * 0.6f);
                    bool isNotch = (y > height - 9 && notch1 < 1.3f) || (y < 9 && notch2 < 1.3f);

                    if (distToEdge < -0.5f)
                    {
                        pixels[y * width + x] = Color.clear;
                    }
                    else
                    {
                        float aa = Mathf.Clamp01(distToEdge + 0.5f);
                        Color c;
                        if (distToEdge < borderOut)
                        {
                            c = outerDark;
                        }
                        else if (distToEdge < borderOut + bevel)
                        {
                            float tRim = Mathf.Clamp01((distToEdge - borderOut) / bevel);
                            c = Color.Lerp(outerDark, rimCol, 0.45f + 0.55f * tRim);
                        }
                        else
                        {
                            float innerT = Mathf.Clamp01((distToEdge - (borderOut + bevel)) / 2.0f);
                            c = Color.Lerp(rimCol, bodyCol, innerT);
                        }

                        if (isNotch && distToEdge >= borderOut)
                        {
                            c = Color.Lerp(c, outerDark, 0.55f);
                        }

                        pixels[y * width + x] = new Color(c.r, c.g, c.b, c.a * aa);
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
