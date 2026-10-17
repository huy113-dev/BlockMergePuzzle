using UnityEngine;

namespace BlockMergePuzzle.Utils
{
    public static class SpriteGenerator
    {
        private static Sprite _cachedBlockSprite;
        private static Sprite _cachedSlotSprite;
        private static Sprite _cachedCircleSprite;

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
    }
}
