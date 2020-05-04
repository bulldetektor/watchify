[CmdletBinding(SupportsShouldProcess)]
param (
    [string]$target = "all",
    [string]$buildConfig = "release",
    [switch]$incrementMajor = $false,
    [switch]$incrementMinor = $false,
    [switch]$incrementPatch = $false
)

. .\build_helper.ps1

$build_props_file = (Get-ChildItem "Directory.Build.props")
$build_folder = "./.build"
$test_results_folder = "./.test-results"
$dist_folder = "./.dist"
$tool_name = "Bulldetektor.Watchify"

function Clean() {
    Run-Task "Clean" {
        if (Test-Path $build_folder) {
            Remove-Item -Path $build_folder -Force -Recurse
        }    
        if (Test-Path $dist_folder) {
            Remove-Item -Path $dist_folder -Force -Recurse
        }    
    }
}

function Restore() {
    Run-Task "Restore" {
        dotnet restore
    }
}

function Version(){
    [CmdletBinding(SupportsShouldProcess=$true)]
    param()
    
    Run-task "Version" {
        [xml]$proj_props = Get-Content $build_props_file
        $current_version = [System.Version]$proj_props.Project.PropertyGroup.Version
    
        $revision_number = 0
        
        if ($incrementMajor){
            $version = [System.Version]::new("$($current_version.Major + 1).0.0.$revision_number");
        }
        elseif ($incrementMinor){
            $version = [System.Version]::new("$($current_version.Major).$($current_version.Minor + 1).0.$revision_number");
        }
        elseif ($incrementPatch){
            $version = [System.Version]::new("$($current_version.Major).$($current_version.Minor).$($current_version.Build + 1).$revision_number");
        }
        else{
            $version = $current_version
        }
    
        if ($version -ne $current_version){
            Write-Host "Incrementing version from $($current_version) to $version"
            $proj_props.Project.PropertyGroup.Version = $version
            if ($PSCmdlet.ShouldProcess("$($build_props_file.Name)", "Save")){
                $proj_props.Save($build_props_file);
            }
        }
    }
}

function Build() {
    Run-Task "Build" {
        dotnet build -o $build_folder -c $buildConfig --no-incremental --no-restore
    }
}

function Test() {
    Run-Task "Test" {
        dotnet test -o $build_folder -r $test_results_folder
    }
}

function Package-Tool() {
    Run-Task "Package" {
        dotnet pack -o $dist_folder
    }
}

function Install-Tool() {
    Run-Task "Install-Tool" {
        dotnet tool uninstall --global $tool_name
        dotnet tool install --global --add-source $dist_folder $tool_name
    }
}

function Deploy() {
    Run-Task "Deploy" {
        Write-Error "TODO: Publish to nuget.org"
    }
}


$target -split "," | ForEach-Object {
    switch ($_) {
        "all" { 
            Clean
            Restore
            Version
            Build
            Publish
        }
        "clean" {
            Clean
        }
        "version" {
            Version
        }
        "build-only" {
            Build
        }
        "clean-build" {
            Clean
            Restore
            Build
        }
        "test" {
            Test
        }
        "publish" {
            Publish
        }
        "package" {
            Package-Tool
        }
        "install" {
            Install-Tool
        }
        "pre-deploy" {
            Clean
            Restore
            Version
            Build
            # Publish
            Package-Tool
        }
        "deploy-local" {
            Clean
            Restore
            Version
            Build
            Package-Tool
            Install-Tool
        }
        "deploy"{
            Deploy
        }
        Default { 
            throw "Unknown target"
        }
    }
}

End-Build