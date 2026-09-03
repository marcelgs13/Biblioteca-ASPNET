# Etapa 1: Build da aplicação
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia o csproj e restaura as dependências primeiro (otimiza cache)
COPY ["BibliotecaAPI.csproj", "./"]
RUN dotnet restore "BibliotecaAPI.csproj"

# Copia o restante do código e compila
COPY . .
RUN dotnet publish "BibliotecaAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa 2: Imagem final de execução
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Expõe as portas padrões do Kestrel (.NET 10 usa 8080 por padrão)
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BibliotecaAPI.dll"]