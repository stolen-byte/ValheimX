BEGIN {
	FS = OFS = "."
	f = (field == "major") ? 1 : (field == "minor") ? 2 : 3
}

{
	if (NF != 3) {
		print "INVALID"
		exit 1
	}

	prefix = ""
	if ($1 ~ /^v/) {
		prefix = "v"
		sub(/^v/, "", $1)
	}

	$f = $f + 1 # bump version field
	for (i = f + 1; i <= NF; i++) $i = 0 # zero out lower-order fields

	if (prefix != "")
		$1 = prefix $1

	print $0
}
