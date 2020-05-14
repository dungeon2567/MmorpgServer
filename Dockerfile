FROM mcr.microsoft.com/dotnet/core/sdk:5.0 AS build
WORKDIR /app

COPY /*.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o out

ENTRYPOINT ["dotnet", "out/MmorpgServer.dll"]
