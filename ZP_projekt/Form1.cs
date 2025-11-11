using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace ZP_projekt
{

    public partial class Form1 : Form
    {
        public class Params
        {
            public int m, L, tabuListSize, neighboursNum, maxIterations, timeLimit;
            public Multiset D;
            public Solution starterSolution;
        }

        DataTable DTable = new DataTable();
        DataTable DTable2 = new DataTable();

        Params paramsPackage = new Params();

        volatile bool Active = true;
        volatile bool Stop = false;

        public Form1()
        {
            InitializeComponent();
            tabPage1.Text = "generator";
            tabPage2.Text = "tabu";

            dataGridView1.DataSource = DTable;
            dataGridView2.DataSource = DTable2;
        }

        //KOD ZAKŁADKI Z GENERATOREM*********************************************************


        private void buttonGenerate_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "" && textBox2.Text != "" && textBox3.Text != "" && errorProvider1.GetError(textBox1) == "" && errorProvider2.GetError(textBox2) == "" && errorProvider3.GetError(textBox3) == "")
            {

                //odczyt podanych wartości parametrów
                int.TryParse(textBox1.Text, out int m);
                int.TryParse(textBox2.Text, out int errors);
                int.TryParse(textBox3.Text, out int L);

                //wygenerowanie rozwiązania oraz instancji dla niego, na podstawie parametrów
                Solution randomSolution = new Solution();
                randomSolution = randomSolution.GetRandomSolution(m, L);
                Multiset D = randomSolution.ComputeDistances();

                //dodanie błędów
                if (errors > 0)
                {
                    D = D.includeErrors(errors);
                }

                paramsPackage.m = m;
                paramsPackage.L = L;
                paramsPackage.D = D;

                //wyświetlenie zbiorów S i D
                DTable.Clear();
                DTable.Columns.Add();
                foreach (var number in randomSolution.map)
                {
                    DataRow newRow = DTable.NewRow();
                    newRow[0] = number;
                    DTable.Rows.Add(newRow);
                }
                dataGridView1.Refresh();

                DTable2.Clear();
                dataGridView2.Refresh();
                DTable2.Columns.Add();
                dataGridView2.DataSource = null;
                foreach (var number in D.elements)
                {
                    DataRow newRow = DTable2.NewRow();
                    newRow[0] = number;
                    DTable2.Rows.Add(newRow);
                }
                dataGridView2.DataSource = DTable2;
                dataGridView2.Refresh();

            }
            else
                MessageBox.Show("Błędne wartości parametrów.", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        


        private void buttonClean_Click(object sender, EventArgs e)
        {
            DTable.Clear();
            DTable.Columns.Add();
            DTable2.Clear();
            DTable2.Columns.Add();
        }

        private void buttonRead_Click(object sender, EventArgs e)
        {
            DTable.Clear();

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                DTable2.Clear();
                DTable2.Columns.Add();
                dataGridView2.DataSource = null;

                using (StreamReader reader = new StreamReader(openFileDialog.FileName))
                {
                    string line;
                    int value;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (int.TryParse(line, out value))
                            DTable2.Rows.Add(value);
                        else
                            MessageBox.Show("Nieprawidłowy format danych: " + line);
                    }
                }
            }
            dataGridView2.DataSource = DTable2;
            dataGridView2.Refresh();
            if (ValidInputInstance())
            {
                Multiset D = new Multiset();
                D.elements = new List<int>();

                foreach (DataRow row in DTable2.Rows)
                {
                    int value = Convert.ToInt32(row[0]);
                    D.elements.Add(value);
                }
                paramsPackage.D = D;
                paramsPackage.L = D.elements[D.elements.Count() - 1];

                int numOfElements = CountNonEmptyRows();
                for (int m = 2; m <= numOfElements; m++)
                {
                    if (BinomialCoefficientOf2(m) == numOfElements)
                        paramsPackage.m = m;
                }
            }
        }

        private void buttonWrite_Click(object sender, EventArgs e)
        {
            if (ValidInputInstance())
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Text files (*.txt)|*.txt";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))
                    {
                        foreach (DataRow row in DTable2.Rows)
                        {
                            writer.WriteLine(row[0].ToString());
                        }
                    }
                }
            }
            else
                MessageBox.Show("Podana instancja jest nieprawidłowa.");
        }








//KOD ZAKŁADKI Z TABU******************************************************************************
        
        public class tabuProgress
        {
            public double timePassedSeconds { get; set; }
            public Solution newBest { get; set; }

            public tabuProgress(double timePassed, Solution solution)
            {
                timePassedSeconds = timePassed;
                newBest = solution;
            }
        }

        private void buttonRun_Click(object sender, EventArgs e)
        {
            progressBar.Value = 0;

            //generowanie rozwiązania początkowego
            paramsPackage.D.GetPairsAndScores();

            Solution starterSolution = new Solution();
            starterSolution = starterSolution.GetStarterSolution(paramsPackage.D, paramsPackage.L, paramsPackage.m);
            if(starterSolution.map.Count < paramsPackage.m)
            {
                MessageBox.Show("Brak możliwości zbudowania jakiejkolwiek mapy. Za mało unkalnych elementów.");
                return;
            }

            paramsPackage.starterSolution = starterSolution;
            paramsPackage.starterSolution.ComputeFitness(paramsPackage.D);
            label17.Text = Math.Round(paramsPackage.starterSolution.fitness, 3).ToString();

            if (textBox4.Text != "" && textBox5.Text != "" && textBox6.Text != "" && textBox7.Text != "" && errorProvider5.GetError(textBox4) == "" && errorProvider6.GetError(textBox5) == "" && errorProvider7.GetError(textBox6) == "" && errorProvider8.GetError(textBox7) == "")
            {
                int.TryParse(textBox4.Text, out paramsPackage.tabuListSize);
                int.TryParse(textBox5.Text, out paramsPackage.neighboursNum);
                int.TryParse(textBox6.Text, out paramsPackage.maxIterations);
                int.TryParse(textBox7.Text, out paramsPackage.timeLimit);

                progressBar.Maximum = paramsPackage.timeLimit;

                BackgroundWorker bw = new BackgroundWorker();
                bw.WorkerReportsProgress = true;
                bw.DoWork += new DoWorkEventHandler(
                    delegate (object o, DoWorkEventArgs args)
                    {
                        BackgroundWorker b = o as BackgroundWorker;
                        Params paramsPackage = args.Argument as Params;

                        TabuSearch tabuSearch = new TabuSearch
                        {
                            neighboursNum = paramsPackage.neighboursNum,
                            maxIterations = paramsPackage.maxIterations,
                            timeLimit = paramsPackage.timeLimit,
                            tabuListSize = paramsPackage.tabuListSize,
                            inputMultiset = paramsPackage.D,
                            starterSolution = paramsPackage.starterSolution,
                            currentSolution = starterSolution.Copy(),
                            bestSolution = starterSolution.Copy(),
                            tabuListAdded = new Queue<int>(),
                            tabuListRemoved = new Queue<int>(),
                            topSolutions = new List<Solution>()
                        };

                        bw.ReportProgress(0, new tabuProgress(0, tabuSearch.bestSolution));

                        int moveSize = tabuSearch.UpdateMoveSize(starterSolution.m);
                        int iterationsWOProgress = 0, resets = 0;
                        bool timeForReset = false;
                        HashSet<int> setOldSolution;
                        HashSet<int> setNewSolution;

                        Stopwatch stopwatch = Stopwatch.StartNew();

                        while (stopwatch.Elapsed.TotalSeconds < tabuSearch.timeLimit)
                        {

                            if (Active)
                            {
                                stopwatch.Start();
                                bw.ReportProgress(0, new tabuProgress(stopwatch.Elapsed.TotalSeconds, tabuSearch.bestSolution));
                                
                                //sprawdzenie, czy użytkownik nie zmienił parametrów tabu
                                if (paramsPackage.tabuListSize != tabuSearch.tabuListSize)
                                    tabuSearch.tabuListSize = paramsPackage.tabuListSize;
                                if (paramsPackage.neighboursNum != tabuSearch.neighboursNum)
                                    tabuSearch.neighboursNum = paramsPackage.neighboursNum;
                                if (paramsPackage.maxIterations!= tabuSearch.maxIterations)
                                    tabuSearch.maxIterations = paramsPackage.maxIterations;
                                if (paramsPackage.timeLimit != tabuSearch.timeLimit)
                                    tabuSearch.timeLimit = paramsPackage.timeLimit;

                                

                                //sprawdzenie, czy trzeba zrobić reset
                                if (timeForReset) 
                                {
                                    resets++;
                                    timeForReset = false;
                                    iterationsWOProgress = 0;
                                    tabuSearch.currentSolution = tabuSearch.Reset();
                                    tabuSearch.currentSolution.ComputeFitness(tabuSearch.inputMultiset);
                                }

                                //dostosowanie wielkości ruchu
                                moveSize = tabuSearch.UpdateMoveSize(tabuSearch.currentSolution.m);

                                //wygenerowanie sąsiedztwa
                                List<Solution> neighbours = tabuSearch.GetNeighbours(moveSize);
                                //Console.WriteLine($"\nbest fitness: {tabuSearch.bestSolution.fitness} neigh fitness: {neighbours[0].fitness}\n");

                                if (neighbours[0].fitness > tabuSearch.currentSolution.fitness) //w sąsiedztwie jest rozwiązanie z lepszym fitness
                                {
                                    //wykonaj polepszający ruch
                                    setOldSolution = new HashSet<int>(tabuSearch.currentSolution.map);
                                    setNewSolution = new HashSet<int>(neighbours[0].map);
                                    HashSet<int> moves = new HashSet<int>(setOldSolution.Except(setNewSolution));
                                    foreach (int move in moves)
                                        tabuSearch.UpdateTabuListRemoved(move);
                                    HashSet<int> moves2 = new HashSet<int>(setNewSolution.Except(setOldSolution));
                                    foreach (int move in moves2)
                                        tabuSearch.UpdateTabuListAdded(move);


                                    tabuSearch.currentSolution = neighbours[0];

                                    //nowe najlepsze rozwiązanie
                                    if (tabuSearch.currentSolution.fitness > tabuSearch.bestSolution.fitness)
                                    {
                                        tabuSearch.bestSolution = tabuSearch.currentSolution;
                                        tabuSearch.UpdateTopSolutions();
                                        WriteBestSolution(tabuSearch.bestSolution.map);
                                        iterationsWOProgress = 0;
                                    }

                                    //znaleziono rozwiązanie optymalne
                                    if (tabuSearch.bestSolution.fitness == 1)
                                    {
                                        bw.ReportProgress(0, new tabuProgress(stopwatch.Elapsed.TotalSeconds, tabuSearch.bestSolution));
                                        args.Result = tabuSearch.bestSolution;
                                        break;
                                    }
                                }
                                else //brak ruchów polepszających
                                {
                                    //sprawdzenie kryterium aspiracji
                                    List<Solution> neighboursTabu = tabuSearch.GetNeighbours(moveSize, false);
                                    if (tabuSearch.bestSolution.fitness < neighboursTabu[0].fitness)
                                    {
                                        tabuSearch.currentSolution = neighboursTabu[0];
                                        tabuSearch.bestSolution = tabuSearch.currentSolution;
                                        iterationsWOProgress = 0;
                                    }

                                    else
                                    {
                                        iterationsWOProgress++;
                                        //Console.WriteLine("\niteration w/o progress\n");

                                        //sprawdzenie czy osiągnięto limit iteracji bez poprawy rozwiązania
                                        if (iterationsWOProgress > tabuSearch.maxIterations)
                                        {
                                            timeForReset = true;
                                            continue;
                                        }

                                        //wykonaj (najmniej) pogarszający ruch
                                        setOldSolution = new HashSet<int>(tabuSearch.currentSolution.map);
                                        setNewSolution = new HashSet<int>(neighbours[0].map);

                                        HashSet<int> moves = new HashSet<int>(setOldSolution.Except(setNewSolution));
                                        foreach (int move in moves)
                                            tabuSearch.UpdateTabuListRemoved(move);
                                        HashSet<int> moves2 = new HashSet<int>(setNewSolution.Except(setOldSolution));
                                        foreach (int move in moves2)
                                            tabuSearch.UpdateTabuListAdded(move);

                                        tabuSearch.currentSolution = neighbours[0];
                                    }
                                }


                            }
                            else
                            {
                                stopwatch.Stop();
                                Thread.Sleep(500);
                            } 
                            if (Stop)
                            {
                                Stop = false;
                                stopwatch.Stop();
                                break;
                            }
                        }
                        stopwatch.Stop();
                        args.Result = tabuSearch.bestSolution;
                    }
                );



                bw.ProgressChanged += new ProgressChangedEventHandler(
                    delegate (object o, ProgressChangedEventArgs args) {
                        tabuProgress tabuProgress = args.UserState as tabuProgress;
                        if (float.TryParse(label18.Text, out float labelFloat))
                        {
                            if (tabuProgress.newBest != null && tabuProgress.newBest.fitness.ToString("F3").Trim() != label18.Text.Trim())
                            {
                                label18.Text = Math.Round(tabuProgress.newBest.fitness, 3).ToString();
                                richTextBox1.Text = $"{string.Join(", ", tabuProgress.newBest.map)}";
                                label20.Text = (tabuProgress.timePassedSeconds).ToString("F3");
                            }
                        }

                        if (tabuProgress.timePassedSeconds > progressBar.Minimum && tabuProgress.timePassedSeconds < progressBar.Maximum)
                        { 
                            progressBar.Value = (int)(tabuProgress.timePassedSeconds); 
                            label21.Text = (tabuProgress.timePassedSeconds).ToString("F3");
                        }
                    });



                bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
                delegate (object o, RunWorkerCompletedEventArgs args) {
                    Solution solution = args.Result as Solution;
                    if (solution != null)
                    {
                        if (solution.fitness == 1)
                            Console.WriteLine($"\nSukces! Znaleziono optymalne rozwiązanie o funkcji celu: {solution.fitness}\n" +
                            $"Solution: {string.Join(", ", solution.map)}\n\n\n\n");
                        else
                            Console.WriteLine($"\nPrzekroczono limit czasowy lub przerwano obliczenia. Najlepsze fitness: {solution.fitness}\n" +
                            $"Najlepsze rozw. jakie udało się znaleźć: {string.Join(", ", solution.map)}\n\n\n\n");
                    }
                    else
                        Console.WriteLine("Algorytm zakończył działanie z nieznanych przyczyn...");

                    progressBar.Value = progressBar.Maximum;
                });

                bw.RunWorkerAsync(paramsPackage);
            }
            else
                MessageBox.Show("Błędne wartości parametrów.", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }


        void WriteBestSolution(List <int> map)
        {
            string filePath = "solution.txt";
            using (StreamWriter writer = new StreamWriter(filePath, false))
            {
                foreach (int number in map)
                    writer.WriteLine(number);
            }
        }



        private void buttonPauseResume_Click(object sender, EventArgs e)
        {
            if (buttonPauseResume.Text == "PAUZA")
            {
                Active = false;
                buttonPauseResume.Text = "KONTYNUUJ";
                buttonPauseResume.BackColor = Color.Green;
            }
            else
            {
                Active = true;
                buttonPauseResume.Text = "PAUZA";
                buttonPauseResume.BackColor = Color.Red;
            }

            Application.DoEvents();
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            Stop = true;
        }




        //ZABEZPIECZENIA**********************************************************************************

        //poprawność parametrów instancji
        private void ValidateTextBox1()
        {
            int value;
            if (!int.TryParse(textBox1.Text, out value) || value < 2)
            {
                errorProvider1.SetError(textBox1, "M musi być równe co najmniej 2.");
            }
            else
            {
                errorProvider1.SetError(textBox1, "");
                                                       
                ValidateTextBox2();
                ValidateTextBox3();
            }
        }

        private void ValidateTextBox2()
        {
            if (int.TryParse(textBox1.Text, out int value))
            {
                int max = (BinomialCoefficientOf2(value));
                int textBox2Value;
                if (!int.TryParse(textBox2.Text, out textBox2Value) || textBox2Value < 0 || textBox2Value >= max)
                {
                    errorProvider2.SetError(textBox2, $"Liczba błędów musi być całkowita z przedziału od 0 do {max - 1}.");
                }
                else
                {
                    errorProvider2.SetError(textBox2, "");
                }
            }
        }

        private void ValidateTextBox3()
        {
            if (int.TryParse(textBox1.Text, out int value))
            {
                int textBox3Value;
                if (!int.TryParse(textBox3.Text, out textBox3Value) || textBox3Value < value - 1)
                {
                    errorProvider3.SetError(textBox3, $"Długość musi być całkowita i wynosić co najmniej {value - 1}.");
                }
                else
                {
                    errorProvider3.SetError(textBox3, "");
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            ValidateTextBox1();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            ValidateTextBox2();
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            ValidateTextBox3();
        }

        //poprawność instancji (datagridview)
        public static int BinomialCoefficientOf2(int m)
        {
            if (m < 2)
            {
                throw new ArgumentException("m musi być >= 2.");
            }
            return m * (m - 1) / 2;
        }

        public bool ValidNumOfElements(int count)
        {
            for(int m=2; m <= count; m++)
            {
                if (BinomialCoefficientOf2(m) == count)
                    return true;
            }
            return false;
        }

        private bool ValidInputInstance()
        {
            errorProvider4.Clear();
            bool hasErrors = false;

            foreach (DataGridViewRow row in dataGridView2.Rows)
            {
                if (!row.IsNewRow && !IsRowEmpty(row))
                {
                    int value;
                    if (!int.TryParse(row.Cells[0].Value?.ToString(), out value) || value <= 0)
                    {
                        errorProvider4.SetError(dataGridView2, "Wszystkie wartości muszą być dodatnimi liczbami całkowitymi.");
                        hasErrors = true;
                    }
                }
            }

            int currentNumOfElements = CountNonEmptyRows();
            if (!ValidNumOfElements(currentNumOfElements))
            {
                errorProvider4.SetError(dataGridView2, $"Nieprawidłowa liczność instancji.");
                hasErrors = true;
            }
            return !hasErrors;
        }


        private bool IsRowEmpty(DataGridViewRow row)
        {
            foreach (DataGridViewCell cell in row.Cells)
            {
                if (cell.Value != null && !string.IsNullOrWhiteSpace(cell.Value.ToString()))
                    return false;
            }
            return true;
        }

        private int CountNonEmptyRows()
        {
            int count = 0;
            foreach (DataGridViewRow row in dataGridView2.Rows)
            {
                if (!row.IsNewRow && !IsRowEmpty(row))
                    count++;
            }
            return count;
        }

        
        private void dataGridView2_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if(ValidInputInstance())
            {
                Multiset D = new Multiset();
                D.elements = new List<int>();

                int numOfElements = CountNonEmptyRows();
                for (int m = 2; m <= numOfElements; m++)
                {
                    if (BinomialCoefficientOf2(m) == numOfElements)
                        paramsPackage.m = m;
                }

                int value;
                foreach (DataGridViewRow row in dataGridView2.Rows)
                {
                    if (!row.IsNewRow && !IsRowEmpty(row))
                    {
                        if (int.TryParse(row.Cells[0].Value.ToString(), out value))
                        {
                            D.elements.Add(value);
                        }
                    }
                }
                D.elements.Sort();
                paramsPackage.L = D.elements[D.elements.Count-1];
                paramsPackage.D = D;
            }
        }
        
        private void tab1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if ((tab1.SelectedIndex == 1 && !ValidInputInstance()) || (tab1.SelectedIndex == 1 && paramsPackage.D == null))
            {
                e.Cancel = true;
                MessageBox.Show("Zanim przejdziesz do kolejnej zakładki, popraw instancję wejściową.", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }

        //poprawność parametrów tabu
        private bool ValidateTabuTextBoxes()
        {
            bool isValid = true;

            errorProvider5.Clear();
            errorProvider6.Clear();
            errorProvider7.Clear();
            errorProvider8.Clear();

            if (!int.TryParse(textBox4.Text, out int value1) || value1 <= 0)
            {
                errorProvider5.SetError(textBox4, "Wartość musi być całkowita i dodatnia.");
                isValid = false;
            }

            if (!int.TryParse(textBox5.Text, out int value2) || value2 <= 0)
            {
                errorProvider6.SetError(textBox5, "Wartość musi być całkowita i dodatnia.");
                isValid = false;
            }

            if (!int.TryParse(textBox6.Text, out int value3) || value3 < 0)
            {
                errorProvider7.SetError(textBox6, "Wartość musi być całkowita i nieujemna.");
                isValid = false;
            }

            if (!int.TryParse(textBox7.Text, out int value4) || value4 < 0)
            {
                errorProvider8.SetError(textBox7, "Wartość musi być całkowita i nieujemna.");
                isValid = false;
            }

            return isValid;
        }

        private void tabuTextBoxes_TextChanged(object sender, EventArgs e)
        {
            if (ValidateTabuTextBoxes())
            {
                int.TryParse(textBox4.Text, out paramsPackage.tabuListSize);
                int.TryParse(textBox5.Text, out paramsPackage.neighboursNum);
                int.TryParse(textBox6.Text, out paramsPackage.maxIterations);
                int.TryParse(textBox7.Text, out int seconds);
                {
                    if(paramsPackage.timeLimit < seconds)
                    {
                        paramsPackage.timeLimit = seconds;
                        progressBar.Maximum = paramsPackage.timeLimit;
                    }
                }
            }
        }

    }
}
