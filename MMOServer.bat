rem msbuild
dotnet publish -c Release --framework net7.0
xcopy /Y /I "C:\Users\Neal\source\repos\ScreepsDotNetWorld8\ScreepsDotNet\bin\Release\net7.0\browser-wasm\AppBundle\world\*" "C:\Users\Neal\AppData\Local\Screeps\scripts\screeps.com\default\"
REM END