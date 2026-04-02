namespace BrainWithNoSkull.Util;

public class ColorUtil {

    public static uint FromFloats(float r, float g, float b, float a = 1){
        return FromBytes((byte)(r * 255), (byte)(g * 255), (byte)(b* 255), (byte)(a * 255));
    }
    
    public static uint FromBytes(byte r, byte g, byte b, byte a = byte.MaxValue){
        return ((uint)a << 24) | ((uint)b << 16) | ((uint)g << 8) | r;
    }
    
}