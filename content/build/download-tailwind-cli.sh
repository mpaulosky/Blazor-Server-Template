#!/usr/bin/env bash
# Downloads the standalone Tailwind CSS CLI binary for the current platform.
# Usage: download-tailwind-cli.sh <version> <destination-dir>
set -euo pipefail

version="$1"
dest_dir="$2"

os="$(uname -s)"
arch="$(uname -m)"

case "$os" in
	Linux) platform="linux" ;;
	Darwin) platform="macos" ;;
	*) echo "Unsupported OS: $os" >&2; exit 1 ;;
esac

case "$arch" in
	x86_64|amd64) cpu="x64" ;;
	arm64|aarch64) cpu="arm64" ;;
	*) echo "Unsupported architecture: $arch" >&2; exit 1 ;;
esac

asset="tailwindcss-${platform}-${cpu}"
url="https://github.com/tailwindlabs/tailwindcss/releases/download/v${version}/${asset}"

mkdir -p "$dest_dir"
out="$dest_dir/tailwindcss"

echo "Downloading Tailwind CLI v${version} (${asset})..."
curl -fsSL "$url" -o "$out"
chmod +x "$out"
echo "Tailwind CLI installed at $out"
