// See https://aka.ms/new-console-template for more information
using Spire.Pdf;

namespace PdfConvToExcel{
    internal class Program
    {
        private static readonly string _tempPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "temp.xlsx");
        
        [STAThread]
        static void Main()
        {
            var exeParam = ExecuteParams.Create(Environment.GetCommandLineArgs());

            var  pdf = new PdfDocument();
            pdf.LoadFromFile(exeParam.SrcFile);

            var dst = string.IsNullOrEmpty(exeParam.DstFile) ? _tempPath : exeParam.DstFile;
            pdf.SaveToFile(dst, FileFormat.XLSX);

            var excel = new Microsoft.Office.Interop.Excel.Application();
            excel.Workbooks.Open(dst);
            excel.Visible = true;
        }

        private class ExecuteParams
        {
            public string SrcFile { get; set; }
            public string DstFile { get; set; }

            public static ExecuteParams Create(string[] args)
            {
                // exeファイル名分引く
                var argsCnt = args.Length - 1;
                switch (argsCnt)
                {
                    case 0:
                        return new ExecuteParams(string.Empty);
                    case 1:
                        {
                            var src = GetSrcFilePath(args[1]);
                            return new ExecuteParams(src);
                        }
                    case 2:
                        {
                            var src = GetSrcFilePath(args[1]);
                            return new ExecuteParams(src, args[2]);
                        }

                    default: throw new ArgumentException();
                }
            }

            private static string GetSrcFilePath(string arg)
            {
                var src = arg;
                if(!IsValidSrcFile(src))
                {
                    using (var ofd = new OpenFileDialog())
                    {
                        ofd.Filter = "pdfファイル|*.pdf";
                        
                        if(ofd.ShowDialog() == DialogResult.OK)
                        {
                            src = ofd.FileName;
                        }
                    }
                }
                return src;
            }

            private ExecuteParams(string src)
            {
                if (!IsValidSrcFile(src))
                {
                    using (var ofd = new OpenFileDialog())
                    {
                        ofd.Filter = "pdfファイル|*.pdf";
                        ofd.ShowDialog();
                        src = ofd.FileName;
                    }
                }
                SrcFile = src;
            }

            private ExecuteParams(string src, string dst)
            {
                if (!IsValidSrcFile(src))
                {
                    using (var ofd = new OpenFileDialog())
                    {
                        ofd.Filter = "pdfファイル|*.pdf";
                        ofd.ShowDialog();
                        src = ofd.FileName;
                    }
                }
                if (!IsValidDstFile(dst)) throw new Exception("変換後ファイル指定が不正");
                SrcFile = src;
                DstFile = dst;
            }

            private static bool IsValidSrcFile(string src)
            {
                if (!File.Exists(src)) return false;
                if (!Path.GetExtension(src).Equals(".pdf")) return false;
                return true;
            }

            private static bool IsValidDstFile(string dst)
            {
                if (!Path.GetExtension(dst).Equals(".xlsx")) return false;
                return true;
            }
        }
    } 
}
