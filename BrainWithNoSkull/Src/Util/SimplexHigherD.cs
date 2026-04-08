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
        _valueScalar *= MathF.Pow(dimensions - 1, -3.5f) * 100 + 13;
        
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

    public unsafe float Noise(long seed, float[] loc){
        float s = loc.Sum() * _skew;
        int[] skewedLocs = loc.Select(f => OpenSimplex2.FastFloor(f + s)).ToArray();
        float t = skewedLocs.Sum() * _unskew;
        
        float[] cellDist = new float[_dimensions];
        for (int i = 0; i < _dimensions; i++) {
            cellDist[i] = loc[i] - skewedLocs[i] + t;
        }

        Tuple<float, int>[] pairedOrders = new Tuple<float, int>[_dimensions];
        for (var i = 0; i < _dimensions; i++) {
            pairedOrders[i] = new Tuple<float, int>(cellDist[i], i);
        }

        int[] distOrder = pairedOrders
            .OrderBy(tuple => tuple.Item1)
            .ThenBy(tuple => tuple.Item2)
            .Select(tuple => tuple.Item2).Reverse().ToArray();

        List<int> skewedLocList = new List<int>(skewedLocs);

        float n = 0.0f;
        float skewOffset = 0.0f;

        int[] iter = [-1, ..distOrder];
        foreach(int v in iter) {
            if (v != -1) {
                skewedLocList[v] += 1;
            }

            float[] u = new float[_dimensions];
            for (int i = 0; i < _dimensions; i++) {
                u[i] = cellDist[i] - (skewedLocList[i] - skewedLocs[i]) + skewOffset;
            }

            t = _cornerToFaceSqr;
            
            foreach (float a in u) {
                t -= a * a;
            }

            if (t > 0) {
                long index = seed;
                for (int i = 0; i < _dimensions; i++) {
                    index += skewedLocList[i] << (5*i % 16);
                }

                float[] vec = _gradientVectors[(int)(RandHash((byte*)&index) % _gradientVectors.Count)];

                float dot = 0;
                for (int i = 0; i < _dimensions; i++) {
                    dot += vec[i] * u[i];
                }

                float t4 = t * t * t * t;
                n += dot * t4;
            }

            skewOffset += _unskew;
        }

        n *= _valueScalar;
        n /= _dimensions;

        return n;
    }

    private static unsafe uint RandHash(byte* index){
        uint hash = 0;

        for (uint i = 0; i < 8; i++) {
            hash += index[i];
            hash += hash << 10;
            hash ^= hash >> 6;
        }
        
        hash += hash << 3;
        hash ^= hash >> 11;
        hash += hash << 15;
        return hash;
    }
    
}