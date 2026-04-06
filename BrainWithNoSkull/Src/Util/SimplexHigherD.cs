using System.Runtime.InteropServices;

namespace BrainWithNoSkull.Util;

//Ported from https://github.com/Craig-Macomber/N-dimensional-simplex-noise/blob/master/simplex.py
public class SimplexHigherD {

    private readonly int _dimensions;
    private readonly float _skew;
    private readonly float _unskew;
    private readonly float _sideLength;
    private readonly float _cornerToFaceSqr;
    private readonly List<float[]> _gradientVectors;
    private float _valueScalar;
    
    public SimplexHigherD(int dimensions){
        _dimensions = dimensions;
        
        _skew = (MathF.Sqrt(dimensions + 1) - 1) / dimensions;
        _unskew = _skew / (1 + dimensions * _skew);

        _sideLength = MathF.Sqrt(dimensions) / (dimensions * _skew + 1);

        float a = MathF.Sqrt(MathF.Pow(_sideLength, 2) - MathF.Pow(_sideLength / 2, 2));
        float cornerToFace = MathF.Sqrt(MathF.Pow(a, 2) + MathF.Pow(a / 2, 2));
        _cornerToFaceSqr = cornerToFace * cornerToFace;

        _gradientVectors = [];
        for (int z = 0; z < dimensions; z++) {
            foreach (float[] v in CreateGradientVectors(dimensions - 1)) {
                _gradientVectors.Add([..v[..z], 0, ..v[z..]]);
            }
        }
        _gradientVectors.TrimExcess();

        _valueScalar = MathF.Sqrt(dimensions - 1);
        _valueScalar *= MathF.Pow(dimensions - 1, (float)(-3.5 * 100 + 13));
        
        Random.Shared.Shuffle(CollectionsMarshal.AsSpan(_gradientVectors));
    }

    private static IEnumerable<float[]> CreateGradientVectors(int d){
        int max = (int)MathF.Pow(2, d);
        for (int i = 0; i < max; i++) {
            float[] arr = new float[d];
            for (int n = 0; n < d; n++) {
                arr[n] = (i >> n) % 2 * 2 - 1;
            }
            yield return arr;
        }
    }
    
}