using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZP_projekt
{
    public class TabuSearch
    {
        public int neighboursNum, maxIterations, maxResets, tabuListSize, timeLimit;
        
        public Multiset inputMultiset;
        public Solution starterSolution, currentSolution, bestSolution;

        public Queue<int> tabuListAdded, tabuListRemoved;
        public List<Solution> topSolutions;


        public int UpdateMoveSize(int solutionSize)
        {
            double size = 1;
            if(currentSolution.fitness < 0.6)
                size = Math.Ceiling((double) (solutionSize-2) / 4);
            else if(currentSolution.fitness < 0.7)
                size = Math.Ceiling((double)(solutionSize - 2) / 5);
            else if (currentSolution.fitness < 0.8)
                size = Math.Ceiling((double)(solutionSize - 2) / 7);
            else if (currentSolution.fitness < 0.9)
                size = Math.Ceiling((double)(solutionSize - 2) / 8);
            else if (currentSolution.fitness < 1.0)
                size = Math.Ceiling((double)(solutionSize - 2) / 10);
            return Convert.ToInt32(size);
        }
      



        public Solution MakeAMove(int moveSize, Random random, bool useTabuList = true)
        {
            Solution modifiedSolution = currentSolution.Copy();
            List<int> paired = new List<int>(inputMultiset.paired);
            List<(int, int)> unpaired = new List<(int, int)>(inputMultiset.unpairedWithScores);

            List<int> chosenElements = new List<int>();

            for(int i = 0; i < moveSize; i++)
            {
                int index;
                while (true)
                {
                    index = random.Next(1, modifiedSolution.map.Count - 1);
                    if (!chosenElements.Contains(index) && ((!tabuListAdded.Contains(modifiedSolution.map[index])) || !useTabuList))
                        break;
                }
                chosenElements.Add(index);
            }

            bool chosen;
            
            foreach (int index in chosenElements)
            {
                chosen = false;
                int element = modifiedSolution.map[index];
                int correspondingElement = modifiedSolution.L - element;

                if (!chosen && paired.Any() && random.NextDouble() < 0.6 &&
                    paired.Contains(correspondingElement) &&
                    !modifiedSolution.map.Contains(correspondingElement) &&
                    (!tabuListRemoved.Contains(correspondingElement) || !useTabuList))
                {
                    modifiedSolution.ApplyElementaryMove(index, correspondingElement);
                    chosen = true;
                }

                else if (!chosen && paired.Any() && random.NextDouble() < 0.9)
                {
                    int randomIndex = random.Next(paired.Count);
                    int pairedElement = paired[randomIndex];
                    if (!modifiedSolution.map.Contains(pairedElement) &&
                        (!tabuListRemoved.Contains(pairedElement) || !useTabuList))
                    {
                        modifiedSolution.ApplyElementaryMove(index, pairedElement);
                        chosen = true;
                    }
                }
                else if (!chosen && unpaired.Any())
                {

                    foreach (var tuple in unpaired)
                    {
                        int unpairedElement = tuple.Item1;
                        if (!modifiedSolution.map.Contains(unpairedElement) && (!tabuListRemoved.Contains(unpairedElement) || !useTabuList))
                        {
                            modifiedSolution.ApplyElementaryMove(index, unpairedElement);
                            chosen = true;
                            break;
                        }
                    }
                }

            }
            
                return modifiedSolution;
        }




        public List<Solution> GetNeighbours(int moveSize, bool useTabuList = true)
        {
            List<Solution> neighbours = new List<Solution>();
            Random random = new Random();

            for (int i=0; i < neighboursNum; i++)
            {
                Solution neighbourSolution = MakeAMove(moveSize, random);
                neighbourSolution.ComputeFitness(inputMultiset);
                neighbours.Add(neighbourSolution);
            }
            neighbours = neighbours.OrderByDescending(sol => sol.fitness).ToList();

            foreach (Solution sol in neighbours)
            {
                Console.Write(sol.fitness + " ");
            }

            return neighbours;
        }



        public void UpdateTabuListAdded(int element)
        {
            tabuListAdded.Enqueue(element);

            if (tabuListAdded.Count > (double)(tabuListSize / 2))
                tabuListAdded.Dequeue();
        }


        public void UpdateTabuListRemoved(int element)
        {
            tabuListRemoved.Enqueue(element);

            if (tabuListRemoved.Count > (double)(tabuListSize/2))
                tabuListRemoved.Dequeue();
        }


        public void UpdateTopSolutions()
        {
            Solution newBestSolution = this.bestSolution.Copy();
            topSolutions.Add(newBestSolution);
            topSolutions = topSolutions.OrderByDescending(sol => sol.fitness).ToList();
            if (topSolutions.Count > 10)
                topSolutions.RemoveAt(topSolutions.Count - 1);
        }



        private List<Solution> previousTopSolutions = new List<Solution>();

        public Solution Reset()
        {
            Solution resetSolution = currentSolution.Copy();
            if (previousTopSolutions != null && !topSolutions.SequenceEqual(previousTopSolutions))
            {
                List<int> allElements = topSolutions.SelectMany(sol => sol.map).ToList();

                Dictionary<int, int> elementCounts = new Dictionary<int, int>();
                foreach (int element in allElements)
                {
                    if (elementCounts.ContainsKey(element))
                        elementCounts[element]++;
                    else
                        elementCounts[element] = 1;
                }

                var sortedCounts = elementCounts.OrderByDescending(x => x.Value);
                List<int> chosenElements = sortedCounts.Take(topSolutions.First().map.Count).Select(x => x.Key).ToList();

                resetSolution.map = chosenElements;
                resetSolution.map.Sort();
                Console.WriteLine("\nSoft reset.\n");
                
            }
            else
            {
                resetSolution = resetSolution.GetStarterSolution(inputMultiset, currentSolution.L, currentSolution.m);
                Console.WriteLine("\nHard reset.\n");
            }
            previousTopSolutions = topSolutions;
            tabuListAdded.Clear();
            tabuListRemoved.Clear();

            return resetSolution;
        }
    }
}
