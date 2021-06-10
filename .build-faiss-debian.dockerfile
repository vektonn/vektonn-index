#######################################################################################################################
# Based on https://github.com/facebookresearch/faiss/blob/f6d2efd1dffd3e8cac7ee6241395c8557892f814/.circleci/
#######################################################################################################################

#######################################################################################################################
# Build stage
FROM cimg/base:stable-20.04 AS build

ARG FAISS_VERSION

WORKDIR /src

RUN sudo apt-get update && \
    sudo apt-get install -y libmkl-dev

RUN wget -nv -O - https://github.com/Kitware/CMake/releases/download/v3.17.1/cmake-3.17.1-Linux-x86_64.tar.gz | \
    sudo tar xzf - --strip-components=1 -C /usr

RUN git clone --depth 1 --branch v${FAISS_VERSION} https://github.com/facebookresearch/faiss.git faiss_src && \
    pushd faiss_src && \
    cmake -B build \
        -DBUILD_SHARED_LIBS=ON \
        -DBUILD_TESTING=OFF \
        -DFAISS_ENABLE_GPU=OFF \
        -DFAISS_ENABLE_PYTHON=OFF \
        -DFAISS_ENABLE_C_API=ON \
        -DCMAKE_BUILD_TYPE=Release \
        -DFAISS_OPT_LEVEL=avx2 \
        -DBLA_VENDOR=Intel10_64_dyn \
        . && \
    make -C build -j $(nproc) faiss_avx2 faiss_c && \
    popd
#######################################################################################################################


#######################################################################################################################
# Final stage
FROM bash:5.1

WORKDIR /lib-faiss-native

# see https://github.com/facebookresearch/faiss/issues/1836 & https://github.com/facebookresearch/faiss/pull/1838#issuecomment-850416178
COPY --from=build /src/faiss_src/build/faiss/libfaiss_avx2.so ./libfaiss.so
COPY --from=build /src/faiss_src/build/c_api/libfaiss_c.so .
COPY --from=build /lib/x86_64-linux-gnu/libmkl_rt.so .
COPY --from=build /lib/x86_64-linux-gnu/libgomp.so.1 .
COPY --from=build /lib/x86_64-linux-gnu/libm.so.6 .
COPY --from=build /lib/x86_64-linux-gnu/libstdc++.so.6 .

CMD ["ls", "-lah"]
#######################################################################################################################
