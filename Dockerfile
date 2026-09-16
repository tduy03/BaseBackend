# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution và project files
COPY ["VdcdHotel.sln", "./"]
COPY ["VDCD/VDCD.csproj", "VDCD/"]
COPY ["VDCD.Business/VDCD.Business.csproj", "VDCD.Business/"]
COPY ["VDCD.Entities/VDCD.Entities.csproj", "VDCD.Entities/"]
COPY ["VDCD.DataAccess/VDCD.DataAccess.csproj", "VDCD.DataAccess/"]

# Restore dependencies - cache NuGet packages giữa các lần build
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore VDCD/VDCD.csproj

# Copy everything else
COPY . .

# Build và publish - cache NuGet packages ở đây luôn (dotnet publish có thể resolve lại)
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish VDCD/VDCD.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "VDCD.dll"]