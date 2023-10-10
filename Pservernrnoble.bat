rem msbuild
dotnet publish -c Release --framework net7.0
xcopy /Y /I "C:\Users\Neal\source\repos\World9\ScreepsDotNet\bin\Release\net7.0\browser-wasm\AppBundle\world\*" "C:\Users\Neal\AppData\Local\Screeps\scripts\10_0_0_69___21025\Pservnrnoble\"
REM END