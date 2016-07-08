import java.util.*;
import java.io.*;
import java.math.*;

class Player {

    public static void main(String args[]) {
        Scanner in = new Scanner(System.in);
        int road = in.nextInt(); // the length of the road before the gap.
        int gap = in.nextInt(); // the length of the gap.
        int platform = in.nextInt(); // the length of the landing platform.

        // game loop
        while (true) {
            int speed = in.nextInt(); // the motorbike's speed.
            int coordX = in.nextInt(); // the position on the road of the motorbike.

            
            if (speed < gap + 1 && coordX <= road)
                System.out.println("SPEED");
            else if (speed == gap + 1 && coordX < road - 1)
                System.out.println("WAIT");
            else if (coordX == road - 1)
                System.out.println("JUMP");
            else if (coordX >= gap + road)
                System.out.println("SLOW");
            else 
                System.out.println("SLOW");
        }
    }
}