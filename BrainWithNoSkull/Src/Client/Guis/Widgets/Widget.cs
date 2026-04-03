using BrainWithNoSkull.Client.Render.Data;
using Silk.NET.Input;

namespace BrainWithNoSkull.Client.Guis.Widgets;

public abstract class Widget {

    public int X;
    public int Y;
    public float Z;
    public int Width;
    public int Height;
    public Gui Parent;

    protected Widget(int x, int y, float z, int width, int height, Gui parent){
        X = x;
        Y = y;
        Z = z;
        Width = width;
        Height = height;
        Parent = parent;
    }

    public abstract void Render(VertexBuilder guiVb, int mouseX, int mouseY);

    public virtual bool OnMouseDown(MouseButton btn, int relX, int relY){
        return false;
    }

    public bool IsWithin(int x, int y){
        return x >= X && x < X + Width && y >= Y && y < Y + Height;
    }
}