using System;
using System.IO;
using System.Text;

namespace FloppyImageExtract
{
    class Tools
    {
        public const int BYTES_PER_SECTOR = 512;
        public const int SECTORS_PER_TRACK = 18;
        public const int TRACKS_PER_SIDE = 80;
        public const int SIDES_PER_DISK = 2;
        public const int IMAGESIZE =
            BYTES_PER_SECTOR *
            SECTORS_PER_TRACK *
            TRACKS_PER_SIDE *
            SIDES_PER_DISK;
        //IMAGESIZE=0x168000=1474560

        public const int NOT_FOUND = -1;

        public static string b2s(byte[] Data)
        {
            StringBuilder SB = new StringBuilder(Data.Length * 2);
            foreach (byte b in Data)
            {
                SB.AppendFormat("{0:X2}", b);
            }
            return SB.ToString();
        }

        public static int GetOffset(byte[] Source, byte[] Search, int Start)
        {
            for (var i = Start; i < Source.Length - Search.Length; i++)
            {
                for (var j = 0; j <= Search.Length; j++)
                {
                    //if we reach this point, the content was found
                    if (j == Search.Length)
                    {
                        return i;
                    }
                    else
                    {
                        //if the content at this position does not matches,
                        //abort the loop
                        if (Source[i + j] != Search[j])
                        {
                            break;
                        }
                    }
                }
            }
            return NOT_FOUND;
        }
    }
}
