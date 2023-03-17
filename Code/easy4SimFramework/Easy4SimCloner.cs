using System;
using System.Linq;
using System.Reflection;

namespace Easy4SimFramework
{
    /// <summary>
    /// Static Class that is used to transfer fields values from one object to another
    /// </summary>
    public static class Easy4SimCloner
    {
        /// <summary>
        /// Copies all values from Fields and Properties in the baseObject that implement the IValueable Interface to the clonedObject Field with the same name if such a field exists.
        /// All ParameterTypes from the Framework implement the IValueable interface.
        /// </summary>
        /// <param name="baseObject"></param>
        /// <param name="clonedObject"></param>
        public static void CopyAllFrameworkVariables(object baseObject, object clonedObject)
        {
            FieldInfo[] fInfos = baseObject.GetType().GetFields();
            foreach (FieldInfo fInfo in fInfos)
            {
                Type t = fInfo.FieldType;
                foreach (Type type in t.GetInterfaces())
                {
                    // Every ParameterType of the environment should implement the IValueable Interface
                    if (!type.Name.StartsWith("IValueable")) continue;
                    FieldInfo f2 = clonedObject.GetType().GetFields()
                        .FirstOrDefault(x => x.Name == fInfo.Name);
                    if (f2 != null)
                    {
                        f2.SetValue(clonedObject, fInfo.GetValue(baseObject));
                    }
                }
            }
            PropertyInfo[] pInfos = baseObject.GetType().GetProperties();
            foreach (PropertyInfo pInfo in pInfos)
            {
                Type t = pInfo.PropertyType;
                foreach (Type type in t.GetInterfaces())
                {
                    // Every ParameterType of the environment should implement the IValueable Interface
                    if (!type.Name.StartsWith("IValueable")) continue;
                    PropertyInfo f2 = clonedObject.GetType().GetProperties()
                        .FirstOrDefault(x => x.Name == pInfo.Name);
                    if (f2 != null)
                    {
                        f2.SetValue(clonedObject, pInfo.GetValue(baseObject));
                    }
                }
            }

        }
    }
}
