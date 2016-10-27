#!/bin/bash

read N
# reads all power values and store them in an array
for (( i=0; i<N; i++ ))
do
    read p
    power[$i]=$p
done

# sorts the array
power=($(for p in ${power[@]}; do echo $p; done | sort -n))

# at this point, power contains all the power sorted by increading order
# the result expected is the smallest difference between 2 consecutive values
min=9999999
for (( i=0; i<${#power[@]}-1; i++ ))
    do
        current=${power[$i]}
        next=${power[$i+1]}
        diff=$(( next - current))
        if [ $diff -lt $min ]
        then
            min=$diff
        fi
    done

echo $min