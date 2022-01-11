namespace FSNifti

open System.Runtime.InteropServices

[<Struct; StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)>]
type Nifti2Header = {
    /// Size of the image header; must be 540.
    sizeof_hdr: int
    /// <summary> 
    /// Specifies that this is a NIFTI-2 file; should be <c>n+2</c>.
    /// </summary>
    [<MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)>]
    magic: string
    /// Specifies the data type of the image data
    data_type: int16
    /// Number of bits per voxel
    bitpix: int16
    /// <summary> 
    /// Array dimensions of the image data.
    /// The first element, <c>dim[0]</c>, specifies the number of dimensions
    /// and must be between 1 and 7; otherwise, check endianness.
    /// The value <c>dim[n]</c> is a positive integer representing
    /// the length of the nth dimension. Dimensions 1-3 (<c>dim[1-3]</c>) are
    /// inferred to be x, y, and z, and the 4th dimension (<c>dim[4]</c>) is 
    /// assumed to be time. The remaining dimensions (<c>dim[5-7]</c>) may be
    /// anything else.
    /// </summary>
    [<MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)>]
    dim: int64[]
    /// 1st intent parameter.
    intent_p1: double
    /// 2nd intent paramter.
    intent_p2: double       
    /// 3rd intent parameter.              
    intent_p3: double
    /// <summary> 
    /// Grid spacing (unit per dimension).
    /// <c>pixdim[0]</c> must be either 1 or -1.
    /// </summary>
    [<MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)>]
    pixdim: double[]                     // Grid spacings
    vox_offset: int64
    scl_slope: double                    // Data scaling: slope
    scl_inter: double                    // Data scaling: offset
    cal_max: double                      // Max display intensity
    cal_min: double                      // Min display intensity
    slice_duration: double               // Time for 1 slice
    toffset: double                      // Time axis shift
    slice_start: int64                   // First slice index
    slice_end: int64                     // Last slice index
    [<MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)>]
    descrip: string                      // Description of the data 
    [<MarshalAs(UnmanagedType.ByValTStr, SizeConst = 24)>]
    aux_file: string                     // Auxiliary filename
    qform_code: int                      // Use quaternion fields
    sform_code: int                      // use affine fields
    quatern_b: double                    // Quarternion b parameter
    quatern_c: double                    // Quarternion c parameter
    quatern_d: double                    // Quarternion d parameter
    qoffset_x: double                    // Quarternion x shift
    qoffset_y: double                    // Quarternion y shift
    qoffset_z: double                    // Quarternion x shift
    [<MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)>]
    srow_x: double[]                    // 1st row affine transform
    [<MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)>]
    srow_y: double[]                    // 2nd row affine transform
    [<MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)>]
    srow_z: double[]                    // 3rd row affine transform
    slice_code: int                     // Slice timing order
    xyzt_units: int                     // Units of pixdim[1..4]
    intent_code: int                   // NIFTI_INTENT_* code
    [<MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)>]
    intent_name: string                  // Name of the data
    dim_info: byte                       // MRI slice ordering
    [<MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)>]
    unused_str: string                   // UNUSED, to be padded with zeros
    }