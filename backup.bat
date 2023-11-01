@echo off
echo start of backup
xcopy /Y /I "C:\Users\Neal\source\repos\World9\ScreepsDotNet\bin\Release\net7.0\browser-wasm\AppBundle\world\*" "C:\Users\Neal\source\repos\World9\bak\"
echo End of backup