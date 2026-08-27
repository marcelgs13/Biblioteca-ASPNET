# 1. Estágio de Build 
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia o arquivo de projeto e restaura dependências (aproveita cache de camadas)
COPY ["BibliotecaAPI.csproj", "./"]
RUN dotnet restore "BibliotecaAPI.csproj"

# Copia todo o restante dos arquivos e compila a aplicação
COPY . .
RUN dotnet publish "BibliotecaAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. Estágio Final de Execução (Runtime leve)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "BibliotecaAPI.dll"]