namespace RAMvader.CodeInjection
{
	/** This class is used to keep low-level definitions, such as opcodes that can be used to generate x86 code. */
	public static class LowLevel
	{
		#region PUBLIC CONSTANTS
		/** The byte value for the x86 NOP instruction. */
		public static readonly byte OPCODE_x86_NOP = 0x90;
		/** The byte value for the x86 INT3 instruction. */
		public static readonly byte OPCODE_x86_INT3 = 0xCC;
		/** Represents the byte value for the x86 CALL instruction. */
		public static readonly byte OPCODE_x86_CALL = 0xE8;
		/** Represents the byte value for the x86 NEAR JMP instruction. */
		public static readonly byte OPCODE_x86_NEAR_JMP = 0xEB;
		/** Represents the byte value for the x86 NEAR JA instruction. */
		public static readonly byte OPCODE_x86_NEAR_JA  = 0x77;
		/** Represents the byte value for the x86 NEAR JB instruction. */
		public static readonly byte OPCODE_x86_NEAR_JB  = 0x72;
		/** Represents the byte value for the x86 NEAR JG instruction. */
		public static readonly byte OPCODE_x86_NEAR_JG  = 0x7F;
		/** Represents the byte value for the x86 NEAR JL instruction. */
		public static readonly byte OPCODE_x86_NEAR_JL  = 0x7C;
		/** Represents the byte value for the x86 NEAR JE instruction. */
		public static readonly byte OPCODE_x86_NEAR_JE  = 0x74;
		/** Represents the byte value for the x86 NEAR JNE instruction. */
		public static readonly byte OPCODE_x86_NEAR_JNE = 0x75;
		/** Represents the byte value for the x86 FAR JMP instruction. */
		public static readonly byte [] OPCODE_x86_FAR_JMP = { 0xE9 };
		/** Represents the byte value for the x86 FAR JA instruction. */
		public static readonly byte [] OPCODE_x86_FAR_JA  = { 0x0F, 0x87 };
		/** Represents the byte value for the x86 FAR JB instruction. */
		public static readonly byte [] OPCODE_x86_FAR_JB  = { 0x0F, 0x82 };
		/** Represents the byte value for the x86 FAR JG instruction. */
		public static readonly byte [] OPCODE_x86_FAR_JG  = { 0x0F, 0x8F };
		/** Represents the byte value for the x86 FAR JL instruction. */
		public static readonly byte [] OPCODE_x86_FAR_JL  = { 0x0F, 0x8C };
		/** Represents the byte value for the x86 FAR JE instruction. */
		public static readonly byte [] OPCODE_x86_FAR_JE  = { 0x0F, 0x84 };
		/** Represents the byte value for the x86 FAR JNE instruction. */
		public static readonly byte [] OPCODE_x86_FAR_JNE = { 0x0F, 0x85 };
		/** The size of a x86 CALL instruction, given in bytes. */
		public const int INSTRUCTION_SIZE_x86_CALL = 5;
		/** The size of a x86 NEAR JUMP instruction, given in bytes. Near jumps allow jumps to instructions up to a distance of 0xFF bytes. */
		public const int INSTRUCTION_SIZE_x86_NEAR_JUMP = 2;
		#endregion
	}
}
