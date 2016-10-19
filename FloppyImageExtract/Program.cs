using System;
using System.IO;
using System.Linq;

namespace FloppyImageExtract
{
    class Program
    {
        private const string FILENAME = @"C:\Windows\System32\diskcopy.dll";
        private const string OUTPUT_FILE= "BootImage.bin";

        private static readonly byte[] Search = new byte[]
        {
            //"···········FAT12···"
            0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
            0x20, 0x46, 0x41, 0x54, 0x31, 0x32, 0x20, 0x20, 0x20
        };


        private const int SEARCH_OFFSET = 43;

#if DEBUG
        private const bool DEBUG = true;
#else
        private const bool DEBUG = false;
#endif

        static void Main(string[] args)
        {
            if (DEBUG && File.Exists(OUTPUT_FILE))
            {
                File.Delete(OUTPUT_FILE);
            }

            if (!File.Exists(OUTPUT_FILE))
            {
                Console.WriteLine("Searching for Image...");
                byte[] Data = File.ReadAllBytes(FILENAME);
                int Offset = Tools.GetOffset(Data, Search, 0) - SEARCH_OFFSET;
                if (Offset >= 0 && Offset + Tools.IMAGESIZE < Data.Length)
                {
                    using (FileStream FS = File.Create(OUTPUT_FILE))
                    {
                        FS.Write(Data, Offset, Tools.IMAGESIZE);
                    }
                    Console.WriteLine("Done.");
                }
                else
                {
                    Console.WriteLine("Can't find floppy disk image");
                }
            }
            byte[] ImageData = File.ReadAllBytes(OUTPUT_FILE);

            using (MemoryStream Temp = new MemoryStream())
            {
                Temp.Write(ImageData, 0, ImageData.Length);

                var newCRC = CRC.FixCRC(ImageData);

                Console.WriteLine("Initial: {0}",Tools.b2s(BitConverter.GetBytes(CRC.GetCRC(Temp))));


                Temp.Seek(-4, SeekOrigin.End);
                for (int i = 0; i < 4; i++)
                {
                    byte current = (byte)Temp.ReadByte();
                    Temp.Seek(-1, SeekOrigin.Current);
                    current ^= (byte)((CRC.ReverseBits(newCRC) >> (i * 8)) & 0xFF);
                    Temp.WriteByte(current);
                }
                //Temp.Write(newCRC, 0, 4);
                Temp.Seek(0, SeekOrigin.Begin);
                Console.WriteLine("Current: {0}", Tools.b2s(BitConverter.GetBytes(CRC.GetCRC(Temp))));
                File.WriteAllBytes(OUTPUT_FILE, Temp.ToArray());
            }
#if DEBUG
            Console.Error.WriteLine("#END");
            Console.ReadKey(true);
#endif
        }
    }
}
