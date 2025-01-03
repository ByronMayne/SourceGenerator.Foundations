# Used to send a telemetry event whenever a package is installed. It does not
# post any information that can be traced back to a user. It's used just so
# I can see which projects are being used the most.

param($installPath, $toolsPath, $package, $project)

$package = @{Id ="SourceGenerator.Foundations"; Version="2.0.11" }


function Get-BuildAgent {

    $agentKind = "None";

    if(Test-Path env:APPVEYOR_API_URL) {
        $agentKind = "AppVeyor";
    }

    if(Test-Path env:TF_BUILD) {
        $agentKind = "AzurePipelines";
    }

    if(Test-Path env:BITBUCKET_WORKSPACE) {
        $agentKind = "BitBucket";
    }

    if(Test-Path env:BUILDKITE) {
        $agentKind = "BuildKite";
    }

    if(Test-Path env:CODEBUILD_WEBHOOK_HEAD_REF) {
        $agentKind = "CodeBuild";
    }

    if(Test-Path env:ContinuaCI.Versin) {
        $agentKind = "ContinuaCi";
    }

    if(Test-Path env:DRONE) {
        $agentKind = "Drone";
    }

    if(Test-Path env:ENVRUN_DATABASE) {
        $agentKind = "EnvRun";
    }

    if(Test-Path env:GITHUB_ACTIONS) {
        $agentKind = "GitHubActions";
    }

    if(Test-Path env:GITLAB_CI) {
        $agentKind = "GitLabCi";
    }

    if(Test-Path env:JENKINS_URL) {
        $agentKind = "Jenkins";
    }

    if(Test-Path env:BuildRunner) {
        $agentKind = "MyGet";
    }

    if(Test-Path env:SpaceAutomation) {
        $agentKind = "MyGet";
    }

    if(Test-Path env:TEAMCITY_VERSION) {
        $agentKind = "TeamCity";
    }

    if(Test-Path env:TRAVIS) {
        $agentKind = "TravisCi";
    }
    return $agentKind;
}

function Get-MachineHash {
      $machineName = [Environment]::MachineName
      $userName = [Environment]::UserName
      $processorCount = [Environment]::ProcessorCount
      $osVersion = [System.Environment]::OSVersion;
      $data = "$machineName|$userName|$processorCount|$osVersion"
      $bytes = [System.Text.Encoding]::UTF8.GetBytes($data)
      $sha256 = [System.Security.Cryptography.SHA256]::Create()
      $hashBytes = $sha256.ComputeHash($bytes)
      $hashString = -join ($hashBytes | ForEach-Object { $_.ToString("x2") })
      return $hashString
}


$packageName = $package.Id;
$packageVersion = $package.Version;
$applicationInsightsUrl = "https://dc.services.visualstudio.com/v2/track"
$operationId = 
$buildAgent = Get-BuildAgent
$machineHash =  Get-MachineHash

$tracePayload = @{
    name = "Microsoft.ApplicationInsights.Event"
    time = (Get-Date).ToString("o") 
    iKey = "43813c6c-bcf0-4610-bd97-9f6933a02b44"
    tags = @{
        "ai.operation.id" = $operationId
        "ai.cloud.role" = "NuGetScript"
    }
    data = @{
        baseType = "EventData"
        baseData = @{
            name = "package_init"
            properties = @{
                "PackageName" = "$packageName"
                "PackageVersion" = "$packageVersion"
                "BuildAgent" = "$buildAgent"
                "MachineId" = $machineHash
            }
        }
    }
}
$traceJson = $tracePayload | ConvertTo-Json -Depth 10 
Invoke-WebRequest -Uri $applicationInsightsUrl -Method Post -Body $traceJson -ContentType "application/json" | Out-Null