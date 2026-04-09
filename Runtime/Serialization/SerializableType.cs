// TecVooDoo Games
// Copyright (c) 2026 TecVooDoo LLC. All rights reserved.
// Based on SerializableType by Adam Myhre (adammyhre)

using System;
using System.Linq;
using UnityEngine;

namespace TecVooDoo.Games
{
    /// <summary>
    /// Serializable wrapper for System.Type. Stores the assembly-qualified name
    /// and provides implicit conversions to/from Type.
    /// </summary>
    [Serializable]
    public class SerializableType : ISerializationCallbackReceiver
    {
        [SerializeField] string assemblyQualifiedName = string.Empty;

        public Type Type { get; private set; }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            assemblyQualifiedName = Type?.AssemblyQualifiedName ?? assemblyQualifiedName;
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (!TryGetType(assemblyQualifiedName, out Type type))
            {
                Debug.LogError($"Type {assemblyQualifiedName} not found");
                return;
            }
            Type = type;
        }

        static bool TryGetType(string typeString, out Type type)
        {
            type = Type.GetType(typeString);
            return type != null || !string.IsNullOrEmpty(typeString);
        }

        public static implicit operator Type(SerializableType sType) => sType.Type;
        public static implicit operator SerializableType(Type type) => new SerializableType { Type = type };
    }

    /// <summary>
    /// Attribute to filter types shown in the SerializableType inspector dropdown.
    /// </summary>
    public class TypeFilterAttribute : PropertyAttribute
    {
        public Func<Type, bool> Filter { get; }

        public TypeFilterAttribute(Type filterType)
        {
            Filter = type => !type.IsAbstract
                          && !type.IsInterface
                          && !type.IsGenericType
                          && type.InheritsOrImplements(filterType);
        }
    }

    /// <summary>
    /// Extension methods for System.Type inspection.
    /// </summary>
    public static class TypeExtensions
    {
        public static bool InheritsOrImplements(this Type type, Type baseType)
        {
            type = ResolveGenericType(type);
            baseType = ResolveGenericType(baseType);

            while (type != typeof(object))
            {
                if (baseType == type || HasAnyInterfaces(type, baseType)) return true;
                type = ResolveGenericType(type.BaseType);
                if (type == null) return false;
            }

            return false;
        }

        static Type ResolveGenericType(Type type)
        {
            if (type == null || !type.IsGenericType) return type;
            Type genericType = type.GetGenericTypeDefinition();
            return genericType != type ? genericType : type;
        }

        static bool HasAnyInterfaces(Type type, Type interfaceType)
        {
            Type[] interfaces = type.GetInterfaces();
            for (int i = 0; i < interfaces.Length; i++)
            {
                if (ResolveGenericType(interfaces[i]) == interfaceType) return true;
            }
            return false;
        }
    }
}

#if UNITY_EDITOR
namespace TecVooDoo.Games.Editor
{
    using UnityEditor;

    [CustomPropertyDrawer(typeof(SerializableType))]
    public class SerializableTypeDrawer : PropertyDrawer
    {
        TypeFilterAttribute typeFilter;
        string[] typeNames;
        string[] typeFullNames;

        void Initialize()
        {
            if (typeFullNames != null) return;

            typeFilter = (TypeFilterAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(TypeFilterAttribute));

            System.Reflection.Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            System.Collections.Generic.List<Type> filteredTypes = new System.Collections.Generic.List<Type>();

            for (int i = 0; i < assemblies.Length; i++)
            {
                Type[] types = assemblies[i].GetTypes();
                for (int j = 0; j < types.Length; j++)
                {
                    Type t = types[j];
                    bool passes = typeFilter == null ? DefaultFilter(t) : typeFilter.Filter(t);
                    if (passes) filteredTypes.Add(t);
                }
            }

            typeNames = new string[filteredTypes.Count];
            typeFullNames = new string[filteredTypes.Count];
            for (int i = 0; i < filteredTypes.Count; i++)
            {
                Type t = filteredTypes[i];
                typeNames[i] = t.ReflectedType == null ? t.Name : $"{t.ReflectedType.Name}.{t.Name}";
                typeFullNames[i] = t.AssemblyQualifiedName;
            }
        }

        static bool DefaultFilter(Type type)
        {
            return !type.IsAbstract && !type.IsInterface && !type.IsGenericType;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize();
            SerializedProperty typeIdProperty = property.FindPropertyRelative("assemblyQualifiedName");

            if (string.IsNullOrEmpty(typeIdProperty.stringValue) && typeFullNames.Length > 0)
            {
                typeIdProperty.stringValue = typeFullNames[0];
                property.serializedObject.ApplyModifiedProperties();
            }

            int currentIndex = Array.IndexOf(typeFullNames, typeIdProperty.stringValue);
            int selectedIndex = EditorGUI.Popup(position, label.text, currentIndex, typeNames);

            if (selectedIndex >= 0 && selectedIndex != currentIndex)
            {
                typeIdProperty.stringValue = typeFullNames[selectedIndex];
                property.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
#endif
