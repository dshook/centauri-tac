using System;
using UnityEngine;

namespace ctac.util
{
    public static class ColorExtensions
    {
        public static Color HexToColor(string hex)
        {
            hex = hex.Replace("0x", "");//in case the string is formatted 0xFFFFFF
            hex = hex.Replace("#", "");//in case the string is formatted #FFFFFF
            byte a = 255;//assume fully visible unless specified in hex
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            //Only use alpha if the string has enough characters
            if (hex.Length == 8)
            {
                a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
            }
            return new Color32(r, g, b, a);
        }

        public static string ToHex(this Color32 c)
        {
            return "#" + c.r.ToString("X2") + c.g.ToString("X2") + c.b.ToString("X2");
        }

        public static Color DesaturateColor(Color c, float amount)
        {
            var L = 0.3f * c.r + 0.6f * c.g + 0.1f * c.b;
            float new_r = c.r + amount * (L - c.r);
            float new_g = c.g + amount * (L - c.g);
            float new_b = c.b + amount * (L - c.b);
            return new Color(new_r, new_g, new_b);
        }

        public static HSVColor ToHSV(this Color c){
            float H,S,V;

            Color.RGBToHSV(c, out H, out S, out V);

            return new HSVColor(){ H = H, S = S, V = V, A = c.a };
        }

        public static Color ToColor(this HSVColor hsvc){
            var c = Color.HSVToRGB(hsvc.H, hsvc.S, hsvc.V);
            c.a = hsvc.A;
            return c;
        }
    }

    public class HSVColor
    {
        float h;
        public float H
        {
           get{ return h; }
           set{
               h = value;
               //try to rotate around the values if they exceed the boundaries, but clamp just to make sure
               if(h > 1) h -= 1;
               if(h < 0) h += 1;
               h = Mathf.Clamp(value, 0, 1f);
           }
        }

        float s;
        public float S
        {
           get{ return s; }
           set{
               s = Mathf.Clamp(value, 0, 1f);
           }
        }

        float v;
        public float V
        {
           get{ return v; }
           set{
               v = Mathf.Clamp(value, 0, 1f);
           }
        }

        float a;
        public float A
        {
           get{ return a; }
           set{
               a = Mathf.Clamp(value, 0, 1f);
           }
        }
    }
}
