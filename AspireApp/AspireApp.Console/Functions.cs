using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace ConsoleApp
{
    internal static class Functions
    {
        static int minimumSwaps(int[] arr)
        {
            int swaps = 0;
            List<int> a = arr.ToList();

            Console.WriteLine("Start: {0}", string.Join(",", arr.ToArray()));

            for (int p = 0; p < a.Count - 1; p++)
            {

                if (a[p] != p + 1)
                {

                    int temp = a[p];
                    int target = a.FindIndex(p + 1, x => x == p + 1);
                    a[p] = a[target];
                    a[target] = temp;
                    swaps++;
                    Console.WriteLine("[{0}] -> [{1}] => ({2})", p, target, string.Join(",", a.ToArray()));
                }
            }
            return swaps;
        }

        static long arrayManipulation(int n, int[][] queries)
        {
            var res = new List<int>(new int[n]);

            Console.WriteLine("Array: {0}", string.Join(",", res.ToArray()));

            for (int p = 0; p < queries.Length; p++)
            {
                Console.WriteLine("[{0}]-[{1}] / Add: {2}", queries[p][0], queries[p][1], queries[p][2]);

                for (int q = queries[p][0] - 1; q < queries[p][1]; q++)
                {
                    res[q] += queries[p][2];
                }
                Console.WriteLine("queries: {0}", string.Join(",", res.ToArray()));
            }

            return res.Max();
        }

        static int missingInteger(int[] A)
        {
            int res = 0;
            int max = 0;
            for (int k = 0; k < A.Length; k++)
            {
                int ind = Array.FindIndex(A, x => x == k + 1);
                Console.WriteLine("ind={0}", ind);
                if (ind == -1)
                {
                    res = k + 1;
                    break;
                }
                else
                {
                    res = k + 2;
                }

                if (max < A[k])
                    max = A[k];

            }
            if (max == 0)
                res = 1;
            // write your code in C# 6.0 with .NET 4.5 (Mono)
            return res;
        }

        static int sherlockAndAnagrams(string s)
        {
            int res = 0;
            if (s.Length > 1 && s.Length <= 100)
            {

                Console.WriteLine("s={0}", s);
                for (int p = 1; p < s.Length; p++)
                {
                    for (int q = 0; q < s.Length - p; q++)
                    {
                        var str1 = new string(s.Substring(q, p).OrderBy(x => x).ToArray());
                        string s1 = s.Substring(q + 1);
                        /*
                        for (int k=0; k < s1.Length - p + 1; k++) {
                            var str2 = s1.Substring(k, p).OrderBy(x => x);                            

                            if (str1.SequenceEqual(str2)) {                                                                          
                                res++;
                                //Console.WriteLine("str1={0} / s1={1} / str2={2} / k={3}", s.Substring(q, p), s1, s1.Substring(k, p), k); 
                            }   
                        }*/
                        var list = new List<string>();

                        for (int k = 0; k < s1.Length - p + 1; k++)
                        {
                            var str2 = new string(s1.Substring(k, p).OrderBy(x => x).ToArray());
                            //Console.WriteLine("str2={0}", s1.Substring(k, p)); 
                            list.Add(str2);
                            //Console.WriteLine("str1={0} / s1={1} / str2={2} / list={3}", s.Substring(q, p), s1, s1.Substring(k, p), list.Count);  
                        }
                        res += list.Count(x => x == str1);
                        //Console.WriteLine("count={0}", list.Count(x => x == str1));
                    }
                }
            }
            return res;
        }

        static string isValid(string s)
        {
            string res = "NO";
            var dict = new Dictionary<char, int>();

            for (int k = 0; k < s.Length; k++)
            {
                if (!dict.ContainsKey(s[k]))
                {
                    dict.Add(s[k], 1);
                }
                else
                {
                    dict[s[k]]++;
                }
            }

            foreach (KeyValuePair<char, int> item in dict)
            {
                Console.WriteLine("Key: {0}, Value: {1}", item.Key, item.Value);
            }

            var diffChar = dict.Keys.Distinct();
            int maxNumber = dict.Values.Max();
            int maxCount = dict.Count(x => x.Value == maxNumber);
            int minNumber = dict.Values.Min();
            int minCount = dict.Count(x => x.Value == minNumber);

            Console.WriteLine("diff_Char={0} / maxNumber={1} / maxCount={2} / maxNumber={3} / minCount={4}", diffChar.Count(), maxNumber, maxCount, minNumber, minCount);

            if (diffChar.Count() - maxCount < 2 || ((maxCount == 1 || minCount == 1) && (maxNumber - minNumber <= 1)))
            {
                res = "YES";
            }

            return res;
        }

        static long substrCount(int n, string s)
        {
            int res = n;

            for (int p = 2; p <= 3; p++)
            {
                for (int q = 0; q < s.Length - p + 1; q++)
                {
                    string str = s.Substring(q, p);

                    if (str.PadLeft(str.Length / 2).ToList().SequenceEqual(str.PadRight(str.Length / 2).Reverse()))
                    {
                        res++;
                        //Console.WriteLine("str={0}", str);
                    }
                }
            }
            return res;
        }

        static long repeatedString(string s, long n)
        {
            long l = Convert.ToInt32(n / s.Length);
            string s1 = string.Empty;
            for (long k = 0; k < l; k++)
            {
                s1 += s;
            }
            long d = n - s1.Length;
            Console.WriteLine("s1={0} / d={1}", s1, d);

            for (int k = 0; k < d; k++)
            {
                s1 += s[k];
            }
            Console.WriteLine("s1={0}", s1);

            //s1 = s1.Remove(Convert.ToInt32(n), Convert.ToInt32(s1.Length - l));

            long r = s1.Count(x => (x == 'a'));

            Console.WriteLine("s={0} / s.Count={1} / #a={2}", s1, s1.Length, r);

            return r;
        }

        static void virusIndices(string p, string v)
        {
            /*
             * Print the answer for this test case in a single line
             */
            int l = v.Length;
            var res = new List<int>();
            //Console.WriteLine("vo={0}", string.Concat(vo));

            for (int k = 0; k < p.Length - l + 1; k++)
            {
                bool match = false;
                int n = 0;
                var str = p.Substring(k, l);
                //Console.WriteLine("v={0} / str={1}", v, str);

                for (int i = 0; i < l; i++)
                {
                    if (l == 1)
                    {
                        match = true;
                    }
                    else if (p.Length >= l && str == v)
                    {
                        match = true;
                    }
                    else
                    {
                        if (v[i] == str[i] && str[i] >= 'a' && str[i] <= 'z')
                        {
                            n++;
                            if (n == l - 1)
                            {
                                match = true;
                                break;
                            }
                        }
                    }
                }
                if (match)
                    res.Add(k);

            }

            if (res.Count > 0)
                Console.WriteLine("{0}", string.Join(" ", res));
            else
                Console.WriteLine("No Match!");
        }

        static void miniMaxSum(int[] arr)
        {
            List<long> n = arr.Select(x => (long)x).ToList();
            n = n.OrderBy(x => x).Where(x => x > 0 && x <= 1000000000).ToList();
            long min = (long)(n.Sum() - n.Max());
            long max = n.Sum() - n.Min();
            Console.WriteLine("{0} {1}", min, max);
        }

        static string caesarCipher(string s, int k)
        {
            var res = new StringBuilder();
            if (s.Length > 0 && s.Length <= 100 && k >= 0 && k <= 100)
            {
                if (k > 26)
                    k = k % 26;

                Console.WriteLine("k={0}", k);

                foreach (char chr in s)
                {
                    char temp = chr;
                    int i = (int)temp;
                    if (i >= 65 && i <= 90)
                    {
                        if (i + k > 90)
                        {
                            temp = (char)(i + k - 26);
                        }
                        else
                        {
                            temp = (char)(i + k);
                        }
                    }
                    else if (i >= 97 && i <= 122)
                    {
                        if (i + k > 122)
                        {
                            temp = (char)(i + k - 26);
                        }
                        else
                        {
                            temp = (char)(i + k);
                        }
                    }
                    Console.WriteLine("i={0} / chr={1} -> {2}", i, chr, temp);

                    res.Append(temp);
                }
            }
            return res.ToString();
        }

        static int migratoryBirds(List<int> arr)
        {
            var dict = new Dictionary<int, int>() { { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 } };
            if (arr.Count >= 5 && arr.Count <= 200000)
            {
                foreach (var item in arr)
                {
                    dict[item]++;
                }
            }
            int max = dict.Values.Max();
            var res = dict.FirstOrDefault(x => x.Value == max).Key;
            foreach (var item in dict)
            {
                Console.WriteLine("key={0} / value={1}", item.Key, item.Value);
            }
            Console.WriteLine("Max={0} / res={1}", max, res);
            return res;
        }

        static string dayOfProgrammer(int year)
        {
            string res = "";

            if (year >= 1700 && year <= 2700)
            {

                if (year == 1918)
                {
                    res = "26.09.";
                }
                else if (year < 1918)
                {
                    if (year % 4 == 0)
                    {
                        res = "12.09.";
                    }
                    else
                    {
                        res = "13.09.";
                    }
                }
                else if (year > 1918)
                {
                    if (((year % 4 == 0 && year % 100 != 0) || (year % 400 == 0)))
                        res = "12.09.";
                    else
                        res = "13.09.";
                }
                res += year.ToString();

            }
            return res;
        }

        static List<int> freqQuery(List<List<int>> queries)
        {
            var res = new List<int>();
            var temp = new Dictionary<int, int>();

            foreach (var item in queries)
            {
                //Console.WriteLine("[0]:{0}, [1]:{1}, temp.Count:{2}", item[0], item[1], temp.Count);
                switch (item[0])
                {
                    case 1:
                        {
                            if (!temp.Keys.Contains(item[1]))
                                temp.Add(item[1], 1);
                            else
                                temp[item[1]]++;
                            break;
                        }
                    case 2:
                        {
                            if (item[1] >= 0 && item[1] < temp.Count && temp.Keys.Contains(item[1]))
                                temp[item[1]]--;
                            break;
                        }
                    case 3:
                        {
                            if (temp.Values.Contains(item[1]))
                            {
                                res.Add(1);
                            }
                            else
                            {
                                res.Add(0);
                            }
                            break;
                        }
                }
            }
            return res;
        }

        static List<int> climbingLeaderboard(List<int> ranked, List<int> player)
        {
            var shortcut = new Dictionary<int, int>();
            var ranked1 = ranked.Distinct().ToList();
            var res = new List<int>();

            Console.WriteLine("count: {0}", ranked1.Count);

            if (ranked.Count > 0 && ranked.Count <= 200000 && player.Count > 0 && player.Count <= 200000 && ranked1.Count > 0)
            {

                for (int k = 0; k < player.Count; k++)
                {
                    var path = new Stack<int>();
                    bool search = true;
                    int l = 0, r = ranked1.Count() - 1;
                    int i = l + (r - l) / 2;

                    if (player[k] >= ranked1[0])
                        i = 0;
                    else if (player[k] <= ranked1.Last())
                        i = ranked1.Count - 1;

                    if (shortcut.ContainsKey(player[k]))
                        res.Add(shortcut[player[k]]);
                    else
                    {
                        while (search)
                        {
                            //Console.WriteLine("player: {0}, ranked1:{1}, i={2}", player[k], ranked1[i], i);

                            if (!path.Contains(i) && i < ranked1.Count && i >= 0)
                            {
                                path.Push(i);

                                if (i == 0 || i == ranked1.Count() - 1)
                                {
                                    search = false;
                                }
                                else
                                {
                                    if (player[k] == ranked1[i])
                                    {
                                        search = false;
                                    }
                                    else
                                    {
                                        if (player[k] > ranked1[i])
                                        {
                                            r = i - 1;
                                        }
                                        else if (player[k] < ranked1[i])
                                        {
                                            l = i + 1;
                                        }
                                        i = l + (r - l) / 2;
                                        if (i == 0) i = 1;
                                        else if (i == ranked1.Count() - 1) i = ranked1.Count() - 2;
                                    }

                                }
                            }
                            else
                            {
                                search = false;
                            }
                        }

                        if (!search)
                        {
                            int n = path.First();
                            //Console.WriteLine("n={0}", n);                 
                            if (player[k] < ranked1[n])
                            {
                                res.Add(n + 2);
                            }
                            else if (player[k] >= ranked1[n])
                            {
                                res.Add(n + 1);
                            }
                        }

                        if (!shortcut.ContainsKey(player[k]))
                            shortcut.Add(player[k], res.Last());

                    }
                    //Console.WriteLine("{0}", res.Last());
                    //Console.WriteLine("player: {0}, ranked1:{1}, i={2}", player[k], ranked1[i], i);

                }
            }
            //Console.WriteLine("res: {0}, res={1}", string.Join(" ", res), res.Count);
            return res;

        }

        /// <summary>
        /// Optimizes the number of servers based on their power and cost.
        /// </summary>
        /// <param name="power"></param>
        /// <param name="cost"></param>
        /// <returns></returns>
        public static int OptimizeServers(List<int> power, List<int> cost)
        {
            int result = 0;
            if (power.Count > 0 && cost.Count > 0 && power.Count == cost.Count)
            {
                for (int k = 0; k < power.Count - 1; k++)
                {
                    result++;
                    if ((power[k] < power[k + 1] && cost[k] > cost[k + 1]) || (power[k] > power[k + 1] && cost[k] < cost[k + 1]))
                    {
                        result--;
                    }
                }
                if ((power[power.Count - 1] < power[0] && cost[cost.Count - 1] > cost[0]) || (power[power.Count - 1] > power[0] && cost[cost.Count - 1] < cost[0]))
                {
                    result--;
                }
                else
                    result++;
            }
            return result;
        }

        /// <summary>
        /// Calculates the parking bill based on the entrance and leaving times.
        /// </summary>
        /// <param name="E"></param>
        /// <param name="L"></param>
        /// <returns></returns>
        public static int ParkingBill(string E, string L)
        {
            const int entranceFee = 2, firstHourFee = 3, afterHourFee = 4;
            int result = 0;
            if (!string.IsNullOrEmpty(E) && !string.IsNullOrEmpty(L))
            {
                var timeE = E.Split(':').Select(int.Parse).ToArray();
                var timeL = L.Split(':').Select(int.Parse).ToArray();
                result += entranceFee;
                if (timeE.Length == 2 && timeL.Length == 2 && timeE[0] >= 0 && timeE[0] < 24 && timeE[1] >= 0 && timeE[1] < 60 && timeL[0] >= 0 && timeL[0] < 24 && timeL[1] >= 0 && timeL[1] < 60)
                {
                    int hours = timeL[0] - timeE[0];
                    int minutes = timeL[1] - timeE[1];

                    if (minutes > 0)
                        hours++;

                    result = entranceFee + (hours > 0 ? firstHourFee : 0) + afterHourFee * (hours - 1);
                }
            }
            return result;
        }

        /// <summary>
        /// Calculates the parity degree of a number.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static int ParityDegree(int n)
        {
            int result = 0, power = 0;
            if (n > 0 && n <= 1000000000)
            {
                int k = 1;
                while (n >= k)
                {
                    if (n % k == 0)
                        result = power;

                    power++;
                    k = (int)Math.Pow(2, power);
                }
            }
            return result;
        }

        public static void TransformJson(ref JsonNode node)
        {
            //Console.WriteLine($"node:{node}");
            var temp = JsonNode.Parse(node.ToJsonString())!.AsObject();
           if (temp is JsonObject jsonObject)
            {
                foreach (var item in jsonObject)
                {
                    if (item.Value is JsonValue)
                    {
                        if (item.Value.ToString() == "N/A" && jsonObject.ContainsKey(item.Key))
                        {
                            ((JsonObject)node).Remove(item.Key);
                            Console.WriteLine($"Removed: {item.Key}");
                        }
                    }
                    else if (item.Value is JsonObject)
                    {
                        JsonNode jsonNode = JsonNode.Parse(JsonSerializer.Serialize(item.Value))!;
                        TransformJson(ref jsonNode);
                        node[item.Key] = jsonNode;
                    }
                }
            }
        }

    }
}
