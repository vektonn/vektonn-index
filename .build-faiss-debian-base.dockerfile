#######################################################################################################################
# Based on https://github.com/facebookresearch/faiss/blob/f6d2efd1dffd3e8cac7ee6241395c8557892f814/.circleci/
#######################################################################################################################

FROM cimg/base:stable-20.04

RUN sudo apt-get update && \
    sudo apt-get upgrade && \
    sudo apt-get install -y libmkl-dev

RUN wget -nv -O - https://github.com/Kitware/CMake/releases/download/v3.17.1/cmake-3.17.1-Linux-x86_64.tar.gz | \
    sudo tar xzf - --strip-components=1 -C /usr
