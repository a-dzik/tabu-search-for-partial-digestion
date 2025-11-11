using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZP_projekt
{
    public class Multiset
    {
        public List<int> elements { get; set; }
        public List<int> paired { get; set; }
        public List<(int, int)> unpairedWithScores { get; set; }

        public Multiset Copy()
        {
            Multiset copy = new Multiset
            {
                elements = new List<int>(this.elements),
                paired = new List<int>(this.paired),
                unpairedWithScores = new List<(int, int)>(this.unpairedWithScores)
            };

            return copy;
        }



        public Multiset includeErrors(int errors)
        {
            Random random = new Random();
            List<int> errorIndexes = new List<int>();

            for (int i = 1; i <= errors; i++)
            {
                int newNum = random.Next(this.elements.Count());
                while (errorIndexes.Contains(newNum))
                {
                    newNum = random.Next(this.elements.Count());
                }
                errorIndexes.Add(newNum);
            }
            foreach (int index in errorIndexes)
            {
                if (random.Next(2) == 0)
                {
                    this.elements[index] = (int)(this.elements[index] + Math.Ceiling(this.elements[index] * 0.05));
                }
                else
                {
                    if(this.elements[index] != 1)
                        this.elements[index] = (int)(this.elements[index] - Math.Ceiling(this.elements[index] * 0.05));
                }
            }
            return this;
        }



        public void GetPairsAndScores()
        {
            List<int> elementsCopy = new List<int>(this.elements);
            List<int> paired = new List<int>();
            List<int> unpaired = new List<int>();
            List<(int, int)> scoresOfUnpaired = new List<(int, int)>();

            int L = this.elements[this.elements.Count - 1];

            foreach (int currentElement in this.elements)
            {
                int difference = L - currentElement;
                if (elementsCopy.Contains(difference))
                {
                    paired.Add(currentElement);
                    elementsCopy.Remove(difference);
                }  
                else
                    unpaired.Add(currentElement);
            }
            this.paired = paired;

            HashSet<int> differences = new HashSet<int>();
            HashSet<int> pairedSet = new HashSet<int>(paired);
            foreach (int x in pairedSet)
            {
                foreach (int y in pairedSet)
                {
                    if (x != y)
                        differences.Add(Math.Abs(x - y));
                }
            }

            HashSet<int> elements = new HashSet<int>(this.elements);
            HashSet<int> unpairedSet = new HashSet<int>(unpaired);
            List<int> unpairedSetList = unpairedSet.ToList();
            int count = unpairedSet.Count;

            for (int i = 0; i < count; i++)
            {
                int score = 0;
                for (int j = count - 1; j > 0; j--)
                {
                    if (elements.Contains(unpairedSetList[i] + unpairedSetList[j]))
                        score++;
                }
                if (differences.Contains(unpairedSetList[i]))
                    score++;

                scoresOfUnpaired.Add((unpairedSetList[i], score));
            }
            scoresOfUnpaired = scoresOfUnpaired.OrderByDescending(tuple => tuple.Item2).ToList();
            this.unpairedWithScores = scoresOfUnpaired;
        }
    }
}
