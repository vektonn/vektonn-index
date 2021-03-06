# build & test stage
FROM mcr.microsoft.com/dotnet/sdk:6.0-bullseye-slim-amd64 AS build

WORKDIR /src

COPY . .

ENV LD_LIBRARY_PATH=/src/lib-faiss-native

RUN ls -lah lib-faiss-native && \
    ldd lib-faiss-native/libfaiss_c.so

RUN dotnet test --configuration Release


# final stage
FROM build
CMD ["dotnet", "--info"]
