# build & test stage
FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim-amd64 AS build
WORKDIR /src
COPY . .
ENV LD_LIBRARY_PATH=/src/lib-faiss-native
RUN dotnet test --configuration Release

# final stage
FROM build
CMD ["dotnet", "--info"]
