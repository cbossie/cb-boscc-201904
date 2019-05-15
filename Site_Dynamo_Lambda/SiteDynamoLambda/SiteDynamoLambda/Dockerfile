FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["SiteDynamoLambda/SiteDynamoLambda.csproj", "SiteDynamoLambda/"]
COPY ["BostonCodeCampServices/BostonCodeCampServices.csproj", "BostonCodeCampServices/"]
COPY ["BostonCodeCampModels/BostonCodeCampModels.csproj", "BostonCodeCampModels/"]
RUN dotnet restore "SiteDynamoLambda/SiteDynamoLambda.csproj"
COPY . .
WORKDIR "/src/SiteDynamoLambda"
RUN dotnet build "SiteDynamoLambda.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "SiteDynamoLambda.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "SiteDynamoLambda.dll"]