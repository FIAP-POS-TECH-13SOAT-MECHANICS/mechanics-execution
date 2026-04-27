param (
    [ValidateSet('ADMINISTRATOR', 'ATTENDANT', 'MECHANIC', 'CUSTOMER_USER', 'CUSTOMER_ADMIN')]
    [string]$Role,
    [string]$UserId,
    [string]$CustomerId
)

if (-not $Role) {
    $options = @(
        New-Object System.Management.Automation.Host.ChoiceDescription "&Administrator", "Full system access"
        New-Object System.Management.Automation.Host.ChoiceDescription "A&ttendant",     "Front desk attendant"
        New-Object System.Management.Automation.Host.ChoiceDescription "&Mechanic",      "Workshop mechanic"
        New-Object System.Management.Automation.Host.ChoiceDescription "Customer&User",  "Customer (standard)"
        New-Object System.Management.Automation.Host.ChoiceDescription "Customer&Admin", "Customer (admin)"
    )
    $selectedIndex = $host.UI.PromptForChoice("Role selection", "Select the role for the token:", $options, 0)
    $Role = @('ADMINISTRATOR', 'ATTENDANT', 'MECHANIC', 'CUSTOMER_USER', 'CUSTOMER_ADMIN')[$selectedIndex]
}

if (-not $UserId)
{
    $UserId = [System.Guid]::NewGuid().ToString()
}

if (@('CUSTOMER_USER', 'CUSTOMER_ADMIN') -contains $Role)
{
    if (-not $CustomerId)
    {
        $CustomerId = [System.Guid]::NewGuid().ToString()
    }
}
else
{
    $CustomerId = [System.Guid]::Empty.ToString()
}

$now = [System.DateTimeOffset]::UtcNow
$iat = $now.ToUnixTimeSeconds()
$nbf = $iat
$exp = $now.AddHours(3).ToUnixTimeSeconds()

function ConvertTo-Base64Url([string]$json)
{
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($json)
    [Convert]::ToBase64String($bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_')
}

function ConvertTo-Base64UrlBytes([byte[]]$bytes)
{
    [Convert]::ToBase64String($bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_')
}

$header = '{"alg":"HS256","typ":"JWT"}'
$payloadObj = [ordered]@{
    iss = "fiap-mechanics"
    exp = $exp
    iat = $iat
    nbf = $nbf
    sub = $UserId
    customerId = $CustomerId
    role = $Role.ToUpper()
}
$payload = $payloadObj | ConvertTo-Json -Compress
$secret = [System.Text.Encoding]::UTF8.GetBytes("fiap-mechanics-local-secret")
$hmac = [System.Security.Cryptography.HMACSHA256]::new($secret)

$headerEncoded = ConvertTo-Base64Url $header
$payloadEncoded = ConvertTo-Base64Url $payload
$signingInput = "$headerEncoded.$payloadEncoded"

$sigBytes = $hmac.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($signingInput))
$sigEncoded = ConvertTo-Base64UrlBytes $sigBytes

$token = "$signingInput.$sigEncoded"

Write-Host $token
$token | Set-Clipboard
Write-Host -ForegroundColor Yellow "JWT token copied to clipboard."
