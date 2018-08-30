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

namespace RAMvader.CodeInjection
{
	/// <summary>An exception thrown by the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> to indicate the injection method has failed.</summary>
	public class InjectionFailureException : InjectorException
    {
		#region PUBLIC ENUMERATIONS
		/// <summary>Indicates the type of failure which caused the exception.</summary>
		public enum EFailureType
        {
			/// <summary>
			///    Indicates that the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> has not been initialized with a
			///    <see cref="RAMvaderTarget"/> object. This can be done by calling <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}.SetTargetProcess(RAMvaderTarget)"/>.
			/// </summary>
			evFailureRAMvaderTargetNull,
			/// <summary>
			///    Indicates that the <see cref="RAMvaderTarget"/> object associated to the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> is currently not
			///    attached to any process.
			/// </summary>
			evFailureNotAttached,
			/// <summary>
			///    Indicates that the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> has tried to allocate virtual memory in the target process'
			///    memory space, but the allocation failed. This usually happens either when the system denies the allocation
			///    or if there are no code caves and injection variables to be injected - which effectivelly means that there's
			///    actually NOTHING to be injected into the target process' memory space, making the injection completely unnecessary.
			///    Memory allocation happens when you call the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}.Inject()"/> method (the parameterless version of it).
			/// </summary>
			evFailureMemoryAllocation,
			/// <summary>Indicates that the call to <see cref="RAMvaderTarget.WriteToTarget(System.IntPtr, byte[])"/> method has failed.</summary>
			evFailureWriteToTarget,
        }
		#endregion





		#region PRIVATE FIELDS
		/// <summary>Keeps the type of failure that caused the exception to be thrown.</summary>
		private EFailureType m_failureType;
		#endregion





		#region PUBLIC PROPERTIES
		/// <summary>The type of failure that caused the exception to be thrown.</summary>
		public EFailureType FailureType
        {
            get { return m_failureType; }
        }
		#endregion





		#region PUBLIC METHODS
		/// <summary>Constructor.</summary>
		/// <param name="failureType">The type of failure which caused the exception to be thrown.</param>
		public InjectionFailureException( EFailureType failureType )
            : base($"Injection process has failed with code: {failureType.ToString()}.")
        {
            m_failureType = failureType;
        }
        #endregion
    }
}
