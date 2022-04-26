using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilities
{
    public static string ConvertSecondsToMinutesAndSecondsString(int seconds)
    {
        int minutes = 0;

        while(seconds - 60 >= 0)
        {
            minutes++;
            seconds -= 60;
        }
        
        if(seconds == 0)
        {
            return minutes.ToString() + ":00";
        }
        else if(seconds < 10)
        {
            return minutes.ToString() + ":0" + seconds;
        }

        return minutes.ToString() + ":" + seconds;
    }
}
