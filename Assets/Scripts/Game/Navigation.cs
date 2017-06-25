using System;
using System.Collections.Generic;

namespace Game {

public class Navigation {
    public const int impassibleTag = -1;

    public readonly Map map;

    // Slopes are from 0 (90 degree slope, parallel to up vector)
    // to 1 (0 degree slope, perpendicular to up vector).
    DReal slopeLimit = (DReal)8 / 10; // 25 degrees, ish. Use cos to convert angle to slope.

    public const int granularity = 16;
    int[,] reachability;

    uint checksum;

    public Navigation(Map map) {
        this.map = map;
    }

    public void ReadData(System.IO.BinaryReader stream) {
        reachability = new int[map.width / granularity,map.depth / granularity];
        for(var z = 0; z < reachability.GetLength(1); z += 1) {
            for(var x = 0; x < reachability.GetLength(0); x += 1) {
                reachability[x,z] = stream.ReadInt32();
            }
        }
    }

    public void WriteData(System.IO.BinaryWriter stream) {
        for(var z = 0; z < reachability.GetLength(1); z += 1) {
            for(var x = 0; x < reachability.GetLength(0); x += 1) {
                stream.Write(reachability[x,z]);
            }
        }
    }

    public int Reachability(DVector3 point) {
        point += new DVector3(1, 1, 1) * (DReal)granularity / 2;
        point = map.WrapPosition(point);
        return reachability[(int)point.x / granularity, (int)point.z / granularity];
    }

    // Directly examine the map to determine if a tile is passible.
    bool MapPassable(int x, int z) {
        for(int xoff = 0; xoff < granularity; xoff += 1) {
            for(int zoff = 0; zoff < granularity; zoff += 1) {
                var pos = new DVector3(x + xoff + DReal.Half, 0, z + zoff + DReal.Half);
                var normal = map.Normal(pos);
                var up = new DVector3(0,1,0);
                var slope = DVector3.Dot(normal, up);
                if(slope < slopeLimit) {
                    // Too sloped.
                    return false;
                }
                if(map.Height(pos) < 0) {
                    // Underwater.
                    return false;
                }
            }
        }
        return true;
    }

    // Rebuild the reachability array.
    public void ComputeReachability() {
        reachability = new int[map.width / granularity,map.depth / granularity];
        // Walk every element in the map, and flood-fill from any with a 0 region.
        var next_reachability_tag = 1;
        for(var z = 0; z < map.depth / granularity; z += 1) {
            for(var x = 0; x < map.width / granularity; x += 1) {
                if(reachability[x,z] != 0) {
                    // Already visited.
                    continue;
                }
                if(!MapPassable(x * granularity, z * granularity)) {
                    reachability[x,z] = impassibleTag;
                    continue;
                }
                var watch = new System.Diagnostics.Stopwatch();
                watch.Start();
                ComputeReachabilityFrom(x * granularity, z * granularity, next_reachability_tag);
                watch.Stop();
                UnityEngine.Debug.Log(string.Format("Took {0}s to build reachability region {1}", watch.Elapsed, next_reachability_tag));
                next_reachability_tag += 1;
            }
        }

        // Recalculate the checksum.
        checksum = map.Checksum();
        for(int x = 0; x < reachability.GetLength(0); x += 1) {
            for(int z = 0; z < reachability.GetLength(1); z += 1) {
                checksum ^= (uint)reachability[x,z];
            }
        }
    }

    void ComputeReachabilityFrom(int x, int z, int tag) {
        var worklist = new List<DVector3>();
        worklist.Add(new DVector3(x, 0, z));

        while(worklist.Count != 0) {
            var current = worklist[worklist.Count-1];
            worklist.RemoveAt(worklist.Count-1);
            reachability[(int)current.x / granularity, (int)current.z / granularity] = tag;
            foreach(var n in neighbors(current)) {
                var nr = reachability[(int)n.x / granularity, (int)n.z / granularity];
                if(nr == impassibleTag) {
                    // Visited and impassible, ignore.
                    continue;
                } else if(nr == tag) {
                    // Visited and part of this region, ignore.
                    continue;
                } else if(nr != 0) {
                    // Visited and part of some other region.
                    // Impossible, implies that two reachability regions are adjacent.
                    throw new Exception("Fuuuuck!");
                }
                if(!MapPassable((int)n.x, (int)n.z)) {
                    // Impassible. Mark as such & ignore.
                    reachability[(int)n.x / granularity, (int)n.z / granularity] = impassibleTag;
                    continue;
                }
                worklist.Add(n);
            }
        }
    }

    DVector3 remove_best(List<DVector3> open_set, Dictionary<DVector3, DReal> f_score) {
        var best_index = 0;
        var best_score = f_score[open_set[0]];
        for(var i = 0; i < open_set.Count; i += 1) {
            if(f_score[open_set[i]] < best_score) {
                best_score = f_score[open_set[i]];
                best_index = i;
            }
        }
        var result = open_set[best_index];
        open_set.RemoveAt(best_index);
        return result;
    }

    bool passable(DVector3 p) {
        return Reachability(p) != impassibleTag;
    }

    public List<DVector3> BuildPath(DVector3 origin, DVector3 dest) {
        var orig_dest = dest;
        origin = new DVector3(((int)origin.x / granularity) * granularity, 0, ((int)origin.z / granularity) * granularity);
        dest = new DVector3(((int)dest.x / granularity) * granularity, 0, ((int)dest.z / granularity) * granularity);

        var open_set = new List<DVector3>();
        var closed_set = new HashSet<DVector3>();
        var g_score = new Dictionary<DVector3, DReal>();
        var f_score = new Dictionary<DVector3, DReal>();
        var came_from = new Dictionary<DVector3, DVector3>();

        var ops = 0;

        var origin_reach = Reachability(origin);
        var dest_reach = Reachability(dest);

        UnityEngine.Debug.Log(string.Format("Gen path from {0}/{1} to {2}/{3}", origin, origin_reach, dest, dest_reach));

        if(!passable(dest) || origin_reach != dest_reach) {
            return null;
        }

        open_set.Add(origin);
        g_score[origin] = 0;
        f_score[origin] = map.DistanceSqr(origin, dest);
        while(open_set.Count != 0) {
            ops += 1;
            if(ops > 1000) {
                UnityEngine.Debug.Log("Path too long!");
                return null;
            }
            var current = remove_best(open_set, f_score);
            if(current == dest) {
                var path = new List<DVector3>();
                while(came_from.ContainsKey(current)) {
                    // Try to path through the middle of the map square.
                    path.Add(current + new DVector3(1, 1, 1) * (DReal)granularity / 2);
                    current = came_from[current];
                }
                path.Reverse();
                if(path.Count != 0) {
                    path.RemoveAt(path.Count-1);
                }
                path.Add(orig_dest);
                return path;
            }
            closed_set.Add(current);
            foreach(var n in neighbors(current)) {
                if(!passable(n)) {
                    continue;
                }
                if(closed_set.Contains(n)) {
                    continue;
                }
                if(!open_set.Contains(n)) {
                    open_set.Add(n);
                    g_score.Add(n, DReal.MaxValue);
                    f_score.Add(n, DReal.MaxValue);
                }
                var tentative_g_score = g_score[current] + map.DistanceSqr(current, n);
                if(tentative_g_score >= g_score[n]) {
                    continue;
                }
                came_from[n] = current;
                g_score[n] = tentative_g_score;
                f_score[n] = tentative_g_score + map.DistanceSqr(dest, n);
            }
        }
        return null;
    }

    IEnumerable<DVector3> neighbors(DVector3 point) {
        yield return map.WrapPosition(point + new DVector3( 0, 0, +1) * granularity);
        yield return map.WrapPosition(point + new DVector3( 0, 0, -1) * granularity);
        yield return map.WrapPosition(point + new DVector3(+1, 0,  0) * granularity);
        yield return map.WrapPosition(point + new DVector3(-1, 0,  0) * granularity);
        yield return map.WrapPosition(point + new DVector3(+1, 0, +1) * granularity);
        yield return map.WrapPosition(point + new DVector3(+1, 0, -1) * granularity);
        yield return map.WrapPosition(point + new DVector3(-1, 0, +1) * granularity);
        yield return map.WrapPosition(point + new DVector3(-1, 0, -1) * granularity);
    }

    public uint Checksum() {
        return checksum;
    }
}

}
