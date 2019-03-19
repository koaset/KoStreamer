#!/bin/bash
for file in *.csproj
do 
    folder="${file//.csproj}"
    mkdir "${folder}"
    mv "${file}" "${folder}"
done
