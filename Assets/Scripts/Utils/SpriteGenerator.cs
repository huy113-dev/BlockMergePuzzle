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
        private static Sprite _cachedIceOverlaySprite;
        private static Sprite _cachedVictoryCardSprite;
        private static Sprite _cachedVictoryRibbonSprite;
        private static Sprite _cachedVictoryNextBtnSprite;
        private static Sprite _cachedVictoryReplayBtnSprite;
        private static Sprite _cachedGoldenStarSprite;
        private static Sprite _cachedWinProgressTrackSprite;
        private static Sprite _cachedWinProgressFillSprite;
        private static Sprite _cachedWinProgressNodeSprite;
        private static Sprite _cachedWinSpeechBubbleSprite;
        private static Sprite _cachedWinSunburstSprite;
        private static Sprite _cachedWinShineSweepSprite;
        private static Sprite _cachedLoseCardFrameSprite;
        private static Sprite _cachedLoseHeaderBannerSprite;
        private static Sprite _cachedLoseOfferCardSprite;
        private static Sprite _cachedLoseBestOfferRibbonSprite;
        private static Sprite _cachedLoseGreenButtonSprite;
        private static Sprite _cachedLoseResourcePillSprite;
        private static Sprite _cachedLosePlusBadgeSprite;
        private static Sprite _cachedLoseStopwatchSprite;
        private static Sprite _cachedLoseHeartLeftSprite;
        private static Sprite _cachedLoseHeartRightSprite;
        private static Sprite _cachedLoseHeartSmallSprite;
        private static Sprite _cachedLoseReviveWingsSprite;
        private static Sprite _cachedLosePlanetRingedSprite;
        private static Sprite _cachedLosePlanetMiniSprite;
        private static Sprite _cachedHomeLadderBeamSprite;
        private static Sprite _cachedHomeFacebookIconSprite;
        private static Sprite _cachedHomeBadgePillSprite;
        private static Sprite _cachedRemoveAdsRewardBarSprite;
        private static Sprite _cachedRemoveAdsMiniBadgeSprite;
        private static Sprite _cachedRemoveAdsGoldButtonSprite;
        private static Sprite _cachedRemoveAdsShadowBlobSprite;
        private static Sprite _cachedAxisArrowHorizontalSprite;
        private static Sprite _cachedAxisArrowVerticalSprite;
        private static Sprite _cachedStoneObstacleSprite;
        private static Sprite _cachedFreezeScreenBorderSprite;
        private static Sprite _cachedSnowflakeSprite;
        private static Sprite _cachedFreezeIceShellSprite;
        private static Sprite _cachedTimerFrostFrameSprite;
        private static Sprite _cachedFreezeTooltipCardSprite;

        public static Sprite GetFreezeScreenBorderSprite()
        {
            if (_cachedFreezeScreenBorderSprite == null)
                _cachedFreezeScreenBorderSprite = CreateFreezeScreenBorderSprite(384, 682);
            return _cachedFreezeScreenBorderSprite;
        }

        public static Sprite GetSnowflakeSprite()
        {
            if (_cachedSnowflakeSprite == null)
                _cachedSnowflakeSprite = CreateSnowflakeSprite(96);
            return _cachedSnowflakeSprite;
        }

        public static Sprite GetFreezeIceShellSprite()
        {
            if (_cachedFreezeIceShellSprite == null)
                _cachedFreezeIceShellSprite = CreateFreezeIceShellSprite(256);
            return _cachedFreezeIceShellSprite;
        }

        public static Sprite GetTimerFrostFrameSprite()
        {
            if (_cachedTimerFrostFrameSprite == null)
                _cachedTimerFrostFrameSprite = CreateTimerFrostFrameSprite(360, 140);
            return _cachedTimerFrostFrameSprite;
        }

        public static Sprite GetFreezeTooltipCardSprite()
        {
            if (_cachedFreezeTooltipCardSprite == null)
                _cachedFreezeTooltipCardSprite = CreateFreezeTooltipCardSprite(640, 180, 30);
            return _cachedFreezeTooltipCardSprite;
        }

        public static Sprite GetAxisArrowSprite(bool horizontal)
        {
            if (horizontal)
            {
                if (_cachedAxisArrowHorizontalSprite == null)
                    _cachedAxisArrowHorizontalSprite = CreateAxisArrowSprite(128, 64, true);
                return _cachedAxisArrowHorizontalSprite;
            }
            if (_cachedAxisArrowVerticalSprite == null)
                _cachedAxisArrowVerticalSprite = CreateAxisArrowSprite(64, 128, false);
            return _cachedAxisArrowVerticalSprite;
        }

        public static Sprite GetStoneObstacleSprite()
        {
            if (_cachedStoneObstacleSprite == null)
                _cachedStoneObstacleSprite = CreateStoneObstacleSprite(160, 160);
            return _cachedStoneObstacleSprite;
        }

        public static Sprite GetRemoveAdsRewardBarSprite()
        {
            if (_cachedRemoveAdsRewardBarSprite != null) return _cachedRemoveAdsRewardBarSprite;
            _cachedRemoveAdsRewardBarSprite = CreateRemoveAdsRewardBarSprite(720, 186);
            return _cachedRemoveAdsRewardBarSprite;
        }

        public static Sprite GetRemoveAdsMiniBadgeSprite()
        {
            if (_cachedRemoveAdsMiniBadgeSprite != null) return _cachedRemoveAdsMiniBadgeSprite;
            _cachedRemoveAdsMiniBadgeSprite = CreateRemoveAdsMiniBadgeSprite(160);
            return _cachedRemoveAdsMiniBadgeSprite;
        }

        public static Sprite GetRemoveAdsGoldButtonSprite()
        {
            if (_cachedRemoveAdsGoldButtonSprite != null) return _cachedRemoveAdsGoldButtonSprite;
            _cachedRemoveAdsGoldButtonSprite = CreateRemoveAdsGoldButtonSprite(640, 180);
            return _cachedRemoveAdsGoldButtonSprite;
        }

        public static Sprite GetRemoveAdsShadowBlobSprite()
        {
            if (_cachedRemoveAdsShadowBlobSprite != null) return _cachedRemoveAdsShadowBlobSprite;
            _cachedRemoveAdsShadowBlobSprite = CreateRemoveAdsShadowBlobSprite(160, 64);
            return _cachedRemoveAdsShadowBlobSprite;
        }

        public static Sprite GetHomeLadderBeamSprite()
        {
            if (_cachedHomeLadderBeamSprite != null) return _cachedHomeLadderBeamSprite;
            _cachedHomeLadderBeamSprite = CreateHomeLadderBeamSprite(64, 512);
            return _cachedHomeLadderBeamSprite;
        }

        public static Sprite GetHomeFacebookIconSprite()
        {
            if (_cachedHomeFacebookIconSprite != null) return _cachedHomeFacebookIconSprite;
            _cachedHomeFacebookIconSprite = CreateHomeFacebookIconSprite(96);
            return _cachedHomeFacebookIconSprite;
        }

        public static Sprite GetHomeBadgePillSprite()
        {
            if (_cachedHomeBadgePillSprite != null) return _cachedHomeBadgePillSprite;
            _cachedHomeBadgePillSprite = CreateHomeBadgePillSprite(140, 46);
            return _cachedHomeBadgePillSprite;
        }

        public static Sprite GetLoseCardFrameSprite()
        {
            if (_cachedLoseCardFrameSprite != null) return _cachedLoseCardFrameSprite;
            _cachedLoseCardFrameSprite = CreateLoseCardFrameSprite(820, 690);
            return _cachedLoseCardFrameSprite;
        }

        public static Sprite GetLoseHeaderBannerSprite()
        {
            if (_cachedLoseHeaderBannerSprite != null) return _cachedLoseHeaderBannerSprite;
            _cachedLoseHeaderBannerSprite = CreateLoseHeaderBannerSprite(680, 128);
            return _cachedLoseHeaderBannerSprite;
        }

        public static Sprite GetLoseOfferCardSprite()
        {
            if (_cachedLoseOfferCardSprite != null) return _cachedLoseOfferCardSprite;
            _cachedLoseOfferCardSprite = CreateLoseOfferCardSprite(820, 310);
            return _cachedLoseOfferCardSprite;
        }

        public static Sprite GetLoseBestOfferRibbonSprite()
        {
            if (_cachedLoseBestOfferRibbonSprite != null) return _cachedLoseBestOfferRibbonSprite;
            _cachedLoseBestOfferRibbonSprite = CreateLoseBestOfferRibbonSprite(276, 62);
            return _cachedLoseBestOfferRibbonSprite;
        }

        public static Sprite GetLoseGreenButtonSprite()
        {
            if (_cachedLoseGreenButtonSprite != null) return _cachedLoseGreenButtonSprite;
            _cachedLoseGreenButtonSprite = CreateLoseGreenButtonSprite(360, 148, 36);
            return _cachedLoseGreenButtonSprite;
        }

        public static Sprite GetLoseResourcePillSprite()
        {
            if (_cachedLoseResourcePillSprite != null) return _cachedLoseResourcePillSprite;
            _cachedLoseResourcePillSprite = CreateLoseResourcePillSprite(330, 82);
            return _cachedLoseResourcePillSprite;
        }

        public static Sprite GetLosePlusBadgeSprite()
        {
            if (_cachedLosePlusBadgeSprite != null) return _cachedLosePlusBadgeSprite;
            _cachedLosePlusBadgeSprite = CreateLosePlusBadgeSprite(64);
            return _cachedLosePlusBadgeSprite;
        }

        public static Sprite GetLoseStopwatchSprite()
        {
            if (_cachedLoseStopwatchSprite != null) return _cachedLoseStopwatchSprite;
            _cachedLoseStopwatchSprite = CreateLoseStopwatchSprite(256);
            return _cachedLoseStopwatchSprite;
        }

        public static Sprite GetLoseHeartLeftSprite()
        {
            if (_cachedLoseHeartLeftSprite != null) return _cachedLoseHeartLeftSprite;
            _cachedLoseHeartLeftSprite = CreateLoseHeartHalfSprite(256, isLeft: true);
            return _cachedLoseHeartLeftSprite;
        }

        public static Sprite GetLoseHeartRightSprite()
        {
            if (_cachedLoseHeartRightSprite != null) return _cachedLoseHeartRightSprite;
            _cachedLoseHeartRightSprite = CreateLoseHeartHalfSprite(256, isLeft: false);
            return _cachedLoseHeartRightSprite;
        }

        public static Sprite GetLoseHeartSmallSprite()
        {
            if (_cachedLoseHeartSmallSprite != null) return _cachedLoseHeartSmallSprite;
            _cachedLoseHeartSmallSprite = CreateLoseHeartSmallSprite(96);
            return _cachedLoseHeartSmallSprite;
        }

        public static Sprite GetLoseReviveWingsSprite()
        {
            if (_cachedLoseReviveWingsSprite != null) return _cachedLoseReviveWingsSprite;
            _cachedLoseReviveWingsSprite = CreateLoseReviveWingsSprite(220, 180);
            return _cachedLoseReviveWingsSprite;
        }

        public static Sprite GetLosePlanetRingedSprite()
        {
            if (_cachedLosePlanetRingedSprite != null) return _cachedLosePlanetRingedSprite;
            _cachedLosePlanetRingedSprite = CreateLosePlanetRingedSprite(256, 200);
            return _cachedLosePlanetRingedSprite;
        }

        public static Sprite GetLosePlanetMiniSprite()
        {
            if (_cachedLosePlanetMiniSprite != null) return _cachedLosePlanetMiniSprite;
            _cachedLosePlanetMiniSprite = CreateLosePlanetMiniSprite(128);
            return _cachedLosePlanetMiniSprite;
        }

        public static Sprite GetWinSunburstSprite()
        {
            if (_cachedWinSunburstSprite != null) return _cachedWinSunburstSprite;
            _cachedWinSunburstSprite = CreateWinSunburstSprite(256);
            return _cachedWinSunburstSprite;
        }

        public static Sprite GetWinShineSweepSprite()
        {
            if (_cachedWinShineSweepSprite != null) return _cachedWinShineSweepSprite;
            _cachedWinShineSweepSprite = CreateWinShineSweepSprite(128, 128);
            return _cachedWinShineSweepSprite;
        }

        public static Sprite GetWinProgressTrackSprite()
        {
            if (_cachedWinProgressTrackSprite != null) return _cachedWinProgressTrackSprite;
            _cachedWinProgressTrackSprite = CreateWinProgressBarSprite(512, 44, isFill: false);
            return _cachedWinProgressTrackSprite;
        }

        public static Sprite GetWinProgressFillSprite()
        {
            if (_cachedWinProgressFillSprite != null) return _cachedWinProgressFillSprite;
            _cachedWinProgressFillSprite = CreateWinProgressBarSprite(512, 44, isFill: true);
            return _cachedWinProgressFillSprite;
        }

        public static Sprite GetWinProgressNodeSprite()
        {
            if (_cachedWinProgressNodeSprite != null) return _cachedWinProgressNodeSprite;
            _cachedWinProgressNodeSprite = CreateWinProgressNodeSprite(96);
            return _cachedWinProgressNodeSprite;
        }

        public static Sprite GetWinSpeechBubbleSprite()
        {
            if (_cachedWinSpeechBubbleSprite != null) return _cachedWinSpeechBubbleSprite;
            _cachedWinSpeechBubbleSprite = CreateWinSpeechBubbleSprite(128, 140);
            return _cachedWinSpeechBubbleSprite;
        }

        public static Sprite GetVictoryCardSprite()
        {
            if (_cachedVictoryCardSprite != null) return _cachedVictoryCardSprite;
            _cachedVictoryCardSprite = CreateVictoryCardSprite(640, 520, 42);
            return _cachedVictoryCardSprite;
        }

        public static Sprite GetVictoryRibbonSprite()
        {
            if (_cachedVictoryRibbonSprite != null) return _cachedVictoryRibbonSprite;
            _cachedVictoryRibbonSprite = CreateVictoryRibbonSprite(520, 112);
            return _cachedVictoryRibbonSprite;
        }

        public static Sprite GetVictoryNextButtonSprite()
        {
            if (_cachedVictoryNextBtnSprite != null) return _cachedVictoryNextBtnSprite;
            _cachedVictoryNextBtnSprite = CreateGlossyButtonSprite(
                520, 124, 34,
                new Color(0.27f, 0.94f, 0.52f, 1f),
                new Color(0.10f, 0.72f, 0.34f, 1f),
                new Color(0.05f, 0.45f, 0.21f, 1f),
                new Color(0.62f, 1.00f, 0.75f, 1f));
            return _cachedVictoryNextBtnSprite;
        }

        public static Sprite GetVictoryReplayButtonSprite()
        {
            if (_cachedVictoryReplayBtnSprite != null) return _cachedVictoryReplayBtnSprite;
            _cachedVictoryReplayBtnSprite = CreateGlossyButtonSprite(
                440, 104, 30,
                new Color(0.35f, 0.28f, 0.64f, 1f),
                new Color(0.22f, 0.16f, 0.44f, 1f),
                new Color(0.13f, 0.09f, 0.28f, 1f),
                new Color(0.58f, 0.50f, 0.92f, 1f));
            return _cachedVictoryReplayBtnSprite;
        }

        public static Sprite GetGoldenStarSprite()
        {
            if (_cachedGoldenStarSprite != null) return _cachedGoldenStarSprite;
            _cachedGoldenStarSprite = CreateGoldenStarSprite(128);
            return _cachedGoldenStarSprite;
        }

        public static Sprite GetIceOverlaySprite()
        {
            if (_cachedIceOverlaySprite != null) return _cachedIceOverlaySprite;
            _cachedIceOverlaySprite = CreateIceOverlaySprite(256, 256, 36);
            return _cachedIceOverlaySprite;
        }

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

        private static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float lenSq = ab.sqrMagnitude;
            if (lenSq < 0.0001f) return Vector2.Distance(p, a);
            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / lenSq);
            Vector2 proj = a + t * ab;
            return Vector2.Distance(p, proj);
        }

        private static Sprite CreateIceOverlaySprite(int width, int height, int radius)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];
            float r = radius;

            // Frosty periwinkle-blue ice palette matching reference image
            Color iceTop = new Color(0.51f, 0.66f, 0.96f, 0.96f);
            Color iceMid = new Color(0.42f, 0.56f, 0.90f, 0.96f);
            Color iceBot = new Color(0.33f, 0.47f, 0.83f, 0.97f);
            Color bevelLight = new Color(0.70f, 0.85f, 1.00f, 0.98f);
            Color bevelDark = new Color(0.24f, 0.37f, 0.74f, 0.98f);
            Color topRimWhite = new Color(0.88f, 0.98f, 1.00f, 0.99f);
            Color crackGlowCol = new Color(0.38f, 0.93f, 1.00f, 1.00f);
            Color crackCoreCol = new Color(0.90f, 0.99f, 1.00f, 1.00f);

            // Crack line segments in normalized UV (0..1)
            Vector2[,] segments = new Vector2[,]
            {
                // Main diagonal bottom-left to top-right
                { new Vector2(0.06f, 0.14f), new Vector2(0.27f, 0.20f) },
                { new Vector2(0.27f, 0.20f), new Vector2(0.54f, 0.49f) },
                { new Vector2(0.54f, 0.49f), new Vector2(0.93f, 0.88f) },
                // Top-left to bottom-right branch
                { new Vector2(0.09f, 0.91f), new Vector2(0.36f, 0.67f) },
                { new Vector2(0.36f, 0.67f), new Vector2(0.66f, 0.43f) },
                { new Vector2(0.66f, 0.43f), new Vector2(0.92f, 0.23f) },
                // Steep right fracture
                { new Vector2(0.91f, 0.86f), new Vector2(0.77f, 0.46f) },
                { new Vector2(0.77f, 0.46f), new Vector2(0.67f, 0.07f) },
                // Bottom-left star fracture branches
                { new Vector2(0.27f, 0.20f), new Vector2(0.18f, 0.05f) },
                { new Vector2(0.27f, 0.20f), new Vector2(0.45f, 0.14f) },
                { new Vector2(0.27f, 0.20f), new Vector2(0.16f, 0.42f) },
                // Upper-left frost sub-branch
                { new Vector2(0.36f, 0.67f), new Vector2(0.54f, 0.88f) },
                { new Vector2(0.16f, 0.42f), new Vector2(0.06f, 0.56f) }
            };
            int segCount = segments.GetLength(0);

            // Frost intersection nodes (bright icy hotspots)
            Vector2[] nodes = new Vector2[]
            {
                new Vector2(0.27f, 0.20f),
                new Vector2(0.10f, 0.89f),
                new Vector2(0.77f, 0.46f),
                new Vector2(0.90f, 0.85f)
            };

            for (int y = 0; y < height; y++)
            {
                float ny = (float)y / (height - 1);
                for (int x = 0; x < width; x++)
                {
                    float nx = (float)x / (width - 1);

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
                        float dL = x;
                        float dR = (width - 1) - x;
                        float dB = y;
                        float dT = (height - 1) - y;
                        distToEdge = Mathf.Min(Mathf.Min(dL, dR), Mathf.Min(dB, dT));
                    }

                    if (distToEdge < -0.5f)
                    {
                        pixels[y * width + x] = Color.clear;
                        continue;
                    }

                    float aa = Mathf.Clamp01(distToEdge + 0.5f);

                    // Base vertical gradient
                    Color col = ny > 0.5f
                        ? Color.Lerp(iceMid, iceTop, (ny - 0.5f) * 2f)
                        : Color.Lerp(iceBot, iceMid, ny * 2f);

                    // Gem-like facet bevels (top/left lighter, bottom/right darker)
                    float bevelW = 24f;
                    if (distToEdge < bevelW)
                    {
                        float bt = 1f - (distToEdge / bevelW);
                        float lightWeight = Mathf.Clamp01((ny - nx + 0.35f) * 0.85f);
                        Color facetCol = Color.Lerp(bevelDark, bevelLight, lightWeight);
                        col = Color.Lerp(col, facetCol, bt * 0.52f);
                    }

                    // Upper-left crystal corner facet sheen
                    float cornerDiag = (1f - nx) + ny; // max 2 at top-left
                    if (cornerDiag > 1.52f && distToEdge >= 10f && distToEdge <= 44f)
                    {
                        float sheen = Mathf.Clamp01((cornerDiag - 1.52f) / 0.25f) * 0.45f;
                        col = Color.Lerp(col, topRimWhite, sheen);
                    }

                    // Top specular ice rim bar
                    if (ny > 0.93f && nx > 0.14f && nx < 0.86f)
                    {
                        float topT = Mathf.Clamp01((ny - 0.93f) / 0.055f);
                        col = Color.Lerp(col, topRimWhite, topT * 0.78f);
                    }

                    // Frost loop subtle ring on middle-left (matching image detail)
                    Vector2 uv = new Vector2(nx, ny);
                    float loopDist = Mathf.Abs(Vector2.Distance(uv, new Vector2(0.16f, 0.56f)) - 0.14f);
                    if (loopDist < 0.035f)
                    {
                        float lt = 1f - (loopDist / 0.035f);
                        col = Color.Lerp(col, crackGlowCol, lt * lt * 0.38f);
                    }

                    // Glowing ice crack veins with organic curvature
                    Vector2 warpedUv = new Vector2(
                        uv.x + Mathf.Sin(uv.y * 11.5f + uv.x * 6.0f) * 0.018f,
                        uv.y + Mathf.Cos(uv.x * 10.5f - uv.y * 7.0f) * 0.018f);
                    float minSegDist = 10f;
                    for (int s = 0; s < segCount; s++)
                    {
                        float d = DistanceToSegment(warpedUv, segments[s, 0], segments[s, 1]);
                        if (d < minSegDist) minSegDist = d;
                    }

                    float crackD = Mathf.Max(0f, minSegDist);

                    if (crackD < 0.058f)
                    {
                        float glowT = 1f - (crackD / 0.058f);
                        col = Color.Lerp(col, crackGlowCol, glowT * glowT * 0.62f);
                    }
                    if (crackD < 0.016f)
                    {
                        float coreT = 1f - (crackD / 0.016f);
                        col = Color.Lerp(col, crackCoreCol, coreT * coreT * 0.88f);
                    }

                    // Frost hotspots at vein intersections
                    for (int n = 0; n < nodes.Length; n++)
                    {
                        float nd = Vector2.Distance(uv, nodes[n]);
                        float rad = (n == 0) ? 0.11f : 0.075f;
                        if (nd < rad)
                        {
                            float nt = 1f - (nd / rad);
                            col = Color.Lerp(col, crackCoreCol, nt * nt * 0.72f);
                        }
                    }

                    // Crisp outer ice rim
                    if (distToEdge < 3.0f)
                    {
                        float rimT = 1f - (distToEdge / 3.0f);
                        Color outerRim = Color.Lerp(bevelDark, bevelLight, ny);
                        col = Color.Lerp(col, outerRim, rimT * 0.65f);
                    }

                    pixels[y * width + x] = new Color(col.r, col.g, col.b, col.a * aa);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
        }

        private static float ComputeRoundedRectEdgeDist(int x, int y, int width, int height, float radius)
        {
            float dx = 0f;
            float dy = 0f;
            if (x < radius) dx = radius - x;
            else if (x > width - radius - 1) dx = x - (width - radius - 1);

            if (y < radius) dy = radius - y;
            else if (y > height - radius - 1) dy = y - (height - radius - 1);

            if (dx > 0f && dy > 0f)
            {
                return radius - Mathf.Sqrt(dx * dx + dy * dy);
            }

            float dLeft = x;
            float dRight = (width - 1) - x;
            float dBot = y;
            float dTop = (height - 1) - y;
            return Mathf.Min(Mathf.Min(dLeft, dRight), Mathf.Min(dBot, dTop));
        }

        private static Sprite CreateVictoryCardSprite(int width, int height, int radius)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];
            Color outlineCol = new Color(0.063f, 0.035f, 0.149f, 0.99f); // #100926
            Color rimTop = new Color(0.957f, 0.784f, 0.314f, 1.0f);     // #F4C850
            Color rimMid = new Color(0.478f, 0.361f, 0.878f, 1.0f);     // #7A5CE0
            Color rimBot = new Color(0.231f, 0.714f, 1.000f, 1.0f);     // #3BB6FF
            Color grooveCol = new Color(0.078f, 0.051f, 0.200f, 0.98f); // #140D33
            Color fillBot = new Color(0.118f, 0.086f, 0.271f, 0.98f);   // #1E1645
            Color fillTop = new Color(0.200f, 0.145f, 0.439f, 0.98f);   // #332570
            Color sheenCol = new Color(0.46f, 0.38f, 0.85f, 1.0f);
            Color warmSpotCol = new Color(0.50f, 0.33f, 0.65f, 1.0f);

            for (int y = 0; y < height; y++)
            {
                float ny = (float)y / (height - 1);
                Color outerBevel = ny >= 0.5f
                    ? Color.Lerp(rimMid, rimTop, (ny - 0.5f) * 2f)
                    : Color.Lerp(rimBot, rimMid, ny * 2f);
                Color bodyBase = Color.Lerp(fillBot, fillTop, ny);

                for (int x = 0; x < width; x++)
                {
                    float distToEdge = ComputeRoundedRectEdgeDist(x, y, width, height, radius);
                    if (distToEdge < -0.5f)
                    {
                        pixels[y * width + x] = Color.clear;
                        continue;
                    }

                    float aa = Mathf.Clamp01(distToEdge + 0.5f);
                    Color c;
                    if (distToEdge < 3.5f)
                    {
                        c = outlineCol;
                    }
                    else if (distToEdge < 11.5f)
                    {
                        float rimT = (distToEdge - 3.5f) / 8.0f;
                        c = Color.Lerp(outerBevel * 0.85f, outerBevel, Mathf.Sin(rimT * Mathf.PI));
                    }
                    else if (distToEdge < 15.0f)
                    {
                        float grooveT = (distToEdge - 11.5f) / 3.5f;
                        c = Color.Lerp(grooveCol, bodyBase, grooveT);
                    }
                    else
                    {
                        float nx = (float)x / (width - 1);
                        c = bodyBase;

                        // Subtle diagonal crystal sheen
                        float diag = Mathf.Abs((nx * 0.75f + (1f - ny) * 0.65f) - 0.62f);
                        if (diag < 0.16f)
                        {
                            float sT = 1f - (diag / 0.16f);
                            c = Color.Lerp(c, sheenCol, sT * sT * 0.16f);
                        }

                        // Soft warm glow behind stars upper area
                        float spotDist = Vector2.Distance(new Vector2(nx, ny), new Vector2(0.5f, 0.72f));
                        if (spotDist < 0.42f)
                        {
                            float wT = 1f - (spotDist / 0.42f);
                            c = Color.Lerp(c, warmSpotCol, wT * wT * 0.28f);
                        }
                    }

                    pixels[y * width + x] = new Color(c.r, c.g, c.b, c.a * aa);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(48f, 48f, 48f, 48f));
        }

        private static Sprite CreateVictoryRibbonSprite(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];
            float chamfer = 22f;
            Color borderCol = new Color(0.369f, 0.169f, 0.024f, 1.0f); // #5E2B06
            Color specCol = new Color(1.000f, 0.975f, 0.760f, 1.0f);
            Color goldTop = new Color(1.000f, 0.902f, 0.427f, 1.0f);   // #FFE66D
            Color goldBot = new Color(0.941f, 0.608f, 0.114f, 1.0f);   // #F09B1D

            for (int y = 0; y < height; y++)
            {
                float ny = (float)y / (height - 1);
                Color bodyCol = Color.Lerp(goldBot, goldTop, ny);

                for (int x = 0; x < width; x++)
                {
                    float dx = Mathf.Min(x, (width - 1) - x);
                    float dy = Mathf.Min(y, (height - 1) - y);
                    float diag = (dx + dy - chamfer) * 0.7071f;
                    float distToEdge = Mathf.Min(Mathf.Min(dx, dy), diag);

                    if (distToEdge < -0.5f)
                    {
                        pixels[y * width + x] = Color.clear;
                        continue;
                    }

                    float aa = Mathf.Clamp01(distToEdge + 0.5f);
                    Color c;
                    if (distToEdge < 4.5f)
                    {
                        c = borderCol;
                    }
                    else if (distToEdge < 9.0f)
                    {
                        float rT = (distToEdge - 4.5f) / 4.5f;
                        Color rimCol = ny > 0.45f ? specCol : Color.Lerp(borderCol, goldBot, 0.65f);
                        c = Color.Lerp(rimCol, bodyCol, rT * 0.6f);
                    }
                    else
                    {
                        c = bodyCol;
                        if (ny > 0.74f)
                        {
                            float hiT = (ny - 0.74f) / 0.26f;
                            c = Color.Lerp(c, specCol, hiT * 0.35f);
                        }
                    }

                    pixels[y * width + x] = new Color(c.r, c.g, c.b, c.a * aa);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(36f, 26f, 36f, 26f));
        }

        private static Sprite CreateGlossyButtonSprite(int width, int height, int radius, Color topCol, Color midCol, Color lipCol, Color rimCol)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];
            Color darkOutline = new Color(0.05f, 0.04f, 0.13f, 0.96f);
            float lipHeight = 14f;

            for (int y = 0; y < height; y++)
            {
                float ny = (float)y / (height - 1);
                Color bodyFill = Color.Lerp(midCol, topCol, Mathf.Clamp01((ny - 0.10f) / 0.85f));

                for (int x = 0; x < width; x++)
                {
                    float distToEdge = ComputeRoundedRectEdgeDist(x, y, width, height, radius);
                    if (distToEdge < -0.5f)
                    {
                        pixels[y * width + x] = Color.clear;
                        continue;
                    }

                    float aa = Mathf.Clamp01(distToEdge + 0.5f);
                    Color c;
                    if (distToEdge < 2.8f)
                    {
                        c = darkOutline;
                    }
                    else if (y < lipHeight)
                    {
                        float lipBlend = Mathf.Clamp01((y - (lipHeight - 3.5f)) / 3.5f);
                        c = Color.Lerp(lipCol, bodyFill, lipBlend);
                    }
                    else
                    {
                        c = bodyFill;
                        if (distToEdge < 6.0f && ny > 0.35f)
                        {
                            float edgeT = 1f - ((distToEdge - 2.8f) / 3.2f);
                            c = Color.Lerp(c, rimCol, edgeT * 0.75f);
                        }
                        if (ny > 0.72f)
                        {
                            float specT = (ny - 0.72f) / 0.28f;
                            c = Color.Lerp(c, rimCol, specT * 0.38f);
                        }
                    }

                    pixels[y * width + x] = new Color(c.r, c.g, c.b, c.a * aa);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(38f, 38f, 38f, 38f));
        }

        private static Sprite CreateGoldenStarSprite(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[size * size];
            float center = (size - 1) * 0.5f;
            float outerR = size * 0.422f; // ~54 at 128px
            float innerR = size * 0.180f; // ~23 at 128px
            float sectorAngle = Mathf.PI / 5f;
            float sinSector = Mathf.Sin(sectorAngle);
            float cosSector = Mathf.Cos(sectorAngle);

            Vector2[] verts = new Vector2[10];
            for (int i = 0; i < 10; i++)
            {
                float a = Mathf.PI * 0.5f + i * sectorAngle;
                float r = (i % 2 == 0) ? outerR : innerR;
                verts[i] = new Vector2(Mathf.Cos(a) * r, Mathf.Sin(a) * r);
            }

            Color centerGold = new Color(1.000f, 0.969f, 0.698f, 1.0f); // #FFF7B2
            Color midGold = new Color(1.000f, 0.788f, 0.161f, 1.0f);    // #FFC929
            Color deepGold = new Color(0.871f, 0.478f, 0.039f, 1.0f);   // #DE7A0A
            Color rimOutline = new Color(0.49f, 0.23f, 0.02f, 1.0f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 p = new Vector2(x - center, y - center);
                    float dist = p.magnitude;

                    float rawAngle = Mathf.Atan2(p.y, p.x) - Mathf.PI * 0.5f;
                    if (rawAngle < 0f) rawAngle += Mathf.PI * 2f;
                    int sectorIdx = Mathf.FloorToInt(rawAngle / sectorAngle) % 10;
                    float localAngle = rawAngle - sectorIdx * sectorAngle;
                    float phi = (sectorIdx % 2 == 0) ? localAngle : (sectorAngle - localAngle);

                    float denom = innerR * sinSector * Mathf.Cos(phi) + (outerR - innerR * cosSector) * Mathf.Sin(phi);
                    float boundR = denom > 0.0001f ? (outerR * innerR * sinSector) / denom : outerR;
                    bool inside = dist <= boundR;

                    float minEdgeDist = 1000f;
                    for (int i = 0; i < 10; i++)
                    {
                        float d = DistanceToSegment(p, verts[i], verts[(i + 1) % 10]);
                        if (d < minEdgeDist) minEdgeDist = d;
                    }

                    float signedDist = inside ? minEdgeDist : -minEdgeDist;
                    if (signedDist < -0.6f)
                    {
                        pixels[y * size + x] = Color.clear;
                        continue;
                    }

                    float aa = Mathf.Clamp01(signedDist + 0.6f);
                    float rNorm = Mathf.Clamp01(dist / outerR);
                    Color c = Color.Lerp(centerGold, midGold, Mathf.Clamp01(rNorm * 1.15f));

                    // Subtle 3D facet shading per alternating half-ray
                    if (sectorIdx % 2 == 0)
                    {
                        c = Color.Lerp(c, Color.white, 0.16f);
                    }
                    else
                    {
                        c = Color.Lerp(c, deepGold, 0.24f);
                    }

                    if (signedDist < 2.2f)
                    {
                        float outT = 1f - Mathf.Clamp01(signedDist / 2.2f);
                        c = Color.Lerp(c, rimOutline, outT * 0.85f);
                    }
                    else if (signedDist < 5.5f)
                    {
                        float bevT = 1f - ((signedDist - 2.2f) / 3.3f);
                        c = Color.Lerp(c, deepGold, bevT * 0.55f);
                    }

                    pixels[y * size + x] = new Color(c.r, c.g, c.b, c.a * aa);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateWinProgressBarSprite(int width, int height, bool isFill)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];
            float centerY = (height - 1) * 0.5f;
            float coreHalfH = 3.5f;
            float solidHalfH = 8.5f;
            float glowHalfH = (height - 1) * 0.5f;

            Color trackOuter = new Color(0.06f, 0.73f, 0.84f, 1f);  // #0FBBD6
            Color trackGlow = new Color(0.12f, 0.90f, 0.95f, 1f);   // #1EE5F2
            Color trackCore = new Color(0.71f, 1.00f, 1.00f, 1f);   // #B5FFFF

            Color fillLeft = new Color(0.61f, 0.27f, 0.97f, 1f);    // #9C45F8
            Color fillRight = new Color(1.00f, 0.32f, 0.85f, 1f);   // #FF52D9
            Color fillCore = new Color(1.00f, 0.89f, 1.00f, 1f);    // #FFE3FF

            float capR = solidHalfH;
            float rightCapCenterX = width - 1 - capR;

            for (int y = 0; y < height; y++)
            {
                float dy = Mathf.Abs(y - centerY);

                for (int x = 0; x < width; x++)
                {
                    float nx = (float)x / (width - 1);

                    // Left edge soft alpha fade on outer 8%
                    float edgeFade = Mathf.Clamp01(nx / 0.08f);
                    if (!isFill)
                    {
                        edgeFade = Mathf.Min(edgeFade, Mathf.Clamp01((1f - nx) / 0.08f));
                    }

                    // Effective distance from bar centerline (with rounded right cap on fill)
                    float distFromAxis = dy;
                    if (isFill && x > rightCapCenterX)
                    {
                        float dxCap = x - rightCapCenterX;
                        distFromAxis = Mathf.Sqrt(dxCap * dxCap + dy * dy);
                    }

                    if (distFromAxis >= glowHalfH)
                    {
                        pixels[y * width + x] = Color.clear;
                        continue;
                    }

                    Color baseCol = isFill
                        ? Color.Lerp(fillLeft, fillRight, nx)
                        : Color.Lerp(trackOuter, trackGlow, 0.5f);

                    Color col;
                    float alpha;
                    if (distFromAxis <= coreHalfH)
                    {
                        float coreT = 1f - (distFromAxis / coreHalfH);
                        col = Color.Lerp(baseCol, isFill ? fillCore : trackCore, 0.55f + 0.45f * coreT);
                        alpha = 1f;
                    }
                    else if (distFromAxis <= solidHalfH)
                    {
                        float bodyT = (distFromAxis - coreHalfH) / (solidHalfH - coreHalfH);
                        col = Color.Lerp(isFill ? fillCore : trackCore, baseCol, Mathf.Lerp(0.35f, 1f, bodyT));
                        alpha = Mathf.Lerp(1f, 0.90f, bodyT);
                    }
                    else
                    {
                        float glowT = 1f - ((distFromAxis - solidHalfH) / Mathf.Max(0.001f, glowHalfH - solidHalfH));
                        col = isFill ? baseCol : trackGlow;
                        alpha = Mathf.Pow(Mathf.Clamp01(glowT), 2.1f) * 0.65f;
                    }

                    pixels[y * width + x] = new Color(col.r, col.g, col.b, alpha * edgeFade);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateWinProgressNodeSprite(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[size * size];
            float center = (size - 1) * 0.5f;
            float outerR = size * 0.43f;
            float rimWidth = 3.8f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float signedDist = outerR - dist;

                    if (signedDist < -2.0f)
                    {
                        pixels[y * size + x] = Color.clear;
                        continue;
                    }

                    float aa = Mathf.Clamp01(signedDist + 0.6f);
                    float nx = dx / outerR;
                    float ny = dy / outerR;

                    // Subtle glossy spherical shading (upper-left brighter, lower-right deeper)
                    float gloss = Mathf.Clamp01(0.88f + (-nx * 0.14f + ny * 0.18f));
                    float rimT = (signedDist >= 0f && signedDist < rimWidth) ? (1f - signedDist / rimWidth) : 0f;
                    float shade = Mathf.Lerp(gloss, 1.0f, rimT * 0.85f);

                    // Small crisp upper-left specular dot
                    float specDist = Vector2.Distance(new Vector2(nx, ny), new Vector2(-0.34f, 0.36f));
                    float spec = specDist < 0.24f ? (1f - specDist / 0.24f) * 0.22f : 0f;
                    float finalLum = Mathf.Clamp01(shade + spec);

                    pixels[y * size + x] = new Color(finalLum, finalLum, finalLum, aa);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateWinSpeechBubbleSprite(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];
            Color cardTop = new Color(0.965f, 0.945f, 1.000f, 1f);    // #F6F1FF
            Color cardBot = new Color(0.875f, 0.831f, 0.973f, 1f);    // #DFD4F8
            Color borderCol = new Color(0.494f, 0.384f, 0.667f, 1f);  // #7E62AA

            // Card rect in bottom-up Unity Texture2D coordinates: x[8..119], y[26..131]
            int cardMinX = 8;
            int cardMaxX = width - 9;
            int cardMinY = 26;
            int cardMaxY = height - 9;
            int cardW = cardMaxX - cardMinX + 1;
            int cardH = cardMaxY - cardMinY + 1;
            float radius = 22f;

            // Downward triangle pointer at bottom center: base at y=28, tip at y=6
            float tipX = (width - 1) * 0.5f;
            float tipY = 6f;
            float tailBaseY = 29f;
            float tailHalfW = 15f;
            Vector2 pLeft = new Vector2(tipX - tailHalfW, tailBaseY);
            Vector2 pRight = new Vector2(tipX + tailHalfW, tailBaseY);
            Vector2 pTip = new Vector2(tipX, tipY);

            for (int y = 0; y < height; y++)
            {
                float ny = Mathf.Clamp01((float)(y - tipY) / Mathf.Max(1f, cardMaxY - tipY));
                Color fillCol = Color.Lerp(cardBot, cardTop, ny);

                for (int x = 0; x < width; x++)
                {
                    float dCard = ComputeRoundedRectEdgeDist(x - cardMinX, y - cardMinY, cardW, cardH, radius);

                    // Signed distance to triangle (pLeft, pRight, pTip)
                    float dTri = -100f;
                    if (y >= tipY - 3f && y <= tailBaseY + 2f)
                    {
                        float tY = Mathf.Clamp01((y - tipY) / (tailBaseY - tipY));
                        float halfSpan = tailHalfW * tY;
                        float dxTri = halfSpan - Mathf.Abs(x - tipX);
                        float dEdgeL = DistanceToSegment(new Vector2(x, y), pLeft, pTip);
                        float dEdgeR = DistanceToSegment(new Vector2(x, y), pRight, pTip);
                        float minSlopeDist = Mathf.Min(dEdgeL, dEdgeR);
                        bool insideTri = (y >= tipY && y <= tailBaseY && Mathf.Abs(x - tipX) <= halfSpan);
                        dTri = insideTri ? minSlopeDist : -minSlopeDist;
                    }

                    float unionDist = Mathf.Max(dCard, dTri);
                    if (unionDist < -0.75f)
                    {
                        pixels[y * width + x] = Color.clear;
                        continue;
                    }

                    float aa = Mathf.Clamp01(unionDist + 0.75f);
                    Color c = fillCol;

                    // Crisp lavender-purple border on outer perimeter
                    if (unionDist < 3.2f)
                    {
                        float borderT = 1f - Mathf.Clamp01((unionDist - 1.4f) / 1.8f);
                        c = Color.Lerp(fillCol, borderCol, borderT);
                    }

                    pixels[y * width + x] = new Color(c.r, c.g, c.b, aa);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateWinSunburstSprite(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[size * size];
            float center = (size - 1) * 0.5f;
            float maxR = size * 0.49f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float rNorm = Mathf.Clamp01(dist / maxR);
                    if (rNorm >= 1f)
                    {
                        pixels[y * size + x] = Color.clear;
                        continue;
                    }

                    float angle = Mathf.Atan2(dy, dx);
                    float rays = Mathf.Pow(0.5f + 0.5f * Mathf.Cos(angle * 12f), 2.4f);
                    float centerGlow = Mathf.Pow(Mathf.Clamp01(1f - rNorm * 1.15f), 2.0f);
                    float radialFalloff = Mathf.Pow(1f - rNorm, 1.55f);
                    float alpha = Mathf.Clamp01((rays * 0.72f + centerGlow * 0.55f) * radialFalloff);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateWinShineSweepSprite(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                float ny = (float)y / (height - 1);
                float vertFade = Mathf.Sin(Mathf.Clamp01(ny) * Mathf.PI);

                for (int x = 0; x < width; x++)
                {
                    float nx = (float)x / (width - 1);
                    float dist = Mathf.Abs((nx - 0.5f) - (ny - 0.5f) * 0.45f);
                    float band = Mathf.Clamp01(1f - dist / 0.24f);
                    float alpha = Mathf.Pow(band, 1.8f) * 0.65f * Mathf.Pow(vertFade, 0.4f);
                    pixels[y * width + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
        }

        private static float Hash01(int x, int y)
        {
            uint n = (uint)(x * 374761393 + y * 668265263);
            n = (n ^ (n >> 13)) * 1274126177u;
            return ((n ^ (n >> 16)) & 0xFFFFu) / 65535f;
        }

        private static float ComputeChamferedRectEdgeDist(int x, int y, int width, int height, float chamfer, float cornerRadius)
        {
            float dx = Mathf.Min(x, (width - 1) - x);
            float dy = Mathf.Min(y, (height - 1) - y);
            float straight = Mathf.Min(dx, dy);
            float diag = (dx + dy - chamfer) * 0.70710678f;
            float baseDist = Mathf.Min(straight, diag);

            // Smooth tiny vertex transitions slightly
            if (cornerRadius > 0.1f && dx < chamfer && dy < chamfer)
            {
                float dCorner = ComputeRoundedRectEdgeDist(x, y, width, height, cornerRadius);
                return Mathf.Max(baseDist, Mathf.Min(baseDist, dCorner));
            }
            return baseDist;
        }

        private static float EvaluateHeartSDF(float nx, float ny, float scaleX = 1.48f, float scaleY = 1.44f, float yOffset = 0.14f)
        {
            float px = (nx - 0.5f) * scaleX;
            float py = (ny - yOffset) * scaleY;
            float ax = Mathf.Abs(px);

            if (py + ax > 1.0f)
            {
                float dx = ax - 0.25f;
                float dy = py - 0.75f;
                return Mathf.Sqrt(dx * dx + dy * dy) - 0.35355339f;
            }

            float dx1 = ax;
            float dy1 = py - 1.0f;
            float d1Sq = dx1 * dx1 + dy1 * dy1;
            float m = 0.5f * Mathf.Max(ax + py, 0f);
            float dx2 = ax - m;
            float dy2 = py - m;
            float d2Sq = dx2 * dx2 + dy2 * dy2;
            return Mathf.Sqrt(Mathf.Min(d1Sq, d2Sq)) * Mathf.Sign(ax - py);
        }

        private static float EvaluateHeartCrackX(float ny)
        {
            // Deterministic jagged zig-zag crack curve down center x ~ 0.50
            if (ny > 0.66f)
            {
                float t = Mathf.Clamp01((0.86f - ny) / 0.20f);
                return Mathf.Lerp(0.500f, 0.455f, t);
            }
            if (ny > 0.50f)
            {
                float t = (0.66f - ny) / 0.16f;
                return Mathf.Lerp(0.455f, 0.548f, t);
            }
            if (ny > 0.34f)
            {
                float t = (0.50f - ny) / 0.16f;
                return Mathf.Lerp(0.548f, 0.462f, t);
            }
            if (ny > 0.18f)
            {
                float t = (0.34f - ny) / 0.16f;
                return Mathf.Lerp(0.462f, 0.528f, t);
            }
            float tailT = Mathf.Clamp01((0.18f - ny) / 0.14f);
            return Mathf.Lerp(0.528f, 0.500f, tailT);
        }

        private static Sprite CreateLoseCardFrameSprite(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];
            Color outlineCol = new Color(0.063f, 0.031f, 0.149f, 0.99f);      // #100826
            Color rimLight = new Color(0.737f, 0.549f, 1.000f, 1.0f);         // #BC8CFF
            Color rimMid = new Color(0.490f, 0.290f, 0.878f, 1.0f);           // #7D4AE0
            Color rimDark = new Color(0.282f, 0.145f, 0.608f, 1.0f);          // #48259B
            Color grooveCol = new Color(0.086f, 0.047f, 0.220f, 0.99f);       // #160C38
            Color bodyBot = new Color(0.102f, 0.063f, 0.251f, 0.99f);         // #1A1040
            Color bodyTop = new Color(0.180f, 0.114f, 0.408f, 0.99f);         // #2E1D68
            Color nebulaCol = new Color(0.55f, 0.28f, 0.86f, 1.0f);
            Color arcCol = new Color(0.65f, 0.48f, 0.98f, 1.0f);
            Color crystalGlow = new Color(0.84f, 0.42f, 1.00f, 1.0f);

            float chamfer = 56f;

            for (int y = 0; y < height; y++)
            {
                float ny = (float)y / (height - 1);
                Color bodyBase = Color.Lerp(bodyBot, bodyTop, ny);

                for (int x = 0; x < width; x++)
                {
                    float nx = (float)x / (width - 1);
                    float distToEdge = ComputeChamferedRectEdgeDist(x, y, width, height, chamfer, 26f);

                    if (distToEdge < -0.75f)
                    {
                        pixels[y * width + x] = Color.clear;
                        continue;
                    }

                    float aa = Mathf.Clamp01(distToEdge + 0.75f);
                    Color c;

                    if (distToEdge < 4.5f)
                    {
                        c = outlineCol;
                    }
                    else if (distToEdge < 26.0f)
                    {
                        // Faceted amethyst gemstone bevel matching Group 331.png
                        float rimT = (distToEdge - 4.5f) / 21.5f;
                        float facetWave = Mathf.Sin((nx * 14f + ny * 9f) * Mathf.PI) * 0.5f + 0.5f;
                        float dxEdge = Mathf.Min(x, (width - 1) - x);
                        float dyEdge = Mathf.Min(y, (height - 1) - y);
                        bool inCornerZone = (dxEdge < 95f && dyEdge < 95f);

                        Color bevelCol = ny > 0.52f
                            ? Color.Lerp(rimMid, rimLight, (ny - 0.52f) / 0.48f)
                            : Color.Lerp(rimDark, rimMid, ny / 0.52f);

                        if (inCornerZone)
                        {
                            float chisel = Mathf.Abs(((nx - 0.5f) * 1.1f) - ((ny - 0.5f) * 0.9f));
                            bevelCol = Color.Lerp(bevelCol, facetWave > 0.48f ? rimLight : rimDark, 0.36f + 0.15f * Mathf.Clamp01(chisel));
                        }
                        else
                        {
                            bevelCol = Color.Lerp(bevelCol, rimLight, Mathf.Sin(rimT * Mathf.PI) * 0.25f);
                        }

                        if (distToEdge > 21.5f)
                        {
                            float innerLipT = (distToEdge - 21.5f) / 4.5f;
                            bevelCol = Color.Lerp(bevelCol, grooveCol, innerLipT * 0.75f);
                        }
                        c = bevelCol;
                    }
                    else if (distToEdge < 31.0f)
                    {
                        float gt = (distToEdge - 26.0f) / 5.0f;
                        c = Color.Lerp(grooveCol, bodyBase, gt);
                    }
                    else
                    {
                        c = bodyBase;

                        // Soft cosmic nebula glow
                        float neb1 = Vector2.Distance(new Vector2(nx, ny), new Vector2(0.50f, 0.64f));
                        if (neb1 < 0.46f)
                        {
                            float nt = 1f - (neb1 / 0.46f);
                            c = Color.Lerp(c, nebulaCol, nt * nt * 0.24f);
                        }

                        // Subtle curved planetary orbit arcs inside frame
                        float orbDist1 = Mathf.Abs(Vector2.Distance(new Vector2(nx * 1.15f, ny), new Vector2(0.58f, 0.12f)) - 0.66f);
                        if (orbDist1 < 0.008f)
                        {
                            float ot = 1f - (orbDist1 / 0.008f);
                            c = Color.Lerp(c, arcCol, ot * 0.12f);
                        }
                        float orbDist2 = Mathf.Abs(Vector2.Distance(new Vector2(nx * 1.10f, ny), new Vector2(0.45f, 1.05f)) - 0.62f);
                        if (orbDist2 < 0.007f)
                        {
                            float ot2 = 1f - (orbDist2 / 0.007f);
                            c = Color.Lerp(c, arcCol, ot2 * 0.10f);
                        }

                        // Subtle twinkling cosmic micro-stars
                        int cellX = x / 28;
                        int cellY = y / 28;
                        float h = Hash01(cellX, cellY);
                        if (h > 0.78f)
                        {
                            float sx = cellX * 28f + 6f + Hash01(cellX + 17, cellY) * 16f;
                            float sy = cellY * 28f + 6f + Hash01(cellX, cellY + 31) * 16f;
                            float sd = Vector2.Distance(new Vector2(x, y), new Vector2(sx, sy));
                            float sr = h > 0.92f ? 3.2f : 2.1f;
                            if (sd < sr)
                            {
                                float st = 1f - (sd / sr);
                                Color starCol = h > 0.90f ? new Color(0.85f, 0.94f, 1.0f) : new Color(0.96f, 0.80f, 1.0f);
                                c = Color.Lerp(c, starCol, st * st * 0.42f);
                            }
                        }

                        // Subtle glowing amethyst crystal accent glow near bottom corners
                        float bLeft = Vector2.Distance(new Vector2(nx, ny), new Vector2(0.08f, 0.08f));
                        float bRight = Vector2.Distance(new Vector2(nx, ny), new Vector2(0.92f, 0.08f));
                        float minCorner = Mathf.Min(bLeft, bRight);
                        if (minCorner < 0.16f)
                        {
                            float ct = 1f - (minCorner / 0.16f);
                            c = Color.Lerp(c, crystalGlow, ct * ct * 0.20f);
                        }
                    }

                    pixels[y * width + x] = new Color(c.r, c.g, c.b, c.a * aa);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(68f, 68f, 68f, 68f));
        }

        private static Sprite CreateLoseHeaderBannerSprite(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];
            Color outlineCol = new Color(0.063f, 0.031f, 0.149f, 1.0f);   // #100826
            Color topRim = new Color(0.820f, 0.655f, 1.000f, 1.0f);       // #D1A7FF
            Color gradTop = new Color(0.549f, 0.337f, 0.918f, 1.0f);      // #8C56EA
            Color gradBot = new Color(0.349f, 0.157f, 0.722f, 1.0f);      // #5928B8
            Color botRim = new Color(0.220f, 0.090f, 0.490f, 1.0f);       // #38177D
            Color diamondGem = new Color(0.96f, 0.85f, 1.00f, 1.0f);

            float chamfer = 46f;

            for (int y = 0; y < height; y++)
            {
                float ny = (float)y / (height - 1);
                Color bodyBase = Color.Lerp(gradBot, gradTop, ny);

                for (int x = 0; x < width; x++)
                {
                    float nx = (float)x / (width - 1);
                    float dx = Mathf.Min(x, (width - 1) - x);
                    float dy = Mathf.Min(y, (height - 1) - y);
                    // Chamfered hexagonal gemstone tablet matching Group 331.png top tab
                    float diag = (dx * 0.75f + dy - chamfer) * 0.78f;
                    float distToEdge = Mathf.Min(Mathf.Min(dx, dy), diag);

                    if (distToEdge < -0.75f)
                    {
                        pixels[y * width + x] = Color.clear;
                        continue;
                    }

                    float aa = Mathf.Clamp01(distToEdge + 0.75f);
                    Color c;
                    if (distToEdge < 4.2f)
                    {
                        c = outlineCol;
                    }
                    else if (distToEdge < 11.5f)
                    {
                        float bevelT = (distToEdge - 4.2f) / 7.3f;
                        Color edgeTint = ny > 0.45f ? topRim : botRim;
                        c = Color.Lerp(edgeTint, bodyBase, bevelT * 0.65f);
                    }
                    else
                    {
                        c = bodyBase;
                        // Top glossy specular band
                        if (ny > 0.70f)
                        {
                            float specT = (ny - 0.70f) / 0.30f;
                            c = Color.Lerp(c, topRim, specT * 0.28f);
                        }
                        // Chiseled angled crystal sheen across banner
                        float sheen = Mathf.Abs((nx - 0.35f) * 0.9f - (ny - 0.5f) * 0.45f);
                        if (sheen < 0.10f)
                        {
                            float st = 1f - (sheen / 0.10f);
                            c = Color.Lerp(c, Color.white, st * 0.10f);
                        }

                        // Small faceted diamond gem accents on left and right sides
                        float dL = Mathf.Abs(nx - 0.085f) * 2.2f + Mathf.Abs(ny - 0.50f);
                        float dR = Mathf.Abs(nx - 0.915f) * 2.2f + Mathf.Abs(ny - 0.50f);
                        float dGem = Mathf.Min(dL, dR);
                        if (dGem < 0.12f)
                        {
                            float gt = 1f - (dGem / 0.12f);
                            c = Color.Lerp(c, diamondGem, gt * 0.62f);
                        }
                    }

                    pixels[y * width + x] = new Color(c.r, c.g, c.b, aa);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(84f, 28f, 84f, 28f));
        }

        private static Sprite CreateLoseOfferCardSprite(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];
            Color outlineCol = new Color(0.071f, 0.035f, 0.169f, 0.99f);   // #12092B
            Color rimTop = new Color(0.620f, 0.408f, 1.000f, 1.0f);       // #9E68FF
            Color rimBot = new Color(0.306f, 0.149f, 0.620f, 1.0f);       // #4E269E
            Color trayBot = new Color(0.149f, 0.094f, 0.353f, 0.99f);     // #26185A
            Color trayTop = new Color(0.235f, 0.145f, 0.533f, 0.99f);     // #3C2588
            Color footerBot = new Color(0.165f, 0.078f, 0.369f, 0.99f);   // #2A145E
            Color footerTop = new Color(0.290f, 0.145f, 0.588f, 0.99f);   // #4A2596
            Color dividerCol = new Color(0.68f, 0.52f, 1.00f, 1.0f);
            Color warmShowcase = new Color(0.58f, 0.38f, 0.95f, 1.0f);

            const float splitNy = 0.35f; // y=0 is bottom -> footer 0..0.35, showcase tray 0.35..1.0

            for (int y = 0; y < height; y++)
            {
                float ny = (float)y / (height - 1);
                Color rimCol = Color.Lerp(rimBot, rimTop, ny);

                for (int x = 0; x < width; x++)
                {
                    float nx = (float)x / (width - 1);
                    float distToEdge = ComputeChamferedRectEdgeDist(x, y, width, height, 34f, 24f);

                    if (distToEdge < -0.75f)
                    {
                        pixels[y * width + x] = Color.clear;
                        continue;
                    }

                    float aa = Mathf.Clamp01(distToEdge + 0.75f);
                    Color c;

                    if (distToEdge < 3.8f)
                    {
                        c = outlineCol;
                    }
                    else if (distToEdge < 13.0f)
                    {
                        float rimT = (distToEdge - 3.8f) / 9.2f;
                        c = Color.Lerp(rimCol, rimCol * 1.15f, Mathf.Sin(rimT * Mathf.PI) * 0.45f);
                    }
                    else if (ny >= splitNy)
                    {
                        float trayT = (ny - splitNy) / (1f - splitNy);
                        c = Color.Lerp(trayBot, trayTop, trayT);

                        // Warm cosmic center showcase glow behind coins & boosters
                        float spotD = Vector2.Distance(new Vector2(nx, (ny - splitNy) / (1f - splitNy)), new Vector2(0.50f, 0.52f));
                        if (spotD < 0.44f)
                        {
                            float st = 1f - (spotD / 0.44f);
                            c = Color.Lerp(c, warmShowcase, st * st * 0.34f);
                        }

                        // Tiny stars in showcase zone
                        int cellX = x / 34;
                        int cellY = y / 34;
                        float h = Hash01(cellX + 11, cellY + 7);
                        if (h > 0.80f)
                        {
                            float sx = cellX * 34f + 8f + Hash01(cellX + 5, cellY) * 18f;
                            float sy = cellY * 34f + 8f + Hash01(cellX, cellY + 19) * 18f;
                            float sd = Vector2.Distance(new Vector2(x, y), new Vector2(sx, sy));
                            if (sd < 2.2f)
                            {
                                c = Color.Lerp(c, new Color(0.94f, 0.86f, 1.0f), (1f - sd / 2.2f) * 0.45f);
                            }
                        }

                        // Bright luminous divider line at seam
                        float divD = Mathf.Abs(ny - splitNy);
                        if (divD < 0.014f)
                        {
                            float dt = 1f - (divD / 0.014f);
                            c = Color.Lerp(c, dividerCol, dt * 0.65f);
                        }
                    }
                    else
                    {
                        float footT = ny / splitNy;
                        c = Color.Lerp(footerBot, footerTop, footT);
                        float divD = Mathf.Abs(ny - splitNy);
                        if (divD < 0.014f)
                        {
                            float dt = 1f - (divD / 0.014f);
                            c = Color.Lerp(c, outlineCol, dt * 0.45f);
                        }
                    }

                    pixels[y * width + x] = new Color(c.r, c.g, c.b, c.a * aa);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(48f, 48f, 48f, 48f));
        }

        private static Sprite CreateLoseBestOfferRibbonSprite(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];
            Color outlineCol = new Color(0.220f, 0.024f, 0.212f, 1.0f);   // #380636
            Color leftMagenta = new Color(1.000f, 0.271f, 0.722f, 1.0f);  // #FF45B8
            Color rightPurple = new Color(0.631f, 0.133f, 0.902f, 1.0f);  // #A122E6
            Color topSpec = new Color(1.000f, 0.78f, 0.96f, 1.0f);

            float radius = 16f;
            float halfH = (height - 1) * 0.5f;

            for (int y = 0; y < height; y++)
            {
                float ny = (float)y / (height - 1);
                float dyMid = Mathf.Abs(y - halfH) / halfH; // 0 at vertical middle, 1 at top/bottom

                for (int x = 0; x < width; x++)
                {
                    float nx = (float)x / (width - 1);
                    // V-notch cut on the right tail
                    float rightEdgeX = (width - 1) - (1f - dyMid) * 20f;
                    float distRight = rightEdgeX - x;

                    float distLeft;
                    if (x < radius && y < radius)
                        distLeft = radius - Mathf.Sqrt((radius - x) * (radius - x) + (radius - y) * (radius - y));
                    else if (x < radius && y > height - 1 - radius)
                        distLeft = radius - Mathf.Sqrt((radius - x) * (radius - x) + (y - (height - 1 - radius)) * (y - (height - 1 - radius)));
                    else
                        distLeft = Mathf.Min(x, Mathf.Min(y, (height - 1) - y));

                    float distToEdge = Mathf.Min(distLeft, distRight);
                    if (distToEdge < -0.75f)
                    {
                        pixels[y * width + x] = Color.clear;
                        continue;
                    }

                    float aa = Mathf.Clamp01(distToEdge + 0.75f);
                    Color baseCol = Color.Lerp(leftMagenta, rightPurple, nx);
                    Color c;
                    if (distToEdge < 3.0f)
                    {
                        c = outlineCol;
                    }
                    else
                    {
                        c = baseCol;
                        if (ny > 0.68f)
                        {
                            c = Color.Lerp(c, topSpec, ((ny - 0.68f) / 0.32f) * 0.42f);
                        }
                        if (ny < 0.18f)
                        {
                            c = Color.Lerp(c, outlineCol, (1f - ny / 0.18f) * 0.38f);
                        }
                    }

                    pixels[y * width + x] = new Color(c.r, c.g, c.b, aa);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(22f, 16f, 36f, 16f));
        }

        private static Sprite CreateLoseGreenButtonSprite(int width, int height, int radius)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];
            Color darkOutline = new Color(0.086f, 0.043f, 0.212f, 0.99f); // #160B36
            Color topCol = new Color(0.239f, 0.949f, 0.478f, 1.0f);       // #3DF27A
            Color midCol = new Color(0.078f, 0.722f, 0.306f, 1.0f);       // #14B84E
            Color lipCol = new Color(0.039f, 0.471f, 0.204f, 1.0f);       // #0A7834
            Color rimCol = new Color(0.596f, 1.000f, 0.722f, 1.0f);       // #98FFB8
            float lipHeight = 16f;

            for (int y = 0; y < height; y++)
            {
                float ny = (float)y / (height - 1);
                Color bodyFill = Color.Lerp(midCol, topCol, Mathf.Clamp01((ny - 0.12f) / 0.85f));

                for (int x = 0; x < width; x++)
                {
                    float distToEdge = ComputeRoundedRectEdgeDist(x, y, width, height, radius);
                    if (distToEdge < -0.6f)
                    {
                        pixels[y * width + x] = Color.clear;
                        continue;
                    }

                    float aa = Mathf.Clamp01(distToEdge + 0.6f);
                    Color c;
                    if (distToEdge < 3.4f)
                    {
                        c = darkOutline;
                    }
                    else if (y < lipHeight)
                    {
                        float lipBlend = Mathf.Clamp01((y - (lipHeight - 4.0f)) / 4.0f);
                        c = Color.Lerp(lipCol, bodyFill, lipBlend);
                    }
                    else
                    {
                        c = bodyFill;
                        if (distToEdge < 7.5f && ny > 0.32f)
                        {
                            float edgeT = 1f - ((distToEdge - 3.4f) / 4.1f);
                            c = Color.Lerp(c, rimCol, edgeT * 0.72f);
                        }
                        if (ny > 0.74f)
                        {
                            float specT = (ny - 0.74f) / 0.26f;
                            c = Color.Lerp(c, rimCol, specT * 0.40f);
                        }
                    }

                    pixels[y * width + x] = new Color(c.r, c.g, c.b, c.a * aa);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(42f, 42f, 42f, 42f));
        }

        private static Sprite CreateLoseResourcePillSprite(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];
            float radius = (height - 2) * 0.5f;
            Color outerDark = new Color(0.043f, 0.020f, 0.125f, 0.96f);   // #0B0520
            Color rimGlow = new Color(0.553f, 0.361f, 0.941f, 1.0f);      // #8D5CF0
            Color fillTop = new Color(0.145f, 0.090f, 0.340f, 0.95f);
            Color fillBot = new Color(0.090f, 0.055f, 0.227f, 0.95f);     // #170E3A

            for (int y = 0; y < height; y++)
            {
                float ny = (float)y / (height - 1);
                Color fill = Color.Lerp(fillBot, fillTop, ny);

                for (int x = 0; x < width; x++)
                {
                    float distToEdge = ComputeRoundedRectEdgeDist(x, y, width, height, radius);
                    if (distToEdge < -0.6f)
                    {
                        pixels[y * width + x] = Color.clear;
                        continue;
                    }

                    float aa = Mathf.Clamp01(distToEdge + 0.6f);
                    Color c;
                    if (distToEdge < 2.2f)
                    {
                        c = outerDark;
                    }
                    else if (distToEdge < 6.0f)
                    {
                        float rimT = (distToEdge - 2.2f) / 3.8f;
                        c = Color.Lerp(rimGlow, fill, rimT * 0.35f);
                    }
                    else
                    {
                        c = fill;
                    }

                    pixels[y * width + x] = new Color(c.r, c.g, c.b, c.a * aa);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(38f, 26f, 38f, 26f));
        }

        private static Sprite CreateLosePlusBadgeSprite(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[size * size];
            float center = (size - 1) * 0.5f;
            float r = size * 0.45f;
            Color rimDark = new Color(0.05f, 0.24f, 0.14f, 1.0f);
            Color topCol = new Color(0.28f, 0.96f, 0.58f, 1.0f);
            Color botCol = new Color(0.08f, 0.68f, 0.34f, 1.0f);

            for (int y = 0; y < size; y++)
            {
                float ny = (float)y / (size - 1);
                Color body = Color.Lerp(botCol, topCol, ny);

                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float sd = r - dist;
                    if (sd < -0.6f)
                    {
                        pixels[y * size + x] = Color.clear;
                        continue;
                    }

                    float aa = Mathf.Clamp01(sd + 0.6f);
                    Color c = sd < 2.8f ? rimDark : body;

                    // Crisp white '+' icon
                    float adx = Mathf.Abs(dx);
                    float ady = Mathf.Abs(dy);
                    float armHalfLen = size * 0.22f;
                    float armHalfThick = size * 0.072f;
                    bool inPlus = (adx <= armHalfLen && ady <= armHalfThick) || (ady <= armHalfLen && adx <= armHalfThick);
                    if (inPlus)
                    {
                        c = Color.white;
                    }

                    pixels[y * size + x] = new Color(c.r, c.g, c.b, aa);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateLoseStopwatchSprite(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[size * size];
            float cx = size * 0.50f;
            float cy = size * 0.45f;
            float bodyR = size * 0.335f;
            float dialR = size * 0.255f;

            Color outlineCol = new Color(0.11f, 0.04f, 0.24f, 1.0f);
            Color bezelLight = new Color(0.76f, 0.54f, 1.00f, 1.0f);
            Color bezelDark = new Color(0.41f, 0.19f, 0.76f, 1.0f);
            Color goldLight = new Color(1.00f, 0.90f, 0.36f, 1.0f);
            Color goldDark = new Color(0.84f, 0.54f, 0.10f, 1.0f);
            Color dialCenter = new Color(0.97f, 0.96f, 1.00f, 1.0f);
            Color dialEdge = new Color(0.84f, 0.80f, 0.98f, 1.0f);
            Color urgencyCol = new Color(1.00f, 0.34f, 0.68f, 1.0f);
            Color handCol = new Color(0.17f, 0.07f, 0.36f, 1.0f);
            Color orbitCol = new Color(0.68f, 0.88f, 1.00f, 0.92f);

            // Top crown + ring & angled side pusher
            Vector2 crownPos = new Vector2(cx, cy + bodyR + size * 0.07f);
            Vector2 ringPos = new Vector2(cx, cy + bodyR + size * 0.135f);
            float sideAng = 42f * Mathf.Deg2Rad;
            Vector2 sidePusherPos = new Vector2(cx + Mathf.Sin(sideAng) * (bodyR + size * 0.055f),
                                                cy + Mathf.Cos(sideAng) * (bodyR + size * 0.055f));

            float orbitAngle = -24f * Mathf.Deg2Rad;
            float cosO = Mathf.Cos(orbitAngle);
            float sinO = Mathf.Sin(orbitAngle);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 p = new Vector2(x, y);
                    Vector2 rel = p - new Vector2(cx, cy);
                    float distBody = rel.magnitude;

                    // Tilted planetary orbit ring around stopwatch
                    float ox = rel.x * cosO - rel.y * sinO;
                    float oy = rel.x * sinO + rel.y * cosO;
                    float orbitR = Mathf.Sqrt((ox / (size * 0.455f)) * (ox / (size * 0.455f)) +
                                              (oy / (size * 0.145f)) * (oy / (size * 0.145f)));
                    float orbitRingDist = Mathf.Abs(orbitR - 1.0f) * size * 0.18f;
                    bool isFrontOrbit = (oy < 0f);
                    bool inOrbit = orbitRingDist < 4.5f;

                    // Top stem + ring
                    float dStem = Mathf.Max(Mathf.Abs(p.x - crownPos.x) - size * 0.052f, Mathf.Abs(p.y - crownPos.y) - size * 0.055f);
                    float dRingOuter = Vector2.Distance(p, ringPos) - size * 0.068f;
                    float dRingInner = size * 0.036f - Vector2.Distance(p, ringPos);
                    float dRing = Mathf.Max(dRingOuter, dRingInner);
                    float dSide = Vector2.Distance(p, sidePusherPos) - size * 0.044f;

                    float dHardware = Mathf.Min(dStem, Mathf.Min(dRing, dSide));
                    float dWatch = distBody - bodyR;

                    Color col = Color.clear;

                    // Back orbit ring first
                    if (inOrbit && !isFrontOrbit)
                    {
                        float oa = Mathf.Clamp01(1f - orbitRingDist / 4.5f) * 0.78f;
                        col = new Color(orbitCol.r, orbitCol.g, orbitCol.b, oa);
                    }

                    // Crown / side pusher hardware behind main body
                    if (dHardware < 0.75f && dWatch > 0f)
                    {
                        float haa = Mathf.Clamp01(0.75f - dHardware);
                        Color hCol = dHardware > -3.2f ? outlineCol : Color.Lerp(goldDark, goldLight, Mathf.Clamp01((p.y - cy) / (size * 0.5f)));
                        col = new Color(hCol.r, hCol.g, hCol.b, haa);
                    }

                    // Main watch body
                    if (dWatch < 0.75f)
                    {
                        float waa = Mathf.Clamp01(0.75f - dWatch);
                        Color wCol;
                        if (distBody > bodyR - 3.6f)
                        {
                            wCol = outlineCol;
                        }
                        else if (distBody > dialR + 5.5f)
                        {
                            float bt = Mathf.Clamp01((rel.y - rel.x * 0.45f) / (bodyR * 1.3f) + 0.5f);
                            wCol = Color.Lerp(bezelDark, bezelLight, bt);
                        }
                        else if (distBody > dialR)
                        {
                            // Inner gold accent ring around dial
                            wCol = Color.Lerp(goldDark, goldLight, Mathf.Clamp01(rel.y / bodyR + 0.5f));
                        }
                        else
                        {
                            float rNorm = distBody / dialR;
                            wCol = Color.Lerp(dialCenter, dialEdge, rNorm * rNorm);

                            // Urgency magenta-gold quadrant arc (12 o'clock to 3 o'clock)
                            float ang = Mathf.Atan2(rel.y, rel.x); // 0 is 3 o'clock, PI/2 is 12 o'clock
                            if (ang >= 0f && ang <= Mathf.PI * 0.5f && distBody < dialR * 0.90f && distBody > dialR * 0.12f)
                            {
                                wCol = Color.Lerp(wCol, urgencyCol, 0.28f);
                            }

                            // 12 dial tick marks
                            if (distBody >= dialR * 0.75f && distBody <= dialR * 0.91f)
                            {
                                float deg = Mathf.Atan2(rel.y, rel.x) * Mathf.Rad2Deg;
                                float mod = Mathf.Abs(Mathf.Repeat(deg + 15f, 30f) - 15f);
                                if (mod < 2.2f)
                                {
                                    wCol = Color.Lerp(wCol, bezelDark, 0.70f);
                                }
                            }

                            // Clock hands: vertical minute hand pointing up, hour hand pointing ~2 o'clock
                            float dMin = DistanceToSegment(rel, Vector2.zero, new Vector2(0f, dialR * 0.72f));
                            float dHour = DistanceToSegment(rel, Vector2.zero, new Vector2(dialR * 0.46f, dialR * 0.24f));
                            if (dMin < 4.5f || dHour < 5.0f)
                            {
                                wCol = handCol;
                            }

                            // Center golden pin
                            if (distBody < size * 0.036f)
                            {
                                wCol = distBody < size * 0.022f ? goldLight : outlineCol;
                            }
                        }

                        col = Color.Lerp(col, new Color(wCol.r, wCol.g, wCol.b, 1f), waa);
                        col.a = Mathf.Max(col.a, waa);
                    }

                    // Front orbital ring swooping across lower watch body
                    if (inOrbit && isFrontOrbit)
                    {
                        float oa = Mathf.Clamp01(1f - orbitRingDist / 4.5f) * 0.92f;
                        col = Color.Lerp(col, orbitCol, oa);
                        col.a = Mathf.Max(col.a, oa);
                    }

                    pixels[y * size + x] = col;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateLoseHeartHalfSprite(int size, bool isLeft)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[size * size];
            Color outlineCol = new Color(0.227f, 0.031f, 0.141f, 1.0f);   // #3A0824
            Color rubyLight = new Color(1.000f, 0.286f, 0.510f, 1.0f);    // #FF4982
            Color rubyMid = new Color(0.922f, 0.141f, 0.369f, 1.0f);      // #EB245E
            Color rubyDeep = new Color(0.690f, 0.067f, 0.275f, 1.0f);     // #B01146
            Color crackSeam = new Color(1.000f, 0.780f, 0.900f, 1.0f);

            float pixelStep = 1.0f / size;

            for (int y = 0; y < size; y++)
            {
                float ny = (float)y / (size - 1);
                float crackX = EvaluateHeartCrackX(ny);

                for (int x = 0; x < size; x++)
                {
                    float nx = (float)x / (size - 1);
                    float sd = EvaluateHeartSDF(nx, ny);
                    if (sd > pixelStep * 1.4f)
                    {
                        pixels[y * size + x] = Color.clear;
                        continue;
                    }

                    float outerAA = Mathf.Clamp01((-sd) / (pixelStep * 1.6f) + 0.5f);
                    float dxCrack = nx - crackX;
                    float seamAA = isLeft
                        ? Mathf.Clamp01((-dxCrack) / (pixelStep * 1.5f) + 0.5f)
                        : Mathf.Clamp01((dxCrack) / (pixelStep * 1.5f) + 0.5f);

                    float alpha = outerAA * seamAA;
                    if (alpha <= 0.005f)
                    {
                        pixels[y * size + x] = Color.clear;
                        continue;
                    }

                    Color c;
                    if (sd > -0.042f)
                    {
                        c = outlineCol;
                    }
                    else
                    {
                        float gradT = Mathf.Clamp01(ny * 0.78f + (0.65f - nx) * 0.22f);
                        c = gradT > 0.5f
                            ? Color.Lerp(rubyMid, rubyLight, (gradT - 0.5f) * 2f)
                            : Color.Lerp(rubyDeep, rubyMid, gradT * 2f);

                        // Crystal facet sheen on upper-left and upper-right lobes
                        float specL = Vector2.Distance(new Vector2(nx, ny), new Vector2(0.32f, 0.70f));
                        float specR = Vector2.Distance(new Vector2(nx, ny), new Vector2(0.66f, 0.71f));
                        if (specL < 0.14f)
                        {
                            float st = 1f - (specL / 0.14f);
                            c = Color.Lerp(c, Color.white, st * st * 0.48f);
                        }
                        else if (specR < 0.10f)
                        {
                            float st = 1f - (specR / 0.10f);
                            c = Color.Lerp(c, Color.white, st * st * 0.28f);
                        }

                        // Subtle crystal highlight along fractured crack seam
                        float seamDist = Mathf.Abs(dxCrack);
                        if (seamDist < 0.022f)
                        {
                            float st = 1f - (seamDist / 0.022f);
                            c = Color.Lerp(c, crackSeam, st * 0.72f);
                        }
                    }

                    pixels[y * size + x] = new Color(c.r, c.g, c.b, alpha);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateLoseHeartSmallSprite(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[size * size];
            Color outlineCol = new Color(0.227f, 0.031f, 0.141f, 1.0f);
            Color rubyLight = new Color(1.000f, 0.30f, 0.52f, 1.0f);
            Color rubyDeep = new Color(0.720f, 0.07f, 0.28f, 1.0f);
            float pixelStep = 1.0f / size;

            for (int y = 0; y < size; y++)
            {
                float ny = (float)y / (size - 1);
                for (int x = 0; x < size; x++)
                {
                    float nx = (float)x / (size - 1);
                    float sd = EvaluateHeartSDF(nx, ny, 1.50f, 1.46f, 0.14f);
                    if (sd > pixelStep * 1.5f)
                    {
                        pixels[y * size + x] = Color.clear;
                        continue;
                    }

                    float aa = Mathf.Clamp01((-sd) / (pixelStep * 1.8f) + 0.5f);
                    Color c;
                    if (sd > -0.055f)
                    {
                        c = outlineCol;
                    }
                    else
                    {
                        c = Color.Lerp(rubyDeep, rubyLight, Mathf.Clamp01(ny));
                        float spec = Vector2.Distance(new Vector2(nx, ny), new Vector2(0.32f, 0.70f));
                        if (spec < 0.16f)
                        {
                            c = Color.Lerp(c, Color.white, (1f - spec / 0.16f) * 0.45f);
                        }
                    }

                    pixels[y * size + x] = new Color(c.r, c.g, c.b, aa);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateLoseReviveWingsSprite(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];
            Color outlineCol = new Color(0.24f, 0.14f, 0.48f, 1.0f);
            Color wingWhite = new Color(0.98f, 0.96f, 1.00f, 1.0f);
            Color wingLav = new Color(0.76f, 0.65f, 0.98f, 1.0f);
            Color haloGold = new Color(1.00f, 0.91f, 0.48f, 1.0f);
            Color haloLav = new Color(0.92f, 0.78f, 1.00f, 1.0f);

            // 3 symmetric feather capsules per wing side
            Vector2[] featherStarts = {
                new Vector2(0.14f, 0.46f),
                new Vector2(0.17f, 0.34f),
                new Vector2(0.21f, 0.23f)
            };
            Vector2[] featherEnds = {
                new Vector2(0.42f, 0.58f),
                new Vector2(0.39f, 0.42f),
                new Vector2(0.36f, 0.28f)
            };
            float[] featherRadii = { 0.105f, 0.095f, 0.080f };

            for (int y = 0; y < height; y++)
            {
                float ny = (float)y / (height - 1);
                for (int x = 0; x < width; x++)
                {
                    float nx = (float)x / (width - 1);
                    float ax = 0.5f - Mathf.Abs(nx - 0.5f); // symmetric mirror [0..0.5]
                    Vector2 mp = new Vector2(ax * 1.1f, ny);

                    float minWingDist = 10f;
                    for (int f = 0; f < 3; f++)
                    {
                        float d = DistanceToSegment(mp, new Vector2(featherStarts[f].x * 1.1f, featherStarts[f].y),
                                                      new Vector2(featherEnds[f].x * 1.1f, featherEnds[f].y)) - featherRadii[f];
                        if (d < minWingDist) minWingDist = d;
                    }

                    // Floating tilted halo ring above center
                    float hx = (nx - 0.50f) / 0.23f;
                    float hy = (ny - 0.75f) / 0.085f;
                    float hNorm = Mathf.Sqrt(hx * hx + hy * hy);
                    float haloDist = Mathf.Abs(hNorm - 1.0f) * 0.10f;

                    Color c = Color.clear;
                    if (minWingDist < 0.015f)
                    {
                        float waa = Mathf.Clamp01((0.015f - minWingDist) / 0.018f);
                        Color wCol = minWingDist > -0.016f
                            ? outlineCol
                            : Color.Lerp(wingLav, wingWhite, Mathf.Clamp01((ny - 0.18f) / 0.50f));
                        c = new Color(wCol.r, wCol.g, wCol.b, waa);
                    }

                    if (haloDist < 0.036f)
                    {
                        float haa = Mathf.Clamp01(1f - haloDist / 0.036f);
                        Color hCol = haloDist > 0.022f ? outlineCol : Color.Lerp(haloGold, haloLav, Mathf.Abs(nx - 0.5f) * 1.2f);
                        c = Color.Lerp(c, new Color(hCol.r, hCol.g, hCol.b, 1f), haa);
                        c.a = Mathf.Max(c.a, haa);
                    }

                    pixels[y * width + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateLosePlanetRingedSprite(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];
            float cx = width * 0.5f;
            float cy = height * 0.5f;
            float sphereR = height * 0.34f;
            float ringRx = width * 0.46f;
            float ringRy = height * 0.15f;
            float tilt = -18f * Mathf.Deg2Rad;
            float cosT = Mathf.Cos(tilt);
            float sinT = Mathf.Sin(tilt);

            Color planetDark = new Color(0.24f, 0.12f, 0.50f, 1.0f);
            Color planetMid = new Color(0.541f, 0.337f, 0.910f, 1.0f);   // #8A56E8
            Color planetRim = new Color(0.840f, 0.680f, 1.000f, 1.0f);   // #C794FF+
            Color ringOuter = new Color(0.75f, 0.44f, 0.98f, 0.88f);
            Color ringInner = new Color(0.95f, 0.72f, 1.00f, 0.92f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dx = x - cx;
                    float dy = y - cy;
                    float distSphere = Mathf.Sqrt(dx * dx + dy * dy);
                    float sphereAA = Mathf.Clamp01(sphereR + 1.0f - distSphere);

                    // Rotated ring ellipse coordinates
                    float rx = dx * cosT - dy * sinT;
                    float ry = dx * sinT + dy * cosT;
                    float rEllip = Mathf.Sqrt((rx / ringRx) * (rx / ringRx) + (ry / ringRy) * (ry / ringRy));
                    float ringThickness = Mathf.Abs(rEllip - 0.84f);
                    bool inRingBand = ringThickness < 0.18f;
                    bool isFrontRing = (ry < 0f);

                    Color ringCol = Color.clear;
                    if (inRingBand)
                    {
                        float ra = Mathf.Clamp01(1f - ringThickness / 0.18f);
                        ringCol = Color.Lerp(ringOuter, ringInner, Mathf.Clamp01(1f - Mathf.Abs(rEllip - 0.84f) / 0.12f));
                        ringCol.a *= ra * (isFrontRing ? 0.94f : 0.65f);
                    }

                    Color sphereCol = Color.clear;
                    if (sphereAA > 0f)
                    {
                        float nx = dx / sphereR;
                        float ny = dy / sphereR;
                        float nz = Mathf.Sqrt(Mathf.Max(0f, 1f - nx * nx - ny * ny));
                        // Upper-left light source
                        float light = Mathf.Clamp01(nx * -0.52f + ny * 0.56f + nz * 0.64f);
                        float bands = Mathf.Sin((ny * 5.2f + nx * 1.1f) * Mathf.PI) * 0.5f + 0.5f;

                        sphereCol = light > 0.45f
                            ? Color.Lerp(planetMid, planetRim, (light - 0.45f) / 0.55f)
                            : Color.Lerp(planetDark, planetMid, light / 0.45f);
                        sphereCol = Color.Lerp(sphereCol, planetRim, bands * 0.14f);

                        // Edge atmospheric rim glow
                        if (nz < 0.35f)
                        {
                            float rimGlowT = 1f - (nz / 0.35f);
                            sphereCol = Color.Lerp(sphereCol, planetRim, rimGlowT * 0.40f);
                        }
                        sphereCol.a = sphereAA;
                    }

                    Color outCol = Color.clear;
                    if (isFrontRing)
                    {
                        outCol = sphereCol;
                        if (ringCol.a > 0f)
                        {
                            float srcA = ringCol.a;
                            outCol = new Color(
                                Mathf.Lerp(outCol.r, ringCol.r, srcA),
                                Mathf.Lerp(outCol.g, ringCol.g, srcA),
                                Mathf.Lerp(outCol.b, ringCol.b, srcA),
                                Mathf.Clamp01(outCol.a + srcA * (1f - outCol.a)));
                            if (sphereCol.a <= 0f) outCol = ringCol;
                        }
                    }
                    else
                    {
                        outCol = ringCol;
                        if (sphereCol.a > 0f)
                        {
                            float srcA = sphereCol.a;
                            outCol = new Color(
                                Mathf.Lerp(outCol.r, sphereCol.r, srcA),
                                Mathf.Lerp(outCol.g, sphereCol.g, srcA),
                                Mathf.Lerp(outCol.b, sphereCol.b, srcA),
                                Mathf.Clamp01(outCol.a + srcA * (1f - outCol.a)));
                            if (ringCol.a <= 0f) outCol = sphereCol;
                        }
                    }

                    pixels[y * width + x] = outCol;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateLosePlanetMiniSprite(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[size * size];
            float center = (size - 1) * 0.5f;
            float planetR = size * 0.34f;
            float haloR = size * 0.48f;

            Color dark = new Color(0.24f, 0.13f, 0.54f, 1.0f);
            Color mid = new Color(0.56f, 0.34f, 0.92f, 1.0f);
            Color lightRim = new Color(0.86f, 0.72f, 1.00f, 1.0f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist >= haloR)
                    {
                        pixels[y * size + x] = Color.clear;
                        continue;
                    }

                    if (dist <= planetR + 0.75f)
                    {
                        float aa = Mathf.Clamp01(planetR + 0.75f - dist);
                        float nx = Mathf.Clamp(dx / planetR, -1f, 1f);
                        float ny = Mathf.Clamp(dy / planetR, -1f, 1f);
                        float nz = Mathf.Sqrt(Mathf.Max(0f, 1f - nx * nx - ny * ny));
                        float light = Mathf.Clamp01(nx * -0.50f + ny * 0.55f + nz * 0.67f);
                        Color c = light > 0.45f
                            ? Color.Lerp(mid, lightRim, (light - 0.45f) / 0.55f)
                            : Color.Lerp(dark, mid, light / 0.45f);

                        // Subtle craters
                        float c1 = Vector2.Distance(new Vector2(nx, ny), new Vector2(-0.22f, -0.15f));
                        float c2 = Vector2.Distance(new Vector2(nx, ny), new Vector2(0.26f, 0.24f));
                        if (c1 < 0.22f) c = Color.Lerp(c, dark, (1f - c1 / 0.22f) * 0.30f);
                        if (c2 < 0.15f) c = Color.Lerp(c, dark, (1f - c2 / 0.15f) * 0.26f);

                        pixels[y * size + x] = new Color(c.r, c.g, c.b, aa);
                    }
                    else
                    {
                        float ht = 1f - Mathf.Clamp01((dist - planetR) / (haloR - planetR));
                        pixels[y * size + x] = new Color(0.68f, 0.44f, 1.00f, ht * ht * 0.42f);
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateHomeLadderBeamSprite(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];
            float centerX = (width - 1) * 0.5f;
            float coreHalfW = 7.5f;
            float rimHalfW = 11.5f;
            float glowHalfW = (width - 1) * 0.5f;

            Color coreCol = new Color(0.953f, 0.918f, 1.000f, 1.0f); // #F3EAFF
            Color midCol = new Color(0.760f, 0.620f, 1.000f, 1.0f);
            Color glowCol = new Color(0.608f, 0.408f, 1.000f, 1.0f); // #9B68FF

            for (int y = 0; y < height; y++)
            {
                float ny = (float)y / (height - 1);
                float vertFade = ny > 0.86f ? Mathf.Clamp01((1.0f - ny) / 0.14f) : 1.0f;
                vertFade = Mathf.SmoothStep(0f, 1f, vertFade);

                for (int x = 0; x < width; x++)
                {
                    float dx = Mathf.Abs(x - centerX);
                    if (dx >= glowHalfW)
                    {
                        pixels[y * width + x] = Color.clear;
                        continue;
                    }

                    Color col;
                    float alpha;
                    if (dx <= coreHalfW)
                    {
                        float t = dx / coreHalfW;
                        col = Color.Lerp(coreCol, midCol, t * t * 0.55f);
                        alpha = 0.96f;
                    }
                    else if (dx <= rimHalfW)
                    {
                        float t = (dx - coreHalfW) / (rimHalfW - coreHalfW);
                        col = Color.Lerp(midCol, glowCol, t);
                        alpha = Mathf.Lerp(0.94f, 0.62f, t);
                    }
                    else
                    {
                        float t = (dx - rimHalfW) / Mathf.Max(0.001f, glowHalfW - rimHalfW);
                        col = glowCol;
                        alpha = Mathf.Pow(1f - Mathf.Clamp01(t), 2.1f) * 0.48f;
                    }

                    pixels[y * width + x] = new Color(col.r, col.g, col.b, alpha * vertFade);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateHomeFacebookIconSprite(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[size * size];
            float center = (size - 1) * 0.5f;
            float r = size * 0.46f;
            Color rimCol = new Color(0.039f, 0.161f, 0.447f, 1.0f);   // #0A2972
            Color topBlue = new Color(0.169f, 0.498f, 1.000f, 1.0f);  // #2B7FFF
            Color botBlue = new Color(0.078f, 0.329f, 0.800f, 1.0f);  // #1454CC
            Color specCol = new Color(0.62f, 0.84f, 1.00f, 1.0f);

            for (int y = 0; y < size; y++)
            {
                float ny = (float)y / (size - 1);
                Color circleGrad = Color.Lerp(botBlue, topBlue, ny);

                for (int x = 0; x < size; x++)
                {
                    float nx = (float)x / (size - 1);
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float sd = r - dist;
                    if (sd < -0.75f)
                    {
                        pixels[y * size + x] = Color.clear;
                        continue;
                    }

                    float aa = Mathf.Clamp01(sd + 0.75f);
                    Color c = sd < 3.0f ? rimCol : circleGrad;

                    // Upper-left glossy highlight
                    if (sd >= 3.0f && ny > 0.58f)
                    {
                        float specT = (ny - 0.58f) / 0.42f;
                        c = Color.Lerp(c, specCol, specT * 0.24f);
                    }

                    // Crisp geometric white 'f' letter centered inside circle
                    float fx = nx - 0.50f;
                    float fy = ny - 0.46f;
                    bool inStem = (fx >= -0.045f && fx <= 0.085f && fy >= -0.34f && fy <= 0.18f);
                    bool inBar = (fx >= -0.14f && fx <= 0.17f && fy >= -0.02f && fy <= 0.10f);

                    // Top right curve hook of lowercase 'f'
                    float hdx = fx - 0.10f;
                    float hdy = fy - 0.16f;
                    float hDist = Mathf.Sqrt(hdx * hdx + hdy * hdy);
                    bool inHookArc = (hDist >= 0.055f && hDist <= 0.195f && fy >= 0.16f && fx <= 0.18f);
                    bool inHookTip = (fx >= 0.09f && fx <= 0.21f && fy >= 0.22f && fy <= 0.35f);

                    if (inStem || inBar || inHookArc || inHookTip)
                    {
                        c = Color.white;
                    }

                    pixels[y * size + x] = new Color(c.r, c.g, c.b, aa);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateHomeBadgePillSprite(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];
            float radius = (height - 2) * 0.5f;
            Color outlineCol = new Color(0.176f, 0.094f, 0.384f, 1.0f); // #2D1862
            Color topCol = new Color(0.937f, 0.918f, 1.000f, 1.0f);     // #EFEAFF
            Color botCol = new Color(0.788f, 0.729f, 0.980f, 1.0f);     // #C9BAFA

            for (int y = 0; y < height; y++)
            {
                float ny = (float)y / (height - 1);
                Color fill = Color.Lerp(botCol, topCol, ny);

                for (int x = 0; x < width; x++)
                {
                    float distToEdge = ComputeRoundedRectEdgeDist(x, y, width, height, radius);
                    if (distToEdge < -0.6f)
                    {
                        pixels[y * width + x] = Color.clear;
                        continue;
                    }

                    float aa = Mathf.Clamp01(distToEdge + 0.6f);
                    Color c = distToEdge < 2.8f ? outlineCol : fill;
                    pixels[y * width + x] = new Color(c.r, c.g, c.b, aa);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(20f, 14f, 20f, 14f));
        }

        private static Sprite CreateRemoveAdsRewardBarSprite(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];
            float radius = (height - 4) * 0.5f;
            Color outerOutline = new Color(0.118f, 0.063f, 0.302f, 0.90f); // #1E104D
            Color rimLavender = new Color(0.510f, 0.439f, 0.808f, 0.95f);  // #8270CE
            Color fillTop = new Color(0.165f, 0.106f, 0.396f, 0.88f);      // #2A1B65
            Color fillBot = new Color(0.125f, 0.075f, 0.310f, 0.88f);      // #20134F
            Color innerSheen = new Color(0.56f, 0.48f, 0.92f, 0.20f);

            for (int y = 0; y < height; y++)
            {
                float ny = (float)y / (height - 1);
                Color body = Color.Lerp(fillBot, fillTop, ny);

                for (int x = 0; x < width; x++)
                {
                    float distToEdge = ComputeRoundedRectEdgeDist(x, y, width, height, radius);
                    if (distToEdge < -0.75f)
                    {
                        pixels[y * width + x] = Color.clear;
                        continue;
                    }

                    float aa = Mathf.Clamp01(distToEdge + 0.75f);
                    Color c;
                    if (distToEdge < 2.4f)
                    {
                        c = outerOutline;
                    }
                    else if (distToEdge < 5.6f)
                    {
                        float rimT = (distToEdge - 2.4f) / 3.2f;
                        c = Color.Lerp(rimLavender, body, rimT * 0.35f);
                    }
                    else
                    {
                        c = body;
                        if (ny > 0.72f)
                        {
                            float specT = (ny - 0.72f) / 0.28f;
                            c = Color.Lerp(c, innerSheen, specT * 0.45f);
                        }
                        if (ny < 0.14f)
                        {
                            float botT = 1f - (ny / 0.14f);
                            c = Color.Lerp(c, outerOutline, botT * 0.25f);
                        }
                    }

                    pixels[y * width + x] = new Color(c.r, c.g, c.b, c.a * aa);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(48f, 48f, 48f, 48f));
        }

        private static Sprite CreateRemoveAdsMiniBadgeSprite(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[size * size];
            float center = (size - 1) * 0.5f;
            float outerR = size * 0.45f;
            float innerR = size * 0.315f;
            float slashHalfW = size * 0.060f;

            Color redOutline = new Color(0.50f, 0.04f, 0.09f, 1.0f);
            Color redTop = new Color(1.000f, 0.510f, 0.525f, 1.0f);     // #FF8286
            Color redMain = new Color(0.949f, 0.231f, 0.251f, 1.0f);    // #F23B40
            Color redBot = new Color(0.643f, 0.075f, 0.106f, 1.0f);     // #A4131B
            Color discTop = new Color(0.32f, 0.17f, 0.60f, 1.0f);
            Color discBot = new Color(0.20f, 0.09f, 0.42f, 1.0f);

            for (int y = 0; y < size; y++)
            {
                float ny = (float)y / (size - 1);
                Color ringCol = ny > 0.55f
                    ? Color.Lerp(redMain, redTop, (ny - 0.55f) / 0.45f)
                    : Color.Lerp(redBot, redMain, ny / 0.55f);
                Color discCol = Color.Lerp(discBot, discTop, ny);

                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float sdOuter = outerR - dist;
                    if (sdOuter < -0.75f)
                    {
                        pixels[y * size + x] = Color.clear;
                        continue;
                    }

                    float aa = Mathf.Clamp01(sdOuter + 0.75f);
                    // Diagonal bar at +45 deg (bottom-left to top-right)
                    float perpDist = Mathf.Abs(dx - dy) * 0.70710678f;
                    bool inRing = dist >= innerR || perpDist <= slashHalfW;

                    Color c;
                    if (sdOuter < 2.2f)
                    {
                        c = redOutline;
                    }
                    else if (inRing)
                    {
                        float rimFactor = (dist - innerR);
                        if (dist < innerR + 2.0f && perpDist > slashHalfW)
                        {
                            c = Color.Lerp(redOutline, ringCol, Mathf.Clamp01(rimFactor / 2.0f));
                        }
                        else if (dist < innerR && Mathf.Abs(perpDist - slashHalfW) < 2.2f)
                        {
                            c = Color.Lerp(ringCol, redOutline, 0.35f);
                        }
                        else
                        {
                            c = ringCol;
                        }
                    }
                    else
                    {
                        c = discCol;
                    }

                    pixels[y * size + x] = new Color(c.r, c.g, c.b, aa);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateRemoveAdsGoldButtonSprite(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];
            float hw = (width - 1) * 0.5f;
            float hh = (height - 1) * 0.5f;
            float chamfer = 34f;

            Color darkEdge = new Color(0.54f, 0.24f, 0.00f, 1.0f);
            Color deepAmber = new Color(0.722f, 0.349f, 0.000f, 1.0f);  // #B85900
            Color midAmber = new Color(0.875f, 0.494f, 0.016f, 1.0f);   // #DF7E04
            Color botGold = new Color(1.000f, 0.729f, 0.114f, 1.0f);    // #FFBA1D
            Color topGold = new Color(1.000f, 0.851f, 0.239f, 1.0f);    // #FFD93D
            Color topSpecRim = new Color(1.000f, 0.949f, 0.580f, 1.0f); // #FFF294
            float lipHeight = 20f;

            for (int y = 0; y < height; y++)
            {
                float ny = (float)y / (height - 1);
                float ay = Mathf.Abs(y - hh);
                float bodyT = Mathf.Clamp01((y - lipHeight) / Mathf.Max(1f, height - 1 - lipHeight));
                Color faceCol = Color.Lerp(botGold, topGold, bodyT);

                for (int x = 0; x < width; x++)
                {
                    float ax = Mathf.Abs(x - hw);
                    float distX = hw - ax;
                    float distY = hh - ay;
                    float distDiag = ((hw + hh - chamfer) - (ax + ay)) * 0.70710678f;
                    float distToEdge = Mathf.Min(Mathf.Min(distX, distY), distDiag);

                    if (distToEdge < -0.75f)
                    {
                        pixels[y * width + x] = Color.clear;
                        continue;
                    }

                    float aa = Mathf.Clamp01(distToEdge + 0.75f);
                    Color c;
                    if (distToEdge < 3.2f)
                    {
                        c = y > hh ? midAmber : darkEdge;
                    }
                    else if (y < lipHeight)
                    {
                        float lt = Mathf.Clamp01((y - 3.2f) / (lipHeight - 3.2f));
                        c = Color.Lerp(deepAmber, midAmber, lt);
                    }
                    else
                    {
                        c = faceCol;
                        // Upper bevel highlight
                        if (distToEdge < 14f && ny > 0.40f)
                        {
                            float bevelT = 1f - ((distToEdge - 3.2f) / 10.8f);
                            c = Color.Lerp(c, topSpecRim, Mathf.Clamp01(bevelT) * 0.65f);
                        }
                        if (ny > 0.82f)
                        {
                            float topGlow = (ny - 0.82f) / 0.18f;
                            c = Color.Lerp(c, topSpecRim, topGlow * 0.35f);
                        }
                        // Soft subtle bevel line separating face & bottom lip
                        if (y < lipHeight + 5f)
                        {
                            float crease = 1f - ((y - lipHeight) / 5f);
                            c = Color.Lerp(c, midAmber, crease * 0.32f);
                        }
                    }

                    pixels[y * width + x] = new Color(c.r, c.g, c.b, aa);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(72f, 44f, 72f, 44f));
        }

        private static Sprite CreateRemoveAdsShadowBlobSprite(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];
            float hw = (width - 1) * 0.5f;
            float hh = (height - 1) * 0.5f;
            Color shadowCol = new Color(0.071f, 0.031f, 0.180f, 1.0f); // #12082E

            for (int y = 0; y < height; y++)
            {
                float dy = (y - hh) / Mathf.Max(1f, hh);
                for (int x = 0; x < width; x++)
                {
                    float dx = (x - hw) / Mathf.Max(1f, hw);
                    float r2 = dx * dx + dy * dy;
                    if (r2 >= 1f)
                    {
                        pixels[y * width + x] = Color.clear;
                        continue;
                    }
                    float falloff = Mathf.Pow(1f - Mathf.Sqrt(r2), 1.6f);
                    pixels[y * width + x] = new Color(shadowCol.r, shadowCol.g, shadowCol.b, falloff * 0.62f);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateAxisArrowSprite(int width, int height, bool horizontal)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];
            float cx = (width - 1) * 0.5f;
            float cy = (height - 1) * 0.5f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Normalize coordinates so arrow always runs horizontally along u in [-1, 1], v in [-1, 1]
                    float u = horizontal ? (x - cx) / Mathf.Max(1f, cx) : (y - cy) / Mathf.Max(1f, cy);
                    float v = horizontal ? (y - cy) / Mathf.Max(1f, cy) : (x - cx) / Mathf.Max(1f, cx);
                    float au = Mathf.Abs(u);
                    float av = Mathf.Abs(v);

                    // Bidirectional arrow: shaft in au <= 0.44, triangular arrowheads from au 0.44..0.88
                    bool inShaft = au <= 0.46f && av <= 0.16f;
                    float headT = (au - 0.40f) / 0.48f; // 0 at base, 1 at tip
                    bool inHead = au >= 0.40f && au <= 0.88f && av <= Mathf.Lerp(0.46f, 0.0f, Mathf.Clamp01(headT));

                    // Soft outline distance
                    float distShaft = Mathf.Max(au - 0.46f, av - 0.16f);
                    float headMaxV = Mathf.Lerp(0.46f, 0.0f, Mathf.Clamp01((au - 0.40f) / 0.48f));
                    float distHead = au < 0.40f ? (0.40f - au) : Mathf.Max(au - 0.88f, av - headMaxV);
                    float dist = Mathf.Min(distShaft, distHead);

                    if (inShaft || inHead)
                    {
                        float rim = Mathf.Clamp01(-dist * 10f);
                        Color core = Color.Lerp(new Color(0.38f, 0.96f, 1.0f, 0.94f), new Color(1.0f, 1.0f, 1.0f, 0.98f), rim * 0.72f);
                        pixels[y * width + x] = core;
                    }
                    else if (dist < 0.14f)
                    {
                        float glowA = Mathf.Clamp01(1f - dist / 0.14f);
                        pixels[y * width + x] = new Color(0.06f, 0.14f, 0.28f, glowA * 0.78f);
                    }
                    else
                    {
                        pixels[y * width + x] = Color.clear;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateStoneObstacleSprite(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];
            float r = 24f;
            float hw = (width - 1) * 0.5f;
            float hh = (height - 1) * 0.5f;

            Color darkEdge = new Color(0.11f, 0.12f, 0.20f, 1f);
            Color baseSlate = new Color(0.24f, 0.25f, 0.36f, 1f);
            Color topHighlight = new Color(0.42f, 0.44f, 0.58f, 1f);
            Color shadowFacet = new Color(0.16f, 0.17f, 0.26f, 1f);
            Color cyanCrack = new Color(0.24f, 0.78f, 0.95f, 0.72f);

            for (int y = 0; y < height; y++)
            {
                float ny = (float)y / (height - 1);
                for (int x = 0; x < width; x++)
                {
                    float nx = (float)x / (width - 1);
                    float dx = Mathf.Max(0f, Mathf.Abs(x - hw) - (hw - r));
                    float dy = Mathf.Max(0f, Mathf.Abs(y - hh) - (hh - r));
                    float distOut = Mathf.Sqrt(dx * dx + dy * dy) - r;

                    if (distOut > 0.75f)
                    {
                        pixels[y * width + x] = Color.clear;
                        continue;
                    }

                    float aa = Mathf.Clamp01(0.75f - distOut);
                    float distIn = -distOut;

                    Color c = Color.Lerp(shadowFacet, baseSlate, ny * 0.75f + (1f - nx) * 0.25f);

                    // Faceted stone diagonal bevels
                    float diag1 = nx * 0.7f + ny * 0.9f;
                    if (diag1 > 1.05f) c = Color.Lerp(c, topHighlight, 0.34f);
                    else if (diag1 < 0.52f) c = Color.Lerp(c, shadowFacet, 0.45f);

                    // Outer dark border and crisp top-left bevel catchlight
                    if (distIn < 5.5f)
                    {
                        c = darkEdge;
                    }
                    else if (distIn < 14f)
                    {
                        float rimT = 1f - (distIn - 5.5f) / 8.5f;
                        if (ny > 0.42f || nx < 0.48f) c = Color.Lerp(c, topHighlight, rimT * 0.55f);
                        else c = Color.Lerp(c, darkEdge, rimT * 0.45f);
                    }

                    // Diagonal stone cracks + subtle glowing crystal vein in center
                    float crack1 = Mathf.Abs((ny - 0.5f) - 0.55f * (nx - 0.5f));
                    float crack2 = Mathf.Abs((ny - 0.52f) + 0.72f * (nx - 0.48f));
                    bool inRegion1 = nx > 0.22f && nx < 0.78f;
                    bool inRegion2 = nx > 0.34f && nx < 0.68f;
                    if ((inRegion1 && crack1 < 0.020f) || (inRegion2 && crack2 < 0.016f))
                    {
                        c = Color.Lerp(darkEdge, cyanCrack, 0.38f);
                    }

                    pixels[y * width + x] = new Color(c.r, c.g, c.b, aa);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateFreezeScreenBorderSprite(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];
            Color deepIce = new Color(0.18f, 0.66f, 0.98f, 1f);
            Color brightFrost = new Color(0.76f, 0.96f, 1.00f, 1f);
            Color rimWhite = new Color(0.92f, 0.99f, 1.00f, 1f);

            for (int y = 0; y < height; y++)
            {
                float ny = (float)y / (height - 1);
                float dy = Mathf.Min(ny, 1f - ny);
                float edgeV = Mathf.Pow(Mathf.Clamp01(1f - dy / 0.115f), 2.0f);

                for (int x = 0; x < width; x++)
                {
                    float nx = (float)x / (width - 1);
                    float dx = Mathf.Min(nx, 1f - nx);
                    float edgeH = Mathf.Pow(Mathf.Clamp01(1f - dx / 0.145f), 2.0f);

                    float cornerDist = Mathf.Sqrt(dx * dx + dy * dy);
                    float cornerBoost = Mathf.Pow(Mathf.Clamp01(1f - cornerDist / 0.25f), 1.75f);

                    float baseEdge = Mathf.Clamp01(Mathf.Max(edgeH, edgeV) + cornerBoost * 0.65f);
                    if (baseEdge <= 0.002f)
                    {
                        pixels[y * width + x] = Color.clear;
                        continue;
                    }

                    float veinWave = 0.85f + 0.15f * Mathf.Sin(nx * 31f + Mathf.Cos(ny * 23f) * 1.4f)
                                                   * Mathf.Cos(ny * 29f - Mathf.Sin(nx * 19f) * 1.3f);
                    float frost = Mathf.Clamp01(baseEdge * veinWave);

                    float outerRim = Mathf.Clamp01(1f - Mathf.Min(dx / 0.020f, dy / 0.012f));
                    Color col = Color.Lerp(deepIce, brightFrost, Mathf.Clamp01(frost * 0.85f + cornerBoost * 0.35f));
                    col = Color.Lerp(col, rimWhite, outerRim * 0.58f);

                    float alpha = Mathf.Clamp01(frost * 0.85f + outerRim * 0.30f);
                    pixels[y * width + x] = new Color(col.r, col.g, col.b, alpha);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateSnowflakeSprite(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[size * size];
            float half = (size - 1) * 0.5f;
            float maxR = size * 0.44f;
            float sector = Mathf.PI / 3f;
            float armHalfW = size * 0.026f;
            float barbHalfW = size * 0.020f;

            Color coreWhite = new Color(0.98f, 1.00f, 1.00f, 1f);
            Color cyanEdge = new Color(0.58f, 0.92f, 1.00f, 1f);

            for (int y = 0; y < size; y++)
            {
                float py = y - half;
                for (int x = 0; x < size; x++)
                {
                    float px = x - half;
                    float r = Mathf.Sqrt(px * px + py * py);
                    if (r > maxR + 3f)
                    {
                        pixels[y * size + x] = Color.clear;
                        continue;
                    }

                    float angle = Mathf.Atan2(py, px);
                    float folded = Mathf.Abs(Mathf.Repeat(angle + sector * 0.5f, sector) - sector * 0.5f);

                    float u = r * Mathf.Cos(folded);
                    float v = r * Mathf.Sin(folded);

                    // Main arm spine
                    float dSpine = u <= maxR * 0.94f ? v : Vector2.Distance(new Vector2(u, v), new Vector2(maxR * 0.94f, 0f));
                    float spineAlpha = Mathf.Clamp01(1f - (dSpine - armHalfW) / 1.8f);

                    // Two V-shaped crystal barbs per arm
                    float b1 = DistanceToSegment(new Vector2(u, v), new Vector2(maxR * 0.40f, 0f), new Vector2(maxR * 0.58f, maxR * 0.22f));
                    float b2 = DistanceToSegment(new Vector2(u, v), new Vector2(maxR * 0.66f, 0f), new Vector2(maxR * 0.81f, maxR * 0.16f));
                    float barbAlpha = Mathf.Max(
                        Mathf.Clamp01(1f - (b1 - barbHalfW) / 1.6f),
                        Mathf.Clamp01(1f - (b2 - barbHalfW) / 1.6f));

                    // Hexagonal center hub
                    float hubDist = u;
                    float hubAlpha = Mathf.Clamp01(1f - (hubDist - maxR * 0.17f) / 1.8f);

                    float shapeAlpha = Mathf.Max(spineAlpha, Mathf.Max(barbAlpha, hubAlpha));
                    float softGlow = Mathf.Pow(Mathf.Clamp01(1f - r / maxR), 2.4f) * 0.32f;
                    float finalAlpha = Mathf.Clamp01(shapeAlpha + softGlow * (1f - shapeAlpha));

                    Color c = Color.Lerp(cyanEdge, coreWhite, Mathf.Clamp01(shapeAlpha * 0.85f + (1f - r / maxR) * 0.25f));
                    pixels[y * size + x] = new Color(c.r, c.g, c.b, finalAlpha);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateFreezeIceShellSprite(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[size * size];
            Color bodyDeep = new Color(0.24f, 0.72f, 0.98f, 0.82f);
            Color bodyLight = new Color(0.62f, 0.94f, 1.00f, 0.78f);
            Color facetWhite = new Color(0.93f, 0.99f, 1.00f, 0.96f);

            for (int y = 0; y < size; y++)
            {
                float ny = (float)y / (size - 1);
                for (int x = 0; x < size; x++)
                {
                    float nx = (float)x / (size - 1);
                    float cx = nx - 0.5f;
                    float cy = ny - 0.36f;

                    // Cup-shaped crystalline cluster wrapping the lower/side stopwatch
                    float dxNorm = Mathf.Abs(cx) / 0.44f;
                    float topRidge = 0.58f
                        + 0.16f * Mathf.Abs(cx * 2.1f)
                        + 0.055f * Mathf.Sin(nx * 36f)
                        + 0.035f * Mathf.Cos(nx * 62f);
                    float botCurve = 0.08f + 0.18f * (dxNorm * dxNorm);

                    if (dxNorm > 1.0f || ny > topRidge || ny < botCurve)
                    {
                        pixels[y * size + x] = Color.clear;
                        continue;
                    }

                    float distTop = topRidge - ny;
                    float distBot = ny - botCurve;
                    float distSide = (1.0f - dxNorm) * 0.44f;
                    float minEdge = Mathf.Min(distTop, Mathf.Min(distBot, distSide));
                    float aa = Mathf.Clamp01(minEdge * size * 0.45f);

                    // Faceted shard stripes inside crystal body
                    float facet1 = Mathf.Abs(Mathf.Sin((nx * 5.2f + ny * 3.4f) * Mathf.PI));
                    float facet2 = Mathf.Abs(Mathf.Cos((nx * 4.1f - ny * 4.8f) * Mathf.PI));
                    float facetHighlight = Mathf.Pow(Mathf.Clamp01(facet1 * 0.55f + facet2 * 0.45f), 2.2f);

                    Color c = Color.Lerp(bodyDeep, bodyLight, ny * 0.7f + facetHighlight * 0.4f);
                    if (minEdge < 0.032f)
                    {
                        float rimT = 1f - minEdge / 0.032f;
                        c = Color.Lerp(c, facetWhite, rimT * 0.88f);
                    }
                    else
                    {
                        c = Color.Lerp(c, facetWhite, facetHighlight * 0.28f);
                    }

                    // Keep upper-middle slightly more translucent so stopwatch face shines through
                    float centerSeeThrough = (ny > 0.30f && Mathf.Abs(cx) < 0.24f) ? 0.62f : 0.92f;
                    pixels[y * size + x] = new Color(c.r, c.g, c.b, c.a * aa * centerSeeThrough);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateTimerFrostFrameSprite(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];
            Color iceCyan = new Color(0.45f, 0.89f, 1.00f, 1f);
            Color snowWhite = new Color(0.93f, 0.99f, 1.00f, 1f);

            // 6 icicles hanging along the bottom edge (x normalized center, length)
            float[] icicleX = { 0.16f, 0.30f, 0.43f, 0.58f, 0.72f, 0.85f };
            float[] icicleLen = { 0.23f, 0.16f, 0.26f, 0.18f, 0.24f, 0.15f };

            for (int y = 0; y < height; y++)
            {
                float ny = (float)y / (height - 1);
                for (int x = 0; x < width; x++)
                {
                    float nx = (float)x / (width - 1);

                    // Top snowy cap & side rim (banner sits roughly in ny [0.24..0.95], nx [0.04..0.96])
                    float topCapLine = 0.78f + 0.03f * Mathf.Sin(nx * 24f);
                    float botRimLine = 0.28f;

                    float alpha = 0f;
                    Color col = iceCyan;

                    // Snowy cap along top
                    if (ny >= topCapLine && ny <= 0.97f && nx >= 0.05f && nx <= 0.95f)
                    {
                        float dEdge = Mathf.Min(ny - topCapLine, 0.97f - ny);
                        float dSide = Mathf.Min(nx - 0.05f, 0.95f - nx);
                        float mask = Mathf.Clamp01(Mathf.Min(dEdge * 32f, dSide * 26f));
                        float snowMix = Mathf.Clamp01((ny - topCapLine) / 0.14f);
                        col = Color.Lerp(iceCyan, snowWhite, snowMix);
                        alpha = Mathf.Max(alpha, mask * 0.92f);
                    }

                    // Glowing icy left/right/bottom rim
                    float dOuterX = Mathf.Min(nx - 0.03f, 0.97f - nx);
                    float dOuterY = Mathf.Min(ny - 0.24f, 0.96f - ny);
                    if (dOuterX >= 0f && dOuterY >= 0f)
                    {
                        float rimDist = Mathf.Min(dOuterX, dOuterY * 0.55f);
                        if (rimDist < 0.055f)
                        {
                            float rimA = Mathf.Clamp01(1f - rimDist / 0.055f) * 0.82f;
                            col = Color.Lerp(iceCyan, snowWhite, rimA * 0.55f);
                            alpha = Mathf.Max(alpha, rimA);
                        }
                    }

                    // Hanging icicles along bottom
                    if (ny < botRimLine && ny >= 0.02f)
                    {
                        float drop = botRimLine - ny;
                        for (int k = 0; k < icicleX.Length; k++)
                        {
                            float len = icicleLen[k];
                            if (drop <= len)
                            {
                                float halfW = 0.032f * (1f - drop / len);
                                float dx = Mathf.Abs(nx - icicleX[k]);
                                if (dx <= halfW + 0.008f)
                                {
                                    float icicleA = Mathf.Clamp01(1f - (dx - halfW) / 0.008f) * 0.92f;
                                    float shine = Mathf.Clamp01(1f - dx / Mathf.Max(0.002f, halfW));
                                    col = Color.Lerp(iceCyan, snowWhite, shine * 0.72f);
                                    alpha = Mathf.Max(alpha, icicleA);
                                }
                            }
                        }
                    }

                    pixels[y * width + x] = new Color(col.r, col.g, col.b, alpha);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateFreezeTooltipCardSprite(int width, int height, int radius)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];
            float r = radius;
            float borderThickness = 5.0f;

            Color fillTop = new Color(1.00f, 1.00f, 1.00f, 1.00f);
            Color fillBot = new Color(0.95f, 0.96f, 1.00f, 1.00f);
            Color borderColor = new Color(0.66f, 0.72f, 0.96f, 1.00f);

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
    }
}
