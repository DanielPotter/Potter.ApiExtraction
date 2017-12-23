using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace Potter.ApiExtraction.Core.Generation
{
    internal static class ReflectionExtensions
    {
        private const BindingFlags AnyMemberFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        public static MemberExtensionKind GetExtensionKind(this EventInfo eventInfo)
        {
            EventInfo locateBase(Type baseType)
            {
                if (baseType == null)
                {
                    return null;
                }

                EventInfo baseEventInfo = baseType.GetEvent(eventInfo.Name, AnyMemberFlags);
                if (baseEventInfo != null)
                {
                    return baseEventInfo;
                }

                return locateBase(baseType.BaseType);
            }

            MethodInfo firstAccessor = eventInfo.GetAddMethod();

            // Check if the event is an override. (https://stackoverflow.com/a/16530993/2503153
            // 11/8/2017)
            if (firstAccessor.GetBaseDefinition().DeclaringType != firstAccessor.DeclaringType)
            {
                return MemberExtensionKind.Override;
            }

            MethodInfo baseEventAccessor = locateBase(eventInfo.DeclaringType?.BaseType)?.GetAddMethod();
            bool hasBase = baseEventAccessor != null;

            if (hasBase)
            {
                return MemberExtensionKind.New;
            }

            return MemberExtensionKind.Normal;
        }

        public static MemberExtensionKind GetExtensionKind(this PropertyInfo propertyInfo)
        {
            PropertyInfo locateBase(Type baseType)
            {
                if (baseType == null)
                {
                    return null;
                }

                PropertyInfo basePropertyInfo = baseType.GetProperty(propertyInfo.Name, AnyMemberFlags);
                if (basePropertyInfo != null)
                {
                    return basePropertyInfo;
                }

                return locateBase(baseType.BaseType);
            }

            MethodInfo firstAccessor = propertyInfo.GetAccessors()[0];

            // Check if the property is an override. (https://stackoverflow.com/a/16530993/2503153
            // 11/8/2017)
            if (firstAccessor.GetBaseDefinition().DeclaringType != firstAccessor.DeclaringType)
            {
                return MemberExtensionKind.Override;
            }

            MethodInfo basePropertyAccessor = locateBase(propertyInfo.DeclaringType?.BaseType)?.GetAccessors()[0];
            bool hasBase = basePropertyAccessor != null;

            if (hasBase)
            {
                return MemberExtensionKind.New;
            }

            return MemberExtensionKind.Normal;
        }

        public static MemberExtensionKind GetExtensionKind(this MethodInfo methodInfo)
        {
            // Check if the method is an override. (https://stackoverflow.com/a/16530993/2503153
            // 11/8/2017)
            if (methodInfo.GetBaseDefinition().DeclaringType != methodInfo.DeclaringType)
            {
                return MemberExtensionKind.Override;
            }

            Type[] parameterTypes = methodInfo.GetParameters().Select(parameter => parameter.ParameterType).ToArray();
            MethodInfo locateBase(Type baseType)
            {
                if (baseType == null)
                {
                    return null;
                }

                MethodInfo basePropertyInfo = baseType.GetMethod(methodInfo.Name, AnyMemberFlags, null, parameterTypes, null);
                if (basePropertyInfo != null)
                {
                    return basePropertyInfo;
                }

                return locateBase(baseType.BaseType);
            }

            MethodInfo baseMethod = locateBase(methodInfo.DeclaringType?.BaseType);
            bool hasBase = baseMethod != null;

            if (hasBase)
            {
                return MemberExtensionKind.New;
            }

            return MemberExtensionKind.Normal;
        }

        public static bool IsDeprecated(this MemberInfo memberInfo, out string reason)
        {
            string innerReason = null;

            bool isDeprecated = HasAttribute(memberInfo, hasAttributeTest, checkAccessors: true);

            reason = innerReason;

            return isDeprecated;

            bool hasAttributeTest(CustomAttributeData attributeData)
            {
                if (typeof(ObsoleteAttribute).IsEquivalentTo(attributeData.AttributeType)
                    || attributeData.AttributeType.FullName == "Windows.Foundation.Metadata.DeprecatedAttribute")
                {
                    if (attributeData.ConstructorArguments?.Count > 0)
                    {
                        innerReason = attributeData.ConstructorArguments[0].Value?.ToString();
                    }

                    return true;
                }

                return false;
            }
        }

        public static bool HasAttribute<T>(this MemberInfo memberInfo, bool checkAccessors)
            where T : Attribute
        {
            Type attributeType = typeof(T);

            Type declaringType = memberInfo as Type ?? memberInfo.DeclaringType;
            bool isReflectionOnly = declaringType.Assembly.ReflectionOnly;

            return hasAttribute(memberInfo, hasAttributeTest, checkAccessors);

            bool hasAttributeTest(MemberInfo innerMemberInfo)
            {
                if (isReflectionOnly)
                {
                    IList<CustomAttributeData> customAttributes = innerMemberInfo.GetCustomAttributesData();

                    return customAttributes.Any(attribute => attributeType.IsEquivalentTo(attribute.AttributeType));
                }
                else
                {
                    return innerMemberInfo.IsDefined(attributeType);
                }
            }
        }

        public static bool HasAttribute(this MemberInfo memberInfo, Func<CustomAttributeData, bool> attributeTest, bool checkAccessors)
        {
            return hasAttribute(memberInfo, hasAttributeTest, checkAccessors);

            bool hasAttributeTest(MemberInfo innerMemberInfo)
            {
                IList<CustomAttributeData> customAttributes = innerMemberInfo.GetCustomAttributesData();

                return customAttributes.Any(attributeTest);
            }
        }

        private static bool hasAttribute(MemberInfo memberInfo, Func<MemberInfo, bool> attributeTest, bool checkAccessors)
        {
            if (attributeTest(memberInfo))
            {
                return true;
            }

            if (checkAccessors)
            {
                switch (memberInfo.MemberType)
                {
                    case MemberTypes.Event:
                        EventInfo eventInfo = (EventInfo) memberInfo;
                        return attributeTest(eventInfo.AddMethod) || attributeTest(eventInfo.RemoveMethod);

                    case MemberTypes.Property:
                        return ((PropertyInfo) memberInfo).GetAccessors().Any(attributeTest);

                    default:
                        return false;
                }
            }

            return false;
        }

        public static bool HasAttribute<T>(this ParameterInfo parameterInfo)
            where T : Attribute
        {
            Type attributeType = typeof(T);

            Type declaringType = parameterInfo.Member as Type ?? parameterInfo.Member.DeclaringType;
            bool isReflectionOnly = declaringType.Assembly.ReflectionOnly;

            if (isReflectionOnly)
            {
                IList<CustomAttributeData> customAttributes = parameterInfo.GetCustomAttributesData();

                return customAttributes.Any(attribute => attributeType.IsEquivalentTo(attribute.AttributeType));
            }
            else
            {
                return parameterInfo.IsDefined(attributeType);
            }
        }

        public static bool HasAttribute(this ParameterInfo parameterInfo, Func<CustomAttributeData, bool> attributeTest)
        {
            IList<CustomAttributeData> customAttributes = parameterInfo.GetCustomAttributesData();

            return customAttributes.Any(attributeTest);
        }
    }
}
