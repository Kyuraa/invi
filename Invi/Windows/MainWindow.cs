using Dalamud.Bindings.ImGui;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;


namespace Invi.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly string goatImagePath;
    private readonly Plugin plugin;
    private readonly string doroImagePath;
    //private readonly string tippypath;
    //private readonly IDalamudTextureWrap tippySpriteSheet;
    private int count = 0;

    public MainWindow(Plugin plugin, string goatImagePath)
        : base("Invi owo##With a hidden ID", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(600, 600),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.goatImagePath = goatImagePath;
        this.plugin = plugin;
        doroImagePath = Path.Combine(Plugin.PluginInterface.AssemblyLocation.Directory?.FullName!, "dorodance.gif");

        

        //tippypath= Path.Combine(Plugin.PluginInterface.AssemblyLocation.Directory?.FullName!, "dorodance.gif");
        //tippySpriteSheet = Plugin.TextureProvider.GetFromFile(doroImagePath).GetWrapOrEmpty();


    }   

    public void Dispose() {
        Plugin.chatGui.ChatMessage -= ChatGui_ChatMessage;
        resetCancellation?.Cancel();
        resetCancellation?.Dispose();
        resetCancellation = null;

    }

    public override void Draw()
    {

        ImGui.TextUnformatted($"The random config bool is {plugin.Configuration.SomePropertyToBeSavedAndWithADefault}");

        if (ImGui.Button("Show Settings"))
        {

            plugin.ToggleConfigUi();
        }

        if (ImGui.Button("send message"))
        {

            //chatGui.Print("test");
            //Plugin.chatGui.Print("test2");

            
        }

        Plugin.chatGui.ChatMessage += ChatGui_ChatMessage;
        

        ImGui.Spacing();

        using (var child = ImRaii.Child("SomeChildWithAScrollbar", Vector2.Zero, true))
        {
            if (child.Success)
            {
                /*
                ImGui.TextUnformatted("Have a goat:");
                var goatImage = Plugin.TextureProvider.GetFromFile(goatImagePath).GetWrapOrDefault();
                if (goatImage != null)
                {
                    using (ImRaii.PushIndent(55f))
                    {
                        ImGui.Image(goatImage.Handle, goatImage.Size);
                    }
                }
                else
                {
                    ImGui.TextUnformatted("Image not found.");
                }
                */
                ImGuiHelpers.ScaledDummy(20.0f);

                var localPlayer = Plugin.ClientState.LocalPlayer;
                if (localPlayer == null)
                {
                    ImGui.TextUnformatted("Our local player is currently not loaded.");
                    return;
                }

                if (!localPlayer.ClassJob.IsValid)
                {
                    ImGui.TextUnformatted("Our current job is currently not valid.");
                    return;
                }

                ImGui.TextUnformatted($"Our current job is ({localPlayer.ClassJob.RowId}) \"{localPlayer.ClassJob.Value.Abbreviation}\"");

                var territoryId = Plugin.ClientState.TerritoryType;
                if (Plugin.DataManager.GetExcelSheet<TerritoryType>().TryGetRow(territoryId, out var territoryRow))
                {
                    ImGui.TextUnformatted($"We are currently in ({territoryId}) \"{territoryRow.PlaceName.Value.Name}\"");
                }
                else
                {
                    ImGui.TextUnformatted("Invalid territory.");
                }

                

                //// test section
                var test = Plugin.ClientState.ClientLanguage;
                ImGui.TextUnformatted($"Client language is {test}");



                var doroImage = Plugin.TextureProvider.GetFromFile(doroImagePath).GetWrapOrDefault();
                if (doroImage != null)
                {
                    using (ImRaii.PushIndent(25f))
                    {
                        ImGui.Image(doroImage.Handle, doroImage.Size);
                        
                    }
                }
                else
                {
                    ImGui.TextUnformatted("Image not found.");
                }

                //try
                //{
                //    this.frames++;
                //    DrawTippy();
                //}

                //catch (Exception ex)
                //{
                //    Log.Error(ex, "Fools exception OnDraw caught");
                //}


                //chatGui.ChatMessage += ChatGui_ChatMessage;
            }
        }
    }



    private System.Threading.CancellationTokenSource? resetCancellation;

    //private void ChatGui_ChatMessage(Dalamud.Game.Text.XivChatType type, int timestamp, ref Dalamud.Game.Text.SeStringHandling.SeString sender, ref Dalamud.Game.Text.SeStringHandling.SeString message, ref bool isHandled)
    //{
    //    if (count == 0) 
    //    {
    //        Plugin.chatGui.Print($"[Invi] hi");
    //        Plugin.chatGui.Print($"{message}");
    //        count++;
    //    }
        
        
    //}
    private void ChatGui_ChatMessage(Dalamud.Game.Text.XivChatType type, int timestamp, ref Dalamud.Game.Text.SeStringHandling.SeString sender, ref Dalamud.Game.Text.SeStringHandling.SeString message, ref bool isHandled)
    {
        if (count == 0 && message.TextValue.ToLower().Contains("invisible"))
        {
            //Plugin.chatGui.Print($"[Invi] hi");
            Plugin.chatGui.Print($"{message.TextValue}");
            count++;
            // Start async reset
            _ = ResetCountAfterDelayAsync();
        }
        

    }

    private async Task ResetCountAfterDelayAsync()
    {
        // Cancel any existing reset operation
        resetCancellation?.Cancel();
        resetCancellation = new CancellationTokenSource();

        try
        {
            await Task.Delay(5000, resetCancellation.Token); // 5 seconds

            if (!resetCancellation.Token.IsCancellationRequested)
            {
                count = 0;
                Plugin.chatGui.Print($"[Invi] Count reset!");
            }
        }
        catch (OperationCanceledException)
        {
            // Timer was cancelled, do nothing
        }
    }

 





    /// <summary>
    /// ////holeeee sheeeet
    /// </summary>

    //private readonly Stopwatch tippyFrameTimer = new Stopwatch();
    //private readonly Stopwatch tippyLogicTimer = new Stopwatch();

    //private const float TippyScale = 1;
    //private long frames = 0;



    //private void DrawTippy()
    //{


    //    var displaySize = ImGui.GetIO().DisplaySize;

    //    var tippyPos = new Vector2(displaySize.X - 400, displaySize.Y - 350);

    //    ImGui.SetNextWindowPos(tippyPos, ImGuiCond.Always);
    //    ImGui.SetNextWindowSize(new Vector2(500, 500), ImGuiCond.Always);

    //    ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0, 0, 0, 0));

    //    ImGui.Begin("###TippyWindow", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoMouseInputs | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoBringToFrontOnFocus);


    //    var shouldDraw = this.tippyAnim != TippyAnimState.Idle;


    //    ImGui.SameLine();

    //    ImGui.SetCursorPosX(230);
    //    ImGui.SetCursorPosY(18 + 55);


    //    DrawTippyAnim();

    //    ImGui.End();

    //    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.6f, 0.6f, 0.6f, 1f));

    //    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0, 0, 0, 1));




    //}
    //private enum TippyAnimState
    //{

    //    Idle
    //}



    //private TippyAnimState tippyAnim = TippyAnimState.Idle;

    //private int startTippyFrame = 0;
    //private int endTippyFrame = 0;
    //private int FramesInTippyAnimation => this.endTippyFrame - this.startTippyFrame;

    //private int currentTippyFrame = 0;
    //private bool tippyAnimUp = true;
    //private bool tippyAnimDone = false;
    //private bool tippyLoopAnim = false;

    //private long currentFrameTime = 0;
    //private long minFrameTime = 150;

    //private readonly int tippySpritesheetW = 27; // amount in row + 1
    //private readonly int tippySpritesheetH = 27;
    //private readonly Vector2 tippySingleSize = new Vector2(124, 93);

    //private class TippyAnimData
    //{
    //    public TippyAnimData(int start, int stop)
    //    {
    //        Start = start;
    //        Stop = stop;
    //    }

    //    public int Start { get; set; }
    //    public int Stop { get; set; }
    //}

    //private readonly Dictionary<TippyAnimState, TippyAnimData> tippyAnimDatas =
    //    new Dictionary<TippyAnimState, TippyAnimData> {
    //            {TippyAnimState.Idle, new TippyAnimData(233, 267)},

    //    };




    //private Vector2 GetTippyTexCoords(int spriteIndex)
    //{
    //    var w = spriteIndex % this.tippySpritesheetW;
    //    var h = spriteIndex / this.tippySpritesheetH;

    //    return new Vector2(this.tippySingleSize.X * w, this.tippySingleSize.Y * h);
    //}

    //private void SetTippyAnim(TippyAnimState anim, bool loop)
    //{
    //    var animData = this.tippyAnimDatas[anim];

    //    this.startTippyFrame = animData.Start;
    //    this.endTippyFrame = animData.Stop;

    //    this.currentTippyFrame = 0;
    //    this.tippyAnim = anim;
    //    this.tippyAnimUp = true;
    //    this.tippyLoopAnim = loop;

    //}

    //private Vector2 ToSpriteSheetScale(Vector2 input) => new Vector2(input.X / this.tippySpriteSheet.Width, input.Y / this.tippySpriteSheet.Height);

    //private void DrawTippyAnim()
    //{
    //    var frameCoords = GetTippyTexCoords(this.startTippyFrame + this.currentTippyFrame);
    //    var botRight = ToSpriteSheetScale(frameCoords + this.tippySingleSize);

    //    if (this.currentTippyFrame > FramesInTippyAnimation - 2)
    //    {
    //        this.tippyAnimDone = true;
    //        if (!this.tippyLoopAnim)
    //            return;
    //        else
    //            this.tippyAnimUp = false;
    //    }

    //    if (this.currentTippyFrame == 0)
    //    {
    //        this.tippyAnimUp = true;
    //    }

    //    ImGui.Image(this.tippySpriteSheet.Handle, this.tippySingleSize * ImGui.GetIO().FontGlobalScale * TippyScale, ToSpriteSheetScale(frameCoords), botRight);

    //    this.currentFrameTime += this.tippyFrameTimer.ElapsedMilliseconds;
    //    this.tippyFrameTimer.Restart();

    //    if (this.currentFrameTime >= this.minFrameTime)
    //    {
    //        if (this.tippyAnimUp)
    //            this.currentTippyFrame++;
    //        else
    //            this.currentTippyFrame--;

    //        this.currentFrameTime -= this.minFrameTime;

    //        if (this.currentFrameTime >= this.minFrameTime)
    //            this.currentFrameTime = 0;
    //    }
    //}


}
