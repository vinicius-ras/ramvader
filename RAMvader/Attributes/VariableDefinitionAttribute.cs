using System;
using System.Reflection;

namespace RAMvader.CodeInjection
{
    /** Keeps the metadata related to an injection variable. */
    [AttributeUsage( AttributeTargets.Field, AllowMultiple=false )]
    public class VariableDefinitionAttribute : Attribute
    {
        #region PRIVATE FIELDS
        /** Stores the initial value for the variable. Used to initialize the
         * variable's value, when it is first injected into the target process'
         * memory. */
        private Object m_initialValue;
        #endregion





        #region PUBLIC PROPERTIES
        /** Backed by the #m_initialValue field. */
        public Object InitialValue
        {
            get { return m_initialValue; }
        }
        #endregion





        #region PUBLIC STATIC METHODS
        /** Utility method which retrieves the #VariableDefinitionAttribute from the given enumerator value.
         * @param elm The enumerator from which the #VariableDefinitionAttribute should be retrieved.
         * @return Returns the #VariableDefinitionAttribute associated with the given enumerator, if any.
         *    Returns null if no #VariableDefinitionAttribute is associated with the given enumerator. */
        public static VariableDefinitionAttribute GetVariableDefinitionAttributeFromEnum( Enum elm )
        {
            Type enumType = elm.GetType();
            FieldInfo fieldInfo = enumType.GetField( elm.ToString() );
            return fieldInfo.GetCustomAttribute<VariableDefinitionAttribute>();
        }
        #endregion





        #region PUBLIC METHODS
        /** Constructor.
         * @param initialValue The initial value of the variable. This should
         *    be specified with structures from the basic values which are
         *    supported by the #Injector (Byte, Int32, UInt64, Single, Double,
         *    etc.). By providing these structures, you are both telling the
         *    injector about the SIZE of the injected variable and its initial
         *    value. */
        public VariableDefinitionAttribute( Object initialValue )
        {
            m_initialValue = initialValue;
        }
        #endregion
    }
}
