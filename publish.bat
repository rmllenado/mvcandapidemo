cd api

dotnet publish --output "c:\temp\MvcAndApiDemo\api"

cd ..\mvc

dotnet publish --output "c:\temp\MvcAndApiDemo\mvc"

cd ..

call run.bat