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

using System.Linq;

namespace RAMvader.CodeInjection
{
	/// <summary>Represents a memory alteration that overwrites instructions of the target process' memory space with NOP instructions.</summary>
	public class MemoryAlterationNOP : MemoryAlterationBase
	{
		#region PUBLIC METHODS
		/// <summary>Constructor.</summary>
		/// <param name="targetIORef">
		///    A reference to the <see cref="RAMvaderTarget"/> object that will be used to read the target process' memory space.
		///    This <see cref="RAMvaderTarget"/> MUST be attached to a process, as it will be used in this constructor method to read the process'
		///    memory and keep a snapshot of the original bytes at the given 'targetAddress' for restoring their values,
		///    whenever <see cref="MemoryAlterationNOP.SetEnabled{TMemoryAlterationID, TCodeCave, TVariable}(Injector{TMemoryAlterationID, TCodeCave, TVariable}, bool)"/> is called to deactivate a memory alteration.
		/// </param>
		/// <param name="targetAddress">The address of the instruction(s) that will be replaced with NOP instructions.</param>
		/// <param name="instructionSize">The size of the instruction(s) that will be replaced with NOP instructions.</param>
		public MemoryAlterationNOP( RAMvaderTarget targetIORef, MemoryAddress targetAddress, int instructionSize )
			: base( targetIORef, targetAddress, instructionSize )
		{
		}
		#endregion





		#region PUBLIC ABSTRACT METHODS IMPLEMENTATION: MemoryAlterationBase
		/// <summary>Called to activate or deactivate a memory alteration into the target process' memory space.</summary>
		/// <typeparam name="TMemoryAlterationID">The enumeration of Memory Alteration Sets used for the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}" />.</typeparam>
		/// <typeparam name="TCodeCave">The enumeration of Code Caves used for the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}" />.</typeparam>
		/// <typeparam name="TVariable">The enumeration of Injection Variables used for the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}" />.</typeparam>
		/// <param name="injectorRef">A reference to an <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> object, with which you can perform I/O operations on the target process' memory space and access related data, like values and addresses of variables and code caves.</param>
		/// <param name="bEnable">A flag specifying if the memory alteration should be enabled or disabled.</param>
		/// <returns>Returns a flag specifying if the operation was successful or not.</returns>
		public override bool SetEnabled<TMemoryAlterationID, TCodeCave, TVariable>(
			Injector<TMemoryAlterationID, TCodeCave, TVariable> injectorRef, bool bEnable )
		{
			// When enabling: replace the original instruction with several NOP instructions.
			// When disabling: replace the instruction with its original bytes.
			byte [] bytesToWrite;
			if ( bEnable )
				bytesToWrite = Enumerable.Repeat<byte>( LowLevel.OPCODE_x86_NOP, this.TargetOriginalBytes.Length ).ToArray();
			else
				bytesToWrite = this.TargetOriginalBytes;

			// Write data into the target process' memory space
			return injectorRef.TargetProcess.WriteToTarget( this.TargetAddress, bytesToWrite );
		}
		#endregion
	}
}
