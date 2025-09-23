using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;


namespace Invi.Windows;

public class doroWindow : Window, IDisposable
{

    private Vector2? size;
    // We give this window a constant ID using ###.
    // This allows for labels to be dynamic, like "{FPS Counter}fps###XYZ counter window",
    // and the window ID will always be "###XYZ counter window" for ImGui
    public doroWindow(Plugin plugin) : base("Doro dorororo###With a constant ID",
        ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse|
        ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoDecoration)
    {



        Size = new Vector2(320, 320);
        SizeCondition = ImGuiCond.Always;





    }

    public void Dispose() {
 
    }

  

    public override void Draw()
    {
        GifLoader.EnsureTexturesUploaded();
        var giftobeload = "doro";
        if (GifLoader.IsLoaded(giftobeload))
        {
            var size = GifLoader.GetSize(giftobeload);
            GifLoader.Draw(giftobeload, new Vector2(size.Value.X, size.Value.Y));
        }
        else
        {
            ImGui.Text($"{giftobeload} is loading or missing!");
        }

    }
}
