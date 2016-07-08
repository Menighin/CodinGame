var n = parseInt(readline()); // the number of temperatures to analyse
var temps = readline().split(' '); // the n temperatures expressed as integers ranging from -273 to 5526

var temp = 0;
var diff = 5526;

printErr(temps);

for(var i = 0; i < n; i++) {
   if (Math.abs(parseInt(temps[i])) < diff || (Math.abs(parseInt(temps[i])) == diff && temps[i] > temp)) {
       diff = Math.abs(parseInt(temps[i]));
       temp = parseInt(temps[i]);
   }
    
}

print(temp);