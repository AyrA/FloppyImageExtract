using System;
using System.IO;
using System.Linq;

namespace FloppyImageExtract
{
    public static class CRC
    {
        public static uint GetCRC(Stream Source)
        {
            return GetCrc32(Source);
        }

        /// <summary>
        /// Sets the CRC of a byte array to 0xFFFFFFFF
        /// </summary>
        /// <param name="Source">Byte array to change CRC</param>
        public static uint FixCRC(byte[] Source)
        {
            uint NewCrc = uint.MaxValue;
            uint delta = 0;

            using (MemoryStream MS = new MemoryStream(Source, false))
            {
                delta = ReverseBits(NewCrc ^
                    BitConverter.ToUInt32(BitConverter.GetBytes(GetCrc32(MS)).Reverse().ToArray(),0)
                );
                MS.Seek(0, SeekOrigin.Begin);
                delta = (uint)MultiplyMod(ReciprocalMod(PowMod(2, 32UL)), delta);
            }
            return delta;
            //return NewCrc;
        }

        #region Utilities

        private const ulong POLYNOMIAL = 0x104C11DB7UL;

        private static uint GetCrc32(Stream f)
        {
            f.Seek(0, SeekOrigin.Begin);

            Crc32 c = new Crc32();
            return BitConverter.ToUInt32(c.ComputeHash(f), 0);

            /*
            uint crc = 0xFFFFFFFF;
            while (true)
            {
                byte[] buffer=new byte[32 * 1024];
                int n = f.Read(buffer,0,buffer.Length);

                if (n == 0)
                {
                    return ~crc;
                }
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        int bit = (buffer[i] >> j) & 1;
                        crc ^= (uint)bit << 31;
                        uint xor = crc >> 31;  // Boolean
                        crc = (uint)((crc & 0x7FFFFFFFF) << 1);
                        if (xor!=0)
                            crc = (uint)(crc ^ 0x104C11DB7);
                    }
                }
            }
            //*/
        }

        /// <summary>
        /// Reverses the bits in an integer
        /// </summary>
        public static uint ReverseBits(uint x)
        {
            uint result = 0;
            int i;
            for (i = 0; i < 32; i++)
                result = (result << 1) | ((x >> i) & 1);
            return result;
        }

        #endregion

        #region Polynominal arithmetic

        /// <summary>
        /// Returns polynomial x multiplied by polynomial y modulo the generator polynomial.
        /// </summary>
        private static ulong MultiplyMod(ulong x, ulong y)
        {
            // Russian peasant multiplication algorithm
            ulong z = 0;
            while (y != 0)
            {
                z ^= x * (y & 1UL);
                y >>= 1;
                x <<= 1;
                if ((x & 0x100000000UL) != 0)
                {
                    x ^= POLYNOMIAL;
                }
            }
            return z;
        }


        /// <summary>
        /// Returns polynomial x to the power of natural number y modulo the generator polynomial.
        /// </summary>
        private static ulong PowMod(ulong x, ulong y)
        {
            // Exponentiation by squaring
            ulong z = 1;
            while (y != 0)
            {
                if ((y & 1) != 0)
                    z = MultiplyMod(z, x);
                x = MultiplyMod(x, x);
                y >>= 1;
            }
            return z;
        }

        /// <summary>
        /// Computes polynomial x divided by polynomial y, returning the quotient and remainder.
        /// </summary>
        private static void DivideAndRemainder(ulong x, ulong y, out ulong q, out ulong r)
        {
            if (y == 0)
            {
                throw new ArgumentException("Division by zero", "y");
            }
            if (x == 0)
            {
                q = 0;
                r = 0;
                return;
            }

            int ydeg = GetDegree(y);
            ulong z = 0;
            int i;
            for (i = GetDegree(x) - ydeg; i >= 0; i--)
            {
                if ((x & (1UL << (i + ydeg))) != 0)
                {
                    x ^= y << i;
                    z |= 1UL << i;
                }
            }
            q = z;
            r = x;
        }

        /// <summary>
        /// Returns the reciprocal of polynomial x with respect to the generator polynomial.
        /// </summary>
        private static ulong ReciprocalMod(ulong x)
        {
            ulong y = x;
            x = POLYNOMIAL;
            ulong a = 0;
            ulong b = 1;
            while (y != 0)
            {
                ulong q, r;
                DivideAndRemainder(x, y, out q, out r);
                ulong c = a ^ MultiplyMod(q, b);
                x = y;
                y = r;
                a = b;
                b = c;
            }
            if (x == 1)
            {
                return a;
            }
            else
            {
                throw new Exception("Reciprocal does not exist\n");
            }
        }

        private static int GetDegree(ulong x)
        {
            int result = -1;
            while (x != 0)
            {
                x >>= 1;
                ++result;
            }
            return result;
        }

        #endregion
    }
}
