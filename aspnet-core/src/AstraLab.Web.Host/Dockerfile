FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src
COPY ["src/AstraLab.Web.Host/AstraLab.Web.Host.csproj", "src/AstraLab.Web.Host/"]
COPY ["src/AstraLab.Web.Core/AstraLab.Web.Core.csproj", "src/AstraLab.Web.Core/"]
COPY ["src/AstraLab.Application/AstraLab.Application.csproj", "src/AstraLab.Application/"]
COPY ["src/AstraLab.Core/AstraLab.Core.csproj", "src/AstraLab.Core/"]
COPY ["src/AstraLab.EntityFrameworkCore/AstraLab.EntityFrameworkCore.csproj", "src/AstraLab.EntityFrameworkCore/"]
WORKDIR "/src/src/AstraLab.Web.Host"
RUN dotnet restore 

WORKDIR /src
COPY ["src/AstraLab.Web.Host", "src/AstraLab.Web.Host"]
COPY ["src/AstraLab.Web.Core", "src/AstraLab.Web.Core"]
COPY ["src/AstraLab.Application", "src/AstraLab.Application"]
COPY ["src/AstraLab.Core", "src/AstraLab.Core"]
COPY ["src/AstraLab.EntityFrameworkCore", "src/AstraLab.EntityFrameworkCore"]
WORKDIR "/src/src/AstraLab.Web.Host"
RUN dotnet publish -c Release -o /publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
EXPOSE 80
WORKDIR /app
COPY --from=build /publish .
ENTRYPOINT ["dotnet", "AstraLab.Web.Host.dll"]
