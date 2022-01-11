using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace NiftiCS
{
    public struct Reflector
    {
        public FieldInfo Field;
        public MarshalAsAttribute Width;
        public int Offset;

        public Reflector(FieldInfo field, MarshalAsAttribute width, int offset)
        {
            Field = field;
            Width = width;
            Offset = offset;
        }

        // Revert this to using an anonymous type
        public static Reflector[] ReflectFieldsOfType(Type reflectedType)
        {
            // Removed binding flags since the Nifti1 header struct fields
            // have public accessors
            FieldInfo[] fieldInfo = reflectedType.GetFields();

            var fieldArray = new Reflector[fieldInfo.Length];

            for (int idx = 0; idx < fieldInfo.Length; idx++)
            {
                FieldInfo field = fieldInfo[idx];
                var width = field.GetCustomAttribute(typeof(MarshalAsAttribute), false);
                int offset = Marshal.OffsetOf(reflectedType, field.Name).ToInt32();

                fieldArray[idx] = new Reflector(field, (MarshalAsAttribute)width, offset);
            }

            return fieldArray;
        }
    }
}
