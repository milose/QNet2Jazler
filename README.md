# QNet2Jazler

A simple program to import Song and Artist data from QNet Radio software to Jazler RadioStar.

## Usage

1. Export the data from QNet Radio to a text or excel file.
2. Export the Jazler RadioStar database to a .mdb file.
3. Open the Jazler database with Microsoft Access and import the QNet text file to a \_qnet table.
4. Compile the QNet2Jazler .NET executable with Visual Studio
5. Run the executable from command line: `QNet2Jazler.exe DATABASE.MDB`
6. Remove the \_qnet table.
7. Optionally compact the database from Microsoft Access' Database Tools

## Todo

- Create a collection of QNet objects instead of DataTable.

## License (MIT)

Copyright (c) 2015 milose

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
