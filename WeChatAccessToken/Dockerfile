FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
VOLUME logs

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["WeChatAccessToken/WeChatAccessToken.csproj", "WeChatAccessToken/"]
RUN dotnet restore "WeChatAccessToken/WeChatAccessToken.csproj"
COPY . .
WORKDIR "/src/WeChatAccessToken"
RUN dotnet build "WeChatAccessToken.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WeChatAccessToken.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WeChatAccessToken.dll"]
