param ([string]$environment, [string]$cpfNumber = '12345678909')
. "$PSScriptRoot/get-environment.ps1"
$environment = Get-Environment $environment

Write-Host "Fetching API Gateway URL..."
$apiGatewayUrl = aws apigatewayv2 get-apis --query "Items[?Name=='fiap-mechanics-$environment-api'].ApiEndpoint" --output text
Write-Host $apiGatewayUrl

Write-Host "Generating token..."
$response = Invoke-RestMethod "$apiGatewayUrl/auth/login" -Method 'POST' -Headers @{ "Content-Type" = "application/json" } -Body "{`"cpfNumber`":`"$cpfNumber`",`"password`":`"5eCre+Key`"}"
$response | ConvertTo-Json

Write-Host
Write-Host -ForegroundColor Yellow "JWT token copied to clipboard."
$response.accessToken | Set-Clipboard
