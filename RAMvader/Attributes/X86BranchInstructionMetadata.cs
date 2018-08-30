using System;

namespace RAMvader.Attributes
{
    /// <summary>Stores metadata about x86 branch instructions which the library is able to generate.</summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    class X86BranchInstructionMetadata : Attribute
    {
        #region PUBLIC PROPERTIES
        /// <summary>Total size of the instruction, given in bytes.</summary>
        public int TotalInstructionSize;
        /// <summary>
        ///    The <see cref="Type"/> of the value which is expected to determine the branch's relative offset.
        ///    This should be <see cref="SByte"/> for 8-bit ("short") branches and <see cref="Int32"/> for 32-bit ("near") branches.
        /// </summary>
        public Type OffsetType;
        /// <summary>
        ///    The byte values which compose the instruction's main opcode.
        ///    The main opcode precedes the relative offset when encoding branch
        ///    instructions in memory.
        /// </summary>
        public byte[] MainOpcodeBytes;
        #endregion
    }
}
