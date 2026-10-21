# Synthesizes narration lines with the offline Windows SAPI voice.
# usage: powershell -ExecutionPolicy Bypass -File sapi_synth.ps1 -Lines lines.json -OutDir dir [-Rate -2]
# lines.json: [{ "id": "pro_1", "text": "..." }, ...]  -> dir/pro_1.wav (44.1 kHz, 16-bit, mono)
param(
    [Parameter(Mandatory = $true)][string]$Lines,
    [Parameter(Mandatory = $true)][string]$OutDir,
    [int]$Rate = -2
)
$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Speech
$synth = New-Object System.Speech.Synthesis.SpeechSynthesizer
$voice = $synth.GetInstalledVoices() | Where-Object { $_.Enabled } | Select-Object -First 1
$synth.SelectVoice($voice.VoiceInfo.Name)
$synth.Rate = $Rate
$synth.Volume = 100
$format = New-Object System.Speech.AudioFormat.SpeechAudioFormatInfo(44100, [System.Speech.AudioFormat.AudioBitsPerSample]::Sixteen, [System.Speech.AudioFormat.AudioChannel]::Mono)
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null
$items = Get-Content -Raw -Path $Lines | ConvertFrom-Json
foreach ($item in $items) {
    $path = Join-Path $OutDir ($item.id + ".wav")
    $synth.SetOutputToWaveFile($path, $format)
    $prompt = New-Object System.Speech.Synthesis.PromptBuilder
    # "..." in the script marks a held breath: speak each part with a pause between.
    $parts = $item.text -split '\.\.\.'
    for ($i = 0; $i -lt $parts.Count; $i++) {
        $p = $parts[$i].Trim()
        if ($p.Length -gt 0) { $prompt.AppendText($p) }
        if ($i -lt $parts.Count - 1) { $prompt.AppendBreak([TimeSpan]::FromMilliseconds(650)) }
    }
    $synth.Speak($prompt)
    $synth.SetOutputToNull()
    Write-Output ("voiced " + $item.id)
}
$synth.Dispose()
