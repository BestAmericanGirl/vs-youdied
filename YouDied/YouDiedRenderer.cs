
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace YouDied
{
    public class YouDiedRenderer : IRenderer
    {
        public double RenderOrder => 0;

        public int RenderRange => 999999;

        private ICoreClientAPI capi;
        private string text = "YOU DIED";
        private float elapsedMs = 0f;
        private float durationMs = 3000f;
        private bool dying = false;

        private LoadedTexture textTexture;
        private LoadedTexture backgroundTexture;
        private ILoadedSound sound = null!;

        public YouDiedRenderer(ICoreClientAPI api)
        {
            capi = api;
            YouDiedModSystem.SettingsChanged += () =>
            {
                UpdateSettings();
            };

            text = Lang.Get("youdied:YouDied");

            textTexture = new LoadedTexture(capi);
            backgroundTexture = new LoadedTexture(capi);
            LoadTextures();
        }

        public void Dispose()
        {
            textTexture.Dispose();
            backgroundTexture.Dispose();
        }

        public void LoadSound()
        {
            sound = capi.World.LoadSound(new SoundParams()
            {
                Location = new AssetLocation("youdied", "sounds/YouDied"),
                ShouldLoop = false,
                DisposeOnFinish = false,
                SoundType = EnumSoundType.Sound,
            });
        }

        public void TriggerDeath()
        {
            dying = true;
            elapsedMs = 0f;
            if (sound == null)
            {
                return;
            }
            sound.SetVolume(YouDiedConfig.Instance.Volume);
            sound.Start();
        }

        public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
        {
            if (!dying)
                return;
            elapsedMs += deltaTime * 1000f;

            durationMs = YouDiedConfig.Instance.DurationInMs;

            // Fade in for the first quarter, fade out the last quarter
            float quarterDurationMs = durationMs / 4;
            float alpha = Math.Min(elapsedMs / durationMs, 1f);

            if (elapsedMs < quarterDurationMs)
            {
                alpha = Math.Min(elapsedMs / quarterDurationMs, 1f);
            }
            else if (elapsedMs > 3 * quarterDurationMs)
            {
                alpha = Math.Max(1 - (elapsedMs - 3 * quarterDurationMs) / quarterDurationMs, 0f);
            }
            else
            {
                alpha = 1f;
            }

            int width = capi.Render.FrameWidth;
            int height = capi.Render.FrameHeight;

            float textX = width / 2f - textTexture.Width / 2f;
            float textY = height / 2f - textTexture.Height / 2f;

            // Rectangle slightly taller than text
            float rectHeight = textTexture.Height * 1.4f;
            float rectY = textY - (rectHeight - textTexture.Height) / 2f;

            Vec4f rectColor = new Vec4f(alpha, alpha, alpha, alpha);

            capi.Render.Render2DTexture(
                backgroundTexture.TextureId,
                0f,
                rectY,
                width,
                rectHeight,
                49,
                rectColor
            );

            // Draw text
            capi.Render.Render2DTexture(
                textTexture.TextureId,
                textX,
                textY,
                textTexture.Width,
                textTexture.Height,
                50,
                new Vec4f(1f, 1f, 1f, alpha)
            );

            // Stop rendering after fade completes
            if (elapsedMs >= durationMs)
            {
                dying = false;
            }
        }

        private void LoadTextures()
        {
            float fontSize = YouDiedConfig.Instance.FontSize;
            double[] color = ColorUtil.Hex2Doubles(YouDiedConfig.Instance.FontColorHex);
            CairoFont font = CairoFont.WhiteSmallText().WithColor(color).WithFontSize(fontSize);
            capi.Gui.TextTexture.GenOrUpdateTextTexture(text, font, ref textTexture);

            //AssetLocation textureLocation = new AssetLocation("youdied", "textures/background.svg");

            //int iconSize = 512;

            //backgroundTexture = capi.Gui.LoadSvg(textureLocation, iconSize, iconSize, iconSize, iconSize, null);

            AssetLocation backgroundLoc = new AssetLocation("youdied", "textures/background2.png");

            capi.Render.GetOrLoadTexture(backgroundLoc, ref backgroundTexture);
        }

        private void UpdateSettings()
        {

            LoadTextures();
        }
    }
}
