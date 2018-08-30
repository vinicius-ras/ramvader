using System;
using System.Collections.Generic;
using System.Reflection;

namespace RAMvader.Attributes
{
    /// <summary>
    ///    Provides utilities for accessing <see cref="Attribute"/>s from enumerated types in an easier manner,
    ///    in the form of extension methods. This class is currently internal to the library's assembly, which
    ///    means it won't be accessible to consumers using the library's code.
    /// </summary>
    static class EnumAttributeUtilityExtensions
    {
        #region PUBLIC STATIC METHODS
        /// <summary>Retrieves all of the attributes associated to a given enumerator.</summary>
        /// <typeparam name="T">The type of the attributes retrieved by this method.</typeparam>
        /// <param name="enumerator">The enumerator whose attributes are to be retrieved.</param>
        /// <param name="inherit">Flag specifying if the attributes should be searched for in all of the type-hierarchy.</param>
        /// <returns>Returns an array containing any found attributes.</returns>
        public static IEnumerable<T> GetAttributes<T>(this Enum enumerator, bool inherit = false)
            where T : Attribute
        {
            var enumeratorName = enumerator.ToString();
            var enumType = enumerator.GetType();

            return enumType.GetField(enumeratorName).GetCustomAttributes<T>(inherit);
        }


        /// <summary>Retrieves a single attribute associated to a given enumerator.</summary>
        /// <typeparam name="T">The type of the attributes retrieved by this method.</typeparam>
        /// <param name="enumerator">The enumerator whose attributes are to be retrieved.</param>
        /// <param name="inherit">Flag specifying if the attributes should be searched for in all of the type-hierarchy.</param>
        /// <returns>
        ///    Returns the found attribute, or <code>null</code> if the given attribute was not found.
        ///    Fires an exception if multiple attributes have been found.
        /// </returns>
        public static T GetAttribute<T>(this Enum enumerator, bool inherit = false)
            where T : Attribute
        {
            var enumeratorName = enumerator.ToString();
            var enumType = enumerator.GetType();

            return enumType.GetField(enumeratorName).GetCustomAttribute<T>(inherit);
        }
        #endregion
    }
}
