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
        String MESSAGE = in.nextLine();

        System.err.println(MESSAGE);

        String binary = "";
        
        for (char c : MESSAGE.toCharArray()) {
            binary += String.format("%7s", Integer.toBinaryString((int)c)).replace(' ','0');
        }
        
        System.err.println(binary);
        
        String output = "";
        
        char a = '%';
        String firstBlock = "";
        int count = 0;
        for (char c : binary.toCharArray()) {
            //System.err.println("CurrChar: " + c + " - Saved Char: " + a);
            if (c != a) {
                String secondBlock = "";
                for (int i = 0; i < count; i++)
                    secondBlock += "0";
                
                if (firstBlock.length() > 0)
                    output += firstBlock + " " + secondBlock + " ";
                
                a = c;
                firstBlock = (a == '0' ? "00" : "0");
                count = 1;
            } else {
                count++;
            }
        }
        
        String secondBlock = "";
        for (int i = 0; i < count; i++)
            secondBlock += "0";
        
        output += firstBlock + " " + secondBlock;
        
        System.out.println(output);
    }
}