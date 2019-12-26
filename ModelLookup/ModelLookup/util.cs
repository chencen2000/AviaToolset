using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ModelLookup
{
    class util
    {
        static Boolean checksunImei(string imei)
        {
            Boolean ret = false;
            int cs = 0;
            // imei is 15 digit
            for(int i = 0; i < 14; i++)
            {
                int j = Int32.Parse(imei.Substring(i, 1));
                if (i % 2 == 0)
                {
                    cs += j;
                }
                else
                {
                    j *= 2;
                    if (j >= 10)
                    {
                        j = (j / 10) + (j % 10);
                    }
                    cs += j;
                }
            }
            // 
            int x = cs * 9 % 10;
            if (string.Compare(x.ToString(), imei.Substring(14, 1)) == 0)
                ret = true;
            return ret;
        }
        public static int check_imei(string imei)
        {
            int ret = -1;
            Regex r = new Regex(@"^\d{15,16}$");
            if (r.Match(imei).Success)
            {
                if (imei.Length == 16)
                {
                    // IMEI SV
                    // AA-BBBBBB-CCCCCC-EE
                    string last_2 = imei.Substring(14, 2);
                    if (string.Compare(last_2, "23") == 0)
                        ret = 0;
                }
                else
                {
                    // old version IMEI 
                    // AA-BBBBBB-CCCCCC-D
                    if (checksunImei(imei))
                        ret = 0;
                    else
                        ret = -1;
                }
            }
            return ret;
        }
    }
}
