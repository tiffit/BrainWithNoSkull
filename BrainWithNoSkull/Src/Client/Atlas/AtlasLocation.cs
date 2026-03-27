namespace BrainWithNoSkull.Client.Atlas;

public record AtlasLocation(float U0, float V0, float U1, float V1) {

    public static AtlasLocation operator /(AtlasLocation loc, float div){
        return new AtlasLocation(loc.U0 / div, loc.V0 / div, loc.U1 / div, loc.V1 / div);
    }
}