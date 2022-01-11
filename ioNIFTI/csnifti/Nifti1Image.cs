using System;
namespace NiftiCS
{
    public class Nifti1Image
    {
        public string File { get; }
        public Endian Endianness { get; set; }
        public Nifti1  Header { get; set; }
        public dynamic ImageData { get; set; }

        public Nifti1Image(string filename)
        {
            File = filename;
        }

        public Nifti1Image(string filename, Endian endianness)
        {
            File = filename;
            Endianness = endianness;
        }

        public Nifti1Image(string filename, Endian endianness, Nifti1 header)
        {
            File = filename;
            Endianness = endianness;
            Header = header;
        }

        public Nifti1Image(string filename, Endian endianness, Nifti1 header, dynamic imagedata)
        {
            File = filename;
            Endianness = endianness;
            Header = header;
            ImageData = imagedata;
        }

        public string Name()
        {
            return File;
        }

        public string GetEndian()
        {
            return Endianness.ToString();
        }

        public Nifti1 ImageHeader()
        {
            return Header;
        }

        public void ShowHeader()
        {
            foreach (var field in Header.GetType().GetFields())
            {
                Console.WriteLine($"{field.Name}: {field.GetValue(Header)}");
            }
        }
    }
}
