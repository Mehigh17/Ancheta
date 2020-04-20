FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

# Copy everything and build
COPY . ./
RUN dotnet restore
RUN dotnet publish -c Release -o out Ancheta.WebApi

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build-env /app/out .

ENV ASPNETCORE_URLS=http://*:5050
EXPOSE 5050

ENTRYPOINT ["dotnet", "Ancheta.WebApi.dll"]