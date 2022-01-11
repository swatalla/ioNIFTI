using System.Runtime.InteropServices;

namespace NiftiCS
{
    /// <summary>
    /// Represents a Nifti1 Header object. This header must be read prior to
    /// attempting to read a Nifti1 Image object. The Nifti1 Header contains
    /// information relevant to the <i>type</i> of the data, as well as
    /// spatial and temporal information necessary for image manipulation.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct Nifti1
    {
        /// <summary> Size of the Nifti1 Header. Must be 348. </summary>
        public int sizeof_hdr;

        #region Unused by Nifti1; maintained for Analyze7.5 compatibility.
        /// <summary> Unused by Nifti1; maintained for Analyze7.5 compatibility. </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
        public string data_type;
        /// <summary> Unused by Nifti1; maintained for Analyze7.5 compatibility. </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 18)]
        public string db_name;
        /// <summary> Unused by Nifti1; maintained for Analyze7.5 compatibility. </summary>
        public int extents;
        /// <summary> Unused by Nifti1; maintained for Analyze7.5 compatibility. </summary>
        public short session_error;
        /// <summary> Unused by Nifti1; maintained for Analyze7.5 compatibility. </summary>
        public char regular;
        #endregion

        /// <summary> MRI slice ordering. </summary>
        public byte dim_info;
        /// <summary> Data array dimensions. </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public short[] dim;

        /// <summary> 1st intent parameter. </summary>
        public float intent_p1;
        /// <summary> 2nd intent parameter. </summary>
        public float intent_p2;
        /// <summary> 3rd intent parameter. </summary>
        public float intent_p3;
        /// <summary> NIFTI_INTENT_* code. </summary>
        public short intent_code;

        /// <summary> Defines the type of the data. </summary>
        public short datatype;        
        /// <summary> Number of bits/voxel. </summary>
        public short bitpix;

        /// <summary> Index of the first slice. </summary>
        public short slice_start;
        /// <summary>
        /// Grid spacing. The first element of this array is
        /// also used to determine the endianness of the image.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public float[] pixdim;
        /// <summary> Offset into .nii file. </summary>
        public float vox_offset;

        /// <summary> Data scaling: slope. </summary>
        public float scl_slope;
        /// <summary> Data scaling: offset </summary>
        public float scl_inter;

        /// <summary> Last slice index. </summary>
        public short slice_end;
        /// <summary> Slice timing order. </summary>
        public byte slice_code;
        /// <summary> Units of pixdim[1..4]. </summary>
        public byte xyzt_units;

        /// <summary> Maximum display intensity. </summary>
        public float cal_max;
        /// <summary> Minimum display intensity. </summary>
        public float cal_min;

        /// <summary> Duration of a single slice. </summary>
        public float slice_duration;
        /// <summary> Time axis shift. </summary>
        public float toffset;

        #region Unused by Nifti1; maintained for Analyze7.5 compatibility.
        /// <summary> Unused by Nifti1; maintained for Analyze7.5 compatibility. </summary>
        public int glmax;
        /// <summary> Unused by Nifti1; maintained for Analyze7.5 compatibility. </summary>
        public int glmin;
        #endregion

        /// <summary> Short text description of the data. </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string descrip;
        /// <summary> Name of auxilliary files. </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 24)]
        public string aux_file;

        /// <summary> NIFTI_XFORM_* code. </summary>
        public short qform_code;      
        /// <summary> NIFTI_XFORM_* code. </summary>
        public short sform_code;

        /// <summary> Quaternian b parameter. </summary>
        public float quatern_b;
        /// <summary> Quaternian c parameter. </summary>
        public float quatern_c;
        /// <summary> Quaternian d parameter. </summary>
        public float quatern_d;

        /// <summary> Quaternion <i>x</i> shift. </summary>
        public float qoffset_x;
        /// <summary> Quaternion <i>y</i> shift. </summary>
        public float qoffset_y;
        /// <summary> Quaternion <i>z</i> shift. </summary>
        public float qoffset_z;

        /// <summary> 1st row of affine transformation matrix. </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] srow_x;
        /// <summary> 2nd row of affine transformation matrix. </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] srow_y;
        /// <summary> 3rd row of affine transformation matrix. </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] srow_z;

        /// <summary> Name of the data. </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string intent_name;
        /// <summary> Magic number; either <i>nil</i> or <i>n+1</i>. </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string magic;

        /// <summary> 3rd party extensions, e.g. AFNI. </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] extension;
    }
}
