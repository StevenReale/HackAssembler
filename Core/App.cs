using System.Text.RegularExpressions;

namespace HackAssembler.Core;

public class App(string fileName)
{
    private const string inputFilePath = "C:\\Users\\StevenReale\\source\\repos\\HackAssembler\\InputFiles\\";
    private const string outputFilePath = "C:\\Users\\StevenReale\\source\\repos\\HackAssembler\\OutputFiles\\";
    private readonly string inputFile = inputFilePath + fileName + ".asm";
    private readonly string outputFile = outputFilePath + fileName + ".hack";
    public Dictionary<string, int> symbolTable = [];
    private int register;

    public void Run()
    {
        if (!File.Exists(inputFile))
        {
            Console.WriteLine($"Error: The file '{fileName}' does not exist.");
            return;
        }
        
        try
        {
            initializeSymbolTable();
            processLabels();

            //main parsing handling
            using StreamReader reader = new(inputFile);
            using StreamWriter writer = new(outputFile);
            register = 16;
            string? line;

            while ((line = reader.ReadLine()) != null)
            {
                line = line.Split("//")[0].Split("(")[0].Trim(); //Removes anything that follows a double-slash, any whitespace, and any label declarations
                if (line == "")
                    continue;

                if (line[0] == '@')
                    line = processAInstruction(line);
                else
                    line = processCInstruction(line);

                writer.WriteLine(line);
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
        }
    }

    private string processAInstruction(string line)
    {
        line = line.Substring(1);
        if (int.TryParse(line, out int regLiteral))
        {
            line = getBinary(regLiteral);
        }
        else
        {
            if (symbolTable.TryGetValue(line, out int value))
            {
                line = getBinary(value);
            }
            else
            {
                symbolTable.Add(line, register);
                line = getBinary(register);
                register++;
            }
        }
        return line;
    }

    private string processCInstruction(string line)
    {
        (string? dest, string? comp, string? jump) = parseLine(line);
        return "111" + parseComp(comp) + parseDest(dest) + parseJump(jump);
    }

    private string parseComp(string comp)
    {
        return comp.Trim() switch
        {
            "0" => "0101010",
            "1" => "0111111",
            "-1" => "0111010",
            "D" => "0001100",
            "A" => "0110000",
            "!D" => "0001101",
            "!A" => "0110001",
            "-D" => "0001111",
            "-A" => "0110011",
            "D+1" => "0011111",
            "A+1" => "0110111",
            "D-1" => "0001110",
            "A-1" => "0110010",
            "D+A" or "A+D" => "0000010",
            "D-A" => "0010011",
            "A-D" => "0000111",
            "D&A" or "A&D" => "0000000",
            "D|A" or "A|D" => "0010101",
            "M" => "1110000",
            "!M" => "1110001",
            "M+1" => "1110010",
            "M-1" => "1110010",
            "D+M" or "M+D" => "1000010",
            "D-M" => "1010011",
            "M-D" => "1000111",
            "D&M" or "M&D" => "1000000",
            "D|M" or "M|D" => "1010101",
            _ => throw new Exception("Error: Bad computation syntax"),
        };
    }
    private string parseDest(string? dest)
    {
        return (dest?.Trim()) switch
        {
            (null) => "000",
            ("M") => "001",
            ("D") => "010",
            ("MD") or ("DM") => "011",
            ("A") => "100",
            ("AM") or ("MA") => "101",
            ("AD") or ("DA") => "110",
            ("AMD") => "111",
            _ => throw new Exception("Error: Bad destination syntax"),
        };
    }
    private string parseJump(string? jump)
    {
        return (jump?.Trim()) switch
        {
            (null) => "000",
            ("JGT") => "001",
            ("JEQ") => "010",
            ("JGE") => "011",
            ("JLT") => "100",
            ("JNE") => "101",
            ("JLE") => "110",
            ("JMP") => "111",
            _ => throw new Exception("Error: Bad jump syntax"),
        };
    }
    private (string?, string?, string?) parseLine(string line)
    {
        string? jump = null;
        string? dest = null;
        string? comp;

        var parts = line.Split(";");
        if (parts.Length > 1)
        {
            jump = parts[1];
        }
        
        var remainingParts = parts[0].Split("=");
        if (remainingParts.Length > 1)
        {
            comp= remainingParts[1];
            dest= remainingParts[0];
        }
        else
        {
            comp = remainingParts[0];
        }

        return (dest, comp, jump);
    }

    private void processLabels()
    {
        using StreamReader reader = new(inputFile);
        string? line;
        int lineNumber = 0;
        while ((line = reader.ReadLine()) != null)
        {
            line = line.Split("//")[0].Trim(); //Removes anything that follows a double-slash and any whitespace
            if (line == "")
                continue;

            Match match = Regex.Match(line, @"^\((\w+)\)$");
            if (match.Success)
            {
                string label = match.Groups[1].Value;
                symbolTable.Add(label, lineNumber);
            }
            else
            {
                lineNumber++;
            }
        }
    }

    private string getBinary(int regLiteral)
    {
        string binaryString = Convert.ToString(regLiteral, 2);
        while (binaryString.Length < 16)
        {
            binaryString = "0" + binaryString;
        }
        return binaryString;
    }

    private void initializeSymbolTable()
    {
        symbolTable.Clear();
        
        //Basic registers
        for (int i = 0; i <= 15; i++)
        {
            symbolTable.Add("R"+i, i);
        }

        //Other pre-defined symbols
        symbolTable.Add("SCREEN", 16384);
        symbolTable.Add("KBD", 24576);
        symbolTable.Add("SP", 0);
        symbolTable.Add("LCL", 1);
        symbolTable.Add("ARG", 2);
        symbolTable.Add("THIS", 3);
        symbolTable.Add("THAT", 4);
    }

}
