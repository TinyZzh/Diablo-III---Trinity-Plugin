using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace QuestTools.Helpers
{
    /// <summary>
    /// This class well get and set static Trinity Properties and Fields. It does not support instances.
    /// </summary>
    public class TrinityApi
    {
        // Trinity_635346394618921335.dll
        private const string AssemblyName = "Trinity";
        // namespace Trinity, public class Trinity
        private const string MainClass = "Trinity.Combat.Abilities.CombatBase";

        private static Assembly _mainAssembly;
        private static Dictionary<Tuple<string, Type>, PropertyInfo> _propertyInfoDictionary = new Dictionary<Tuple<string, Type>, PropertyInfo>();
        private static Dictionary<Tuple<string, Type>, FieldInfo> _fieldInfoDictionary = new Dictionary<Tuple<string, Type>, FieldInfo>();
        private static Dictionary<string, Type> _typeDictionary = new Dictionary<string, Type>();

        public static Dictionary<Tuple<string, Type>, PropertyInfo> PropertyInfoDictionary
        {
            get { return _propertyInfoDictionary; }
            set { _propertyInfoDictionary = value; }
        }

        public static Dictionary<Tuple<string, Type>, FieldInfo> FieldInfoDictionary
        {
            get { return _fieldInfoDictionary; }
            set { _fieldInfoDictionary = value; }
        }

        public static Dictionary<string, Type> TypeDictionary
        {
            get { return _typeDictionary; }
            set { _typeDictionary = value; }
        }

        public static Type GetTrinityType()
        {
            Type trinityType;
            if (!SetType("Trinity.Trinity", out trinityType))
            {
                Logger.Error("Unable to get Trinity Type!");
            }
            return trinityType;
        }

        public static object GetStaticPropertyFromType(Type type, string propertyName)
        {
            PropertyInfo propertyInfo = type.GetProperty(propertyName, BindingFlags.Static | BindingFlags.Public);
            object property = propertyInfo.GetValue(null, null);
            if (property == null)
            {
                Logger.Error("Unable to get Static Property {0} from Type {1}", propertyName, type);
            }
            return property;
        }

        public static object GetInstancePropertyFromObject(object source, string propertyName)
        {
            PropertyInfo propertyInfo = source.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
            object property = propertyInfo.GetValue(source, null);
            if (property == null)
            {
                Logger.Error("Unable to get Instance Property {0} from source object {1}", propertyName, source.GetType());
            }
            return property;
        }

        public static PropertyInfo GetInstancePropertyInfoFromObject(object source, string propertyName)
        {
            return source.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
        }

        /// <summary>
        /// Sets a property with a given value
        /// </summary>
        /// <param name="fullName">The Full Type name (Namespace.Namespace.Class), e.g. Trinity.Combat.Abilities.CombatBase</param>
        /// <param name="memberName">The Member Name</param>
        /// <param name="value">The Value</param>
        /// <returns>If the Property was successfully set</returns>
        public static bool SetProperty(string fullTypeName, string memberName, object value)
        {
            try
            {
                Type targetType;
                if (!SetType(fullTypeName, out targetType))
                {
                    // Type or Assembly not found
                    return false;
                }

                // Make sure TargetType is valid
                if (targetType == null)
                    return false;

                PropertyInfo propertyInfo;
                // Grab cached PropertyInfo object
                if (_propertyInfoDictionary.TryGetValue(new Tuple<string, Type>(memberName, targetType), out propertyInfo))
                {
                    if (propertyInfo != null)
                    {
                        // We have a valid PropertyInfo object, set the value
                        Logger.Debug("Setting Trinity Type={0} Property={1} to value={2}", fullTypeName, memberName, value);
                        propertyInfo.SetValue(null, value, null);
                        return true;
                    }
                }

                propertyInfo = targetType.GetProperty(memberName, BindingFlags.Static | BindingFlags.Public);
                if (propertyInfo != null)
                {
                    // Upsert propertyInfo object into cache
                    var key = new Tuple<string, Type>(memberName, targetType);
                    if (_propertyInfoDictionary.ContainsKey(key))
                        _propertyInfoDictionary[key] = propertyInfo;
                    else
                        _propertyInfoDictionary.Add(key, propertyInfo);

                    // Set the value
                    Logger.Debug("Setting Trinity Type={0} Property={1} to value={2}", fullTypeName, memberName, value);
                    propertyInfo.SetValue(null, value, null);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Error Setting Property {0} from Type {1} - {2}", memberName, fullTypeName, ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Sets a Field with a given value
        /// </summary>
        /// <param name="fullTypeName">The Full Type name (Namespace.Namespace.Class), e.g. Trinity.Combat.Abilities.CombatBase</param>
        /// <param name="memberName">The Member name</param>
        /// <param name="value">The value</param>
        /// <returns>If the Field was successfully set</returns>
        public static bool SetField(string fullTypeName, string memberName, object value)
        {
            try
            {
                Type targetType;
                if (!SetType(fullTypeName, out targetType))
                {
                    return false;
                }
                if (targetType == null)
                    return false;

                FieldInfo fieldInfo;
                if (_fieldInfoDictionary.TryGetValue(new Tuple<string, Type>(memberName, targetType), out fieldInfo))
                {
                    Logger.Debug("Setting Trinity Type={0} Field={1} to value={2}", fullTypeName, memberName, value);
                    fieldInfo.SetValue(null, value);
                    return true;
                }

                fieldInfo = targetType.GetField(memberName, BindingFlags.Static | BindingFlags.Public);
                if (fieldInfo != null)
                {
                    var key = new Tuple<string, Type>(memberName, targetType);
                    if (_fieldInfoDictionary.ContainsKey(key))
                        _fieldInfoDictionary[key] = fieldInfo;
                    else
                        _fieldInfoDictionary.Add(key, fieldInfo);

                    Logger.Debug("Setting Trinity Type={0} Field={1} to value={2}", fullTypeName, memberName, value);
                    fieldInfo.SetValue(null, value);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Error Setting Field {0} from Type {1} - {2}", memberName, fullTypeName, ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Gets a value from a Field
        /// </summary>
        /// <param name="fullName">The Full Type name (Namespace.Namespace.Class), e.g. Trinity.Combat.Abilities.CombatBase</param>
        /// <param name="memberName">The Member Name</param>
        /// <param name="value">The Value</param>
        /// <returns>If the field was successfully returned</returns>
        public static bool GetField(string fullTypeName, string memberName, out object value)
        {
            value = null;
            try
            {
                Type targetType;
                if (!SetType(fullTypeName, out targetType))
                {
                    return false;
                }
                if (targetType == null)
                    return false;

                FieldInfo fieldInfo;
                if (_fieldInfoDictionary.TryGetValue(new Tuple<string, Type>(memberName, targetType), out fieldInfo))
                {
                    value = fieldInfo.GetValue(null);
                    return true;
                }

                fieldInfo = targetType.GetField(memberName, BindingFlags.Static | BindingFlags.Public);
                if (fieldInfo != null)
                {
                    _fieldInfoDictionary.Add(new Tuple<string, Type>(memberName, targetType), fieldInfo);

                    value = fieldInfo.GetValue(null);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Error reading Property {0} from Type {1} - {2}", memberName, fullTypeName, ex.Message);
                return false;
            }
            return false;
        }

        /// <summary>
        /// Gets a value from a Property
        /// </summary>
        /// <param name="fullName">The Full Type name (Namespace.Namespace.Class), e.g. Trinity.Combat.Abilities.CombatBase</param>
        /// <param name="memberName">The Member Name</param>
        /// <param name="value">The value to set</param>
        /// <returns>If the Property was successfully returned</returns>
        public static bool GetProperty(string fullTypeName, string memberName, out object value)
        {
            value = null;
            try
            {
                Type targetType;
                if (!SetType(fullTypeName, out targetType))
                {
                    return false;
                }

                if (targetType == null)
                    return false;

                PropertyInfo propertyInfo;
                if (_propertyInfoDictionary.TryGetValue(new Tuple<string, Type>(memberName, targetType), out propertyInfo))
                {
                    value = propertyInfo.GetValue(null, null);
                    return true;
                }

                propertyInfo = targetType.GetProperty(memberName, BindingFlags.Static | BindingFlags.Public);
                if (propertyInfo != null)
                {
                    _propertyInfoDictionary.Add(new Tuple<string, Type>(memberName, targetType), propertyInfo);
                    value = propertyInfo.GetValue(null, null);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Error reading Property {0} from Type {1} - {2}", memberName, fullTypeName, ex.Message);
                return false;
            }
            return false;
        }

        /// <summary>
        /// Caches the given type by name
        /// </summary>
        /// <param name="fullName">The Full Type name (Namespace.Namespace.Class), e.g. Trinity.Combat.Abilities.CombatBase</param>
        /// <param name="result">The Type</param>
        /// <returns>If the type was found</returns>
        public static bool SetType(string fullName, out Type result)
        {
            result = null;
            try
            {
                SetAssembly();

                if (_mainAssembly == null)
                {
                    return false;
                }

                if (TypeDictionary.TryGetValue(fullName, out result) && result != null)
                    return true;
                result = _mainAssembly.GetType(fullName);

                if (TypeDictionary.ContainsKey(fullName))
                    TypeDictionary[fullName] = result;
                else
                    TypeDictionary.Add(fullName, result);
                
                return true;

            }
            catch (Exception ex)
            {
                Logger.Log("Unable to read type {0} from Assembly: {2}", fullName, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Caches the Assembly
        /// </summary>
        public static void SetAssembly()
        {
            try
            {
                if (_mainAssembly != null)
                    return;
                var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.GetName().Name.StartsWith(AssemblyName + "_")).Where(asm => asm != null);
                foreach (var asm in assemblies)
                {
                    try
                    {
                        var mainType = asm.GetType(MainClass);
                        if (mainType != null)
                        {
                            _mainAssembly = asm;
                            break;
                        }
                    }
                    catch
                    {
                        // Not the Plugin, probably the routine, keep trying
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Unable to set Assembly {2}", ex.Message);
            }
        }
    }
}
