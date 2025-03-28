using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CSharpModelClassGenerator
{
    public class CSharpClassGenerator
    {
        // Metodo per generare il codice della classe
        public string GenerateClassCode(Dictionary<string, ClassNode> classMap, string selectedClass)
        {
            if (!classMap.ContainsKey(selectedClass))
                return string.Empty;

            var classNode = classMap[selectedClass];
            var codeBuilder = new StringBuilder();

            // Genera la dichiarazione della classe
            codeBuilder.AppendLine($"public class {selectedClass}");
            codeBuilder.AppendLine("{");

            // Aggiungi le proprietà della classe
            foreach (var property in classNode.Properties)
            {
                codeBuilder.AppendLine($"    public {property.Type} {property.Name} {{ get; set; }}");
            }

            // Costruttore senza parametri
            codeBuilder.AppendLine();
            codeBuilder.AppendLine($"    public {selectedClass}()");
            codeBuilder.AppendLine("    {");
            codeBuilder.AppendLine("    }");

            // Costruttore protetto che accetta tutte le proprietà
            codeBuilder.AppendLine();
            codeBuilder.AppendLine($"    protected {selectedClass}(");
            foreach (var property in classNode.Properties)
            {
                codeBuilder.AppendLine($"        {property.Type} {property.Name.ToLower()},");
            }
            codeBuilder.Remove(codeBuilder.Length - 3, 1); // Rimuove l'ultima virgola
            codeBuilder.AppendLine("    )");
            codeBuilder.AppendLine("    {");
            foreach (var property in classNode.Properties)
            {
                codeBuilder.AppendLine($"        this.{property.Name} = {property.Name.ToLower()};");
            }
            codeBuilder.AppendLine("    }");

            // Metodo factory
            codeBuilder.AppendLine();
            codeBuilder.AppendLine($"    public static {selectedClass} {selectedClass}Factory(");
            foreach (var property in classNode.Properties)
            {
                string paramName = char.ToLower(property.Name[0]) + property.Name.Substring(1); // Parametro con la minuscola
                codeBuilder.AppendLine($"        {property.Type} {paramName},");
            }
            codeBuilder.Remove(codeBuilder.Length - 3, 1); // Rimuove l'ultima virgola
            codeBuilder.AppendLine("    )");
            codeBuilder.AppendLine("    {");
            codeBuilder.AppendLine($"        return new {selectedClass}(");
            foreach (var property in classNode.Properties)
            {
                string paramName = char.ToLower(property.Name[0]) + property.Name.Substring(1); // Parametro con la minuscola
                codeBuilder.AppendLine($"            {paramName},");
            }
            codeBuilder.Remove(codeBuilder.Length - 3, 1); // Rimuove l'ultima virgola
            codeBuilder.AppendLine("        );");
            codeBuilder.AppendLine("    }");

            codeBuilder.AppendLine("}");

            return codeBuilder.ToString();
        }

        // Metodo per salvare il codice nel file della classe
        public void SaveClassCodeToFile(string filePath, string classCode)
        {
            try
            {
                // Scrivi il codice nel file
                File.WriteAllText(filePath, classCode);
                Console.WriteLine($"Class code saved to {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving class code: {ex.Message}");
            }
        }
    }
}
