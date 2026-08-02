using System;
using System.Collections.Immutable;
using System.Reflection.Metadata;

namespace FsFoxCad.Build;

public sealed class StableSignatureTypeProvider : ISignatureTypeProvider<string, object>
{
    public string GetArrayType(string elementType, ArrayShape shape) =>
        $"{elementType}[rank={shape.Rank};sizes={string.Join(",", shape.Sizes)};lower={string.Join(",", shape.LowerBounds)}]";

    public string GetByReferenceType(string elementType) => elementType + "&";

    public string GetFunctionPointerType(MethodSignature<string> signature) =>
        "fnptr(" + StableMetadataSignature.FormatMethod(signature) + ")";

    public string GetGenericInstantiation(string genericType, ImmutableArray<string> typeArguments) =>
        genericType + "<" + string.Join(",", typeArguments) + ">";

    public string GetGenericMethodParameter(object genericContext, int index) => "!!" + index;

    public string GetGenericTypeParameter(object genericContext, int index) => "!" + index;

    public string GetModifiedType(string modifier, string unmodifiedType, bool isRequired) =>
        (isRequired ? "modreq(" : "modopt(") + modifier + ")" + unmodifiedType;

    public string GetPinnedType(string elementType) => "pinned(" + elementType + ")";

    public string GetPointerType(string elementType) => elementType + "*";

    public string GetPrimitiveType(PrimitiveTypeCode typeCode) => "primitive:" + typeCode;

    public string GetSZArrayType(string elementType) => elementType + "[]";

    public string GetTypeFromDefinition(
        MetadataReader reader,
        TypeDefinitionHandle handle,
        byte rawTypeKind) => Prefix(rawTypeKind) + "[self]" + GetDefinitionName(reader, handle);

    public string GetTypeFromReference(
        MetadataReader reader,
        TypeReferenceHandle handle,
        byte rawTypeKind) => Prefix(rawTypeKind) + GetReferenceName(reader, handle);

    public string GetTypeFromSpecification(
        MetadataReader reader,
        object genericContext,
        TypeSpecificationHandle handle,
        byte rawTypeKind) => reader.GetTypeSpecification(handle).DecodeSignature(this, genericContext);

    private static string Prefix(byte rawTypeKind)
    {
        if (rawTypeKind == (byte)SignatureTypeKind.ValueType)
        {
            return "valuetype:";
        }
        if (rawTypeKind == (byte)SignatureTypeKind.Class)
        {
            return "class:";
        }
        return "type:";
    }

    private static string GetDefinitionName(MetadataReader reader, TypeDefinitionHandle handle)
    {
        var definition = reader.GetTypeDefinition(handle);
        var name = reader.GetString(definition.Name);
        var declaringType = definition.GetDeclaringType();
        if (!declaringType.IsNil)
        {
            return GetDefinitionName(reader, declaringType) + "+" + name;
        }

        var typeNamespace = reader.GetString(definition.Namespace);
        return string.IsNullOrEmpty(typeNamespace) ? name : typeNamespace + "." + name;
    }

    private static string GetReferenceName(MetadataReader reader, TypeReferenceHandle handle)
    {
        var reference = reader.GetTypeReference(handle);
        var name = reader.GetString(reference.Name);
        if (reference.ResolutionScope.Kind == HandleKind.TypeReference)
        {
            return GetReferenceName(reader, (TypeReferenceHandle)reference.ResolutionScope) + "+" + name;
        }

        var typeNamespace = reader.GetString(reference.Namespace);
        var fullName = string.IsNullOrEmpty(typeNamespace) ? name : typeNamespace + "." + name;
        if (reference.ResolutionScope.Kind == HandleKind.AssemblyReference)
        {
            var assembly = reader.GetAssemblyReference((AssemblyReferenceHandle)reference.ResolutionScope);
            return "[" + reader.GetString(assembly.Name) + "]" + fullName;
        }
        if (reference.ResolutionScope.Kind == HandleKind.ModuleReference)
        {
            var module = reader.GetModuleReference((ModuleReferenceHandle)reference.ResolutionScope);
            return "[module:" + reader.GetString(module.Name) + "]" + fullName;
        }
        return "[self]" + fullName;
    }
}

public static class StableMetadataSignature
{
    private static readonly StableSignatureTypeProvider Provider = new();

    public static string FormatMethod(MethodSignature<string> signature) =>
        $"header={signature.Header.RawValue:x2};generic={signature.GenericParameterCount};" +
        $"required={signature.RequiredParameterCount};return={signature.ReturnType};" +
        $"params=({string.Join(",", signature.ParameterTypes)})";

    public static string DecodeMethod(MetadataReader reader, MethodDefinitionHandle handle) =>
        FormatMethod(reader.GetMethodDefinition(handle).DecodeSignature(Provider, null));

    public static string DecodeMemberReferenceMethod(MetadataReader reader, MemberReferenceHandle handle) =>
        FormatMethod(reader.GetMemberReference(handle).DecodeMethodSignature(Provider, null));

    public static string DecodeField(MetadataReader reader, FieldDefinitionHandle handle) =>
        reader.GetFieldDefinition(handle).DecodeSignature(Provider, null);

    public static string DecodeProperty(MetadataReader reader, PropertyDefinitionHandle handle) =>
        FormatMethod(reader.GetPropertyDefinition(handle).DecodeSignature(Provider, null));

    public static string DecodeTypeSpecification(MetadataReader reader, TypeSpecificationHandle handle) =>
        reader.GetTypeSpecification(handle).DecodeSignature(Provider, null);
}
