﻿/*
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
    /** This class is an interface that provides access to the Windows API. */
    public static class WinAPI
    {
        #region ENUMERATIONS
        /** Flags used to determine the allowed access to a process. */
        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VMOperation = 0x00000008,
            VMRead = 0x00000010,
            VMWrite = 0x00000020,
            DupHandle = 0x00000040,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            Synchronize = 0x00100000
        }


        /** Flags used for determining the type of memory allocation in the
         * function VirtualAllocEx. */
        [Flags]
        public enum AllocationType
        {
            Commit = 0x1000,
            Reserve = 0x2000,
            Decommit = 0x4000,
            Release = 0x8000,
            Reset = 0x80000,
            Physical = 0x400000,
            TopDown = 0x100000,
            WriteWatch = 0x200000,
            LargePages = 0x20000000
        }


        /** Flags determining the type of memory protection for a region of allocated
         * pages. */
        [Flags]
        public enum MemoryProtection
        {
            Execute = 0x10,
            ExecuteRead = 0x20,
            ExecuteReadWrite = 0x40,
            ExecuteWriteCopy = 0x80,
            NoAccess = 0x01,
            ReadOnly = 0x02,
            ReadWrite = 0x04,
            WriteCopy = 0x08,
            GuardModifierflag = 0x100,
            NoCacheModifierflag = 0x200,
            WriteCombineModifierflag = 0x400
        }


        /** Flags used for freeing allocated memory, through the VirtualFreeEx function. */
        [Flags]
        public enum FreeType
        {
            Decommit = 0x4000,
            Release = 0x8000,
        }


        #endregion







        #region IMPORTED API METHODS: KERNEL32
        /** Kernel32: OpenProcess function. */
        [DllImport( "kernel32.dll" )]
        public static extern IntPtr OpenProcess( ProcessAccessFlags dwDesiredAccess,
            [MarshalAs( UnmanagedType.Bool )] bool bInheritHandle, int dwProcessId );


        /** Kernel32: CloseHandle function. */
        [DllImport( "kernel32.dll", SetLastError = true )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static extern bool CloseHandle( IntPtr hObject );


        /** Kernel32: VirtualAllocEx function. */
        [DllImport( "kernel32.dll", SetLastError = true, ExactSpelling = true )]
        public static extern IntPtr VirtualAllocEx( IntPtr hProcess, IntPtr lpAddress, uint dwSize,
            AllocationType flAllocationType, MemoryProtection flProtect );


        /** Kernel32: VirtualFreeEx function. */
        [DllImport( "kernel32.dll", SetLastError = true, ExactSpelling = true )]
        public static extern bool VirtualFreeEx( IntPtr hProcess, IntPtr lpAddress,
            int dwSize, FreeType dwFreeType );


        /** Kernel32: WriteProcessMemory function. */
        [DllImport( "kernel32.dll", SetLastError = true )]
        public static extern bool WriteProcessMemory( IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer,
            int nSize, out IntPtr lpNumberOfBytesWritten );


        /** Kernel32: ReadProcessMemory function. */
        [DllImport( "kernel32.dll", SetLastError = true )]
        public static extern bool ReadProcessMemory( IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer,
            int dwSize, out IntPtr lpNumberOfBytesRead );

        #endregion
    }
}