FROM debian:13

# Configuración regional UTF-8 para zsh, Perl y herramientas de consola
ENV LANG=C.UTF-8

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
    postgresql-client \
    libicu-dev \
    libssl-dev \    
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

#Instalacion de .NET SDK 8.0
RUN wget https://dot.net/v1/dotnet-install.sh -O /tmp/dotnet-install.sh \
    && chmod +x /tmp/dotnet-install.sh \
    && /tmp/dotnet-install.sh \
        --channel 8.0 \
        --install-dir /usr/share/dotnet \
    && ln -s /usr/share/dotnet/dotnet /usr/local/bin/dotnet \
    && rm /tmp/dotnet-install.sh

ENV DOTNET_ROOT=/usr/share/dotnet
ENV PATH="${PATH}:${DOTNET_ROOT}"

WORKDIR /workspace

CMD ["zsh"]