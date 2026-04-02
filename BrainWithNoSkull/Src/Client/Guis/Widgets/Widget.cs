using BrainWithNoSkull.Client.Render.Data;

namespace BrainWithNoSkull.Client.Guis.Widgets;

public abstract class Widget {

    public int X;
    public int Y;
    public float Z;
    public int Width;
    public int Height;

    protected Widget(int x, int y, float z, int width, int height){
        X = x;
        Y = y;
        Z = z;
        Width = width;
        Height = height;
    }

    public abstract void Render(VertexBuilder guiVb, Gui parent, int mouseX, int mouseY);
}