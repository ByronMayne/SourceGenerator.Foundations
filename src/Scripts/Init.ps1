# Used to send a telemetry event whenever a package is installed. It does not
# post any information that can be traced back to a user. It's used just so
# I can see which projects are being used the most.

param($installPath, $toolsPath, $package, $project)


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

$packageName = $package.Id;
$packageVersion = $package.Version;
$instrumentationKey = "43813c6c-bcf0-4610-bd97-9f6933a02b44"
$applicationInsightsUrl = "https://dc.services.visualstudio.com/v2/track"
$operationId = [guid]::NewGuid()
$buildAgent = Get-BuildAgent
$machineId = Get-ItemProperty HKLM:SOFTWARE\Microsoft\SQMClient | Select -ExpandProperty MachineID
$machineId = $machineId.Substring(1, 36);

$tracePayload = @{
    name = "Microsoft.ApplicationInsights.Trace"
    time = (Get-Date).ToString("o") 
    iKey = $instrumentationKey
    tags = @{
        "ai.operation.id" = $operationId
        "ai.cloud.role" = "NuGetScript"
    }
    data = @{
        baseType = "MessageData"
        baseData = @{
            message = "package_init"
            severityLevel = 1 # Verbose=0, Information=1, Warning=2, Error=3, Critical=4
            properties = @{
                "PackageName" = "$packageName"
                "PackageVersion" = "$packageVersion"
                "BuildAgent" = "$buildAgent"
                "MachineId" = $machineId
            }
        }
    }
}
$traceJson = $tracePayload | ConvertTo-Json -Depth 10 
_ = Invoke-WebRequest -Uri $applicationInsightsUrl -Method Post -Body $traceJson -ContentType "application/json"