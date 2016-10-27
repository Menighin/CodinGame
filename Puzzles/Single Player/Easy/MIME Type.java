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
        int N = in.nextInt(); // Number of elements which make up the association table.
        in.nextLine();
        int Q = in.nextInt(); // Number Q of file names to be analyzed.
        in.nextLine();
        
        Map<String, String> map = new HashMap<String, String>();
        
        for (int i = 0; i < N; i++) {
            String EXT = in.next(); // file extension
            String MT = in.next(); // MIME type.
            System.err.println(EXT + " - " + MT);
            in.nextLine();
            
            map.put(EXT.toLowerCase(), MT);
        }
        for (int i = 0; i < Q; i++) {
            String FNAME = in.nextLine(); // One file name per line.
            
            String[] splitted = FNAME.split("\\.");
            
            if (FNAME.length() == 0 || splitted.length == 0) {
                System.out.println("UNKNOWN");
                continue;
            }
            
            
            String ext = splitted[splitted.length - 1];
            char last = FNAME.toCharArray()[FNAME.length() - 1];
            
            System.err.println(FNAME + " - " + ext + " - " + splitted.length);
            if (splitted.length > 1 && last != '.' && map.containsKey(ext.toLowerCase()))
                System.out.println(map.get(ext.toLowerCase()));
            else
                System.out.println("UNKNOWN");
        }


     }
}