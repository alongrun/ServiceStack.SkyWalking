for /r . %%a in (*.nupkg) do (
	del %%a
)
for /r . %%a in (*.nuspec) do (
	nuget pack %%~dpna.csproj -build  -Prop Configuration=Release
)
for /r . %%a in (*.nupkg) do (
	nuget push %%a -source http://192.168.100.135:8099/nuget/

)