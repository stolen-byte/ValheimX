#!/usr/bin/env sh
# SPDX-License-Identifier: GPL-3.0-or-later
set -e

# ==============================================================================
die() {
	_fmt=$1
	shift
	# shellcheck disable=SC2059
	printf "Error: ${_fmt}\n" "$@"
	exit 1
}

# ==============================================================================
script_dir="$(dirname "$(readlink -f "$0")")"

if [ $# -eq 0 ]; then
	type="patch"
else
	case $1 in
	major | ma) type="major" ;;
	minor | mi) type="minor" ;;
	patch | pa | p) type="patch" ;;
	-h | --help)
		printf "usage: bump-version.sh [ma[jor]|mi[nor]|pa[tch]]\n\n"
		exit 0
		;;
	*) die "unknown arg '%s'" "$1" ;;
	esac
fi

last=$(git describe --abbrev=0)
tag=$(printf '%s' "$last" | awk -v field="$type" -f "${script_dir}/bump-version.awk")
echo "current: $last"
echo "creating new tag: $tag"
git tag -am "ValheimX ${tag}" "$tag"
