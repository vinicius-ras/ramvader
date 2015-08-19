/* This file implements the basic enumerations used on the library. */

namespace RAMvader
{
    /** Defines the possible endianness options which RAMvader can operate on. */
    public enum EEndianness
    {
        /** A value indicating that RAMvader should operate in the same endianness as the
         * process that RAMvader is running on. */
        evEndiannessDefault,
        /** A value indicating that RAMvader should operate in Little-Endian byte order. */
        evEndiannessLittle,
        /** A value indicating that RAMvader should operate in Big-Endian byte order. */
        evEndiannessBig,
    }


    /** Defines the supported pointer sizes for the target process. */
    public enum EPointerSize
    {
        /** The default pointer size configuration, where the target process' pointer size
         * is assumed to be the same as the pointer size of the process which runs RAMvader.
         * The pointer size can be retrieved through IntPtr.Size. */
        evPointerSizeDefault,
        /** Explicitly identifies a 32-bit pointer. */
        evPointerSize32,
        /** Explicitly identifies a 64-bit pointer. */
        evPointerSize64,
    }


    /** Defines how errors with different pointer sizes are handled by the library. */
    public enum EDifferentPointerSizeError
    {
        /** Throws an exception if the target process and the process which runs RAMvader have
         * different pointer sizes. This is the default behaviour, for safety reasons. */
        evThrowException,
        /** If the target process and the process which uses RAMvader have different pointer sizes,
         * operations with pointers truncate the pointers to 32-bits when necessary. If any data is
         * lost during the truncation process, a #PointerDataLostException is thrown. */
        evSafeTruncation,
        /** If the target process and the process which uses RAMvader have different pointer sizes,
         * operations with pointers truncate the pointers to 32-bits when necessary. If any data is lost
         * during the truncation process, nothing happens. Thus, this is the less recommended option and
         * should be used with caution. */
        evUnsafeTruncation,
    }
}