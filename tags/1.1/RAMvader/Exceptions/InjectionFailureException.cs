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

namespace RAMvader.CodeInjection
{
    /** An exception thrown by the #Injector to indicate the injection method has
     * failed. */
    public class InjectionFailureException : InjectorException
    {
        #region PUBLIC ENUMERATIONS
        /** Indicates the type of failure which caused the exception. */
        public enum EFailureType
        {
            /** Indicates that the #Injector has not been initialized with a
             * #RAMvaderTarget object. This can be done by calling #Injector.SetTargetProcess(). */
            evFailureRAMvaderTargetNull,
            /** Indicates that the #RAMvaderTarget object associated to the #Injector is currently not
             * attached to any process. */
            evFailureNotAttached,
            /** Indicates that the #Injector has tried to allocate virtual memory in the target process'
             * memory space, but the allocation failed. This usually happens either when the system denies the allocation
             * or if there are no code caves and injection variables to be injected - which effectivelly means that there's
             * actually NOTHING to be injected into the target process' memory space, making the injection completely unnecessary.
             * Memory allocation happens when you can the #Injector.Inject() method (the parameterless version of it). */
            evFailureMemoryAllocation,
            /** Indicates that the call to #RAMvaderTarget.WriteToTarget() method has failed. */
            evFailureWriteToTarget,
        }
        #endregion





        #region PRIVATE FIELDS
        /** Keeps the type of failure that caused the exception to be thrown. */
        private EFailureType m_failureType;
        #endregion





        #region PUBLIC PROPERTIES
        /** The type of failure that caused the exception to be thrown. */
        public EFailureType FailureType
        {
            get { return m_failureType; }
        }
        #endregion





        #region PUBLIC METHODS
        /** Constructor.
         * @param failureType The type of failure which caused the exception to be
         *    thrown. */
        public InjectionFailureException( EFailureType failureType )
            : base( string.Format( "Injection process has failed with code: {0}.", failureType.ToString() ) )
        {
            m_failureType = failureType;
        }
        #endregion
    }
}
