import java.util.*;
import java.io.*;
import java.math.*;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Solution {

    public static void main(String args[]) {
        Scanner in = new Scanner(System.in);
        String LON = in.next();
        in.nextLine();
        String LAT = in.next();
        in.nextLine();
        int N = in.nextInt();
        in.nextLine();
        
        Double uLat = Math.toRadians(Double.parseDouble(LAT.replace(',', '.')));
        Double uLon = Math.toRadians(Double.parseDouble(LON.replace(',', '.')));
        String closest = "";
        Double distance = 99999999.99;
        
        for (int i = 0; i < N; i++) {
            String[] DEFIB = in.nextLine().split(";");
            
            Double dLon = Math.toRadians(Double.parseDouble(DEFIB[4].replace(',', '.')));
            Double dLat = Math.toRadians(Double.parseDouble(DEFIB[5].replace(',', '.')));
            
            Double x = (dLon - uLon) * Math.cos((uLat + dLat) / 2.0);
            Double y = (dLat - uLat);
            
            Double d = Math.sqrt(Math.pow(x, 2) + Math.pow(y, 2)) * 6371.0;
            
            
            if (d <= distance) {
                distance = d;
                closest = DEFIB[1];
            }
            
        }

        // Write an action using System.out.println()
        // To debug: System.err.println("Debug messages...");
        
        System.out.println(closest);
        System.err.println(distance);
    }
}