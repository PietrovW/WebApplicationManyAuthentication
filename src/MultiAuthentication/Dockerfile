#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/MultiAuthentication/MultiAuthentication.csproj", "src/MultiAuthentication/"]
RUN dotnet restore "src/MultiAuthentication/MultiAuthentication.csproj"
COPY . .
WORKDIR "/src/src/MultiAuthentication"
RUN dotnet build "MultiAuthentication.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MultiAuthentication.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MultiAuthentication.dll"]