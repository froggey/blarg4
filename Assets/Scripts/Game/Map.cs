using System;
using System.Collections.Generic;

namespace Game {

public class Map {
    public int version { get; private set; }

    public static readonly DReal maxHeight = 127;
    public static readonly DReal minHeight = -128;

    sbyte[,] heightMap;

    // Version the cached checksum was computed against.
    int checksumVersion;
    uint cachedChecksum;

    public int width {
        get {
            return heightMap.GetLength(0);
        }
    }

    public int depth {
        get {
            return heightMap.GetLength(1);
        }
    }

    public DVector3 size {
        get {
            return new DVector3(width, 0, depth);
        }
    }

    public Map(int width, int depth) {
        version = 0;
        checksumVersion = -1;
        heightMap = new sbyte[width, depth];
        for(var z = 0; z < depth; z += 1) {
            for(var x = 0; x < width; x += 1) {
                heightMap[x,z] = 0;
            }
        }
    }

    public void ReadData(System.IO.BinaryReader stream) {
        for(var z = 0; z < depth; z += 1) {
            for(var x = 0; x < width; x += 1) {
                heightMap[x,z] = stream.ReadSByte();
            }
        }
        version += 1;
    }

    public void WriteData(System.IO.BinaryWriter stream) {
        for(var z = 0; z < depth; z += 1) {
            for(var x = 0; x < width; x += 1) {
                stream.Write(heightMap[x,z]);
            }
        }
    }

    public int RawHeight(int x, int z) {
        x = x % width;
        if(x < 0) {
            x += width;
        }
        z = z % depth;
        if(z < 0) {
            z += depth;
        }
        return heightMap[x, z];
    }

    public void SetRawHeight(int x, int z, int height) {
        x = x % width;
        if(x < 0) {
            x += width;
        }
        z = z % depth;
        if(z < 0) {
            z += depth;
        }
        if(height > maxHeight) {
            height = (int)maxHeight;
        } else if(height < minHeight) {
            height = (int)minHeight;
        }
        heightMap[x, z] = (sbyte)height;
        version += 1;
    }

    public DReal Height(DVector3 position) {
        var ix = (int)position.x;
        var iz = (int)position.z;

        // Corner heights.
        var ha = RawHeight(ix, iz);
        var hb = RawHeight(ix+1, iz);
        var hc = RawHeight(ix, iz+1);
        var hd = RawHeight(ix+1, iz+1);

        // Interpolation offsets.
        var tx = position.x - ix;
        var tz = position.z - iz;

        return DReal.BilinearLerp(ha, hb, hc, hd, tx, tz);
    }

    public DVector3 RawNormal(int x, int z) {
        x = x % width;
        if(x < 0) {
            x += width;
        }
        z = z % depth;
        if(z < 0) {
            z += depth;
        }
        var a = RawHeight(x+1,z);
        var b = RawHeight(x-1,z);
        var c = RawHeight(x,z+1);
        var d = RawHeight(x,z-1);
        return (new DVector3(b-a, 2, d-c)).normalized;
    }

    public DVector3 Normal(DVector3 position) {
        var ix = (int)position.x;
        var iz = (int)position.z;

        // Corner normals.
        var ha = RawNormal(ix, iz);
        var hb = RawNormal(ix+1, iz);
        var hc = RawNormal(ix, iz+1);
        var hd = RawNormal(ix+1, iz+1);

        // Interpolation offsets.
        var tx = position.x - ix;
        var tz = position.z - iz;

        return DVector3.BilinearLerp(ha, hb, hc, hd, tx, tz).normalized;
    }

    public DVector3 WrapPosition(DVector3 position) {
        return new DVector3(DReal.Repeat(position.x, width),
                            position.y,
                            DReal.Repeat(position.z, depth));
    }

    public DVector3 WrappedVectorSub(DVector3 lhs, DVector3 rhs) {
        var dx = lhs.x - rhs.x;
        var dy = lhs.y - rhs.y;
        var dz = lhs.z - rhs.z;
        if(dx > width/2) {
            dx = dx - width;
        } else if(dx < -width/2) {
            dx = dx + width;
        }
        if(dz > depth/2) {
            dz = dz - depth;
        } else if(dz < -depth/2) {
            dz = dz + depth;
        }
        return new DVector3(dx,dy,dz);
    }

    public DReal DistanceSqr(DVector3 a, DVector3 b) {
        return WrappedVectorSub(a,b).sqrMagnitude;
    }

    public DReal Distance(DVector3 a, DVector3 b) {
        return WrappedVectorSub(a,b).magnitude;
    }

    public DVector3 Direction(DVector3 origin, DVector3 dest) {
        return WrappedVectorSub(dest, origin).normalized;
    }

    public uint Checksum() {
        if(checksumVersion != version) {
            cachedChecksum = (uint)width ^ (uint)depth;
            for(int x = 0; x < heightMap.GetLength(0); x += 1) {
                for(int z = 0; z < heightMap.GetLength(1); z += 1) {
                    cachedChecksum ^= (uint)heightMap[x,z];
                }
            }
            checksumVersion = version;
        }
        return cachedChecksum;
    }
}

}
