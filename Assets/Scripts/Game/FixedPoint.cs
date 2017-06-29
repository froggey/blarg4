// You are standing at the gate to Gehennom.  Unspeakable cruelty and harm lurk down there.

using System;
using System.Text.RegularExpressions;

namespace Game {

// Fixed point numbers.

[Serializable]
[ProtoBuf.ProtoContract]
public struct DReal : IComparable<DReal> {
    private static int fixedShift = 16;

    public static DReal PI = Create(205887); // 3.14... * 2**16. Change this if fixedShift changes.
    public static DReal HalfPI = Create(102943); // 1.57... * 2**16. Change this if fixedShift changes.
    public static DReal TwoPI = Create(411774); // 1.57... * 2**16. Change this if fixedShift changes.
    public static DReal MaxValue = Create(long.MaxValue);
    public static DReal MinValue = Create(long.MinValue);

    public static DReal Half = ((DReal)1) / 2;

    [ProtoBuf.ProtoMember(1)]
    private long value;

    private static DReal Create(long value) {
        DReal result = new DReal();
        result.value = value;
        return result;
    }

    public static explicit operator DReal(float value) {
        double val_high_precision = value;
        val_high_precision *= 1 << fixedShift;
        return Create((long)val_high_precision);
    }

    public static implicit operator DReal(int value) {
        return Create((long)value << fixedShift);
    }

    public static implicit operator DReal(uint value) {
        return Create((long)((ulong)value << fixedShift));
    }

    public static explicit operator float(DReal value) {
        return (float)((double)value.value / (double)(1 << fixedShift));
    }

    public override string ToString() {
        return ((float)this).ToString();
    }

    public static DReal operator +(DReal lhs, DReal rhs) {
        return Create(lhs.value + rhs.value);
    }

    public static DReal operator -(DReal lhs, DReal rhs) {
        return Create(lhs.value - rhs.value);
    }

    public static DReal operator -(DReal num) {
        return 0 - num;
    }

    public static DReal operator *(DReal lhs, DReal rhs) {
        return Create((lhs.value * rhs.value) >> fixedShift);
    }

    public static DReal operator /(DReal lhs, DReal rhs) {
        return Create((lhs.value << fixedShift) / rhs.value);
    }

    public static DReal operator %(DReal lhs, DReal rhs) {
        return Create(lhs.value % rhs.value);
    }

    public static bool operator ==(DReal lhs, DReal rhs) {
        return lhs.value == rhs.value;
    }

        public static bool operator !=(DReal lhs, DReal rhs) {
            return lhs.value != rhs.value;
        }

            public static bool operator <(DReal lhs, DReal rhs) {
                return lhs.value < rhs.value;
            }

    public static bool operator <=(DReal lhs, DReal rhs) {
        return lhs.value <= rhs.value;
    }

        public static bool operator >(DReal lhs, DReal rhs) {
            return lhs.value > rhs.value;
        }

    public static bool operator >=(DReal lhs, DReal rhs) {
        return lhs.value >= rhs.value;
    }

        public static DReal operator <<(DReal num, int Amount) {
            return Create(num.value << Amount);
        }

    public static DReal operator >>(DReal num, int Amount) {
        return Create(num.value >> Amount);
    }

    public override bool Equals(System.Object obj) {
        // If parameter is null return false.
        if (obj == null) {
            return false;
        }

        // If parameter cannot be cast to Point return false.
        DReal p = (DReal)obj;

        // Return true if the fields match:
        return value == p.value;
    }

    public override int GetHashCode() {
        return (int)((value >> 32) ^ value);
    }

    // Floor remainder (% is truncate remainder).
    public static DReal Mod(DReal number, DReal divisor) {
        DReal rem = number % divisor;

        if(rem != 0 && (divisor < 0 ? number > 0 : number < 0)) {
            return rem + divisor;
        } else {
            return rem;
        }
    }

    public static DReal Sqrt(DReal f, int NumberOfIterations) {
        if(f.value < 0) {//NaN in Math.Sqrt
            throw new System.ArithmeticException("Input Error");
        }
        if(f.value == 0) {
            return 0;
        }

        DReal k = f + (DReal)1 >> 1;
        for(int i = 0; i < NumberOfIterations; i++) {
            k = (k + (f / k)) >> 1;
        }

        if(k.value < 0) {
            throw new System.ArithmeticException("Overflow");
        }

        return k;
    }

    public static DReal Abs(DReal n) {
        return (n < 0) ? -n : n;
    }

    public static DReal Sqrt(DReal f) {
        int numberOfIterations = 8;
        if(f > 100) {
            numberOfIterations = 12;
        }
        if(f > 1000) {
            numberOfIterations = 16;
        }
        return Sqrt(f, numberOfIterations);
    }

    // http://devmaster.net/forums/topic/4648-fast-and-accurate-sinecosine/
    public static DReal Sin(DReal x) {
        x = Mod((x + PI), TwoPI) - PI;
        DReal b = 4 / PI;
        DReal c = -4 / (PI * PI);
        DReal y = b * x + c * x * Abs(x);
        DReal p = Create(14746); // 0.225;
        return p * (y * Abs(y) - y) + y;
    }

    public static DReal Cos(DReal i) {
        return Sin(i + HalfPI);
    }

    public static DReal Tan(DReal i) {
        return Sin(i) / Cos(i);
    }

    public static DReal Atan(DReal F) {
        // http://forums.devshed.com/c-programming-42/implementing-an-atan-function-200106.html
        if(F == 0) {
            return 0;
        }
        if(F < 0) {
            return -Atan(-F);
        }
        // Caution: Magic.
        DReal x = (F - 1) / (F + 1);
        DReal y = x * x;
        DReal result = (Create(51471) + (((((((((((((((((Create(187) * y) - Create(1059)) * y) + Create(2812)) * y) - Create(4934)) * y) + Create(6983)) * y)
                                                - Create(9311))
                                               * y)
                                              + Create(13102))
                                             * y)
                                            - Create(21845))
                                           * y)
                                          + Create(65536))
                                         * x));
        //Debug.Log("Atan(" + F + ") = " + result + "  float: " + Mathf.Atan((float)F) + "  diff: " + Mathf.Abs(Mathf.Atan((float)F) - (float)result));
        return result;
    }

    public static DReal Atan2(DReal y, DReal x)
    {
        if(x > 0) return Atan(y/x);
        if(y >= 0 && x < 0) return Atan(y/x) + PI;
        if(y < 0 && x < 0) return Atan(y/x) - PI;
        if(y > 0 && x == 0) return HalfPI;
        if(y < 0 && x == 0) return -HalfPI;
        return 0;
    }

    public static DReal Radians(DReal degrees) {
        return degrees * (PI / 180);
    }

    public static DReal Degrees(DReal radians) {
        return radians * (180 / PI);
    }

    public static DReal Min(DReal a, DReal b) {
        return (a < b) ? a : b;
    }

    public static DReal Max(DReal a, DReal b) {
        return (a > b) ? a : b;
    }

    public int CompareTo(DReal other) {
        return this < other ? -1 : this > other ? 1 : 0;
    }

    public static DReal Clamp01(DReal value) {
        return Clamp(value, 0, 1);
    }

    public static DReal Clamp(DReal value, DReal min, DReal max) {
        if(value < min) {
            return min;
        } else if(value > max) {
            return max;
        } else {
            return value;
        }
    }

    public static DReal Repeat(DReal value, DReal length) {
        var val = value % length;
        if(val < 0) {
            val += length;
        }
        return val;
    }

    public static DReal Lerp(DReal a, DReal b, DReal t) {
        return a + (b - a) * t;
    }

    //  c (+0,+1)      d (+1,+1)
    //    +--x-------+
    //    |  | icd   |   Interpolate between a and b, producing iab.
    //    |  x       |   Interpolate between c and d, producing icd.
    //    |  | i     |   Interpolate between iab and icd, producing the interpolated result.
    //    |  |       |
    //    |  | iab   |
    //    +--x-------+
    //  a (+0,+0)      b (+1,+0)
    public static DReal BilinearLerp(DReal a, DReal b, DReal c, DReal d, DReal x, DReal y) {
        var iab = Lerp(a, b, x);
        var icd = Lerp(c, d, x);

        return Lerp(iab, icd, y);
    }

    public static DReal Parse(string stringValue) {
        var sign = 1;
        if(stringValue[0] == '-') {
            sign = -1;
            stringValue = stringValue.Substring(1, stringValue.Length);
        }
        var match = Regex.Match(stringValue, @"^[0-9]+$");
        if (match.Success) {
            return int.Parse(stringValue) * sign;
        }

        match = Regex.Match(stringValue, @"^([0-9]+)\.([0-9]+)$");
        if (match.Success) {
            return (DReal)int.Parse(match.Groups[1].Value + match.Groups[2].Value) / (int)Math.Pow(10, match.Groups[2].Length) * sign;
        }

        match = Regex.Match(stringValue, @"^([0-9]+)/([0-9]+)$");
        if (match.Success) {
            return (DReal)int.Parse(match.Groups[1].Value) / (DReal)int.Parse(match.Groups[2].Value) * sign;
        }

        throw new FormatException(stringValue + " is an invalid DReal. Valid formats are '123' or '123.45' or '123/45'.");
    }

    public uint Checksum() {
        return (uint)value ^ ((uint)(value >> 32));
    }
}

[Serializable]
[ProtoBuf.ProtoContract]
public struct DVector2 {
    [ProtoBuf.ProtoMember(1)]
    public DReal x;
    [ProtoBuf.ProtoMember(2)]
    public DReal y;

    public DVector2(DReal x, DReal y) {
        this.x = x;
        this.y = y;
    }

    public override string ToString() {
        return "(" + x + ", " + y + ")";
    }

    public DReal magnitude {
        get { return DReal.Sqrt(x * x + y * y); }
    }
    public DReal sqrMagnitude {
        get { return x * x + y * y; }
    }
    public DVector2 normalized {
        get {
            DReal length = this.magnitude;
            if(length == 0) {
                return new DVector2(0,0);
            } else {
                return this / length;
            }
        }
    }

    public static DVector2 operator +(DVector2 lhs, DVector2 rhs) {
        return new DVector2(lhs.x + rhs.x, lhs.y + rhs.y);
    }

    public static DVector2 operator -(DVector2 lhs, DVector2 rhs) {
        return new DVector2(lhs.x - rhs.x, lhs.y - rhs.y);
    }

    public static DVector2 operator *(DVector2 lhs, DReal rhs) {
        return new DVector2(lhs.x * rhs, lhs.y * rhs);
    }

    public static DVector2 operator /(DVector2 lhs, DReal rhs) {
        return new DVector2(lhs.x / rhs, lhs.y / rhs);
    }

    public static DVector2 FromAngle(DReal radians) {
        return new DVector2(DReal.Cos(radians), DReal.Sin(radians));
    }

    public static DReal ToAngle(DVector2 vector) {
        return DReal.Atan2(vector.y, vector.x);
    }

    public static DReal Dot(DVector2 a, DVector2 b) {
        return a.x * b.x + a.y * b.y;
    }

    public static DVector2 Lerp(DVector2 a, DVector2 b, DReal t) {
        return a + (b - a) * t;
    }

    //  c (+0,+1)      d (+1,+1)
    //    +--x-------+
    //    |  | icd   |   Interpolate between a and b, producing iab.
    //    |  x       |   Interpolate between c and d, producing icd.
    //    |  | i     |   Interpolate between iab and icd, producing the interpolated result.
    //    |  |       |
    //    |  | iab   |
    //    +--x-------+
    //  a (+0,+0)      b (+1,+0)
    public static DVector2 BilinearLerp(DVector2 a, DVector2 b, DVector2 c, DVector2 d, DReal x, DReal y) {
        var iab = Lerp(a, b, x);
        var icd = Lerp(c, d, x);

        return Lerp(iab, icd, y);
    }

    public uint Checksum() {
        return x.Checksum() ^ y.Checksum();
    }
}

[Serializable]
[ProtoBuf.ProtoContract]
public struct DVector3 {
    [ProtoBuf.ProtoMember(1)]
    public DReal x;
    [ProtoBuf.ProtoMember(2)]
    public DReal y;
    [ProtoBuf.ProtoMember(3)]
    public DReal z;

    public DVector3(DReal x, DReal y, DReal z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public override string ToString() {
        return "(" + x + ", " + y + ", " + z + ")";
    }

    public DReal magnitude {
        get { return DReal.Sqrt(sqrMagnitude); }
    }
    public DReal sqrMagnitude {
        get { return x * x + y * y + z * z; }
    }
    public DVector3 normalized {
        get {
            DReal length = this.magnitude;
            if(length == 0) {
                return new DVector3(0,0,0);
            } else {
                return this / length;
            }
        }
    }

    public static DVector3 operator +(DVector3 lhs, DVector3 rhs) {
        return new DVector3(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z);
    }

    public static DVector3 operator -(DVector3 lhs, DVector3 rhs) {
        return new DVector3(lhs.x - rhs.x, lhs.y - rhs.y, lhs.z - rhs.z);
    }

    public static DVector3 operator *(DVector3 lhs, DReal rhs) {
        return new DVector3(lhs.x * rhs, lhs.y * rhs, lhs.z * rhs);
    }

    public static DVector3 operator /(DVector3 lhs, DReal rhs) {
        return new DVector3(lhs.x / rhs, lhs.y / rhs, lhs.z / rhs);
    }

    public static bool operator ==(DVector3 lhs, DVector3 rhs) {
        return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z;
    }

    public static bool operator !=(DVector3 lhs, DVector3 rhs) {
        return !(lhs == rhs);
    }

    public override bool Equals(System.Object obj) {
        // If parameter is null return false.
        if (obj == null) {
            return false;
        }

        // If parameter cannot be cast to Point return false.
        DVector3 p = (DVector3)obj;

        // Return true if the fields match:
        return this == p;
    }

    public override int GetHashCode() {
        return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode();
    }

    public static DReal Dot(DVector3 a, DVector3 b) {
        return a.x * b.x + a.y * b.y + a.z * b.z;
    }

    public static DVector3 Lerp(DVector3 a, DVector3 b, DReal t) {
        return a + (b - a) * t;
    }

    //  c (+0,+1)      d (+1,+1)
    //    +--x-------+
    //    |  | icd   |   Interpolate between a and b, producing iab.
    //    |  x       |   Interpolate between c and d, producing icd.
    //    |  | i     |   Interpolate between iab and icd, producing the interpolated result.
    //    |  |       |
    //    |  | iab   |
    //    +--x-------+
    //  a (+0,+0)      b (+1,+0)
    public static DVector3 BilinearLerp(DVector3 a, DVector3 b, DVector3 c, DVector3 d, DReal x, DReal y) {
        var iab = Lerp(a, b, x);
        var icd = Lerp(c, d, x);

        return Lerp(iab, icd, y);
    }

    public uint Checksum() {
        return x.Checksum() ^ y.Checksum() ^ z.Checksum();
    }

    public static explicit operator UnityEngine.Vector3(DVector3 value) {
        return new UnityEngine.Vector3((float)value.x, (float)value.y, (float)value.z);
    }

    public static explicit operator DVector3(UnityEngine.Vector3 value) {
        return new DVector3((DReal)value.x, (DReal)value.y, (DReal)value.z);
    }
}

}
