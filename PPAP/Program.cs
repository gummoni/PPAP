using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PPAP
{
    class Program
    {
        static void Main(string[] args)
        {
            var you = new Parser();
            while (true)
            {
                Console.Write(">");
                var message = Console.ReadLine();
                if ("quit" == message) break;
                if (string.IsNullOrEmpty(message)) continue;
                try
                {
                    var result = you.Listen(message);
                    Console.WriteLine(result);
                }
                catch
                {
                    Console.WriteLine("Sorry, I can't understand.");
                }
                Console.WriteLine("");
            }
        }
    }

    /// <summary>
    /// 分割
    /// </summary>
    public class Parser
    {
        public Noun This { get; set; }
        public Nouns nouns = new Nouns();

        IEnumerator<string> Parse(string message)
        {
            var words = message.Split(' ');
            foreach (var word in words)
            {
                if (Regex.IsMatch(word, "^(is|are|am)$"))
                {
                    yield return "Be";
                }
                else
                {
                    yield return word;
                }
            }
        }

        public Noun GetNoun(string word)
        {
            if ("This" == word)
            {
                return This;
            }
            else
            {
                return nouns[word];
            }
        }

        public object Listen(string message)
        {
            var words = Parse(message);
            //S
            var word = words.Next();

            if ("What" == word)
            {
                var V = words.Next();   // be
                var O = GetNoun(words.Next()); // noun
                if (O == This)
                {
                    return $"It's {O.Name}";
                }
                else if (0 < O.Sames.Count)
                {
                    return string.Join("\r\n", O.Sames.Select(_ => $"{O.Name} is {_.Name}"));
                }
                else
                {
                    var ns = string.Join(", ", nouns.Where(_ => _.Sames.Contains(O)).Select(_ => _.Name));
                    return $"{O.Name} is {ns}.";
                }
            }
            else if ("Where" == word)
            {
                throw new InvalidProgramException();
            }
            else if ("Who" == word)
            {
                throw new InvalidProgramException();
            }
            else if ("Why" == word)
            {
                throw new InvalidProgramException();
            }
            else if ("When" == word)
            {
                throw new InvalidProgramException();
            }
            else if ("Do" == word)
            {
                var S = GetNoun(words.Next());
                var v = words.Next();
                var V = S.GetType().GetMethod("Do" + v);
                var O = (Noun[])V.Invoke(S, new object[] { this, words });
                if (0 == O.Length)
                {
                    return $"No, {S.Name} {v.ToLower()} not it.";
                }
                else
                {
                    return $"Yes, {S.Name} {v.ToLower()} {O.Length} {O[0].Name}.";
                }
            }
            else
            {
                var S = GetNoun(word);
                var v = words.Next();
                var P = S.GetType().GetProperty(v);
                if (null != P)
                {
                    //this name is foobar.
                    var be = words.Next();
                    P.SetValue(S, words.Next());
                }
                else
                {
                    //this is ben.
                    var V = S.GetType().GetMethod(v);
                    V.Invoke(S, new object[] { this, words });
                }
                return null;
            }
        }
    }

    public static class StringExtentions
    {
        public static string ToCamelCase(this string self)
        {
            return self[0].ToString().ToUpper() + self.Substring(1);
        }
    }

    public static class IEnumeratorExtentions
    {
        public static string Next(this IEnumerator<string> self) => (self.MoveNext()) ? self.Current.ToCamelCase() : string.Empty;
    }

    public class Nouns : List<Noun>
    {
        public Noun this[string name]
        {
            get
            {
                var result = this.FirstOrDefault(_ => _.Name == name);
                if (null == result)
                {
                    result = new Noun() { Name = name };
                    Add(result);
                }
                return result;
            }
        }
    }

    public class Noun
    {
        public string Name { get; set; }
        public List<Noun> Sames { get; set; } = new List<Noun>();
        internal List<Noun> Inventories { get; set; } = new List<Noun>();

        public void Be(Parser sender, IEnumerator<string> words)
        {
            var O = sender.nouns[words.Next()];
            if (!Sames.Contains(O))
            {
                Sames.Add(O);
            }
        }

        public void Have(Parser sender, IEnumerator<string> words)
        {
            var O = sender.nouns[words.Next()];
            if (!Inventories.Contains(O))
            {
                Inventories.Add(O);
            }
        }

        public void Combine(Parser sender, IEnumerator<string> words)
        {
            var A = sender.nouns[words.Next()];
            var and = words.Next();
            var B = sender.nouns[words.Next()];
            var newName = A.Name + "+" + B.Name;
            var AB = new Noun() { Name = newName };
            AB.Inventories.Add(A);
            AB.Inventories.Add(B);
            Inventories.Remove(A);
            Inventories.Remove(B);
            Inventories.Add(AB);
            sender.This = AB;
        }

        public Noun[] DoHave(Parser sender, IEnumerator<string> words)
        {
            var o = words.Next();
            return Inventories.Where(_ => _.Name == o).ToArray();
        }
    }

}
