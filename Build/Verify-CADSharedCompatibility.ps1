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
        packageId = 'IFox.CAD.ACAD2019'
    },
    [pscustomobject]@{
        name = 'AC_2025'
        project = 'src\Fs.Fox.AutoCad2025\Fs.Fox.AutoCad2025.csproj'
        assembly = 'Build\AC_2025_Release\Fs.Fox.AutoCad.dll'
        packageId = 'IFox.CAD.ACAD2025'
    },
    [pscustomobject]@{
        name = 'ZW_2022'
        project = 'src\Fs.Fox.ZwCad2022\Fs.Fox.ZwCad2022.csproj'
        assembly = 'Build\ZW_2022_Release\Fs.Fox.ZwCad.dll'
        packageId = 'IFox.CAD.ZCAD2022'
    },
    [pscustomobject]@{
        name = 'ZW_2025'
        project = 'src\Fs.Fox.ZwCad2025\Fs.Fox.ZwCad2025.csproj'
        assembly = 'Build\ZW_2025_Release\Fs.Fox.ZwCad.dll'
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

function Get-CustomAttributes {
    param(
        [Reflection.Metadata.MetadataReader]$Reader,
        $Handles
    )

    $items = foreach ($handle in $Handles) {
        $attribute = $Reader.GetCustomAttribute($handle)
        '{0:x8}:{1}' -f (Get-MetadataToken $attribute.Constructor),
            (Convert-BytesToHex $Reader.GetBlobBytes($attribute.Value))
    }
    return @($items | Sort-Object)
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
            customAttributes = Get-CustomAttributes $Reader $parameter.GetCustomAttributes()
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
            customAttributes = Get-CustomAttributes $Reader $type.GetCustomAttributes()
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
                    customAttributes = Get-CustomAttributes $Reader $parameter.GetCustomAttributes()
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
                customAttributes = Get-CustomAttributes $Reader $method.GetCustomAttributes()
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
                customAttributes = Get-CustomAttributes $Reader $field.GetCustomAttributes()
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
                customAttributes = Get-CustomAttributes $Reader $property.GetCustomAttributes()
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
                customAttributes = Get-CustomAttributes $Reader $event.GetCustomAttributes()
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

function Get-PackageSnapshot {
    param([string]$Path)

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
            $hash = $null
            if (-not $pathName.EndsWith('.dll', [StringComparison]::OrdinalIgnoreCase) -and
                -not $pathName.EndsWith('.pdb', [StringComparison]::OrdinalIgnoreCase)) {
                $entryStream = $entry.Open()
                try {
                    $hash = Get-StreamHash $entryStream
                }
                finally {
                    $entryStream.Dispose()
                }
            }
            [ordered]@{
                path = $pathName
                length = $entry.Length
                contentSha256 = $hash
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
        [ordered]@{
            name = $target.name
            assembly = Get-AssemblySnapshot $assemblyPath
            package = Get-PackageSnapshot $packages[0].FullName
        }
    }

    $snapshot = [ordered]@{
        schemaVersion = 1
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
            throw 'Current public API, assembly references, or package layout differs from the committed baseline.'
        }
    }
}
finally {
    if (Test-Path -LiteralPath $tempRoot) {
        Remove-Item -LiteralPath $tempRoot -Recurse -Force
    }
}

Write-Host "CADShared compatibility verification passed for $($targets.Count) targets."
