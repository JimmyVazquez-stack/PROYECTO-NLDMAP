# ==========================================================
# NLDMAP Development Environment - ZSH Configuration
# ==========================================================

# ----------------------------------------------------------
# Oh My Zsh
# ----------------------------------------------------------

export ZSH="$HOME/.oh-my-zsh"

ZSH_THEME="powerlevel10k/powerlevel10k"


# ----------------------------------------------------------
# Plugins
# ----------------------------------------------------------

plugins=(
    git
    zsh-autosuggestions
    zsh-syntax-highlighting
)


# ----------------------------------------------------------
# Load Oh My Zsh
# ----------------------------------------------------------

source "$ZSH/oh-my-zsh.sh"


# ----------------------------------------------------------
# Powerlevel10k
# ----------------------------------------------------------

[[ -f ~/.p10k.zsh ]] && source ~/.p10k.zsh


# ----------------------------------------------------------
# General aliases
# ----------------------------------------------------------

alias ll='ls -lah'
alias la='ls -A'
alias l='ls -CF'

alias cls='clear'

alias ..='cd ..'
alias ...='cd ../..'
alias ....='cd ../../..'


# ----------------------------------------------------------
# Git aliases
# ----------------------------------------------------------

alias gs='git status'
alias ga='git add'
alias gaa='git add .'
alias gc='git commit'
alias gp='git push'
alias gpl='git pull'

alias gl='git log --oneline --graph --decorate'
alias gb='git branch'
alias gco='git checkout'


# ----------------------------------------------------------
# Docker aliases
# ----------------------------------------------------------

alias d='docker'
alias dps='docker ps'
alias dpa='docker ps -a'
alias di='docker images'

alias dc='docker compose'
alias dcu='docker compose up -d'
alias dcd='docker compose down'
alias dcb='docker compose build'
alias dcl='docker compose logs -f'


# ----------------------------------------------------------
# Development
# ----------------------------------------------------------

export EDITOR="nano"
export VISUAL="nano"
