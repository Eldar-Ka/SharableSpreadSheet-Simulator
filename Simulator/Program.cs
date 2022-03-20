using System;
using System.Linq;
using System.Threading;

namespace Simulator
{
    class Program
    {
        static int availableThreads;
        private static Random random = new Random();
        static string fileLocation = "spreadsheet.dat";

        static void Main(string[] args)
        {
            int nThreads = Convert.ToInt32(args[2]); // variable that checks the number of free thread to use in any moment
            availableThreads = nThreads;
            // set the number of threads as user wanted
            ThreadPool.SetMinThreads(nThreads, 0);
            ThreadPool.SetMaxThreads(nThreads, 0);
            // how many operation each thread will do
            int nOperations = Convert.ToInt32(args[3]);
            // create the spreadsheet
            int rows = Convert.ToInt32(args[0]);
            int cols = Convert.ToInt32(args[1]);
            SharableSpreadSheet spreadsheet = new SharableSpreadSheet(rows, cols);

            // fill the spreadsheet with defultive values
            for (int i = 1; i <= rows; i++)
            {
                for (int j = 1; j <= cols; j++)
                {
                    spreadsheet.setCell(i, j, "testcell" + i + j);
                }
            }

            // start run all the threads
            for (int i = 0; i < nThreads; i++)
            {
                object[] tuple = { spreadsheet, nOperations }; // pass the spreadsheet and number of operations to make
                ThreadPool.QueueUserWorkItem(ThreadProc, tuple);
                Interlocked.Decrement(ref availableThreads);
            }

            while (nThreads != availableThreads); // wait to all threads done
            spreadsheet.save(fileLocation); // save the file

        }

        static void ThreadProc(Object tuple)
        {
            object[] t = (object[])tuple;
            SharableSpreadSheet spreadsheet = (SharableSpreadSheet)(t[0]); // get the SharableSpreadSheet
            int nOperations = (int)(t[1]); // get the number of operations to make
            int op; // which operation to make

            int row = 0;
            int col = 0;
            int r, c, r2, c2; // need for random
            String s; // need for random      
            String id = "User ["+ Thread.CurrentThread.ManagedThreadId + "]: ";
            for (int i = 0; i < nOperations; i++)
            {
                op = random.Next(1, 14);  // creates a number between 1 and 13
                spreadsheet.getSize(ref row, ref col); // get the size of the table
                r = random.Next(1, row + 1);
                c = random.Next(1, col + 1);
                switch (op) // choose random operation and activte it with random variables
                {
                    case 1: // getCell
                        s = spreadsheet.getCell(r, c);
                        if(s != null)
                            Console.WriteLine(id+"string \""+ s + "\" found in cell["+r+", "+c+"].");
                        else
                            Console.WriteLine(id + "didn't found any string in cell[" + r + ", " + c + "].");
                        break;
                    case 2: // setCell
                        r2 = random.Next(r, row + 1);
                        c2 = random.Next(c, col + 1);
                        s = "testcell" + r2 + c2;
                        if(spreadsheet.setCell(r, c, s))
                            Console.WriteLine(id+ "set the string \"" + s + "\" in cell[" + r + ", " + c + "].");
                        else
                            Console.WriteLine(id + "didn't set the string \"" + s + "\" in cell[" + r + ", " + c + "].");
                        break;
                    case 3: // searchString
                        s = "testcell" + r + c;
                        if(spreadsheet.searchString(s,ref r,ref c))
                            Console.WriteLine(id + "searched the string \"" + s + "\" in cell[" + r + ", " + c + "] and found it.");
                        else
                            Console.WriteLine(id + "searched the string \"" + s + "\" in cell[" + r + ", " + c + "] and didn't found it.");
                        break;
                    case 4: // exchangeRows
                        r2 = random.Next(1, row + 1);
                        if(spreadsheet.exchangeRows(r, r2))
                            Console.WriteLine(id + "rows ["+r+"] and ["+r2+"] exchanged successfully.");
                        else
                            Console.WriteLine(id + "rows [" + r + "] and [" + r2 + "] did't exchanged.");
                        break;
                    case 5: // exchangeCols
                        c2 = random.Next(1, col + 1);
                        if(spreadsheet.exchangeCols(c, c2))
                            Console.WriteLine(id + "cols [" + c + "] and [" + c2 + "] exchanged successfully.");
                        else
                            Console.WriteLine(id + "cols [" + c + "] and [" + c2 + "] exchanged successfully.");
                        break;
                    case 6: // searchInRow
                        s = "testcell" + r + c;
                        if (spreadsheet.searchInRow(r, s, ref c))
                            Console.WriteLine(id + "searched the string \"" + s + "\" in row[" + r + "] and found it.");
                        else
                            Console.WriteLine(id + "searched the string \"" + s + "\" in row[" + r + "] and didn't found it.");
                        break;
                    case 7: // searchInCol
                        s = "testcell" + r + c;
                        if (spreadsheet.searchInCol(c, s, ref r))
                            Console.WriteLine(id + "searched the string \"" + s + "\" in col[" + c + "] and found it.");
                        else
                            Console.WriteLine(id + "searched the string \"" + s + "\" in col[" + c + "] and didn't found it.");
                        break;
                    case 8: // searchInRange
                        r2 = random.Next(r, row + 1);
                        c2 = random.Next(c, col + 1);
                        s = "testcell" + random.Next(1, row + 1) + random.Next(1, col + 1);
                        if (spreadsheet.searchInRange(c, c2, r, r2, s, ref r, ref c))
                            Console.WriteLine(id + "searched the string \"" + s + "\" in range [" + r +"-"+r2+ ", " + c + "-" + c2 +"] and found it.");
                        else
                            Console.WriteLine(id + "searched the string \"" + s + "\" in range [" + r + "-" + r2 + ", " + c + "-" + c2 + "] and didn't found it.");
                        break;
                    case 9: // addRow
                        if(spreadsheet.addRow(r))
                            Console.WriteLine(id + "added new row after row "+r);
                        else
                            Console.WriteLine(id + "didn't added new row after row " + r);
                        break;
                    case 10: // addCol
                        if(spreadsheet.addCol(c))
                            Console.WriteLine(id + "added new column after column " + c);
                        else
                            Console.WriteLine(id + "didn't added new column after column " + c);
                        break;
                    case 11: // getSize
                        Console.WriteLine(id + "the size of the table is [" + row + ", " + col + "]");
                        break;
                    case 12: // setConcurrentSearchLimit
                        r = random.Next(1, 30);
                        if(spreadsheet.setConcurrentSearchLimit(r))
                            Console.WriteLine(id + "set Concurrent Search Limit to "+r);
                        else
                            Console.WriteLine(id + "didn't set Concurrent Search Limit successfuly");
                        break;
                    case 13: // save
                        if(spreadsheet.save(fileLocation))
                            Console.WriteLine(id + "saved the file successfully");
                        else
                            Console.WriteLine(id + "didn't saved the file successfully");
                        break;

                }


                Thread.Sleep(100); // sleep 100ms
            }
            Interlocked.Increment(ref availableThreads);
        }
    }
}
