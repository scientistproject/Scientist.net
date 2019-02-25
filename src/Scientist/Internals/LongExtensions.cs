namespace GitHub.Internals
{
    public static class LongExtensions
    {
        public const string Bytes     = "B";
        public const string KiloBytes = "KB";
        public const string MegaBytes = "MB";
        public const string GigaBytes = "GB";
        public const string TeraBytes = "TB";
        public const string PetaBytes = "PB";

        public static string ToByteCountString(this long value) => 
            value.ToString("0.## ") + Bytes;

        public static string ToKiloByteCountString(this long value) => 
            (value / 1024D).ToString("0.## ") + KiloBytes;

        public static string ToMegaByteCountString(this long value) =>
            ((value >> 10) / 1024D).ToString("0.## ") + MegaBytes;

        public static string ToGigaByteCountString(this long value) =>
            ((value >> 20) / 1024D).ToString("0.## ") + GigaBytes;

        public static string ToTeraByteCountString(this long value) =>
            ((value >> 30) / 1024D).ToString("0.## ") + TeraBytes;

        public static string ToPetaByteCountString(this long value) =>
            ((value >> 40) / 1024D).ToString("0.## ") + PetaBytes;

        public static string ToByteScaleString(this long byteCount)
        {
            var isNegative = byteCount < 0;
            var unsignedBytes = isNegative ? -byteCount : byteCount;
            string value;

            if (unsignedBytes < 0x400)
            {
                value = byteCount.ToByteCountString();
            }
            else if (unsignedBytes < 0x100000)
            {
                value = byteCount.ToKiloByteCountString();
            }
            else if (unsignedBytes < 0x40000000)
            {
                value = byteCount.ToMegaByteCountString();
            }
            else if (unsignedBytes < 0x10000000000)
            {
                value = byteCount.ToGigaByteCountString();
            }
            else if (unsignedBytes < 0x4000000000000)
            {
                value = byteCount.ToTeraByteCountString();
            }
            else
            {
                value = byteCount.ToPetaByteCountString();
            }

            value = (isNegative) ? "-" + value : value;
            return value;
        }

    }
}
