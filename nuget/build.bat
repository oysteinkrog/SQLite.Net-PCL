set version=2.5.2

@mkdir output
..\.nuget\nuget pack SQLite.Net.nuspec -o output -Version %version%
..\.nuget\nuget pack SQLite.Net.Async.nuspec -o output -Version %version%
..\.nuget\nuget pack SQLite.Net.Platform.Win32.nuspec -o output -Version %version%
..\.nuget\nuget pack SQLite.Net.Platform.XamarinAndroid.nuspec -o output -Version %version%
..\.nuget\nuget pack SQLite.Net.Platform.XamarinIOS.nuspec -o output -Version %version%
pause
