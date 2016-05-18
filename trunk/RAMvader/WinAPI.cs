/*
 * Copyright (C) 2014 Vinicius Rogério Araujo Silva
 *
 * This file is part of RAMvader.
 * 
 * RAMvader is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * RAMvader is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with RAMvader.  If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Runtime.InteropServices;


namespace RAMvader
{
	/// <summary>This class is an interface that provides access to the Windows API.</summary>
	public static class WinAPI
    {
		#region ENUMERATIONS
		/// <summary>Flags used to determine the allowed access to a process.</summary>
		[Flags]
        public enum ProcessAccessFlags : uint
        {
			/// <summary>Identifies the "PROCESS_ALL_ACCESS" process access right from the Windows API.</summary>
			All = 0x001F0FFF,
			/// <summary>Identifies the "PROCESS_TERMINATE" process access right from the Windows API.</summary>
			Terminate = 0x00000001,
			/// <summary>Identifies the "PROCESS_CREATE_THREAD" process access right from the Windows API.</summary>
			CreateThread = 0x00000002,
			/// <summary>Identifies the "PROCESS_VM_OPERATION" process access right from the Windows API.</summary>
			VMOperation = 0x00000008,
			/// <summary>Identifies the "PROCESS_VM_READ" process access right from the Windows API.</summary>
			VMRead = 0x00000010,
			/// <summary>Identifies the "PROCESS_VM_WRITE" process access right from the Windows API.</summary>
			VMWrite = 0x00000020,
			/// <summary>Identifies the "PROCESS_DUP_HANDLE" process access right from the Windows API.</summary>
			DupHandle = 0x00000040,
			/// <summary>Identifies the "PROCESS_SET_INFORMATION" process access right from the Windows API.</summary>
			SetInformation = 0x00000200,
			/// <summary>Identifies the "PROCESS_QUERY_INFORMATION" process access right from the Windows API.</summary>
			QueryInformation = 0x00000400,
			/// <summary>Identifies the "SYNCHRONIZE" process access right from the Windows API.</summary>
			Synchronize = 0x00100000
        }


		/// <summary>Flags used for determining the type of memory allocation in the function VirtualAllocEx.</summary>
		[Flags]
        public enum AllocationType
        {
			/// <summary>Identifies the "MEM_COMMIT" memory allocation type from the Windows API.</summary>
			Commit = 0x1000,
			/// <summary>Identifies the "MEM_RESERVE" memory allocation type from the Windows API.</summary>
			Reserve = 0x2000,
			/// <summary>Identifies the "MEM_DECOMMIT" memory freeing type from the Windows API.</summary>
			Decommit = 0x4000,
			/// <summary>Identifies the "MEM_RELEASE" memory freeing type from the Windows API.</summary>
			Release = 0x8000,
			/// <summary>Identifies the "MEM_RESET" memory allocation type from the Windows API.</summary>
			Reset = 0x80000,
			/// <summary>Identifies the "MEM_PHYSICAL" memory allocation type from the Windows API.</summary>
			Physical = 0x400000,
			/// <summary>Identifies the "MEM_TOP_DOWN" memory allocation type from the Windows API.</summary>
			TopDown = 0x100000,
			/// <summary>Identifies the "MEM_WRITE_WATCH" memory allocation type from the Windows API.</summary>
			WriteWatch = 0x200000,
			/// <summary>Identifies the "MEM_LARGE_PAGES" memory allocation type from the Windows API.</summary>
			LargePages = 0x20000000
		}


		/// <summary>Flags determining the type of memory protection for a region of allocated pages.</summary>
		[Flags]
        public enum MemoryProtection
        {
			/// <summary>Identifies the "PAGE_EXECUTE" page protection type from the Windows API.</summary>
			Execute = 0x10,
			/// <summary>Identifies the "PAGE_EXECUTE_READ" page protection type from the Windows API.</summary>
			ExecuteRead = 0x20,
			/// <summary>Identifies the "PAGE_EXECUTE_READWRITE" page protection type from the Windows API.</summary>
			ExecuteReadWrite = 0x40,
			/// <summary>Identifies the "PAGE_EXECUTE_WRITECOPY" page protection type from the Windows API.</summary>
			ExecuteWriteCopy = 0x80,
			/// <summary>Identifies the "PAGE_NOACCESS" page protection type from the Windows API.</summary>
			NoAccess = 0x01,
			/// <summary>Identifies the "PAGE_READONLY" page protection type from the Windows API.</summary>
			ReadOnly = 0x02,
			/// <summary>Identifies the "PAGE_READWRITE" page protection type from the Windows API.</summary>
			ReadWrite = 0x04,
			/// <summary>Identifies the "PAGE_WRITECOPY" page protection type from the Windows API.</summary>
			WriteCopy = 0x08,
			/// <summary>Identifies the "PAGE_GUARD" page protection type modifier from the Windows API.</summary>
			GuardModifierflag = 0x100,
			/// <summary>Identifies the "PAGE_NOCACHE" page protection type modifier from the Windows API.</summary>
			NoCacheModifierflag = 0x200,
			/// <summary>Identifies the "PAGE_WRITECOMBINE" page protection type modifier from the Windows API.</summary>
			WriteCombineModifierflag = 0x400
        }


		/// <summary>Flags used for freeing allocated memory, through the VirtualFreeEx function.</summary>
		[Flags]
        public enum FreeType
        {
			/// <summary>Identifies the "MEM_DECOMMIT" memory region freeing type from the Windows API.</summary>
			Decommit = 0x4000,
			/// <summary>Identifies the "MEM_RELEASE" memory region freeing type from the Windows API.</summary>
			Release = 0x8000,
        }
		#endregion







		#region IMPORTED API METHODS: KERNEL32
		/// <summary>Kernel32: OpenProcess function.</summary>
		[DllImport( "kernel32.dll" )]
		public static extern IntPtr OpenProcess( ProcessAccessFlags dwDesiredAccess,
			[MarshalAs( UnmanagedType.Bool )] bool bInheritHandle, int dwProcessId );


		/// <summary>Kernel32: CloseHandle function.</summary>
		[DllImport( "kernel32.dll", SetLastError = true )]
		[return: MarshalAs( UnmanagedType.Bool )]
		public static extern bool CloseHandle( IntPtr hObject );


		/// <summary>Kernel32: VirtualAllocEx function.</summary>
		[DllImport( "kernel32.dll", SetLastError = true, ExactSpelling = true )]
		public static extern IntPtr VirtualAllocEx( IntPtr hProcess, IntPtr lpAddress, uint dwSize,
			AllocationType flAllocationType, MemoryProtection flProtect );


		/// <summary>Kernel32: VirtualFreeEx function.</summary>
		[DllImport( "kernel32.dll", SetLastError = true, ExactSpelling = true )]
		public static extern bool VirtualFreeEx( IntPtr hProcess, IntPtr lpAddress,
			int dwSize, FreeType dwFreeType );


		/// <summary>Kernel32: WriteProcessMemory function.</summary>
		[DllImport( "kernel32.dll", SetLastError = true )]
		public static extern bool WriteProcessMemory( IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer,
			int nSize, out IntPtr lpNumberOfBytesWritten );


		/// <summary>Kernel32: ReadProcessMemory function.</summary>
		[DllImport( "kernel32.dll", SetLastError = true )]
		public static extern bool ReadProcessMemory( IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer,
			int dwSize, out IntPtr lpNumberOfBytesRead );

		#endregion
	}
}
