param (
    [string]$target = "all",
    [string]$build_config = "release",
    [string]$azure_deploy_pwd = ""
)

. .\build_helper.ps1

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

function Build() {
    Run-Task "Build" {
        dotnet build -o $build_folder -c $build_config --no-incremental --no-restore 
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
            Build
            Publish
        }
        "clean" {
            Clean
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
            Build
            Publish
            Package-Tool
        }
        "deploy-local" {
            Clean
            Restore
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