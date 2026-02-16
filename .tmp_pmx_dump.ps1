param([string]$pmxPath)
$bytes = [System.IO.File]::ReadAllBytes($pmxPath)
$ofs = 0

function ReadByte1 { $script:bytes[$script:ofs++] }
function ReadInt32LE { $v=[System.BitConverter]::ToInt32($script:bytes,$script:ofs); $script:ofs+=4; $v }
function ReadFloatLE { $v=[System.BitConverter]::ToSingle($script:bytes,$script:ofs); $script:ofs+=4; $v }
function ReadBytesN([int]$n){ $arr=New-Object byte[] $n; [Array]::Copy($script:bytes,$script:ofs,$arr,0,$n); $script:ofs+=$n; $arr }
function ReadText([System.Text.Encoding]$enc){ $len=ReadInt32LE; if($len -le 0){ return '' }; $arr=ReadBytesN $len; $enc.GetString($arr) }
function ReadIndex([int]$size){
  switch($size){
    1 { $raw=[int](ReadByte1); if($raw -ge 128){ return ($raw - 256) } else { return $raw } }
    2 { $v=[System.BitConverter]::ToInt16($script:bytes,$script:ofs); $script:ofs+=2; return [int]$v }
    4 { $v=[System.BitConverter]::ToInt32($script:bytes,$script:ofs); $script:ofs+=4; return [int]$v }
    default { throw "unsupported index size: $size" }
  }
}

$magic=[System.Text.Encoding]::ASCII.GetString((ReadBytesN 4))
if($magic -ne 'PMX '){ throw "not PMX: $magic" }
$version=ReadFloatLE
$headerSize=ReadByte1
$globals=ReadBytesN $headerSize
$enc=if($globals[0]-eq 0){ [System.Text.Encoding]::Unicode } else { [System.Text.Encoding]::UTF8 }
$encName = if($globals[0]-eq 0){ 'UTF16LE' } else { 'UTF8' }
$addUv=[int]$globals[1]
$vertexIndexSize=[int]$globals[2]
$textureIndexSize=[int]$globals[3]
$materialIndexSize=[int]$globals[4]
$boneIndexSize=[int]$globals[5]
$morphIndexSize=[int]$globals[6]
$rigidIndexSize=[int]$globals[7]

$modelNameJa=ReadText $enc
$modelNameEn=ReadText $enc
$commentJa=ReadText $enc
$commentEn=ReadText $enc

$vertexCount=ReadInt32LE
for($i=0;$i -lt $vertexCount;$i++){
  $script:ofs += (3+3+2)*4
  if($addUv -gt 0){ $script:ofs += $addUv*4*4 }
  $deformType=ReadByte1
  switch($deformType){
    0 { [void](ReadIndex $boneIndexSize) }
    1 { [void](ReadIndex $boneIndexSize); [void](ReadIndex $boneIndexSize); $script:ofs += 4 }
    2 { for($k=0;$k -lt 4;$k++){ [void](ReadIndex $boneIndexSize) }; $script:ofs += 16 }
    3 { [void](ReadIndex $boneIndexSize); [void](ReadIndex $boneIndexSize); $script:ofs += (1+9)*4 }
    4 { for($k=0;$k -lt 4;$k++){ [void](ReadIndex $boneIndexSize) }; $script:ofs += 16 }
    default { throw "unsupported deform type: $deformType at vertex $i" }
  }
  $script:ofs += 4
}

$surfaceIndexCount=ReadInt32LE
$script:ofs += $surfaceIndexCount * $vertexIndexSize
$textureCount=ReadInt32LE
$textures=@()
for($i=0;$i -lt $textureCount;$i++){ $textures += ,(ReadText $enc) }

$materialCount=ReadInt32LE
$materials=@()
for($i=0;$i -lt $materialCount;$i++){
  $nameJa=ReadText $enc
  $nameEn=ReadText $enc
  $script:ofs += (4+3+1+3)*4
  [void](ReadByte1)
  $script:ofs += (4+1)*4
  $texIdx=ReadIndex $textureIndexSize
  $sphereIdx=ReadIndex $textureIndexSize
  [void](ReadByte1)
  $toonShared=ReadByte1
  if($toonShared -eq 0){ $toonIdx=ReadIndex $textureIndexSize } else { $toonIdx=[int](ReadByte1) }
  [void](ReadText $enc)
  [void](ReadInt32LE)

  $tex=''; if($texIdx -ge 0 -and $texIdx -lt $textures.Count){ $tex=$textures[$texIdx] }
  $sphere=''; if($sphereIdx -ge 0 -and $sphereIdx -lt $textures.Count){ $sphere=$textures[$sphereIdx] }
  if($toonShared -eq 0){ $toon=''; if($toonIdx -ge 0 -and $toonIdx -lt $textures.Count){ $toon=$textures[$toonIdx] } }
  else { $toon=('toon{0:d2}.bmp' -f ($toonIdx+1)) }

  $materials += [pscustomobject]@{
    MaterialIndex=$i; NameJa=$nameJa; NameEn=$nameEn;
    TextureIndex=$texIdx; Texture=$tex;
    ToonShared=$toonShared; ToonIndex=$toonIdx; ToonTexture=$toon;
    SphereIndex=$sphereIdx; SphereTexture=$sphere
  }
}

[pscustomobject]@{
  PmxPath=$pmxPath; Version=$version; Encoding=$encName;
  TextureCount=$textureCount; Textures=$textures;
  MaterialCount=$materialCount; Materials=$materials
} | ConvertTo-Json -Depth 6
