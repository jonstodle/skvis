build-mac:
    @echo "Building macOS binary..."
    dotnet publish Skvis/Skvis.csproj -c Release -r osx-x64
    mkdir Skvis/bin/Release/net8.0/osx-x64/publish/Skvis.app
    cp Skvis/bin/Release/net8.0/osx-x64/publish/Skvis Skvis/bin/Release/net8.0/osx-x64/publish/Skvis.app/

build-linux:
    @echo "Building Linux binary..."
    dotnet publish Skvis/Skvis.csproj -c Release -r linux-x64

build-win:
    @echo "Building Windows binary..."
    dotnet publish Skvis/Skvis.csproj -c Release -r win-x64

build-all: build-mac build-linux build-win
