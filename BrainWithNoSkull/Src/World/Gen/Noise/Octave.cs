using System.Collections;
using BrainWithNoSkull.Util;

namespace BrainWithNoSkull.World.Gen.Noise;

public class Octave {

    private readonly long _seed;
    private readonly float _scale;
    private readonly float _magnitude;
    
    public Octave(long seed, float scale, float magnitude){
        _seed = seed + (int)(scale * 100);
        _scale = scale;
        _magnitude = magnitude;
    }
    
    public float GetValue(float x, float z, uint worldSize){
        double angleX = MathF.PI * 2 * x / (worldSize * Constants.ChunkSize);
        double angleZ = MathF.PI * 2 * z / (worldSize * Constants.ChunkSize);
        double scale = worldSize / (_scale * 8);
        double noiseX = Math.Cos(angleX) * scale;
        double noiseY = Math.Sin(angleX) * scale;
        double noiseZ = Math.Cos(angleZ) * scale;
        double noiseW = Math.Sin(angleZ) * scale;
        
        return _magnitude * OpenSimplex2.Noise4_ImproveXYZ(_seed, noiseX, noiseY, noiseZ, noiseW);
    }
    
}

public class Octaves : IEnumerable {
    private readonly List<Octave> _octaves = [];

    public int GetHeight(float x, float z, uint worldSize, int seaLevel){
        float result = seaLevel;
        foreach (Octave octave in _octaves) {
            result += octave.GetValue(x, z, worldSize);
        }
        return (int)result;
    }

    public void Add(Octave octave){
        _octaves.Add(octave);
    }

    public IEnumerator GetEnumerator(){
        return _octaves.GetEnumerator();
    }
}