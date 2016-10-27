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
        int L = in.nextInt();
        in.nextLine();
        int H = in.nextInt();
        in.nextLine();
        String T = in.nextLine();
        
        for (int i = 0; i < H; i++) {
            String ROW = in.nextLine();
            String output = "";
            for (int j = 0; j < T.length(); j++) {
                char c = T.toUpperCase().charAt(j);
                int aPos = (int) c - 65;
                
                if (aPos < 0 || aPos > 25) {
                    output += ROW.substring(26 * L, 27 * L);
                } else {
                    output += ROW.substring(aPos * L, (aPos + 1) * L);
                }
            }
            System.out.println(output);
            
        }

    }
}