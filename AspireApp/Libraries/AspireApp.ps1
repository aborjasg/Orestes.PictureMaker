# =============================================
# Powershell cmdlets
# Author: Alex BG
# Updated:
# Jan 2025 -> Reorganized scripts for endpoints
# ==============================================

# API endpoint (header)
$header = @{
 "Accept"="application/json"
 "Content-Type"="application/json"
} 

# select test type:
$testType = "Combined NCP (Miniature)"
$testType = "Combined NCP (Picture)"
$testType = "Spectrum (Picture)"
$testType = "Energy/Electrical (Picture)"
$testType = "Energy Calibration (Picture)"
$testType = "Heatmaps (10x8)"
$testType = "D-Number Stability"

# getSourceData:

$bodySourceData = @{ Name=$testType} | ConvertTo-Json
$responseSourceData = Invoke-RestMethod -Uri "https://localhost:7583/getSourceData" -Method 'Post' -Body $bodySourceData -Headers $header | ConvertTo-Json
$jsonSourceData = $responseSourceData | ConvertFrom-Json
$jsonSourceData


# processData:
# Input: CompressedDerivedData [string]
# Output: ActionResponse

$bodyProcess = @{ "Name"=$testType; "CompressedDerivedData"=$jsonSourceData.content } | ConvertTo-Json
$responseProcess = Invoke-RestMethod -Uri "https://localhost:7583/processData" -Method 'Post' -Body $bodyProcess -Headers $header | ConvertTo-Json
$jsonPicture = $responseProcess | ConvertFrom-Json
$responseProcess

# saveRunImage:
$bodySave = @{ "Name"=$testType; "DataSource"=$jsonSourceData.content; "Content"=$jsonPicture.content; } | ConvertTo-Json
$responseSave = Invoke-RestMethod -Uri "https://localhost:7583/saveRunImage" -Method 'Post' -Body $bodySave -Headers $header | ConvertTo-Json
$jsonSave = $responseSave | ConvertFrom-Json
$responseSave 


# getRunImage:
$url = "https://localhost:7583/getRunImage/1"
$stuff = Invoke-RestMethod -Uri $url -Method Get;
$stuff

Invoke-WebRequest $url -Method Get -Body ($body | ConvertTo-Json) | Select StatusCode, StatusDescription, Content, RawContentLenght

