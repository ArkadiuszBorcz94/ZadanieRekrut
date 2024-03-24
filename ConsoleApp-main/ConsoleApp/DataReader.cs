namespace ConsoleApp
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class DataReader
    {
        private List<ImportedObject> ImportedObjects;

        public void ImportAndPrintData(string fileToImport, bool printData = true)
        {
            try
            {
                ImportedObjects = new List<ImportedObject>();

                using (var streamReader = new StreamReader(fileToImport))
                {
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        var values = line.Split(';');
                        if (values.Length >= 7)
                        {
                            var importedObject = new ImportedObject
                            {
                                Type = values[0],
                                Name = values[1],
                                Schema = values[2],
                                ParentName = values[3],
                                ParentType = values[4],
                                DataType = values[5],
                                IsNullable = values[6]
                            };
                            ImportedObjects.Add(importedObject);
                        }
                    }
                }

                CleanUpImportedData();
                AssignNumberOfChildren();

                if (printData)
                    PrintData();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            Console.ReadLine();
        }

        private void CleanUpImportedData()
        {
            foreach (var importedObject in ImportedObjects)
            {
                importedObject.Type = importedObject.Type.Trim().Replace(" ", "").Replace(Environment.NewLine, "").ToUpper();
                importedObject.Name = importedObject.Name.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
                importedObject.Schema = importedObject.Schema.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
                importedObject.ParentName = importedObject.ParentName.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
                importedObject.ParentType = importedObject.ParentType.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
            }
        }

        private void AssignNumberOfChildren()
        {
            foreach (var importedObject in ImportedObjects)
            {
                importedObject.NumberOfChildren = ImportedObjects.Count(obj => obj.ParentType == importedObject.Type && obj.ParentName == importedObject.Name);
            }
        }

        private void PrintData()
        {
            foreach (var database in ImportedObjects.Where(obj => obj.Type == "DATABASE"))
            {
                Console.WriteLine($"Database '{database.Name}' ({database.NumberOfChildren} tables)");

                foreach (var table in ImportedObjects.Where(obj => obj.ParentType.ToUpper() == database.Type && obj.ParentName == database.Name))
                {
                    Console.WriteLine($"\tTable '{table.Schema}.{table.Name}' ({table.NumberOfChildren} columns)");

                    foreach (var column in ImportedObjects.Where(obj => obj.ParentType.ToUpper() == table.Type && obj.ParentName == table.Name))
                    {
                        Console.WriteLine($"\t\tColumn '{column.Name}' with {column.DataType} data type {(column.IsNullable == "1" ? "accepts nulls" : "with no nulls")}");
                    }
                }
            }
        }
    }

    public class ImportedObject
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Schema { get; set; }
        public string ParentName { get; set; }
        public string ParentType { get; set; }
        public string DataType { get; set; }
        public string IsNullable { get; set; }
        public double NumberOfChildren { get; set; }
    }
}