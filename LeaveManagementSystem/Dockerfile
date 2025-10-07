#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src


COPY ["LeaveManagementSystem.csproj", "./"]
RUN dotnet restore "./LeaveManagementSystem.csproj"

COPY . .
WORKDIR "/src/."
RUN dotnet build "LeaveManagementSystem.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LeaveManagementSystem.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LeaveManagementSystem.dll", "--server.urls","http://+:8080"]


