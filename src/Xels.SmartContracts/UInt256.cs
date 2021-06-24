 
using System;
using System.Numerics;

namespace Xels.SmartContracts
{
    public struct UInt256 : IComparable
    {
        private const int WIDTH = 32;

        private UIntBase value;

        public static UInt256 Zero => 0;

        public static UInt256 MinValue => 0;

        public static UInt256 MaxValue => (BigInteger.One << 256) - 1;

        public UInt256(string hex)
        {
            value = new UIntBase(32, hex);
        }

        public static UInt256 Parse(string str)
        {
            return new UInt256(str);
        }

        public UInt256(BigInteger value)
        {
            this.value = new UIntBase(32, value);
        }

        public UInt256(byte[] vch, bool lendian = true)
        {
            value = new UIntBase(32, vch, lendian);
        }

        public static UInt256 operator >>(UInt256 a, int shift)
        {
            return new UInt256(a.value.ShiftRight(shift));
        }

        public static UInt256 operator <<(UInt256 a, int shift)
        {
            return new UInt256(a.value.ShiftLeft(shift));
        }

        public static UInt256 operator -(UInt256 a, UInt256 b)
        {
            return new UInt256(a.value.Subtract(b.value.GetValue()));
        }

        public static UInt256 operator +(UInt256 a, UInt256 b)
        {
            return new UInt256(a.value.Add(b.value.GetValue()));
        }

        public static UInt256 operator *(UInt256 a, UInt256 b)
        {
            return new UInt256(a.value.Multiply(b.value.GetValue()));
        }

        public static UInt256 operator /(UInt256 a, UInt256 b)
        {
            return new UInt256(a.value.Divide(b.value.GetValue()));
        }

        public static UInt256 operator %(UInt256 a, UInt256 b)
        {
            return new UInt256(a.value.Mod(b.value.GetValue()));
        }

        public UInt256(byte[] vch)
            : this(vch, lendian: true)
        {
        }

        public static bool operator <(UInt256 a, UInt256 b)
        {
            return UIntBase.Comparison(a.value, b.value) < 0;
        }

        public static bool operator >(UInt256 a, UInt256 b)
        {
            return UIntBase.Comparison(a.value, b.value) > 0;
        }

        public static bool operator <=(UInt256 a, UInt256 b)
        {
            return UIntBase.Comparison(a.value, b.value) <= 0;
        }

        public static bool operator >=(UInt256 a, UInt256 b)
        {
            return UIntBase.Comparison(a.value, b.value) >= 0;
        }

        public static bool operator ==(UInt256 a, UInt256 b)
        {
            return UIntBase.Comparison(a.value, b.value) == 0;
        }

        public static bool operator !=(UInt256 a, UInt256 b)
        {
            return !(a == b);
        }

        public static implicit operator UInt256(ulong value)
        {
            return new UInt256(value);
        }

        public static implicit operator UInt256(long value)
        {
            return new UInt256(value);
        }

        public static implicit operator UInt256(int value)
        {
            return new UInt256(value);
        }

        public static implicit operator UInt256(uint value)
        {
            return new UInt256(value);
        }

        public static implicit operator UInt256(BigInteger value)
        {
            return new UInt256(value);
        }

        public static implicit operator UInt256(UInt128 value)
        {
            return new UInt256(value);
        }

        public static implicit operator int(UInt256 value)
        {
            return (int)value.value.GetValue();
        }

        public static implicit operator uint(UInt256 value)
        {
            return (uint)value.value.GetValue();
        }

        public static implicit operator long(UInt256 value)
        {
            return (long)value.value.GetValue();
        }

        public static implicit operator ulong(UInt256 value)
        {
            return (ulong)value.value.GetValue();
        }

        public static implicit operator BigInteger(UInt256 value)
        {
            return value.value.GetValue();
        }

        public byte[] ToBytes()
        {
            return value.ToBytes();
        }

        public int CompareTo(object b)
        {
            return value.CompareTo(((UInt256)b).value);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return CompareTo(obj) == 0;
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }
}
#if false // Decompilation log
'266' items in cache
------------------
Resolve: 'System.Runtime, Version=4.2.1.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Runtime, Version=4.2.2.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '4.2.1.0', Got: '4.2.2.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\3.1.0\ref\netcoreapp3.1\System.Runtime.dll'
------------------
Resolve: 'System.Runtime.Numerics, Version=4.1.1.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Runtime.Numerics, Version=4.1.2.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '4.1.1.0', Got: '4.1.2.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\3.1.0\ref\netcoreapp3.1\System.Runtime.Numerics.dll'
------------------
Resolve: 'System.Runtime.Extensions, Version=4.2.1.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Runtime.Extensions, Version=4.2.2.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '4.2.1.0', Got: '4.2.2.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\3.1.0\ref\netcoreapp3.1\System.Runtime.Extensions.dll'
------------------
Resolve: 'System.Runtime, Version=4.2.2.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Runtime, Version=4.2.2.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\3.1.0\ref\netcoreapp3.1\System.Runtime.dll'
#endif
