namespace FSNifti

open System.Runtime.InteropServices

// * Mayo defines this as a struct of structs, but there is really no reason to not have it
// as a single record type...
// However, this could be constructed as a record of records.
// https://rportal.mayo.edu/bir/ANALYZE75.pdf
[<Struct; StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)>]
type Analyze75Header = {
    // Header Key
    sizeof_hdr: int                       // Should be 348
    [<MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)>]
    data_type: string                    
    [<MarshalAs(UnmanagedType.ByValTStr, SizeConst = 18)>]
    db_name: string                      
    extents: int                         
    session_error: int16                 
    regular: char                        
    hkey_un0: byte                       

    // Image Dimensions                      
    [<MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)>]
    dim: int16[]                         
    unused8: int16                    
    unused9: int16                    
    unused10: int16                   
    unused11: int16                    
    unused12: int16                     
    unused13: int16                      
    unused14: int16                    
    datatype: int16
    bitpix: int16
    dim_un0: int16
    [<MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)>]  
    pixdim: single[]                    
    vox_offset: single 
    funused1: single 
    funused2: single
    funused3: single
    cal_max: single     
    cal_min: single      
    compressed: single
    verified: single
    glmax: int32          
    glmin: int32           

    // Data History
    [<MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)>]
    descrip: string  
    [<MarshalAs(UnmanagedType.ByValTStr, SizeConst = 24)>]
    aux_file: string  
    orient: byte
    [<MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)>]
    originator: string                      
    [<MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)>]
    generated: string                     
    [<MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)>]
    scannum: string                 
    [<MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)>]
    patient_id: string               
    [<MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)>]
    exp_date: string                  
    [<MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)>]
    exp_time: string                  
    [<MarshalAs(UnmanagedType.ByValTStr, SizeConst = 3)>]
    hist_un03: string                  
    views: int
    vols_added: int
    start_field: int
    field_skip: int
    omax: int
    omin: int
    smax: int
    smin: int }