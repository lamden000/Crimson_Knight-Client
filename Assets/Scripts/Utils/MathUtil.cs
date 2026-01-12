using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class MathUtil
{
    public static int Distance(int x1, int y1, int x2, int y2)
    {
        int dx = x2 - x1;
        int dy = y2 - y1;
        return (int)Math.Sqrt(dx * dx + dy * dy);
    }


    public static int Distance(BaseObject obj1, BaseObject obj2)
    {
        int x1 = obj1.GetX();
        int y1 = obj1.GetY();
        int x2 = obj2.GetX();
        int y2 = obj2.GetY();
        int dx = x2 - x1;
        int dy = y2 - y1;
        return (int)Math.Sqrt(dx * dx + dy * dy);
    }


    static readonly Random ran = new Random();

    public static int RandomInt(int a, int b)
    {
        return ran.Next(a, b + 1);
    }

    public static string ToPercentString(int value)
    {
        float percent = value / 10000f * 100f;
        return percent.ToString("0.##") + "%";
    }


}
