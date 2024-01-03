FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app
COPY artifacts/server .
ENTRYPOINT ["dotnet", "TransmissionExtras.Server.dll"]
