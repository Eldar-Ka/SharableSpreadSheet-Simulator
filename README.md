# SharableSpreadSheet-Simulator

An in-memory shared spreadsheet management simulator , simulating multiple users(threads) which perform arbitrary operations concurrently.
This project was made as a part of an assignment in the course "Introduction to Operating Systems" Associated with Ben-Gurion University of the Negev

-The spreadsheet represent a table of n*m cells (n=rows, m=columns).

-Each cell holds a string 

-The speardsheet starts at cell 1,1 (top, left).

The spreadsheet supports several elementary operations(simulating by random operations that the "users" do ):

-getCell

-setCell

-searchString

-exchangeRows

-exchangeCols

-searchInRow

-searchInCol

-searchInRange

-addRow

-addCol

-getSize

-setConcurrentSearchLimit

-save

For each attempt of an operation there is an output representing the success or inability of a "user" to execute it.

The initial application arguments given in this project are as follows:
Simulator {rows} {cols} {nThreads} {nOperations}

-nThread is the number of threads (users) cocrrently works on the object

-nOperations is the number of random operations each thread performs
