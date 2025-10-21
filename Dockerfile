FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY *.csproj ./
RUN dotnet restore

COPY ./Fonts/ /usr/local/share/fonts/truetype/

RUN apt install -y fontconfig

RUN fc-cache -fv

COPY . ./
RUN dotnet publish -c Release -o /out

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS runtime
WORKDIR /app

ENV DOTNET_ENVIRONMENT=Production

COPY --from=build /out ./

ENTRYPOINT ["dotnet", "VideoGenerator.dll"]
