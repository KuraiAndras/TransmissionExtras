FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
RUN apk --no-cache add curl
WORKDIR /app
COPY artifacts/server .
HEALTHCHECK --interval=30s --timeout=30s --start-period=5s --retries=3 CMD curl --fail http://localhost:8080/api/healthcheck || exit 1
ENTRYPOINT ["dotnet", "TransmissionExtras.Server.dll"]
