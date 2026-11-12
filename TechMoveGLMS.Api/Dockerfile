FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["TechMoveGLMS.Api/TechMoveGLMS.Api.csproj", "TechMoveGLMS.Api/"]
RUN dotnet restore "TechMoveGLMS.Api/TechMoveGLMS.Api.csproj"

COPY ["TechMoveGLMS.Api/", "TechMoveGLMS.Api/"]
WORKDIR "/src/TechMoveGLMS.Api"

RUN dotnet publish "TechMoveGLMS.Api.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "TechMoveGLMS.Api.dll"]