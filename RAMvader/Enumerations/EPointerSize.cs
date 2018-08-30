/*
 * Copyright (C) 2014 Vinicius Rogério Araujo Silva
 *
 * This file is part of RAMvader.
 *
 * RAMvader is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * RAMvader is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with RAMvader.  If not, see <http://www.gnu.org/licenses/>.
 */

namespace RAMvader
{
    /// <summary>Defines the supported pointer sizes for the target process.</summary>
    public enum EPointerSize
    {
        /// <summary>
        ///    The default pointer size configuration, where the target process' pointer size is assumed to be the same as the pointer
        ///    size of the process which runs RAMvader. The pointer size can be retrieved through IntPtr.Size.
        /// </summary>
        evPointerSizeDefault,
        /// <summary>Explicitly identifies a 32-bit pointer.</summary>
        evPointerSize32,
        /// <summary>Explicitly identifies a 64-bit pointer.</summary>
        evPointerSize64,
    }
}
