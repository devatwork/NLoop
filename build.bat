@echo Off
set config=%1
if "%config%" == "" (
   set config=Release
)

set version=
if not "%PackageVersion%" == "" (
   set version=-Version %PackageVersion%
)

set nuget=
if "%nuget%" == "" (
    set nuget=.nuget\NuGet.exe
)

set nunit="packages\NUnit.Runners.2.6.2\tools\nunit-console.exe"

echo Update self %nuget%
%nuget% update -self
if %errorlevel% neq 0 goto failure

echo Restore packages
%nuget% install ".nuget\packages.config" -OutputDirectory packages -NonInteractive
if %errorlevel% neq 0 goto failure

echo Build
%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild NLoop.sln /t:Rebuild /p:Configuration="%config%" /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=Normal /nr:false
if %errorlevel% neq 0 goto failure

echo Unit tests
%nunit% NLoop.Core.Tests\bin\NLoop.Core.Tests.dll /framework:net-4.5
if %errorlevel% neq 0 goto failure
%nunit% NLoop.IO.Tests\bin\NLoop.IO.Tests.dll /framework:net-4.5
if %errorlevel% neq 0 goto failure
%nunit% NLoop.Net.Tests\bin\NLoop.Net.Tests.dll /framework:net-4.5
if %errorlevel% neq 0 goto failure
%nunit% NLoop.Timing.Tests\bin\NLoop.Timing.Tests.dll /framework:net-4.5
if %errorlevel% neq 0 goto failure

echo Package
mkdir Build
cmd /c %nuget% pack "NLoop.Core\NLoop.Core.csproj" -symbols -o Build -p Configuration=%config% %version%
if %errorlevel% neq 0 goto failure
cmd /c %nuget% pack "NLoop.IO\NLoop.IO.csproj" -symbols -o Build -p Configuration=%config% %version%
if %errorlevel% neq 0 goto failure
cmd /c %nuget% pack "NLoop.Net\NLoop.Net.csproj" -symbols -o Build -p Configuration=%config% %version%
if %errorlevel% neq 0 goto failure
cmd /c %nuget% pack "NLoop.Timing\NLoop.Timing.csproj" -symbols -o Build -p Configuration=%config% %version%
if %errorlevel% neq 0 goto failure

:success
echo Success
goto end

:failure
echo Failed

:end