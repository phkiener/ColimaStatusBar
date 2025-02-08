dotnet publish -r osx-arm64 -c Release -p:UseAppHost=true -o publish/osx-x64 src/ColimaStatusBar

if [ -d publish/ColimaStatusBar.app ]
then
    rm -rf publish/ColimaStatusBar.app
fi

mkdir publish/ColimaStatusBar.app
mkdir publish/ColimaStatusBar.app/Contents
mkdir publish/ColimaStatusBar.app/Contents/MacOS
mkdir publish/ColimaStatusBar.app/Contents/Resources

cp deploy/osx/Info.plist publish/ColimaStatusBar.app/Contents/
cp -a publish/osx-x64/. publish/ColimaStatusBar.app/Contents/MacOS
