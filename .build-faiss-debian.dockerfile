#######################################################################################################################
# Based on https://github.com/facebookresearch/faiss/blob/f6d2efd1dffd3e8cac7ee6241395c8557892f814/.circleci/
#######################################################################################################################

#######################################################################################################################
# Build stage
FROM vektonn/faiss-builder:latest AS build

WORKDIR /faiss_src

ARG FAISS_VERSION
ARG MKL_PATH=/usr/lib/x86_64-linux-gnu

RUN sudo git clone --depth 1 --branch v${FAISS_VERSION} https://github.com/vektonn/faiss.git . && \
    sudo cmake -B build \
        -DBUILD_SHARED_LIBS=ON \
        -DBUILD_TESTING=ON \
        -DFAISS_ENABLE_GPU=OFF \
        -DFAISS_ENABLE_PYTHON=OFF \
        -DFAISS_ENABLE_C_API=ON \
        -DCMAKE_BUILD_TYPE=Release \
        -DFAISS_OPT_LEVEL=avx2 \
        -DBLA_VENDOR=Intel10_64lp \
        "-DMKL_LIBRARIES=-Wl,--start-group;${MKL_PATH}/libmkl_intel_lp64.a;${MKL_PATH}/libmkl_gnu_thread.a;${MKL_PATH}/libmkl_core.a;-Wl,--end-group" \
        . && \
    sudo make -C build -j $(nproc) faiss_avx2 faiss_c faiss_test

ENV OMP_NUM_THREADS=8

RUN sudo make -C build test
#######################################################################################################################


#######################################################################################################################
# Final stage
FROM debian:bullseye-slim

WORKDIR /lib-faiss-native

# see https://github.com/facebookresearch/faiss/issues/1836 & https://github.com/facebookresearch/faiss/pull/1838#issuecomment-850416178
COPY --from=build /faiss_src/build/faiss/libfaiss_avx2.so ./libfaiss.so
COPY --from=build /faiss_src/build/c_api/libfaiss_c.so .
COPY --from=build /lib/x86_64-linux-gnu/libgomp.so.1 .

ENV LD_LIBRARY_PATH=/lib-faiss-native

RUN ls -lah . && \
    ldd libfaiss_c.so

CMD ["ldd", "libfaiss_c.so"]
#######################################################################################################################
