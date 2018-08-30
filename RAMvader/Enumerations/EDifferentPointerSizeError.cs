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
    /// <summary>Defines how errors with different pointer sizes are handled by the library.</summary>
    public enum EDifferentPointerSizeError
    {
        /// <summary>
        ///    Throws an exception if the target process and the process which runs RAMvader have different pointer sizes.
        ///    This is the default behaviour, for safety reasons.
        /// </summary>
        evThrowException,
        /// <summary>
        ///    If the target process and the process which uses RAMvader have different pointer sizes, operations with pointers truncate
        ///    the pointers to 32-bits when necessary. If any data is lost during the truncation process, a <see cref="PointerDataLostException"/>
        ///    is thrown.
        /// </summary>
        evSafeTruncation,
        /// <summary>
        ///    If the target process and the process which uses RAMvader have different pointer sizes, operations with pointers truncate the
        ///    pointers to 32-bits when necessary. If any data is lost during the truncation process, nothing happens. Thus, this is the less
        ///    recommended option and should be used with caution.
        /// </summary>
        evUnsafeTruncation,
    }
}
