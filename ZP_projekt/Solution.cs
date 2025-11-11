using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZP_projekt
{
    public class Solution
    {

        public List<int> map { get; set; }
        public int m, L;
        public float fitness = 0;


        public Solution Copy()
        {
            Solution copy = new Solution
            {
                map = new List<int>(this.map),
                m = this.m,
                L = this.L,
                fitness = this.fitness
            };
            return copy;
        }



        public Solution GetRandomSolution(int m, int L)
        {
            Random random = new Random();
            this.map = new List<int> { 0 };
            this.m = m;
            this.L = L;

            for (int i = 1; i < m - 1; i++)
            {
                int newNum = random.Next(L);
                while (map.Contains(newNum))
                {
                    newNum = random.Next(L);
                }
                map.Add(newNum);
            }
            map.Add(L);

            map.Sort();
            return this;
        }



        public Multiset ComputeDistances()
        {
            Multiset distances = new Multiset();
            distances.elements = new List<int>();

            for (int i = 0; i < m - 1; i++)
            {
                for (int j = i + 1; j < m; j++)
                {
                    int distance = Math.Abs(this.map[i] - this.map[j]);
                    distances.elements.Add(distance);
                }
            }

            distances.elements.Sort();

            return distances;
        }

    

        public Solution GetStarterSolution(in Multiset D, int L, int m)
        {
            this.map = new List<int> { 0, L };
            this.m = m;
            this.L = L;
            Multiset inputMultiset = D.Copy();

            long currentTimeMillis = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            Random random = new Random((int)currentTimeMillis);

            for (int i = 1; i <= m - 2; i++)
            {
                if (inputMultiset.paired.Any())
                {
                    int index = random.Next(inputMultiset.paired.Count);
                    if (!this.map.Contains(inputMultiset.paired[index]) && (!this.map.Contains(L - inputMultiset.paired[index])))
                        this.map.Add(inputMultiset.paired[index]);

                }
            }
            if (this.map.Count < m && inputMultiset.unpairedWithScores.Any())
            {
                int iterations = 0;
                while (this.map.Count != m)
                {
                    (int maxElement, _) = inputMultiset.unpairedWithScores[inputMultiset.unpairedWithScores.Count - 1];

                    if (!this.map.Contains(maxElement))
                    {
                        this.map.Add(maxElement);
                        inputMultiset.unpairedWithScores.RemoveAt(inputMultiset.unpairedWithScores.Count - 1);
                    }

                    iterations++;
                    if (iterations >= inputMultiset.unpairedWithScores.Count)
                        break;
                }
            }
            
            int count = inputMultiset.elements.Count;
            if (this.map.Count < m)
            {
                HashSet<int> uniqueElements = new HashSet<int>(inputMultiset.elements);
                if (uniqueElements.Count < m)
                    return this;
                while (this.map.Count != m)
                {
                    int index = random.Next(count);
                    if (!this.map.Contains(inputMultiset.elements[index]))
                        this.map.Add(inputMultiset.elements[index]);
                }
            }
            this.map.Sort();
            return this;
        }



        public float ComputeFitness(Multiset inputMultiset)
        {
            Multiset currentMultiset = this.ComputeDistances();
            float fitness = 0;
            foreach(int element in inputMultiset.elements)
            {
                if (currentMultiset.elements.Contains(element))
                {
                    fitness++;
                    currentMultiset.elements.Remove(element);
                }
            }
            fitness /= inputMultiset.elements.Count();
            this.fitness = fitness;
            return fitness;
        }



        public void ApplyElementaryMove(int indexOld, int newElement)
        {
            this.map[indexOld] = newElement;
            this.map.Sort();
        }

    }
}
