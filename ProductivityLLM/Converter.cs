using System.IO;
using Microsoft.VisualBasic.FileIO;

namespace ProductivityLLM
{
    public class Converter
    {
        public static void CSVtoText() {            
            string csvFilePath = "C:\\Users\\pranoymadadi\\Downloads\\OutlookME_History_for_FHL.csv";
            string textFilePath = "C:\\Users\\pranoymadadi\\Downloads\\New Text Document.txt";

            // Read the CSV file
            using (TextFieldParser parser = new TextFieldParser(csvFilePath))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");

                // Get the header row
                string[] headerRow = parser.ReadFields();

                // Write the header row to the text file
                File.WriteAllText(textFilePath, string.Join(",", headerRow) + Environment.NewLine);

                // Write the data rows to the text file
                while (!parser.EndOfData)
                {
                    string[] dataRow = parser.ReadFields();
                    File.AppendAllText(textFilePath, string.Join(",", dataRow) + Environment.NewLine);
                }
            }
        }
    }
}
