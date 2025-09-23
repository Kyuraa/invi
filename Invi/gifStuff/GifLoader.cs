using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Numerics;
using SixLabors.ImageSharp;




namespace Invi;


public static class GifLoader
{
    // Directory containing your gif emotes
    private static readonly string EmoteDir = Path.Combine(
        Plugin.PluginInterface.AssemblyLocation.Directory?.FullName!, "Emotes");

    // Cache loaded gifs by filename (without extension)
    private static readonly Dictionary<string, ImGuiGif> EmoteImages = new();

    // Load all .gif files in the directory (should be called once at plugin start)
    public static void LoadAll()
    {
        if (!Directory.Exists(EmoteDir))
            return;

        foreach (var file in Directory.GetFiles(EmoteDir, "*.gif"))
        {
            var name = Path.GetFileNameWithoutExtension(file);
            if (!EmoteImages.ContainsKey(name))
            {
                EmoteImages[name] = new ImGuiGif().Prepare(file);
            }
        }
    }

    // Call this in your UI/Main thread before drawing gifs, e.g. once per frame
    public static void EnsureTexturesUploaded()
    {
        foreach (var gif in EmoteImages.Values)
            gif.EnsureTexturesUploaded();
    }

    // Draw the gif by name (filename without .gif)
    public static void Draw(string name, Vector2 size)
    {
        if (EmoteImages.TryGetValue(name, out var gif) && gif.IsLoaded)
        {
            gif.Draw(size);
        }
        else
        {
            ImGui.Text($"GIF '{name}' not loaded or missing!");
        }
    }

    // Returns true if the named gif is loaded and IsLoaded is true
    public static bool IsLoaded(string name)
    {
        return EmoteImages.TryGetValue(name, out var gif) && gif.IsLoaded;
    }

    // Returns the original size of the gif as a Vector2 (width, height), or null if not loaded.
    public static Vector2? GetSize(string name)
    {
        if (EmoteImages.TryGetValue(name, out var gif) && gif.IsLoaded)
            return new Vector2(gif.Width, gif.Height);
        return null;
    }

    // Call this on shutdown
    public static void Dispose()
    {
        foreach (var gif in EmoteImages.Values)
            gif.InnerDispose();
        EmoteImages.Clear();
    }

    public sealed class ImGuiGif
    {
        private List<(IDalamudTextureWrap Tex, float Delay)> Frames = [];
        private List<byte[]> FrameBuffers = new(); // store raw RGBA pixel buffers for each frame
        private List<float> FrameDelays = new();
        private float FrameTimer;
        private int CurrentFrame;
        private ulong GlobalFrameCount;
        private bool NeedsTextureUpload = false; // set true when frames ready but textures aren't

        public bool IsLoaded { get; private set; }
        public bool Failed { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public ImGuiGif Prepare(string filePath)
        {
            Task.Run(() => LoadFrames(filePath));
            return this;
        }

        // Stage 1: Decode frames/background thread
        private async void LoadFrames(string filePath)
        {
            try
            {
                var imageBytes = await File.ReadAllBytesAsync(filePath);
                using var ms = new MemoryStream(imageBytes);
                using var img = Image.Load<Rgba32>(ms);
                if (img.Frames.Count == 0)
                    return;

                Width = img.Width;
                Height = img.Height;

                var delays = new List<float>();
                var buffers = new List<byte[]>();
                foreach (var frame in img.Frames)
                {
                    var delay = frame.Metadata.GetGifMetadata().FrameDelay / 100f;
                    if (delay < 0.02f)
                        delay = 0.1f;

                    var buffer = new byte[4 * frame.Width * frame.Height];
                    frame.CopyPixelDataTo(buffer);
                    buffers.Add(buffer);
                    delays.Add(delay);
                }

                FrameBuffers = buffers;
                FrameDelays = delays;
                NeedsTextureUpload = true; // Mark for main-thread upload
            }
            catch (Exception ex)
            {
                Failed = true;
                Plugin.Log.Error(ex, $"Unable to load GIF frames from {filePath}");
            }
        }

        // Call from main thread (e.g. once per frame, before drawing)
        public void EnsureTexturesUploaded()
        {
            if (!NeedsTextureUpload || FrameBuffers.Count == 0)
                return;

            var frames = new List<(IDalamudTextureWrap Tex, float Delay)>();
            for (int i = 0; i < FrameBuffers.Count; i++)
            {
                // This must run on main thread!
                var tex = Plugin.TextureProvider.CreateFromRaw(
                    RawImageSpecification.Rgba32(Width, Height), FrameBuffers[i]);
                frames.Add((tex, FrameDelays[i]));
            }

            Frames = frames;
            IsLoaded = true;
            NeedsTextureUpload = false;
            FrameBuffers.Clear(); // free up memory
            FrameDelays.Clear();
        }

        public void Draw(Vector2 size)
        {
            if (Frames.Count == 0)
                return;

            if (CurrentFrame >= Frames.Count)
            {
                CurrentFrame = 0;
                FrameTimer = -1f;
            }

            var frame = Frames[CurrentFrame];
            if (FrameTimer <= 0.0f)
                FrameTimer = frame.Delay;

            ImGui.Image(frame.Tex.Handle, size);

            if (GlobalFrameCount == Plugin.PluginInterface.UiBuilder.FrameCount)
                return;

            GlobalFrameCount = Plugin.PluginInterface.UiBuilder.FrameCount;

            FrameTimer -= ImGui.GetIO().DeltaTime;
            if (FrameTimer <= 0f)
                CurrentFrame++;
        }

        public void InnerDispose()
        {
            Frames.ForEach(f => f.Tex.Dispose());
            Frames.Clear();
        }
    }

    }
