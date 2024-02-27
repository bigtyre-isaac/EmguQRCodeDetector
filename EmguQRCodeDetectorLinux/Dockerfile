#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
# Install dependencies for Emgu.CV
RUN apt-get update && apt-get install -y \
    libgdiplus \
    libc6-dev \
    libgtk-3-0 \
    libtbb2 \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app


FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["EmguQRCodeDetectorLinux/EmguQRCodeDetectorLinux.csproj", "EmguQRCodeDetectorLinux/"]
COPY ["EmguQRCodeDetectorLibrary/EmguQRCodeDetectorLibrary.csproj", "EmguQRCodeDetectorLibrary/"]
RUN dotnet restore "EmguQRCodeDetectorLinux/EmguQRCodeDetectorLinux.csproj"
COPY . .
WORKDIR "/src/EmguQRCodeDetectorLinux"
RUN dotnet build "EmguQRCodeDetectorLinux.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EmguQRCodeDetectorLinux.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "qr-detector.dll"]