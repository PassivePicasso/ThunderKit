using System;
using System.Collections.Generic;
using System.Text;

namespace AssetsTools.NET.MonoReflection
{
    internal struct TypeDefWithSelfRef
    {
        public Type typeRef;
        public Type typeDef;

        public Dictionary<string, TypeDefWithSelfRef> typeParamToArg;

        public TypeDefWithSelfRef(Type typeRef)
        {
            this.typeRef = typeRef;
            typeDef = typeRef.IsGenericType ? typeRef.GetGenericTypeDefinition() : typeRef;
            typeParamToArg = new Dictionary<string, TypeDefWithSelfRef>();

            Type tRef = typeRef;
            Type tDef = typeDef;

            if (tRef.IsArray)
            {
                typeDef = typeDef.GetElementType();

                tRef = tRef.GetElementType();
                tDef = typeDef.IsGenericType ? typeDef.GetGenericTypeDefinition() : typeDef;
            }

            if (tRef.IsGenericType)
            {
                var defGenericArguments = tDef.GetGenericArguments();
                var genericArguments = tRef.GetGenericArguments();
                for (int i = 0; i < genericArguments.Length; i++)
                {
                    typeParamToArg.Add(defGenericArguments[i].Name, new TypeDefWithSelfRef(genericArguments[i]));
                }
            }
        }

        public void AssignTypeParams(TypeDefWithSelfRef parentTypeDef)
        {
            if (parentTypeDef.typeParamToArg.Count > 0 && typeRef.IsGenericType)
            {
                var genericArguments = typeRef.GetGenericArguments();
                for (int i = 0; i < genericArguments.Length; i++)
                {
                    Type genTypeRef = genericArguments[i];
                    if (genTypeRef.IsGenericParameter)
                    {
                        if (parentTypeDef.typeParamToArg.TryGetValue(genTypeRef.Name, out TypeDefWithSelfRef mappedType))
                        {
                            typeParamToArg[genericArguments[i].Name] = mappedType;
                        }
                    }
                }
            }
        }

        public TypeDefWithSelfRef SolidifyType(TypeDefWithSelfRef typeDef)
        {
            if (typeParamToArg.TryGetValue(typeDef.typeRef.Name, out TypeDefWithSelfRef retType))
            {
                return retType;
            }

            return typeDef;
        }

        public static implicit operator TypeDefWithSelfRef(Type typeReference)
        {
            return new TypeDefWithSelfRef(typeReference);
        }
    }
}