using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace NiftiCS
{
    public static class Nifti
    {
        public static (NiftiFormat format, Endian endian) GetFormat(string fpath)
        {
            Endian endianness = Endian.LittleEndian;
            byte[] buffer = new byte[4];

            using FileStream stream =
                new FileStream(fpath, FileMode.Open, FileAccess.Read);
            stream.Read(buffer, 0, buffer.Length);

            if (BitConverter.ToInt32(buffer, 0) > 540 & BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer);
                endianness = Endian.BigEndian;
            }

            int sz = BitConverter.ToInt32(buffer, 0);

            // Header size is constant, should be just as well to
            // match on header size for versioning instead of seeking
            // the magic string, which is only truly feasible if you
            // load in the entire file at once.
            NiftiFormat Format()
            {
                return sz switch
                {
                    348 => NiftiFormat.Nifti1,
                    540 => NiftiFormat.Nifti2,
                    _   => NiftiFormat.Unknown
                };
            }
            // However, jump to magic string to determine if Nifti1 is hdr/img combo
            // after we have checked header size.

            return (Format(), endianness);
        }

        public static byte[] Read<T>(string fpath)
        {
            int bufferSize = Marshal.SizeOf(typeof(T));

            byte[] buffer = new byte[bufferSize];

            try
            {
                using FileStream stream =
                    new FileStream(fpath, FileMode.Open, FileAccess.Read);
                stream.Read(buffer, 0, bufferSize);
            }
            catch (IOException e)
            {
                Console.WriteLine("The file could not be read: ");
                Console.WriteLine(e.Message);
            }

            return buffer;
        }

        public static Endian GetEndianness(byte[] buffer, int position)
        {
            var flag = BitConverter.ToInt16(buffer, position);

            return (1 <= flag & flag <= 7) ? Endian.LittleEndian : Endian.BigEndian;
        }

        public static T ReadHeader<T>(byte[] buffer)
        {
            int size = Marshal.SizeOf(typeof(T));
            IntPtr ptr = Marshal.AllocHGlobal(size);

            int flag = typeof(T) == typeof(Nifti1) ? 40 : 1;
            Endian endian = GetEndianness(buffer, flag);

            foreach (var field in Reflector.ReflectFieldsOfType(typeof(T)))
            {
                var fld = field.Field.FieldType;
                var offset = field.Offset;

                int factor = fld.IsArray ? Marshal.SizeOf(fld.GetElementType()) : 1;
                int fsize = field.Width is null ? Marshal.SizeOf(fld) : field.Width.SizeConst * factor;

                if (endian is Endian.BigEndian)
                    if (fld.IsArray)
                    {
                        int bin = Marshal.SizeOf(fld.GetElementType());

                        if (fsize == bin)
                            Array.Reverse(buffer, offset, fsize);
                        else if (fsize > bin)
                            for (int i = offset; i < (offset + fsize); i += bin)
                                Array.Reverse(buffer, i, bin);
                    }
                    else
                    {
                        if (fld != typeof(string))
                            Array.Reverse(buffer, offset, fsize);
                    }
            }

            // resume here
            try
            {
                Marshal.Copy(buffer, 0, ptr, size);
                return (T)Marshal.PtrToStructure(ptr, typeof(T));
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }


        }
    }
}
