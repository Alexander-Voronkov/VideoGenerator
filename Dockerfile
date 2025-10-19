FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY *.csproj ./
RUN dotnet restore

COPY ./Fonts/*.ttf /usr/local/share/fonts

COPY . ./
RUN dotnet publish -c Release -o /out

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app

ENV DOTNET_ENVIRONMENT=Production

COPY --from=build /out ./

ENTRYPOINT ["dotnet", "VideoGenerator.dll"]
