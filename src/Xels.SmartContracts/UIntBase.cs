#region Assembly Stratis.SmartContracts, Version=1.4.1.0, Culture=neutral, PublicKeyToken=null
// C:\Users\fazle\.nuget\packages\stratis.smartcontracts\1.4.1-alpha\lib\netcoreapp2.1\Stratis.SmartContracts.dll
// Decompiled with ICSharpCode.Decompiler 6.1.0.5902
#endregion

using System;
using System.Globalization;
using System.Numerics;

namespace Xels.SmartContracts
{
    internal struct UIntBase : IComparable
    {
        private int width;

        private BigInteger value;

        public UIntBase(int width) : this()
        {
            if ((width & 3) != 0)
            {
                throw new ArgumentException("The 'width' must be a multiple of 4.");
            }

            this.width = width;
        }

        public UIntBase(int width, BigInteger value)
            : this(width)
        {
            SetValue(value);
        }

        public UIntBase(int width, UIntBase value)
            : this(width)
        {
            SetValue(value.value);
        }

        public UIntBase(int width, ulong b)
            : this(width)
        {
            SetValue(new BigInteger(b));
        }

        public UIntBase(int width, byte[] vch, bool lendian = true)
            : this(width)
        {
            if (vch.Length > this.width)
            {
                throw new FormatException($"The byte array should be {this.width} bytes or less.");
            }

            SetValue(new BigInteger(vch, isUnsigned: true, !lendian));
        }

        public UIntBase(int width, string str)
            : this(width)
        {
            if (str.StartsWith("0x"))
            {
                SetValue(BigInteger.Parse("0" + str.Substring(2), NumberStyles.HexNumber));
            }
            else
            {
                SetValue(BigInteger.Parse(str));
            }
        }

        public UIntBase(int width, uint[] array)
            : this(width)
        {
            int num = this.width / 4;
            if (array.Length != num)
            {
                throw new FormatException($"The array length should be {num}.");
            }

            byte[] array2 = new byte[this.width];
            for (int i = 0; i < num; i++)
            {
                BitConverter.GetBytes(array[i]).CopyTo(array2, i * 4);
            }

            SetValue(new BigInteger(array2, isUnsigned: true));
        }

        private bool TooBig(byte[] bytes)
        {
            if (bytes.Length <= width)
            {
                return false;
            }

            if (bytes.Length == width + 1 && bytes[width] == 0)
            {
                return false;
            }

            return true;
        }

        private void SetValue(BigInteger value)
        {
            if (value.Sign < 0)
            {
                throw new OverflowException("Only positive or zero values are allowed.");
            }

            if (TooBig(value.ToByteArray()))
            {
                throw new OverflowException();
            }

            this.value = value;
        }

        public BigInteger GetValue()
        {
            return value;
        }

        private uint[] ToUIntArray()
        {
            byte[] array = ToBytes();
            int num = width / 4;
            uint[] array2 = new uint[num];
            for (int i = 0; i < num; i++)
            {
                array2[i] = BitConverter.ToUInt32(array, i * 4);
            }

            return array2;
        }

        public byte[] ToBytes(bool lendian = true)
        {
            byte[] array = value.ToByteArray();
            byte[] array2 = new byte[width];
            Array.Copy(array, array2, Math.Min(array.Length, array2.Length));
            if (!lendian)
            {
                Array.Reverse(array2);
            }

            return array2;
        }

        internal BigInteger ShiftRight(int shift)
        {
            return value >> shift;
        }

        internal BigInteger ShiftLeft(int shift)
        {
            return value << shift;
        }

        internal BigInteger Add(BigInteger value2)
        {
            return value + value2;
        }

        internal BigInteger Subtract(BigInteger value2)
        {
            if (value.CompareTo(value2) < 0)
            {
                throw new OverflowException("Result cannot be negative.");
            }

            return value - value2;
        }

        internal BigInteger Multiply(BigInteger value2)
        {
            return value * value2;
        }

        internal BigInteger Divide(BigInteger value2)
        {
            return value / value2;
        }

        internal BigInteger Mod(BigInteger value2)
        {
            return value % value2;
        }

        public int CompareTo(object b)
        {
            return value.CompareTo(((UIntBase)b).value);
        }

        public static int Comparison(UIntBase a, UIntBase b)
        {
            return a.CompareTo(b);
        }

        public override int GetHashCode()
        {
            uint[] array = ToUIntArray();
            uint num = 0u;
            for (int i = 0; i < array.Length; i++)
            {
                num ^= array[i];
            }

            return (int)num;
        }

        public override bool Equals(object obj)
        {
            return CompareTo(obj) == 0;
        }

        private static string ByteArrayToString(byte[] ba)
        {
            return BitConverter.ToString(ba).Replace("-", "");
        }

        public string ToHex()
        {
            return ByteArrayToString(ToBytes(lendian: false)).ToLower();
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }
}
#if false // Decompilation log
'256' items in cache
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
