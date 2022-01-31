FROM mcr.microsoft.com/dotnet/runtime:3.1
COPY PresenceConsoleApp/bin/Release/netcoreapp3.1/publish/ App/
WORKDIR /App
ENV DOTNET_EnableDiagnostics=0

ENTRYPOINT ["dotnet", "PresenceConsoleApp.dll"]
