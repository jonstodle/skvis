build-mac:
    @echo "Building macOS binary..."
    dotnet publish Skvis/Skvis.csproj -c Release -r osx-x64
    mkdir Skvis/bin/Release/net8.0/osx-x64/publish/Skvis.app
    cp Skvis/bin/Release/net8.0/osx-x64/publish/Skvis Skvis/bin/Release/net8.0/osx-x64/publish/Skvis.app/

zip-mac:
    @echo "Zipping macOS binary..."
    cd Skvis/bin/Release/net8.0/osx-x64/publish && zip -r Skvis-mac.zip Skvis.app

build-linux:
    @echo "Building Linux binary..."
    dotnet publish Skvis/Skvis.csproj -c Release -r linux-x64

zip-linux:
    @echo "Zipping Linux binary..."
    cd Skvis/bin/Release/net8.0/linux-x64/publish && zip Skvis-linux.zip Skvis

build-win:
    @echo "Building Windows binary..."
    dotnet publish Skvis/Skvis.csproj -c Release -r win-x64

zip-win:
    @echo "Zipping Windows binary..."
    cd Skvis/bin/Release/net8.0/win-x64/publish && zip Skvis-win.zip Skvis.exe

build-all: build-mac build-linux build-win

zip-all: zip-mac zip-linux zip-win

build-zip-all: build-all zip-all
