function Get-Environment {
    param (
        [Parameter(HelpMessage = "Environment for deploy: dev, stg, prod (dev)")]
        [ValidateSet("dev", "stg", "prod", "")]
        [string]$environment
    )

    if (-not $environment) {
        $options = @(
            New-Object System.Management.Automation.Host.ChoiceDescription "&dev", "Development"
            New-Object System.Management.Automation.Host.ChoiceDescription "&stg", "Staging"
            New-Object System.Management.Automation.Host.ChoiceDescription "&prod", "Production"
        )

        $title = "Environment selection"
        $message = "Select which environment should be used for deploy:"

        $selectedIndex = $host.UI.PromptForChoice($title, $message, $options, 0)

        switch ($selectedIndex) {
            0 { $environment = "dev" }
            1 { $environment = "stg" }
            2 { $environment = "prod" }
        }
    }

    Write-Host -ForegroundColor Yellow "Selected environment: $environment"
    return $environment
}