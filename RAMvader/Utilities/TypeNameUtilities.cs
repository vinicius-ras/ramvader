using System;
using System.Text;
using System.Text.RegularExpressions;

namespace RAMvader.Utilities
{
    /// <summary>Internal utility extensions and methods for retrieving human-readable names for generic types.</summary>
    /// <remarks>Methods provided by this class are also compatible for use with non-generic types.</remarks>
    static class TypeNameUtilities
    {
        #region PRIVATE STATIC FIELDS
        /// <summary>
        ///    Regular expression to extract the "main name" of a generic type, removing any further compiler-generated symbols.
        ///    This regular expression also works for non-generic types.
        /// </summary>
        private static Regex RegexExtractGenericTypeMainName = new Regex(@"^(\w+)");
        #endregion





        #region EXTENSION METHODS
        /// <summary>Extracts the main name of a given type, while removing any extra compiler-generated symbols from it.</summary>
        /// <param name="t">The type whose main name is to be extracted.</param>
        /// <returns>Returns the main name of the given type, removing any extra compiler-generated symbols from it.</returns>
        public static string ExtractMainName(this Type t)
        {
            return RegexExtractGenericTypeMainName.Match(t.Name).Groups[1].Value;
        }


        /// <summary>Extracts the name of a given type, while expanding all of its generic parameters (if applicable).</summary>
        /// <param name="t">Reference to the <see cref="Type"/> whose name will be extracted.</param>
        /// <returns>Returns the name of the type, with all of its generic parameters expanded (if applicable).</returns>
        public static string ExpandedName(this Type t)
        {
            // Non-generic types will only have their main names extracted
            if (t.IsGenericType == false)
                return t.ExtractMainName();

            // For generic types, we will extract their names and build each of their generic parameters' representations recursivelly
            var builder = new StringBuilder(t.ExtractMainName());
            builder.Append('<');

            Type[] genericTypeArgs = t.GenericTypeArguments;
            for (int i = 0; i < genericTypeArgs.Length; i++)
            {
                if (i > 0)
                    builder.Append(", ");
                builder.Append(ExpandedName(genericTypeArgs[i]));
            }
            builder.Append('>');
            return builder.ToString();
        }


        /// <summary>Extracts the name of a given type, while expanding all of its generic parameters (if applicable).</summary>
        /// <typeparam name="T">The <see cref="Type"/> whose name will be extracted.</typeparam>
        /// <returns>Returns the name of the type, with all of its generic parameters expanded (if applicable).</returns>
        public static string ExpandedName<T>()
        {
            Type t = typeof(T);
            return t.ExpandedName();
        }
        #endregion
    }
}
