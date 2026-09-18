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

# ==========================================================
# Powerlevel10k
# ==========================================================

RUN git clone --depth=1 \
    https://github.com/romkatv/powerlevel10k.git \
    /root/.oh-my-zsh/custom/themes/powerlevel10k

# ==========================================================
# Zsh Autosuggestions
# ==========================================================

RUN git clone --depth=1 \
    https://github.com/zsh-users/zsh-autosuggestions \
    /root/.oh-my-zsh/custom/plugins/zsh-autosuggestions


# ==========================================================
# Zsh Syntax Highlighting
# ==========================================================

RUN git clone --depth=1 \
    https://github.com/zsh-users/zsh-syntax-highlighting.git \
    /root/.oh-my-zsh/custom/plugins/zsh-syntax-highlighting


COPY docker/dev/.zshrc /root/.zshrc
COPY docker/dev/.p10k.zsh /root/.p10k.zsh

WORKDIR /workspace

CMD ["zsh"]