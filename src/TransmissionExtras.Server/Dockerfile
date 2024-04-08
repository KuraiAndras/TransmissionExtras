FROM mcr.microsoft.com/dotnet/runtime:8.0.3-alpine3.19
WORKDIR /app
COPY artifacts/server .
ENTRYPOINT ["dotnet", "TransmissionExtras.Server.dll"]
