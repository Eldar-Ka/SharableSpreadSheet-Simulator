using System;
using System.IO;
using System.Threading;

class SharableSpreadSheet
{
    private static Semaphore writerMutex; // writer/readers key
    private static Semaphore searchingSemaphore; // limit the number of searchers
    private static Mutex readerMutex; // let one reader to do before/after function
    private string[,] mat;
    private int rows;
    private int cols;
    private int maxUsersSearching;
    private int curReaders;
    private int curSearching;
    public SharableSpreadSheet(int nRows, int nCols)
    {
        // construct a nRows*nCols spreadsheet
        mat = new string[nRows, nCols];
        rows = nRows;
        cols = nCols; 
        maxUsersSearching = -1; // unlimited users can search
        searchingSemaphore = new Semaphore(0, Int32.MaxValue);
        writerMutex = new Semaphore(1, 1);
        readerMutex = new Mutex();
        curReaders = 0;
        curSearching = 0;
    }

    private void beforeRead(bool searching = false)
    {

        readerMutex.WaitOne();
        curReaders++;

        if (curReaders == 1) // if is the first reader get the key
            writerMutex.WaitOne();

        readerMutex.ReleaseMutex();

        if (searching) // limit the number of searchers
        {
            Interlocked.Increment(ref curSearching);
            if (maxUsersSearching != -1)
                searchingSemaphore.WaitOne();
        }

    }

    private void afterRead(bool searching = false)
    {
        readerMutex.WaitOne();
        curReaders--;
        if (searching)
        {
            if (maxUsersSearching != -1)
                searchingSemaphore.Release();
            Interlocked.Decrement(ref curSearching);
        }

        if (curReaders == 0) // if the last reader release the key
            writerMutex.Release();

        readerMutex.ReleaseMutex();
    }
    public String getCell(int row, int col)
    {
        if (row < 1 || row > rows || col < 1 || col > cols)
            return null;

        return mat[row-1, col-1];
    }
    public bool setCell(int row, int col, String str)
    {
        if (row < 1 || row > rows || col < 1 || col > cols)
            return false;

        writerMutex.WaitOne();

        // set the string at [row,col]
        mat[row-1, col-1] = string.Copy(str);
        writerMutex.Release();
        return true;
    }
    public bool searchString(String str, ref int row, ref int col)
    {
        // search the cell with string str, and return true/false accordingly.
        // stores the location in row,col.
        // return the first cell that contains the string (search from first row to the last row)
        beforeRead(true);
        for (int i=0; i<rows; i++)
        {
            for(int j = 0; j < cols; j++)
            {
                if(mat[i, j].Equals(str))
                {
                    row = i+1;
                    col = j+1;
                    afterRead(true);
                    return true;
                }

            }
        }
        afterRead(true);
        return false;
    }
    public bool exchangeRows(int row1, int row2)
    {
        if (row1 < 1 || row1 > rows || row2 < 1 || row2 > rows)
            return false;
        if (row1 == row2)
            return true;

        writerMutex.WaitOne();

        string temp;
        // exchange the content of row1 and row2
        for (int j = 0; j < cols; j++)
        {
            temp = mat[row1 - 1, j];
            mat[row1 - 1, j] = string.Copy(mat[row2 - 1, j]);
            mat[row2 - 1, j] = string.Copy(temp);
        }

        writerMutex.Release();
        return true;
    }
    public bool exchangeCols(int col1, int col2)
    {
        if (col1 < 1 || col1 > cols || col2 < 1 || col2 > cols)
            return false;
        if (col1 == col2)
            return true;

        writerMutex.WaitOne();

        string temp;
        // exchange the content of row1 and row2
        for (int i = 0; i < rows; i++)
        {
            temp = mat[i, col1 - 1];
            mat[i, col1 - 1] = string.Copy(mat[i, col2 - 1]);
            mat[i, col2 - 1] = string.Copy(temp);
        }
        writerMutex.Release();
        return true;
    }
    public bool searchInRow(int row, String str, ref int col)
    {
        if (row < 1 || row > rows)
            return false;
        beforeRead(true);
        // perform search in specific row
        for (int j = 0; j < cols; j++)
        {
            if (mat[row-1, j].Equals(str))
            {
                col = j+1;
                afterRead(true);
                return true;
            }
        }
        afterRead(true);
        return false;
    }
    public bool searchInCol(int col, String str, ref int row)
    {
        if (col < 1 || col > cols)
            return false;
        beforeRead(true);
        // perform search in specific col
        for (int i = 0; i < rows; i++)
        {
            if (mat[i, col-1].Equals(str))
            {
                row = i+1;
                afterRead(true);
                return true;
            }
        }
        afterRead(true);
        return false;
    }
    public bool searchInRange(int col1, int col2, int row1, int row2, String str, ref int row, ref int col)
    {
        if (row1 < 1 || row1 > rows || col1 < 1 || col1 > cols || row2 < 1 || row2 > rows || col2 < 1 || col2 > cols || col1 > col2 || row1 > row2)
            return false;
        beforeRead(true);
        // perform search within spesific range: [row1:row2,col1:col2] 
        //includes col1,col2,row1,row2
        for (int i = row1-1; i <= row2-1; i++)
        {
            for (int j = col1-1; j <= col2-1; j++)
            {
                if (mat[i, j].Equals(str))
                {
                    row = i+1;
                    col = j+1;
                    afterRead(true);
                    return true;
                }

            }
        }
        afterRead(true);
        return false;
    }
    public bool addRow(int row1)
    {
        if (row1 < 1 || row1 > rows)
            return false;

        writerMutex.WaitOne();
        string[,] newMat = new string[rows+1,cols];       
        int k = 0;
        //add a row after row1
        for (int i = 0; i <= rows; i++)
        {
            
            for (int j = 0; j < cols; j++)
            {
                if (i == row1) // if is the new row start empty
                {
                    newMat[i, j] = "";
                    k = -1;
                }        
                else // copy the old rows
                {
                    newMat[i, j] = string.Copy(mat[k+i, j]);
                    
                }
                    
            }
        }
        

        mat = newMat;
        rows++;
        writerMutex.Release();   
        return true;
    }
    public bool addCol(int col1)
    {
        if (col1 < 1 || col1 > cols)
            return false;

        writerMutex.WaitOne();
        string[,] newMat = new string[rows, cols+1];
        
        //add a col after col1
        for (int i = 0; i < rows; i++)
        {
            int k = 0;
            for (int j = 0; j <= cols; j++)
            {
                if (j == col1) // if is the new col start empty
                    newMat[i, j] = "";
                else // copy the old cols
                    newMat[i, j] = string.Copy(mat[i, k++]);
            }
        }


        mat = newMat;
        cols++;
        writerMutex.Release();
        return true;
    }
    public void getSize(ref int nRows, ref int nCols)
    {
        // return the size of the spreadsheet in nRows, nCols

        nRows = rows;
        nCols = cols;

    }

    public bool setConcurrentSearchLimit(int nUsers)
    {

        readerMutex.WaitOne();
        if (nUsers < 1 || nUsers < curSearching)
        {
            readerMutex.ReleaseMutex();
            return false;
        }
        
        if (maxUsersSearching == -1)
        {
            searchingSemaphore.Release(nUsers);
        }
        else if(nUsers != maxUsersSearching)
        {
            if(nUsers < maxUsersSearching)
            {
                for (int i = nUsers; i < maxUsersSearching;i++)
                    searchingSemaphore.WaitOne();
            }
            else
            {
                searchingSemaphore.Release(nUsers- maxUsersSearching);
            }
        }

        maxUsersSearching = nUsers;
        readerMutex.ReleaseMutex();

        return true;
    }

    public bool save(String fileName)
    {
        // save the spreadsheet to a file fileName.
        // you can decide the format you save the data. There are several options.
        beforeRead();
        try
        {
            using (StreamWriter file = new StreamWriter(fileName))
            {
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        file.Write(mat[i, j] + ",");
                    }
                    file.WriteLine();
                }
            }
        }catch(Exception e)
        {
            afterRead();
            return false;
        }

        afterRead();
        return true;
    }
    public bool load(String fileName)
    {
        // load the spreadsheet from fileName
        // replace the data and size of the current spreadsheet with the loaded data

        // Get the file's text.
        string whole_file;
        try
        {
            whole_file = System.IO.File.ReadAllText(fileName);
        }
        catch (Exception e)
        {
            return false;
        }

        // Split into lines.
        whole_file = whole_file.Replace('\n', '\r');
        string[] lines = whole_file.Split(new char[] { '\r' },
            StringSplitOptions.RemoveEmptyEntries);

        // See how many rows and columns there are.
        int num_rows = lines.Length;
        int num_cols = lines[0].Split(',').Length-1;

        // Allocate the data array.
        string[,] values = new string[num_rows, num_cols];

        // Load the array.
        for (int r = 0; r < num_rows; r++)
        {
            string[] line_r = lines[r].Split(',');
            for (int c = 0; c < num_cols; c++)
            {
                values[r, c] = line_r[c];
            }
        }

        mat = values;
        rows = num_rows;
        cols = num_cols;
        return true;
    }

    public int getCols()
    {
        return cols;
    }
    public int getRows()
    {
        return rows;
    }

}