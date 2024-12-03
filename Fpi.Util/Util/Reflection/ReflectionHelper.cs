using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Fpi.Util.Reflection
{
    /// <summary>
    /// ActivatorWrap ��ժҪ˵����
    /// </summary>
    public static class ReflectionHelper
    {
        private static readonly string strAppDir = Application.StartupPath;
        //private static readonly string dllStoreFile = strAppDir + @"\bin\";
        //Fpi.dllֻ��Ҫ���ڸ�Ŀ¼�� fc
        private static readonly string dllStoreFile = strAppDir;
        private static readonly string libConfigFile = strAppDir + @"\Config\Librarys.Config";
        private static readonly string dllNameTemplate = "Fpi.*.dll";

        static ReflectionHelper()
        {
            lock (TypeTable)
            {
                LoadAssembly();
            }
        }

        public static Hashtable TypeTable { get; } = new Hashtable();

        public static Hashtable AssemblyTable { get; } = new Hashtable();

        public static Type FindType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                return null;
            if (TypeTable.ContainsKey(typeName))
            {
                return TypeTable[typeName] as Type;
            }
            return null;
        }

        public static object CreateInstance(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                throw new InvalidCastException("���Ͳ���Ϊ�ա�");
            }

            Type type = (Type)TypeTable[typeName];

            if (type == null)
            {
                throw new Exception("δ֪����:" + typeName);
            }

            return CreateInstance(type);
        }

        public static object CreateInstance(Type type)
        {
            if (type.IsAbstract) return null;
            MethodInfo info = type.GetMethod("GetInstance", BindingFlags.Static | BindingFlags.Public);
            if (info == null)
            {
                return type.Assembly.CreateInstance(type.FullName);
            }
            else
            {
                return info.Invoke(null, null);
            }
        }

#if !WINCE

        public static object CreateInstance(string typeName, object[] args)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                throw new InvalidCastException("���Ͳ���Ϊ�ա�");
            }

            Type type = (Type)TypeTable[typeName];

            if (type == null)
            {
                throw new Exception("δ֪����:" + typeName);
            }

            return CreateInstance(type, args);
        }

        public static object CreateInstance(Type type, object[] args)
        {
            return type.Assembly.CreateInstance(type.FullName, true, BindingFlags.Default, null, args, null, null);
        }

#endif

        public static Assembly GetAssemblyByFile(string asmFile)
        {
            if (AssemblyTable.ContainsKey(asmFile))
            {
                return (Assembly)AssemblyTable[asmFile];
            }

            throw new Exception("�Ҳ�������:" + asmFile);
        }

        /// <summary>
        /// �������,��������������ö�����������
        /// </summary>
        /// <param name="instance">��������</param>
        /// <returns>����һ���µĶ���</returns>
        public static object CloneObject(object instance)
        {
            if (instance == null) return null;
            object targetDeepCopyObj;
            try
            {
                Type targetType = instance.GetType();
                //ֵ����
                if (targetType.IsValueType == true || (instance is IConvertible && (instance as IConvertible).GetTypeCode() != TypeCode.Object))
                {
                    targetDeepCopyObj = instance;
                }
                //��������
                else
                {
                    targetDeepCopyObj = System.Activator.CreateInstance(targetType);   //�������ö���
                    MemberInfo[] memberCollection = instance.GetType().GetMembers();
                    foreach (System.Reflection.MemberInfo member in memberCollection)
                    {
                        if (member.MemberType == MemberTypes.Field)
                        {
                            System.Reflection.FieldInfo field = (FieldInfo)member;
                            object fieldValue = field.GetValue(instance);
                            if (fieldValue is ICloneable)
                            {
                                field.SetValue(targetDeepCopyObj, (fieldValue as ICloneable).Clone());
                            }
                            else
                            {
                                field.SetValue(targetDeepCopyObj, CloneObject(fieldValue));
                            }
                        }
                        else if (member.MemberType == MemberTypes.Property)
                        {
                            System.Reflection.PropertyInfo myProperty = (PropertyInfo)member;
                            MethodInfo info = myProperty.GetSetMethod(false);
                            if (info != null)
                            {
                                object propertyValue = myProperty.GetValue(instance, null);
                                if (propertyValue is ICloneable)
                                {
                                    myProperty.SetValue(targetDeepCopyObj, (propertyValue as ICloneable).Clone(), null);
                                }
                                else
                                {
                                    myProperty.SetValue(targetDeepCopyObj, CloneObject(propertyValue), null);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("��¡�������:" + instance + "��ϸ:" + ex.Message);
            }

            return targetDeepCopyObj;
        }

#if !WINCE

        public static void SetFieldValue(object instance, string fieldName, object fieldValue)
        {
            FieldInfo[] array = instance.GetType().GetFields();

            foreach (FieldInfo field in array)
            {
                if (field.Name.ToLower() == fieldName.ToLower())
                {
                    try
                    {
                        fieldValue = ChangeType(field, fieldValue);
                        field.SetValue(instance, fieldValue);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                    break;
                }
            }
        }

        public static object ChangeType(FieldInfo field, object fieldValue)
        {
            string typeName = field.FieldType.Name.ToLower();
            return ChangeType(typeName, fieldValue);
        }

        public static object ChangeType(Type type, object fieldValue)
        {
            string typeName = type.Name.ToLower();
            return ChangeType(typeName, fieldValue);
        }

        public static object ChangeType(PropertyInfo prop, object fieldValue)
        {
            string typeName = prop.PropertyType.Name.ToLower();
            return ChangeType(typeName, fieldValue);
        }

        public static object ChangeType(string typeName, object fieldValue)
        {
            if (fieldValue == null)
            {
                return null;
            }
            if (typeName == "bool")
            {
                typeName = "boolean";
            }
            if (typeName == "short")
            {
                typeName = "int16";
            }
            if (typeName == "int")
            {
                typeName = "int32";
            }
            if (typeName == "long")
            {
                typeName = "int64";
            }
            if (typeName == "ushort")
            {
                typeName = "uint16";
            }
            if (typeName == "uint")
            {
                typeName = "uint32";
            }
            if (typeName == "ulong")
            {
                typeName = "uint64";
            }
            switch (typeName)
            {
                case "string":
                    return Convert.ToString(fieldValue);

                case "byte":
                    return Convert.ToByte(fieldValue);

                case "boolean":
                    if (fieldValue is string)
                    {
                        return bool.Parse(fieldValue.ToString());
                    }
                    else if (fieldValue is bool)
                    {
                        return fieldValue;
                    }
                    else
                    {
                        throw new NotSupportedException(typeName + " ��֧�ָ�����ת��Ϊ bool �ͣ�");
                    }
                case "int16":
                    return Convert.ToInt16(fieldValue);

                case "int32":
                    return Convert.ToInt32(fieldValue);

                case "int64":
                    return Convert.ToInt64(fieldValue);

                case "float":
                    return Convert.ToSingle(fieldValue);

                default:
                    throw new NotSupportedException(typeName + " ��֧�ָ�����ת����");
            }
        }

#endif

        public static bool IsValueType(object ob)
        {
            Type type;

            if (ob is Type)
            {
                type = (Type)ob;
                return type.IsValueType;
            }
            if (ob is FieldInfo)
            {
                type = (ob as FieldInfo).FieldType;

                if (type == typeof(string))
                {
                    return true;
                }

                return type.IsValueType;
            }
            if (ob is PropertyInfo)
            {
                type = (ob as PropertyInfo).PropertyType;
                return type.IsValueType;
            }

            if (ob.GetType() == typeof(string))
            {
                return true;
            }
            return ob.GetType().IsValueType;
        }

        public static Hashtable GetChildInstances(Type baseType)
        {
            Hashtable table = new Hashtable();
            Type[] types = GetChildTypes(baseType);
            if (types != null)
            {
                foreach (Type type in types)
                {
                    if (type.IsAbstract || type.IsNotPublic)
                    {
                        continue;
                    }

                    try
                    {
                        object obj = CreateInstance(type);
                        string key = obj.GetType().FullName;
                        table[key] = obj;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"����ʵ��ʧ��:{ex.Message}");
                    }
                }
            }
            return table;
        }

        public static Type[] GetChildTypes(Type baseType)
        {
            ArrayList list = new ArrayList();

            foreach (Type type in TypeTable.Values)
            {
                if (IsNestedBaseType(type, baseType))
                {
                    list.Add(type);
                }
            }

            Type[] result = new Type[list.Count];
            list.CopyTo(result, 0);

            return result;
        }

        public static string[] GetChildTypeNames(Assembly ass, Type baseType)
        {
            Type[] types = GetChildTypes(ass, baseType);

            string[] result = new string[types.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = types[i].FullName;
            }

            return result;
        }

        public static Type[] GetChildTypes(Assembly ass, Type baseType)
        {
            Type[] types = ass.GetTypes();
            ArrayList list = new ArrayList();

            foreach (Type type in types)
            {
                if (IsNestedBaseType(type, baseType))
                {
                    list.Add(type);
                }
            }

            Type[] result = new Type[list.Count];
            list.CopyTo(result, 0);

            return result;
        }

        public static bool IsNestedBaseType(Type type, Type baseType)
        {
            if (type.BaseType != null)
            {
                if (baseType.IsInterface)
                {
                    Type t = null;
#if WINCE
                    Type[] array = type.GetInterfaces();
                    foreach (Type tmp in array)
                    {
                        if(tmp.Name == baseType.Name)
                        {
                            t = tmp;
                            break;
                        }
                    }
#else
                    t = type.GetInterface(baseType.Name);
#endif
                    if (t != null)
                    {
                        return true;
                    }
                    else
                    {
                        return IsNestedBaseType(type.BaseType, baseType);
                    }
                }
                else
                {
                    if (type.BaseType == baseType)
                    {
                        return true;
                    }
                    else
                    {
                        return IsNestedBaseType(type.BaseType, baseType);
                    }
                }
            }
            return false;
        }

        private static void LoadAssembly()
        {
            List<string> libs = GetLibs();

            for (int i = 0; i < libs.Count; i++)
            {
                string asmFile = libs[i].Trim();
                if (AssemblyTable.ContainsKey(asmFile))
                {
                    continue;
                }

                try
                {
                    string fullPath = dllStoreFile + asmFile;
                    Assembly asm;
                    try
                    {
                        asm = GetAssemblyByFullPathFile(fullPath);
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    AssemblyTable.Add(asmFile, asm);

                    Type[] array = asm.GetTypes();
                    foreach (Type type in array)
                    {
                        if (type.IsPublic && !TypeTable.ContainsKey(type.FullName))
                        {
                            TypeTable.Add(type.FullName, type);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //throw new Exception("��ʼ�����쳣��" + asmFile, ex);
                }
            }
        }

        /// <summary>
        /// ��ȡ����ƽ̨���dll�ļ�������
        /// </summary>
        /// <returns></returns>
        private static List<string> GetLibs()
        {
            List<string> libList = new List<string>();

            // �ȼ����ļ�������
            TextReader tr = null;
            try
            {
                tr = File.OpenText(libConfigFile);
                string all = tr.ReadToEnd();
                all = all.Trim().Replace("\n", "");
                string[] tmp = all.Split('\r');
                libList.AddRange(tmp);
            }
            catch (Exception ex)
            {
                //throw new Exception("���ض�̬�������ļ��쳣:" + ex.Message + "\r\n�����ļ�:" + libConfigFile);
            }
            finally
            {
                if (tr != null)
                {
                    tr.Close();
                    tr = null;
                }
            }

            // �ټ���dll·�����������Ϲ淶���ļ�
            var fileDir = new DirectoryInfo(dllStoreFile);
            foreach (var info in fileDir.GetFiles(dllNameTemplate, SearchOption.TopDirectoryOnly))
            {
                if (!libList.Contains(info.Name))
                {
                    libList.Add(info.Name);
                }
            }

            return libList;
        }

        private static Assembly GetAssemblyByFullPathFile(string fullPathAsmFile)
        {
            try
            {
                Assembly asm = Assembly.LoadFrom(fullPathAsmFile);
                return asm;
            }
            catch (IOException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception("���س����쳣��", ex);
            }
        }
    }
}