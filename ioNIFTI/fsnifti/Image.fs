namespace FSNifti

open System
open System.IO
open System.Reflection
open System.IO.MemoryMappedFiles
open System.Text.RegularExpressions
open System.Runtime.InteropServices

module Image =

    [<AutoOpen>]
    module ImageLexer =

        [<AutoOpen>]
        module ErrorMessages =

            let UnknownByteOrderError = 
                "UnknownByteOrder: no knowledge ofthe underlying data's endianness."

            let IndeterminateImageFormatError =
                "IndeterminateImageFormatError: cannot determine the image format."

            let UnsupportedImageFormatError =
                "UnsupportedImageFormatError: selected image format is currently unsupported."

        [<AutoOpen>]
        module Discriminators =

            type Endianness = 
                | LittleEndian
                | BigEndian
                | UnknownByteOrder

            type ImageHeaderFormat =
                | Analyze75HeaderFormat of Analyze75Header
                | Nifti1HeaderFormat of Nifti1Header
                | Nifti2HeaderFormat of Nifti2Header
                | UnknownHeaderFormat

            type ImageFormatType =
                | Analyze75ImageFormat
                | Nifti1ImageFormat
                | Nifti2ImageFormat
                | UnknownImageFormat

            type FileFormatType =
                | ImgHdrDualFileFormat
                | NiiSingleFileFormat
                | UnknownFileFormat

        [<AutoOpen>]
        module Containers =

            type ImageFileInformation =
                {   ByteOrderFormat: Endianness
                    ImageFormat:     ImageFormatType
                    FileFormat:      FileFormatType  }

            type HeaderFieldInfo =
                {   Field: FieldInfo
                    Width: MarshalAsAttribute
                    Offset: int  }

            type ImageInformation = 
                {   Info:   ImageFileInformation
                    Header: ImageHeaderFormat   }

        [<AutoOpen>]  
        module internal Patterns =

            let (|BE|LE|UE|) = function
                | 348 
                | 540 -> 
                    if BitConverter.IsLittleEndian then LE else BE
                | 1543569408 
                | 469893120 ->
                    if BitConverter.IsLittleEndian then BE else LE
                | _ -> UE

            let (|MaybeAnalyze75OrNifti1|MaybeNifti2|Indeterminate|) = function
                | 348 
                | 1543569408 -> MaybeAnalyze75OrNifti1
                | 540 
                | 469893120  -> MaybeNifti2
                | _ -> Indeterminate
            // TODO - refactor so it doesnt use regex
            // try to implement a custom parser
            let (|AZ|N1|N2|NA|) (hdrsize, magic) =
                let nii = new Regex(@"^\w[ni][1]$|^[n][\+][1-2]$")

                match hdrsize with
                | 348 
                | 1543569408 -> 
                    match magic with
                    | _ when magic |> nii.IsMatch -> N1
                    | _ -> AZ
                | 540 
                | 469893120  -> N2
                | _ -> NA
            // TODO - refactor so it doesnt use regex
            let (|ImgHdr|Nii|Unknown|) magic =
                // Regex is typically slower, but it's extensible
                let nii = new Regex(@"^[n][\+][1-2]$")
                let img = new Regex(@"^\w[ni][1]$|\A\z")

                match magic with
                | _ when magic |> img.IsMatch -> ImgHdr
                | _ when magic |> nii.IsMatch -> Nii
                | _ -> Unknown

        module Inspect =
 /// Extracts endian and format information from current file's byte stream.
            let imageInfo (buffer: byte[]) =
                let hdrsize = BitConverter.ToInt32(buffer, 0)

                let endian =
                    match hdrsize with
                    | LE -> LittleEndian
                    | BE -> BigEndian
                    | UE -> UnknownByteOrder

                let magic =
                    match hdrsize with
                    | MaybeAnalyze75OrNifti1 -> 
                        buffer
                        |> Seq.skip 344
                        |> Seq.take 4
                        |> Some
                    | MaybeNifti2 ->
                        buffer
                        |> Seq.skip 4
                        |> Seq.take 8
                        |> Some
                    | Indeterminate -> None
                    |> function
                        | Some possibleMagicString -> 
                            possibleMagicString
                            |> Seq.map char
                            |> Seq.filter (fun x -> x |> Char.IsControl |> not)
                            |> String.Concat
                            |> Some
                        | None -> None

                let imageFormat =
                    match magic with
                    | Some m ->
                        match (hdrsize, m) with
                        | AZ -> Analyze75ImageFormat
                        | N1 -> Nifti1ImageFormat
                        | N2 -> Nifti2ImageFormat
                        | NA -> UnknownImageFormat
                    | None -> UnknownImageFormat

                let fileFormat =
                    match magic with
                    | Some m ->
                        match hdrsize with
                        | Indeterminate -> UnknownFileFormat
                        | MaybeNifti2   -> NiiSingleFileFormat
                        | MaybeAnalyze75OrNifti1 ->
                            match m with
                            | Nii       -> NiiSingleFileFormat
                            | ImgHdr    -> ImgHdrDualFileFormat
                            | Unknown   -> UnknownFileFormat
                    | None -> UnknownFileFormat

                { ByteOrderFormat = endian; ImageFormat = imageFormat; FileFormat = fileFormat }

    [<AutoOpen>]
    module ImageParser =

        module private Introspection =

            /// Reflects on the fields of the header record type.
            let reflect<'T> =
                let bindingFlags = BindingFlags.Instance ||| BindingFlags.NonPublic

                seq { for field in typeof<'T>.GetFields(bindingFlags) do
                        yield { Field = field
                                Width = // Alternatively, this can be downcast with `:?>`
                                    field.GetCustomAttribute(typeof<MarshalAsAttribute>, false)
                                    |> fun attribute -> 
                                        downcast (box attribute) : MarshalAsAttribute
                                Offset = Marshal.OffsetOf(typeof<'T>, field.Name).ToInt32() } }

        module Deserialize =
            /// Moves the buffer to the structure. Call `getHeader` for public API.
            let private marshalHeader<'T> (buffer: byte[]) =
                let size: int   = Marshal.SizeOf(typeof<'T>)
                let ptr: IntPtr = Marshal.AllocHGlobal(size)

                // This shouldn't fail out at any point if the file was successfully read.
                try
                    Marshal.Copy(buffer, 0, ptr, size)

                    Marshal.PtrToStructure(ptr, typeof<'T>)
                    |> fun structure -> 
                        downcast (box structure) : 'T
                finally
                    Marshal.FreeHGlobal(ptr)

            /// Translates byte buffer into header information, specified by type parameter `T`.
            let getImageHeaderAsTypeOf<'T when 'T: struct> (buffer: byte[]) =
                // Buffer copy gets mutated instead of the buffer input; function purity.
                let localBuffer = buffer |> Array.copy

                let byteorder = 
                    localBuffer 
                    |> Inspect.imageInfo
                    |> fun info ->
                        info.ByteOrderFormat
                        |> function
                            | BigEndian -> Ok BigEndian
                            | LittleEndian -> Ok LittleEndian
                            | UnknownByteOrder -> 
                                Error UnknownByteOrderError

                // Iterates over the reflected fields of the header specified by type parameter `T`.
                match byteorder with
                | Ok endianness ->
                    Introspection.reflect<'T>
                    |> Seq.iter (fun field ->
                        let fieldType = field.Field.FieldType
                        let fieldOffset = field.Offset

                        /// Size of the array's underlying type.
                        let fieldElementSize =
                            if fieldType.IsArray then 
                                Marshal.SizeOf(fieldType.GetElementType()) else 1

                        // Size of the field in bytes. If the field is an array, then the field
                        // size is given by the product of the SizeConst and the size of each 
                        // field element of the underlying type.
                        let fieldSize =
                            match field.Width with
                            | null -> Marshal.SizeOf(fieldType)
                            | _ -> field.Width.SizeConst * fieldElementSize              
                        let fieldUpperBound = 
                            fieldOffset + fieldSize - 1

                        // Mutates copy of buffer (localBuffer).
                        // Added benefit of `unit` return type makes for a leaner control flow,
                        // instead of needing to return a new copy of the array at each branch.
                        // Need a custom return type/result type to handle "ByteOrderUnknown"
                        match endianness with
                        | BigEndian ->
                            if fieldType.IsArray then
                                for i in [fieldOffset..fieldElementSize..fieldUpperBound] do
                                    Array.Reverse(localBuffer, i, fieldElementSize)
                            elif fieldType <> typeof<string> then
                                Array.Reverse(localBuffer, fieldOffset, fieldSize)
                        | _ -> () ) |> Ok
                | Error e -> Error e
                |> ignore

                // Hide the actual marshalling in a private function...
                // Call this to get the marshalled header struct.
                localBuffer 
                |> marshalHeader<'T>

        let getImageInformation (buffer: byte[]) =

            let imageInformation = 
                buffer 
                |> Inspect.imageInfo

            let header =
                match imageInformation.ImageFormat with
                | Analyze75ImageFormat ->
                    buffer
                    |> Deserialize.getImageHeaderAsTypeOf<Analyze75Header>
                    |> Analyze75HeaderFormat
                | Nifti1ImageFormat ->
                    buffer 
                    |> Deserialize.getImageHeaderAsTypeOf<Nifti1Header> 
                    |> Nifti1HeaderFormat
                | Nifti2ImageFormat ->
                    buffer 
                    |> Deserialize.getImageHeaderAsTypeOf<Nifti2Header> 
                    |> Nifti2HeaderFormat
                | _ -> UnknownHeaderFormat

            { Info = imageInformation; Header = header }

    type Image = {
        ImageHeader: ImageHeaderFormat
        ImageData: byte[]   }

    type VerifiedImage =
        | ValidImage of Image
        | InvalidImage of string

    module ImageReader = 

        // The NIFTI standard says that the data for each slice is located linearly,
        // which means that the axial view is the standard view from which the image should
        // be analyzed, presumably...

        let readHeader (fileName: string) =

            let fileInfo = new FileInfo(fileName)
            let fileSize = fileInfo.Length

            use mmf = MemoryMappedFile.CreateFromFile(fileName, FileMode.Open)

            let headerSize =
                use accessor = mmf.CreateViewAccessor(0L, sizeof<int> |> int64)

                accessor.ReadInt32(0L)
                |> function
                    | MaybeAnalyze75OrNifti1    -> 348
                    | MaybeNifti2               -> 540
                    | Indeterminate             -> 0

            let headerBuffer = 
                headerSize 
                |> Array.zeroCreate<byte>

            let bufferedBytes =
                use view = mmf.CreateViewStream(0L, headerSize |> int64)
                use stream = new BinaryReader(view)

                stream.Read(headerBuffer, 0, headerSize)
                |> ignore

            headerBuffer
            |> ImageParser.getImageInformation
            |> fun payload -> payload.Header

        module MappedFileHeaderReader =

            let readHeaderFromMappedFile (mmf: MemoryMappedFile) =
                let headerSize =
                    use accessor = mmf.CreateViewAccessor(0L, sizeof<int> |> int64)

                    accessor.ReadInt32(0L)
                    |> function
                        | MaybeAnalyze75OrNifti1 -> 348
                        | MaybeNifti2 -> 540
                        | Indeterminate -> 0

                let headerBuffer =
                    headerSize
                    |> Array.zeroCreate<byte>

                let bufferedBytes =
                    use view = mmf.CreateViewStream(0L, headerSize |> int64)
                    use stream = new BinaryReader(view)

                    stream.Read(headerBuffer, 0, headerSize)
                    |> ignore
                    
                headerBuffer
                |> getImageInformation

        // Reads entire image from a mapped file
        let readImage (fileName: string) =

            let fileInfo = new FileInfo(fileName)
            let fileSize = fileInfo.Length

            use mmf = MemoryMappedFile.CreateFromFile(fileName, FileMode.Open)

            let payload = MappedFileHeaderReader.readHeaderFromMappedFile(mmf)

            let offset =
                match payload.Header with
                | Analyze75HeaderFormat analyzeHdr  -> analyzeHdr.vox_offset |> int64
                | Nifti1HeaderFormat    nifti1Hdr   -> nifti1Hdr.vox_offset |> int64
                | Nifti2HeaderFormat    nifti2Hdr   -> nifti2Hdr.vox_offset
                | UnknownHeaderFormat               -> fileSize

            let imageSize = fileSize - offset

            use view = mmf.CreateViewStream(offset, imageSize)

            let bufferContainer = 
                imageSize
                |> int
                |> Array.zeroCreate<byte>

            let buffer = new Span<byte>(bufferContainer)

            let count = view.Read(buffer)

            match count with
                | x when x = int imageSize -> 
                    buffer.ToArray() 
                    |> fun data ->
                        { ImageHeader = payload.Header; ImageData = data }
                        |> ValidImage
                | _ ->  InvalidImage "Error reading image data"

    module IO =
        let header (filename: string) = ImageReader.readHeader(filename)

        let read (filename: string) =
            ImageReader.readImage(filename)
            |> function
                | ValidImage img -> 
                    Some img
                | InvalidImage msg -> 
                    printfn "%s" msg 
                    None
            

        (*
        // Using Buffer.BlockCopy() instead of Array.Copy() since src type <> dst type
        let getImage<'T> (endian: Endianness) (size: int64) (data: byte[]) : 'T[] =
            let inData, (imageData: 'T[]) = data, Array.zeroCreate (int size/Marshal.SizeOf(typeof<'T>))
            let fastcpy () = 
                match typeof<'T> with
                | t when t = typeof<byte> -> Array.Copy(inData, 0, imageData, 0, int size)
                | _ -> Buffer.BlockCopy(inData, 0, imageData, 0, int size)
            match endian with
            | Endianness.BigEndian ->
                Array.Reverse(inData); fastcpy()
                imageData |> Array.rev
            | _ -> fastcpy(); imageData
            //if endian = Endianness.BigEndian then Array.Reverse(inData)
            //Buffer.BlockCopy(inData, 0, imageData, 0, int size)
            //imageData


        let inline toSingle (data: 'a[]): single[] =
            data |> Array.Parallel.map (fun i -> single i)


        let bytesToData (size: int64) (hdr: Nifti1Header) (endian: Endianness) (data: byte[]) =
            match hdr.datatype with 
            | 2s ->  data |> getImage<byte>   endian size  |> toSingle
            | 4s ->  data |> getImage<int16>  endian size  |> toSingle
            | 8s ->  data |> getImage<int32>  endian size  |> toSingle
            | 16s -> data |> getImage<single> endian size
            | _ -> failwith "No implementation"

        *)


        
        


    // Everything after reading the data in will be implemented as post-processing
    (*
            // In-place rescale
        let rescale (data: single[]) (slope: single) (intercept: single) =
            for i in 0..(data.GetUpperBound 0) do
                data.[i] <- data.[i] * slope + intercept
            data 
    *)
