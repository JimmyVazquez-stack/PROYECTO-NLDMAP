FROM debian:13

ENV DEBIAN_FRONTEND=noninteractive

RUN apt-get update && apt-get install -y \
    bash \
    git \
    curl \
    wget \
    unzip \
    zip \
    ca-certificates \
    sudo \
    nano \
    vim \
    procps \
    iputils-ping \
    zsh \
    && apt-get clean && rm -rf /var/lib/apt/lists/*


#Instalamos oh my zsh
RUN git clone --depth=1\
    https://github.com/ohmyzsh/ohmyzsh.git\
    /root/.oh-my-zsh \
    && cp /root/.oh-my-zsh/templates/zshrc.zsh-template /root/.zshrc

WORKDIR /workspace

CMD ["zsh"]