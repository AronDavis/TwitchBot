using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot
{
    class NameGenerator
    {
        private Random random = new Random();
        private int minSyllables = 2;
        private int maxSyllables = 4;

        private string[] consonants = new string[] { "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "n", "p", "qu", "r", "s", "t", "v", "w", "x", "y", "z" };
        private string[] vowels = new string[] { "a", "e", "i", "o", "u" };

        public NameGenerator()
        {

        }

        public string GetName()
        {
            int syllables = random.Next(minSyllables, maxSyllables+1);
            return GetName(syllables);
        }

        public string GetName(int syllables)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < syllables; i++)
            {
                sb.Append(consonants[random.Next(0, consonants.Length)]);
                sb.Append(vowels[random.Next(0, vowels.Length)]);
            }
            sb[0] = sb[0].ToString().ToUpper()[0];
            return sb.ToString();
        }
    }
}
