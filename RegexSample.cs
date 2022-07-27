using System.Text.RegularExpressions;

// regex sample

var text = "[XIC(InputData[6].8)OTE(Port4.Diag_Cold_junction);]";

var rgxBase = new Regex(@"[a-zA-Z]*\([a-zA-Z|.|0-9|\[|\]|_|#]*\)");
var rgxInst = new Regex(@"[a-zA-Z|0-9]*\(");
var rgxOpe = new Regex(@"\([a-zA-Z|.|0-9|\[|\]|_|#]*\)$");

var matches = rgxBase.Matches(text);

foreach (var m in matches.Select(m => m.Value))
{
    var inst = rgxInst.Match(m).Value.Replace("(", string.Empty);
    var ope = rgxOpe.Match(m).Value.Replace("(", string.Empty).Replace(")", string.Empty);
    Console.WriteLine(inst);
    Console.WriteLine(ope);
}

Console.ReadKey();