#region Assembly Stratis.SmartContracts, Version=1.4.1.0, Culture=neutral, PublicKeyToken=null
// C:\Users\fazle\.nuget\packages\stratis.smartcontracts\1.4.1-alpha\lib\netcoreapp2.1\Stratis.SmartContracts.dll
// Decompiled with ICSharpCode.Decompiler 6.1.0.5902
#endregion

using System;
using System.Numerics;

namespace Xels.SmartContracts
{
    public struct UInt128 : IComparable
    {
        private const int WIDTH = 16;

        private UIntBase value;

        public static UInt128 Zero => 0;

        public static UInt128 MinValue => 0;

        public static UInt128 MaxValue => (BigInteger.One << 128) - 1;

        public UInt128(string hex)
        {
            value = new UIntBase(16, hex);
        }

        public static UInt128 Parse(string str)
        {
            return new UInt128(str);
        }

        public UInt128(BigInteger value)
        {
            this.value = new UIntBase(16, value);
        }

        public UInt128(byte[] vch, bool lendian = true)
        {
            value = new UIntBase(16, vch, lendian);
        }

        public static UInt128 operator >>(UInt128 a, int shift)
        {
            return new UInt128(a.value.ShiftRight(shift));
        }

        public static UInt128 operator <<(UInt128 a, int shift)
        {
            return new UInt128(a.value.ShiftLeft(shift));
        }

        public static UInt128 operator -(UInt128 a, UInt128 b)
        {
            return new UInt128(a.value.Subtract(b.value.GetValue()));
        }

        public static UInt128 operator +(UInt128 a, UInt128 b)
        {
            return new UInt128(a.value.Add(b.value.GetValue()));
        }

        public static UInt128 operator *(UInt128 a, UInt128 b)
        {
            return new UInt128(a.value.Multiply(b.value.GetValue()));
        }

        public static UInt128 operator /(UInt128 a, UInt128 b)
        {
            return new UInt128(a.value.Divide(b.value.GetValue()));
        }

        public static UInt128 operator %(UInt128 a, UInt128 b)
        {
            return new UInt128(a.value.Mod(b.value.GetValue()));
        }

        public UInt128(byte[] vch)
            : this(vch, lendian: true)
        {
        }

        public static bool operator <(UInt128 a, UInt128 b)
        {
            return UIntBase.Comparison(a.value, b.value) < 0;
        }

        public static bool operator >(UInt128 a, UInt128 b)
        {
            return UIntBase.Comparison(a.value, b.value) > 0;
        }

        public static bool operator <=(UInt128 a, UInt128 b)
        {
            return UIntBase.Comparison(a.value, b.value) <= 0;
        }

        public static bool operator >=(UInt128 a, UInt128 b)
        {
            return UIntBase.Comparison(a.value, b.value) >= 0;
        }

        public static bool operator ==(UInt128 a, UInt128 b)
        {
            return UIntBase.Comparison(a.value, b.value) == 0;
        }

        public static bool operator !=(UInt128 a, UInt128 b)
        {
            return !(a == b);
        }

        public static implicit operator UInt128(ulong value)
        {
            return new UInt128(value);
        }

        public static implicit operator UInt128(long value)
        {
            return new UInt128(value);
        }

        public static implicit operator UInt128(int value)
        {
            return new UInt128(value);
        }

        public static implicit operator UInt128(uint value)
        {
            return new UInt128(value);
        }

        public static implicit operator UInt128(BigInteger value)
        {
            return new UInt128(value);
        }

        public static implicit operator UInt128(UInt256 value)
        {
            return new UInt128(value);
        }

        public static implicit operator int(UInt128 value)
        {
            return (int)value.value.GetValue();
        }

        public static implicit operator uint(UInt128 value)
        {
            return (uint)value.value.GetValue();
        }

        public static implicit operator long(UInt128 value)
        {
            return (long)value.value.GetValue();
        }

        public static implicit operator ulong(UInt128 value)
        {
            return (ulong)value.value.GetValue();
        }

        public static implicit operator BigInteger(UInt128 value)
        {
            return value.value.GetValue();
        }

        public byte[] ToBytes()
        {
            return value.ToBytes();
        }

        public int CompareTo(object b)
        {
            return value.CompareTo(((UInt128)b).value);
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
