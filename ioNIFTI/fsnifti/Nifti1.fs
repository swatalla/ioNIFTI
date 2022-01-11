namespace FSNifti

open System.Runtime.InteropServices

[<Struct; StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)>]
type Nifti1Header = {
    /// Size of the image header; must be 348 for NIFTI-1.
    sizeof_hdr: int
    /// UNUSED: maintained for Analyze7.5 compatability.
    [<MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)>]
    data_type: string
    /// UNUSED: maintained for Analyze7.5 compatability.          
    [<MarshalAs(UnmanagedType.ByValTStr, SizeConst = 18)>]
    db_name: string        
    /// UNUSED: maintained for Analyze7.5 compatability.
    extents: int         
    /// UNUSED: maintained for Analyze7.5 compatability.
    session_error: int16            
    /// UNUSED: maintained for Analyze7.5 compatability.
    regular: char    
    /// UNUSED: maintained for Analyze7.5 compatability.
    dim_info: byte
    /// <summary>
    /// Array dimensions of the image data.
    /// The first element, <c>dim[0]</c>, specifies the number of dimensions
    /// and must be between 1 and 7; otherwise, check endianness. The value 
    /// <c>dim[n]</c> is a positive integer representing the length of the 
    /// nth dimension. Dimensions 1-3 (<c>dim[1-3]</c>) are inferred to 
    /// be x, y, and z. The 4th dimension, <c>dim[4]</c>, is assumed to be 
    /// time. The remaining <c>dim[5-7]</c> may be anything else.
    /// </summary>
    [<MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)>]
    dim: int16[]
    /// 1st intent parameter.
    intent_p1: single
    /// 2nd intent parameter.
    intent_p2: single
    /// 3rd intent parameter.
    intent_p3: single
    /// <summary>
    /// Codifies what the data is supposed to contain. Some codes require
    /// extra paremters, such as the degrees of freedom. These codes
    /// are stored in the 3 intent fields <c>intent_p1</c>, <c>intent_p2</c>,
    /// and <c>intent_p3</c> if the intent parameters are meant to be applied 
    /// to the image as a whole, or stored in <c>dim[5]</c> if the parameters 
    /// are meant to be applied voxelwise.
    /// </summary>
    intent_code: int16
    /// Indicates the image data type.
    datatype: int16
    /// Specifies the number of bits per voxel.
    bitpix: int16
    /// <summary>
    /// Specifies the index of the first slice of the slice acquisition
    /// scheme defined by the <c>slice_code</c> field.
    /// </summary>
    slice_start: int16
    /// <summary> 
    /// Grid spacing (unit per dimension). The purpose of each element should
    /// match that of <c>dim</c>. The <c>pixdim[0]</c> is used with the
    /// <c>qform_code</c> and must be either 1 or -1; if not, assume that it is 1.
    /// The units of measurement for <c>pixdim[1-4]</c> are specified by <c>xyzt_units</c>.
    /// </summary>
    [<MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)>]  
    pixdim: single[]                     // Grid spacings
    vox_offset: single                   // Offset into .nii file
    scl_slope: single                    // Data scaling: slope
    scl_inter: single                    // Data scaling: offset
    slice_end: int16                     // Last slice index
    slice_code: byte                     // Slice timing order
    xyzt_units: byte                     // Units of pixdim[1..4]
    cal_max: single                      // Max display intensity
    cal_min: single                      // Min display intensity
    slice_duration: single               // Time for 1 slice
    toffset: single                      // Time axis shift
    glmax: int32                         // UNUSED
    glmin: int32                         // UNUSED
    [<MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)>]
    descrip: string                      // Description of the data 
    [<MarshalAs(UnmanagedType.ByValTStr, SizeConst = 24)>]
    aux_file: string                     // Auxiliary filename
    qform_code: int16                    // NIFTI_XFORM_* code
    sform_code: int16                    // NIFTI_XFORM_* code
    quatern_b: single                    // Quarternion b parameter
    quatern_c: single                    // Quarternion c parameter
    quatern_d: single                    // Quarternion d parameter
    qoffset_x: single                    // Quarternion x shift
    qoffset_y: single                    // Quarternion y shift
    qoffset_z: single                    // Quarternion x shift
    [<MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)>]
    srow_x: single[]                     // 1st row affine transform
    [<MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)>]
    srow_y: single[]                     // 2nd row affine transform
    [<MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)>]
    srow_z: single[]                     // 3rd row affine transform
    [<MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)>]
    intent_name: string                  // Name of the data
    [<MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)>]
    magic: string   }                    // Magic; either `ni1` or `n+1`

    /// <summary>
    /// The Nifti1 header extension contains additional information that doesn't
    /// belong in the header proper. This is responsible for the <c>vox_offset</c>
    /// being greater than <c>sizeof_hdr</c>. Any extra information in this header
    /// will be in size multiples of 16, with the first 8 bytes being two integers,
    /// <c>esize</c> and <c>ecode</c>, indicating the size of the extent and the
    /// format used for the remaining extension, respectively.
    /// </summary>
    [<Struct; StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)>]
    type Nifti1HeaderExtension = {
        [<MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)>]
        extension: byte[]   }