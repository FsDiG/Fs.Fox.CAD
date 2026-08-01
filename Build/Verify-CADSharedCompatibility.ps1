[CmdletBinding()]
param(
    [string]$BaselinePath,
    [switch]$UpdateBaseline
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($BaselinePath)) {
    $BaselinePath = Join-Path $PSScriptRoot 'CADSharedCompatibilityBaseline.json'
}
elseif (-not [IO.Path]::IsPathRooted($BaselinePath)) {
    $BaselinePath = Join-Path $repoRoot $BaselinePath
}
$BaselinePath = [IO.Path]::GetFullPath($BaselinePath)

$targets = @(
    [pscustomobject]@{
        name = 'AC_2019'
        project = 'src\Fs.Fox.AutoCad2019\Fs.Fox.AutoCad2019.csproj'
        assembly = 'Build\AC_2019_Release\Fs.Fox.AutoCad.dll'
        testAssembly = 'Build\AC_2019_Release\TestAcad2019.dll'
        packageId = 'IFox.CAD.ACAD2019'
    },
    [pscustomobject]@{
        name = 'AC_2025'
        project = 'src\Fs.Fox.AutoCad2025\Fs.Fox.AutoCad2025.csproj'
        assembly = 'Build\AC_2025_Release\Fs.Fox.AutoCad.dll'
        testAssembly = 'Build\AC_2025_Release\TestAcad2025.dll'
        packageId = 'IFox.CAD.ACAD2025'
    },
    [pscustomobject]@{
        name = 'ZW_2022'
        project = 'src\Fs.Fox.ZwCad2022\Fs.Fox.ZwCad2022.csproj'
        assembly = 'Build\ZW_2022_Release\Fs.Fox.ZwCad.dll'
        testAssembly = 'Build\ZW_2022_Release\TestZcad2022.dll'
        packageId = 'IFox.CAD.ZCAD2022'
    },
    [pscustomobject]@{
        name = 'ZW_2025'
        project = 'src\Fs.Fox.ZwCad2025\Fs.Fox.ZwCad2025.csproj'
        assembly = 'Build\ZW_2025_Release\Fs.Fox.ZwCad.dll'
        testAssembly = 'Build\ZW_2025_Release\TestZcad2025.dll'
        packageId = 'IFox.CAD.ZCAD2025'
    }
)

function Convert-BytesToHex {
    param([byte[]]$Bytes)
    if ($null -eq $Bytes -or $Bytes.Length -eq 0) {
        return ''
    }
    return ([Convert]::ToHexString($Bytes)).ToLowerInvariant()
}

function Get-StreamHash {
    param([IO.Stream]$Stream)

    $sha256 = [Security.Cryptography.SHA256]::Create()
    try {
        return (Convert-BytesToHex $sha256.ComputeHash($Stream))
    }
    finally {
        $sha256.Dispose()
    }
}

function Get-TextHash {
    param([string]$Text)

    $bytes = [Text.UTF8Encoding]::new($false).GetBytes($Text)
    $stream = [IO.MemoryStream]::new($bytes, $false)
    try {
        return Get-StreamHash $stream
    }
    finally {
        $stream.Dispose()
    }
}

function Get-SnapshotValueKind {
    param($Value)

    if ($null -eq $Value) {
        return 'null'
    }
    if ($Value -is [Array]) {
        return 'array'
    }
    if ($Value -is [Management.Automation.PSCustomObject]) {
        return 'object'
    }
    return 'scalar'
}

function Format-SnapshotValue {
    param($Value)

    if ($null -eq $Value) {
        return 'null'
    }
    $text = $Value | ConvertTo-Json -Depth 20 -Compress
    if ($text.Length -le 320) {
        return $text
    }
    return $text.Substring(0, 317) + '...'
}

function Get-FirstSnapshotDifference {
    param(
        $Expected,
        $Actual,
        [string]$Path
    )

    $expectedKind = Get-SnapshotValueKind $Expected
    $actualKind = Get-SnapshotValueKind $Actual
    if ($expectedKind -cne $actualKind) {
        return [pscustomobject]@{
            path = $Path
            expected = Format-SnapshotValue $Expected
            actual = Format-SnapshotValue $Actual
        }
    }

    if ($expectedKind -eq 'null') {
        return $null
    }

    if ($expectedKind -eq 'array') {
        $sharedCount = [Math]::Min($Expected.Count, $Actual.Count)
        for ($index = 0; $index -lt $sharedCount; $index++) {
            $difference = Get-FirstSnapshotDifference $Expected[$index] $Actual[$index] "$Path[$index]"
            if ($null -ne $difference) {
                return $difference
            }
        }
        if ($Expected.Count -ne $Actual.Count) {
            return [pscustomobject]@{
                path = "$Path.Count"
                expected = $Expected.Count.ToString()
                actual = $Actual.Count.ToString()
            }
        }
        return $null
    }

    if ($expectedKind -eq 'object') {
        $expectedProperties = @($Expected.PSObject.Properties.Name)
        $actualProperties = @($Actual.PSObject.Properties.Name)
        $propertyNames = @($expectedProperties + $actualProperties | Sort-Object -Unique)
        foreach ($propertyName in $propertyNames) {
            if ($propertyName -notin $expectedProperties) {
                return [pscustomobject]@{
                    path = "$Path.$propertyName"
                    expected = '<missing>'
                    actual = Format-SnapshotValue $Actual.$propertyName
                }
            }
            if ($propertyName -notin $actualProperties) {
                return [pscustomobject]@{
                    path = "$Path.$propertyName"
                    expected = Format-SnapshotValue $Expected.$propertyName
                    actual = '<missing>'
                }
            }
            $difference = Get-FirstSnapshotDifference $Expected.$propertyName $Actual.$propertyName "$Path.$propertyName"
            if ($null -ne $difference) {
                return $difference
            }
        }
        return $null
    }

    $expectedJson = $Expected | ConvertTo-Json -Compress
    $actualJson = $Actual | ConvertTo-Json -Compress
    if ($expectedJson -cne $actualJson) {
        return [pscustomobject]@{
            path = $Path
            expected = $expectedJson
            actual = $actualJson
        }
    }
    return $null
}

function Add-SnapshotCategoryDifference {
    param(
        [Collections.Generic.List[object]]$Differences,
        [string]$Target,
        [string]$Category,
        $Expected,
        $Actual
    )

    $difference = Get-FirstSnapshotDifference $Expected $Actual $Category
    if ($null -ne $difference) {
        $Differences.Add([pscustomobject]@{
                target = $Target
                category = $Category
                path = $difference.path
                expected = $difference.expected
                actual = $difference.actual
            }) | Out-Null
    }
}

function Get-CompatibilityDifferences {
    param(
        $Expected,
        $Actual
    )

    $differences = [Collections.Generic.List[object]]::new()
    Add-SnapshotCategoryDifference $differences '<root>' 'schemaVersion' $Expected.schemaVersion $Actual.schemaVersion

    foreach ($expectedTarget in $Expected.targets) {
        $actualTargets = @($Actual.targets | Where-Object { $_.name -ceq $expectedTarget.name })
        if ($actualTargets.Count -ne 1) {
            $differences.Add([pscustomobject]@{
                    target = $expectedTarget.name
                    category = 'target'
                    path = 'target'
                    expected = 'exactly one target'
                    actual = "$($actualTargets.Count) targets"
                }) | Out-Null
            continue
        }

        $actualTarget = $actualTargets[0]
        Add-SnapshotCategoryDifference $differences $expectedTarget.name 'identity' $expectedTarget.assembly.identity $actualTarget.assembly.identity
        Add-SnapshotCategoryDifference $differences $expectedTarget.name 'references' $expectedTarget.assembly.references $actualTarget.assembly.references
        $expectedPublicApi = [pscustomobject]@{
            recordCount = $expectedTarget.assembly.publicApiRecordCount
            types = $expectedTarget.assembly.publicApi
        }
        $actualPublicApi = [pscustomobject]@{
            recordCount = $actualTarget.assembly.publicApiRecordCount
            types = $actualTarget.assembly.publicApi
        }
        Add-SnapshotCategoryDifference $differences $expectedTarget.name 'publicApi' $expectedPublicApi $actualPublicApi
        $expectedPackageMetadata = [pscustomobject]@{
            id = $expectedTarget.package.id
            version = $expectedTarget.package.version
            repository = $expectedTarget.package.repository
            dependencies = $expectedTarget.package.dependencies
            frameworkReferences = $expectedTarget.package.frameworkReferences
        }
        $actualPackageMetadata = [pscustomobject]@{
            id = $actualTarget.package.id
            version = $actualTarget.package.version
            repository = $actualTarget.package.repository
            dependencies = $actualTarget.package.dependencies
            frameworkReferences = $actualTarget.package.frameworkReferences
        }
        Add-SnapshotCategoryDifference $differences $expectedTarget.name 'packageMetadata' $expectedPackageMetadata $actualPackageMetadata
        Add-SnapshotCategoryDifference $differences $expectedTarget.name 'packageAssets' $expectedTarget.package.assets $actualTarget.package.assets
    }

    $expectedTargetNames = @($Expected.targets.name)
    foreach ($actualTarget in $Actual.targets) {
        if ($actualTarget.name -notin $expectedTargetNames) {
            $differences.Add([pscustomobject]@{
                    target = $actualTarget.name
                    category = 'target'
                    path = 'target'
                    expected = '<missing>'
                    actual = 'unexpected target'
                }) | Out-Null
        }
    }

    return $differences
}

function Get-MetadataToken {
    param([Reflection.Metadata.Handle]$Handle)
    return [Reflection.Metadata.Ecma335.MetadataTokens]::GetToken($Handle)
}

function Get-TypeDefinitionName {
    param(
        [Reflection.Metadata.MetadataReader]$Reader,
        [Reflection.Metadata.TypeDefinitionHandle]$Handle,
        [hashtable]$Cache
    )

    $token = Get-MetadataToken $Handle
    $key = 'd:{0:x8}' -f $token
    if ($Cache.ContainsKey($key)) {
        return $Cache[$key]
    }

    $definition = $Reader.GetTypeDefinition($Handle)
    $name = $Reader.GetString($definition.Name)
    $declaringType = $definition.GetDeclaringType()
    if (-not $declaringType.IsNil) {
        $fullName = (Get-TypeDefinitionName $Reader $declaringType $Cache) + '+' + $name
    }
    else {
        $namespace = $Reader.GetString($definition.Namespace)
        $fullName = if ([string]::IsNullOrEmpty($namespace)) { $name } else { "$namespace.$name" }
    }
    $Cache[$key] = $fullName
    return $fullName
}

function Get-TypeReferenceName {
    param(
        [Reflection.Metadata.MetadataReader]$Reader,
        [Reflection.Metadata.TypeReferenceHandle]$Handle,
        [hashtable]$Cache
    )

    $token = Get-MetadataToken $Handle
    $key = 'r:{0:x8}' -f $token
    if ($Cache.ContainsKey($key)) {
        return $Cache[$key]
    }

    $reference = $Reader.GetTypeReference($Handle)
    $name = $Reader.GetString($reference.Name)
    if ($reference.ResolutionScope.Kind -eq [Reflection.Metadata.HandleKind]::TypeReference) {
        $declaringType = [Reflection.Metadata.TypeReferenceHandle]$reference.ResolutionScope
        $fullName = (Get-TypeReferenceName $Reader $declaringType $Cache) + '+' + $name
    }
    else {
        $namespace = $Reader.GetString($reference.Namespace)
        $fullName = if ([string]::IsNullOrEmpty($namespace)) { $name } else { "$namespace.$name" }
    }
    $Cache[$key] = $fullName
    return $fullName
}

function Get-TypeHandleName {
    param(
        [Reflection.Metadata.MetadataReader]$Reader,
        [Reflection.Metadata.EntityHandle]$Handle,
        [hashtable]$Cache
    )

    if ($Handle.IsNil) {
        return $null
    }
    switch ($Handle.Kind) {
        ([Reflection.Metadata.HandleKind]::TypeDefinition) {
            return Get-TypeDefinitionName $Reader ([Reflection.Metadata.TypeDefinitionHandle]$Handle) $Cache
        }
        ([Reflection.Metadata.HandleKind]::TypeReference) {
            return Get-TypeReferenceName $Reader ([Reflection.Metadata.TypeReferenceHandle]$Handle) $Cache
        }
        ([Reflection.Metadata.HandleKind]::TypeSpecification) {
            $specification = $Reader.GetTypeSpecification([Reflection.Metadata.TypeSpecificationHandle]$Handle)
            return 'typespec:' + (Convert-BytesToHex $Reader.GetBlobBytes($specification.Signature))
        }
        default {
            return 'handle:{0}:{1:x8}' -f $Handle.Kind, (Get-MetadataToken $Handle)
        }
    }
}

function Get-CustomAttributeConstructorIdentity {
    param(
        [Reflection.Metadata.MetadataReader]$Reader,
        [Reflection.Metadata.EntityHandle]$Handle,
        [hashtable]$TypeNameCache
    )

    switch ($Handle.Kind) {
        ([Reflection.Metadata.HandleKind]::MethodDefinition) {
            $method = $Reader.GetMethodDefinition([Reflection.Metadata.MethodDefinitionHandle]$Handle)
            return [ordered]@{
                type = Get-TypeDefinitionName $Reader $method.GetDeclaringType() $TypeNameCache
                name = $Reader.GetString($method.Name)
                signature = Convert-BytesToHex $Reader.GetBlobBytes($method.Signature)
            }
        }
        ([Reflection.Metadata.HandleKind]::MemberReference) {
            $member = $Reader.GetMemberReference([Reflection.Metadata.MemberReferenceHandle]$Handle)
            return [ordered]@{
                type = Get-TypeHandleName $Reader $member.Parent $TypeNameCache
                name = $Reader.GetString($member.Name)
                signature = Convert-BytesToHex $Reader.GetBlobBytes($member.Signature)
            }
        }
        default {
            throw "Unsupported custom attribute constructor handle: $($Handle.Kind)"
        }
    }
}

function Get-CustomAttributes {
    param(
        [Reflection.Metadata.MetadataReader]$Reader,
        $Handles,
        [hashtable]$TypeNameCache
    )

    $items = foreach ($handle in $Handles) {
        $attribute = $Reader.GetCustomAttribute($handle)
        [ordered]@{
            constructor = Get-CustomAttributeConstructorIdentity $Reader $attribute.Constructor $TypeNameCache
            value = Convert-BytesToHex $Reader.GetBlobBytes($attribute.Value)
        }
    }
    return @($items | Sort-Object {
            $_.constructor.type
        }, {
            $_.constructor.name
        }, {
            $_.constructor.signature
        }, value)
}

function Get-DefaultConstant {
    param(
        [Reflection.Metadata.MetadataReader]$Reader,
        [Reflection.Metadata.ConstantHandle]$Handle
    )

    if ($Handle.IsNil) {
        return $null
    }
    $constant = $Reader.GetConstant($Handle)
    return [ordered]@{
        typeCode = [int]$constant.TypeCode
        value = Convert-BytesToHex $Reader.GetBlobBytes($constant.Value)
    }
}

function Get-GenericParameters {
    param(
        [Reflection.Metadata.MetadataReader]$Reader,
        $Handles,
        [hashtable]$TypeNameCache
    )

    $items = foreach ($handle in $Handles) {
        $parameter = $Reader.GetGenericParameter($handle)
        $constraints = foreach ($constraintHandle in $parameter.GetConstraints()) {
            $constraint = $Reader.GetGenericParameterConstraint($constraintHandle)
            Get-TypeHandleName $Reader $constraint.Type $TypeNameCache
        }
        [ordered]@{
            index = $parameter.Index
            name = $Reader.GetString($parameter.Name)
            attributes = [int]$parameter.Attributes
            constraints = @($constraints | Sort-Object)
            customAttributes = Get-CustomAttributes $Reader $parameter.GetCustomAttributes() $TypeNameCache
        }
    }
    return @($items | Sort-Object index)
}

function Test-MethodVisible {
    param(
        [Reflection.Metadata.MetadataReader]$Reader,
        [Reflection.Metadata.MethodDefinitionHandle]$Handle
    )

    if ($Handle.IsNil) {
        return $false
    }
    $method = $Reader.GetMethodDefinition($Handle)
    $access = $method.Attributes -band [Reflection.MethodAttributes]::MemberAccessMask
    return $access -in @(
        [Reflection.MethodAttributes]::Public,
        [Reflection.MethodAttributes]::Family,
        [Reflection.MethodAttributes]::FamORAssem
    )
}

function Get-PublicApi {
    param([Reflection.Metadata.MetadataReader]$Reader)

    $typeNameCache = @{}
    $records = [Collections.Generic.List[object]]::new()
    foreach ($typeHandle in $Reader.TypeDefinitions) {
        $type = $Reader.GetTypeDefinition($typeHandle)
        $visibility = $type.Attributes -band [Reflection.TypeAttributes]::VisibilityMask
        if ($visibility -notin @(
                [Reflection.TypeAttributes]::Public,
                [Reflection.TypeAttributes]::NestedPublic,
                [Reflection.TypeAttributes]::NestedFamily,
                [Reflection.TypeAttributes]::NestedFamORAssem
            )) {
            continue
        }

        $typeName = Get-TypeDefinitionName $Reader $typeHandle $typeNameCache
        $interfaces = foreach ($implementationHandle in $type.GetInterfaceImplementations()) {
            $implementation = $Reader.GetInterfaceImplementation($implementationHandle)
            Get-TypeHandleName $Reader $implementation.Interface $typeNameCache
        }
        $layout = $type.GetLayout()
        $records.Add([ordered]@{
            kind = 'type'
            declaringType = ''
            name = $typeName
            signature = ''
            attributes = [int]$type.Attributes
            baseType = Get-TypeHandleName $Reader $type.BaseType $typeNameCache
            interfaces = @($interfaces | Sort-Object)
            layout = [ordered]@{
                packingSize = $layout.PackingSize
                size = $layout.Size
            }
            genericParameters = Get-GenericParameters $Reader $type.GetGenericParameters() $typeNameCache
            customAttributes = Get-CustomAttributes $Reader $type.GetCustomAttributes() $typeNameCache
        })

        foreach ($methodHandle in $type.GetMethods()) {
            $method = $Reader.GetMethodDefinition($methodHandle)
            $access = $method.Attributes -band [Reflection.MethodAttributes]::MemberAccessMask
            if ($access -notin @(
                    [Reflection.MethodAttributes]::Public,
                    [Reflection.MethodAttributes]::Family,
                    [Reflection.MethodAttributes]::FamORAssem
                )) {
                continue
            }
            $parameters = foreach ($parameterHandle in $method.GetParameters()) {
                $parameter = $Reader.GetParameter($parameterHandle)
                [ordered]@{
                    sequence = $parameter.SequenceNumber
                    name = $Reader.GetString($parameter.Name)
                    attributes = [int]$parameter.Attributes
                    default = Get-DefaultConstant $Reader $parameter.GetDefaultValue()
                    customAttributes = Get-CustomAttributes $Reader $parameter.GetCustomAttributes() $typeNameCache
                }
            }
            $records.Add([ordered]@{
                kind = 'method'
                declaringType = $typeName
                name = $Reader.GetString($method.Name)
                signature = Convert-BytesToHex $Reader.GetBlobBytes($method.Signature)
                attributes = [int]$method.Attributes
                implementationAttributes = [int]$method.ImplAttributes
                parameters = @($parameters | Sort-Object sequence)
                genericParameters = Get-GenericParameters $Reader $method.GetGenericParameters() $typeNameCache
                customAttributes = Get-CustomAttributes $Reader $method.GetCustomAttributes() $typeNameCache
            })
        }

        foreach ($fieldHandle in $type.GetFields()) {
            $field = $Reader.GetFieldDefinition($fieldHandle)
            $access = $field.Attributes -band [Reflection.FieldAttributes]::FieldAccessMask
            if ($access -notin @(
                    [Reflection.FieldAttributes]::Public,
                    [Reflection.FieldAttributes]::Family,
                    [Reflection.FieldAttributes]::FamORAssem
                )) {
                continue
            }
            $records.Add([ordered]@{
                kind = 'field'
                declaringType = $typeName
                name = $Reader.GetString($field.Name)
                signature = Convert-BytesToHex $Reader.GetBlobBytes($field.Signature)
                attributes = [int]$field.Attributes
                default = Get-DefaultConstant $Reader $field.GetDefaultValue()
                customAttributes = Get-CustomAttributes $Reader $field.GetCustomAttributes() $typeNameCache
            })
        }

        foreach ($propertyHandle in $type.GetProperties()) {
            $property = $Reader.GetPropertyDefinition($propertyHandle)
            $accessors = $property.GetAccessors()
            if (-not (Test-MethodVisible $Reader $accessors.Getter) -and
                -not (Test-MethodVisible $Reader $accessors.Setter)) {
                continue
            }
            $records.Add([ordered]@{
                kind = 'property'
                declaringType = $typeName
                name = $Reader.GetString($property.Name)
                signature = Convert-BytesToHex $Reader.GetBlobBytes($property.Signature)
                attributes = [int]$property.Attributes
                customAttributes = Get-CustomAttributes $Reader $property.GetCustomAttributes() $typeNameCache
            })
        }

        foreach ($eventHandle in $type.GetEvents()) {
            $event = $Reader.GetEventDefinition($eventHandle)
            $accessors = $event.GetAccessors()
            if (-not (Test-MethodVisible $Reader $accessors.Adder) -and
                -not (Test-MethodVisible $Reader $accessors.Remover) -and
                -not (Test-MethodVisible $Reader $accessors.Raiser)) {
                continue
            }
            $records.Add([ordered]@{
                kind = 'event'
                declaringType = $typeName
                name = $Reader.GetString($event.Name)
                signature = Get-TypeHandleName $Reader $event.Type $typeNameCache
                attributes = [int]$event.Attributes
                customAttributes = Get-CustomAttributes $Reader $event.GetCustomAttributes() $typeNameCache
            })
        }
    }

    return @($records | Sort-Object kind, declaringType, name, signature)
}

function Get-AssemblySnapshot {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "Release assembly not found: $Path"
    }
    $assemblyName = [Reflection.AssemblyName]::GetAssemblyName($Path)
    $stream = [IO.File]::OpenRead($Path)
    try {
        $peReader = [Reflection.PortableExecutable.PEReader]::new($stream)
        try {
            if (-not $peReader.HasMetadata) {
                throw "Assembly has no managed metadata: $Path"
            }
            $reader = [Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($peReader)
            $references = foreach ($referenceHandle in $reader.AssemblyReferences) {
                $reference = $reader.GetAssemblyReference($referenceHandle)
                [ordered]@{
                    name = $reader.GetString($reference.Name)
                    version = $reference.Version.ToString()
                    culture = $reader.GetString($reference.Culture)
                    publicKeyOrToken = Convert-BytesToHex $reader.GetBlobBytes($reference.PublicKeyOrToken)
                    flags = [int]$reference.Flags
                }
            }
            $publicApiRecords = Get-PublicApi $reader
            $publicApi = foreach ($group in ($publicApiRecords | Group-Object {
                        if ($_.kind -eq 'type') { $_.name } else { $_.declaringType }
                    })) {
                $contractJson = @($group.Group) | ConvertTo-Json -Depth 20 -Compress
                [ordered]@{
                    type = $group.Name
                    recordCount = $group.Count
                    contractSha256 = Get-TextHash $contractJson
                }
            }
            return [ordered]@{
                identity = [ordered]@{
                    name = $assemblyName.Name
                    version = $assemblyName.Version.ToString()
                    culture = $assemblyName.CultureName ?? ''
                    publicKeyToken = Convert-BytesToHex $assemblyName.GetPublicKeyToken()
                }
                references = @($references | Sort-Object name, version, culture, publicKeyOrToken)
                publicApiRecordCount = $publicApiRecords.Count
                publicApi = @($publicApi | Sort-Object type)
            }
        }
        finally {
            $peReader.Dispose()
        }
    }
    finally {
        $stream.Dispose()
    }
}

function Get-XmlNodeText {
    param(
        [Xml.XmlNode]$Node,
        [string]$XPath
    )
    $selected = $Node.SelectSingleNode($XPath)
    if ($null -eq $selected) {
        return ''
    }
    return $selected.InnerText
}

function Get-CanonicalXmlHash {
    param([IO.Stream]$Stream)

    $document = [Xml.XmlDocument]::new()
    $document.PreserveWhitespace = $false
    $document.Load($Stream)
    $assemblyName = Get-XmlNodeText $document "/*[local-name()='doc']/*[local-name()='assembly']/*[local-name()='name']"
    $members = foreach ($member in $document.SelectNodes("/*[local-name()='doc']/*[local-name()='members']/*[local-name()='member']")) {
        [ordered]@{
            name = $member.GetAttribute('name')
            content = $member.InnerXml
        }
    }
    $canonical = [ordered]@{
        assembly = $assemblyName
        members = @($members | Sort-Object {
                $_.name
            }, {
                $_.content
            } -CaseSensitive)
    }
    return Get-TextHash ($canonical | ConvertTo-Json -Depth 20 -Compress)
}

function Get-PackageSnapshot {
    param(
        [string]$Path,
        [hashtable]$ExpectedBinaryAssets
    )

    $archive = [IO.Compression.ZipFile]::OpenRead($Path)
    try {
        $nuspecEntry = @($archive.Entries | Where-Object { $_.FullName.EndsWith('.nuspec', [StringComparison]::OrdinalIgnoreCase) })
        if ($nuspecEntry.Count -ne 1) {
            throw "Expected one nuspec in $Path, found $($nuspecEntry.Count)."
        }
        $nuspecStream = $nuspecEntry[0].Open()
        try {
            $textReader = [IO.StreamReader]::new($nuspecStream)
            try {
                [xml]$nuspec = $textReader.ReadToEnd()
            }
            finally {
                $textReader.Dispose()
            }
        }
        finally {
            $nuspecStream.Dispose()
        }

        $metadata = $nuspec.SelectSingleNode("/*[local-name()='package']/*[local-name()='metadata']")
        $repository = $metadata.SelectSingleNode("*[local-name()='repository']")
        $dependencies = foreach ($group in $metadata.SelectNodes("*[local-name()='dependencies']/*[local-name()='group']")) {
            $items = foreach ($dependency in $group.SelectNodes("*[local-name()='dependency']")) {
                [ordered]@{
                    id = $dependency.GetAttribute('id')
                    version = $dependency.GetAttribute('version')
                    include = $dependency.GetAttribute('include')
                    exclude = $dependency.GetAttribute('exclude')
                }
            }
            [ordered]@{
                targetFramework = $group.GetAttribute('targetFramework')
                items = @($items | Sort-Object id, version)
            }
        }
        $frameworkReferences = foreach ($group in $metadata.SelectNodes("*[local-name()='frameworkReferences']/*[local-name()='group']")) {
            $items = foreach ($reference in $group.SelectNodes("*[local-name()='frameworkReference']")) {
                $reference.GetAttribute('name')
            }
            [ordered]@{
                targetFramework = $group.GetAttribute('targetFramework')
                items = @($items | Sort-Object)
            }
        }

        $assets = foreach ($entry in $archive.Entries) {
            $pathName = $entry.FullName.Replace('\', '/')
            if ($pathName.EndsWith('.nuspec', [StringComparison]::OrdinalIgnoreCase) -or
                $pathName -eq '[Content_Types].xml' -or
                $pathName.StartsWith('_rels/', [StringComparison]::OrdinalIgnoreCase) -or
                $pathName.StartsWith('package/', [StringComparison]::OrdinalIgnoreCase) -or
                $pathName.EndsWith('/')) {
                continue
            }
            $isBinary = $pathName.EndsWith('.dll', [StringComparison]::OrdinalIgnoreCase) -or
                $pathName.EndsWith('.pdb', [StringComparison]::OrdinalIgnoreCase)
            $hash = $null
            $hashKind = 'raw-sha256'
            $binarySource = $null
            $entryStream = $entry.Open()
            try {
                if ($isBinary) {
                    $fileName = [IO.Path]::GetFileName($pathName)
                    if (-not $ExpectedBinaryAssets.ContainsKey($fileName)) {
                        throw "Package contains an unexpected binary asset: $pathName"
                    }
                    $binarySource = $ExpectedBinaryAssets[$fileName]
                    $sourcePath = Join-Path $repoRoot $binarySource
                    if (-not (Test-Path -LiteralPath $sourcePath -PathType Leaf)) {
                        throw "Expected binary source does not exist: $binarySource"
                    }
                    $packageHash = Get-StreamHash $entryStream
                    $sourceStream = [IO.File]::OpenRead($sourcePath)
                    try {
                        $sourceHash = Get-StreamHash $sourceStream
                    }
                    finally {
                        $sourceStream.Dispose()
                    }
                    if ($packageHash -cne $sourceHash) {
                        throw "Package binary does not match its current build output: $pathName != $binarySource"
                    }
                    $hashKind = 'matches-build-output-sha256'
                }
                elseif ($pathName.EndsWith('.xml', [StringComparison]::OrdinalIgnoreCase)) {
                    $hash = Get-CanonicalXmlHash $entryStream
                    $hashKind = 'canonical-xml-v1'
                }
                else {
                    $hash = Get-StreamHash $entryStream
                }
            }
            finally {
                $entryStream.Dispose()
            }
            [ordered]@{
                path = $pathName
                length = if ($isBinary) { $null } else { $entry.Length }
                hashKind = $hashKind
                contentSha256 = $hash
                binarySource = $binarySource
            }
        }

        return [ordered]@{
            id = Get-XmlNodeText $metadata "*[local-name()='id']"
            version = Get-XmlNodeText $metadata "*[local-name()='version']"
            repository = [ordered]@{
                type = if ($null -eq $repository) { '' } else { $repository.GetAttribute('type') }
                url = if ($null -eq $repository) { '' } else { $repository.GetAttribute('url') }
            }
            dependencies = @($dependencies | Sort-Object targetFramework)
            frameworkReferences = @($frameworkReferences | Sort-Object targetFramework)
            assets = @($assets | Sort-Object path)
        }
    }
    finally {
        $archive.Dispose()
    }
}

function Assert-ReleaseAssembliesAreFresh {
    $projectItemsPath = Join-Path $repoRoot 'src\CADShared\CADShared.projitems'
    [xml]$projectItems = Get-Content -LiteralPath $projectItemsPath -Raw
    $namespaceManager = [Xml.XmlNamespaceManager]::new($projectItems.NameTable)
    $namespaceManager.AddNamespace('m', 'http://schemas.microsoft.com/developer/msbuild/2003')
    $sourceRoot = Split-Path -Parent $projectItemsPath
    $includePrefix = '$(MSBuildThisFileDirectory)'
    $commonInputs = [Collections.Generic.List[string]]::new()
    $commonInputs.Add($projectItemsPath)
    $commonInputs.Add((Join-Path $repoRoot 'src\Directory.Build.props'))
    foreach ($compile in $projectItems.SelectNodes('//m:Compile', $namespaceManager)) {
        $include = $compile.GetAttribute('Include')
        if (-not $include.StartsWith($includePrefix, [StringComparison]::Ordinal)) {
            throw "Cannot freshness-check Compile Include: $include"
        }
        $commonInputs.Add((Join-Path $sourceRoot $include.Substring($includePrefix.Length)))
    }

    foreach ($target in $targets) {
        $inputPaths = @($commonInputs) + @(
            (Join-Path $repoRoot $target.project),
            (Join-Path $repoRoot 'src\IFoxCAD.AutoCad\GlobalUsings.cs'),
            (Join-Path $repoRoot 'src\IFoxCAD.ZwCad\GlobalUsings.cs')
        )
        $latestInput = $inputPaths |
            ForEach-Object { Get-Item -LiteralPath $_ } |
            Sort-Object LastWriteTimeUtc -Descending |
            Select-Object -First 1
        $assembly = Get-Item -LiteralPath (Join-Path $repoRoot $target.assembly) -ErrorAction SilentlyContinue
        if ($null -eq $assembly) {
            throw "Release assembly not found for $($target.name). Build all Release targets before compatibility verification."
        }
        if ($assembly.LastWriteTimeUtc -lt $latestInput.LastWriteTimeUtc) {
            throw "Release assembly for $($target.name) is older than $($latestInput.FullName). Rebuild before compatibility verification."
        }
        if (-not (Test-Path -LiteralPath (Join-Path $repoRoot $target.testAssembly) -PathType Leaf)) {
            throw "Release test assembly not found for $($target.name). Build all Release targets before compatibility verification."
        }
    }
}

$tempBase = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
$tempRoot = [IO.Path]::GetFullPath((Join-Path $tempBase ("FsFoxCadCompatibility-" + [Guid]::NewGuid().ToString('N'))))
if (-not $tempRoot.StartsWith($tempBase, [StringComparison]::OrdinalIgnoreCase)) {
    throw "Temporary package path escaped the system temp directory: $tempRoot"
}
New-Item -ItemType Directory -Path $tempRoot | Out-Null

try {
    Assert-ReleaseAssembliesAreFresh
    foreach ($target in $targets) {
        $projectPath = Join-Path $repoRoot $target.project
        & dotnet pack $projectPath --configuration Release --no-build --output $tempRoot --nologo --verbosity minimal
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet pack failed for $($target.name) with exit code $LASTEXITCODE."
        }
    }

    $targetSnapshots = foreach ($target in $targets) {
        $assemblyPath = Join-Path $repoRoot $target.assembly
        $packages = @(Get-ChildItem -LiteralPath $tempRoot -Filter "$($target.packageId).*.nupkg" -File)
        if ($packages.Count -ne 1) {
            throw "Expected one package for $($target.packageId), found $($packages.Count)."
        }
        $expectedBinaryAssets = @{}
        foreach ($binarySource in @($target.assembly, $target.testAssembly)) {
            $expectedBinaryAssets[[IO.Path]::GetFileName($binarySource)] = $binarySource.Replace('\', '/')
        }
        [ordered]@{
            name = $target.name
            assembly = Get-AssemblySnapshot $assemblyPath
            package = Get-PackageSnapshot $packages[0].FullName $expectedBinaryAssets
        }
    }

    $snapshot = [ordered]@{
        schemaVersion = 2
        targets = @($targetSnapshots)
    }

    if ($UpdateBaseline) {
        $json = $snapshot | ConvertTo-Json -Depth 30
        [IO.File]::WriteAllText($BaselinePath, $json + "`n", [Text.UTF8Encoding]::new($false))
        Write-Host "Updated CADShared compatibility baseline: $BaselinePath"
    }
    else {
        if (-not (Test-Path -LiteralPath $BaselinePath -PathType Leaf)) {
            throw 'Compatibility baseline does not exist. Run this script once with -UpdateBaseline.'
        }
        $baseline = Get-Content -LiteralPath $BaselinePath -Raw | ConvertFrom-Json
        $baselineJson = $baseline | ConvertTo-Json -Depth 30 -Compress
        $snapshotJson = $snapshot | ConvertTo-Json -Depth 30 -Compress
        if ($baselineJson -cne $snapshotJson) {
            $normalizedSnapshot = $snapshotJson | ConvertFrom-Json
            $differences = @(Get-CompatibilityDifferences $baseline $normalizedSnapshot)
            foreach ($difference in $differences) {
                Write-Host "Compatibility difference: target=$($difference.target); category=$($difference.category); path=$($difference.path)"
                Write-Host "  expected: $($difference.expected)"
                Write-Host "  actual:   $($difference.actual)"
            }
            throw "CADShared compatibility baseline differs in $($differences.Count) target/category entries."
        }
    }
}
finally {
    if (Test-Path -LiteralPath $tempRoot) {
        Remove-Item -LiteralPath $tempRoot -Recurse -Force
    }
}

Write-Host "CADShared compatibility verification passed for $($targets.Count) targets."
