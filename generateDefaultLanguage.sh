#!/bin/bash

input="./UI/Styles/Language.axaml"
output="./UI/Languages/English.json"

echo "{" > "$output"

grep -oP '<sys:String\s+x:Key="\K[^"]+(?=">).*?(?=</sys:String>)' "$input" |
awk -F'">|</sys:String>' '
{
    if (NR > 1) print ","
    sub(/^LANG_/, "", $1)
    printf "  \"%s\": \"%s\"", $1, $2
}
' >> "$output"

echo "" >> "$output"
echo "}" >> "$output"
