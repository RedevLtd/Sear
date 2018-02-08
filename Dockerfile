FROM microsoft/dotnet:sdk AS build-env
WORKDIR /app

# copy csproj and restore as distinct layers
COPY *.sln ./
COPY ./SearAlertingServiceCore/*.csproj ./SearAlertingServiceCore/
COPY ./SearAlertingSystemCore/*.csproj ./SearAlertingSystemCore/
RUN dotnet restore

# copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# build runtime image
FROM microsoft/dotnet:runtime
WORKDIR /app
COPY --from=build-env /app/out ./
ENTRYPOINT ["dotnet", "SearAlertingServiceCore.dll"]