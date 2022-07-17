using Utils;

namespace OmProjectAnalyzer.Models
{
    public class Rung
    {
        public int Idx { get; set; }
        public IReadOnlyList<Contact> Contacts { get; }
        public IReadOnlyList<Coil> Coils { get; }
        public IReadOnlyList<FB> Fbs { get; }
        public IReadOnlyList<BoxST> BoxSTs { get; }

        public Rung(int idx, IReadOnlyList<Contact> contacts, IReadOnlyList<Coil> coils, IReadOnlyList<FB> fbs, IReadOnlyList<BoxST> boxSTs)
        {
            Idx = idx;
            Contacts = contacts;
            Coils = coils;
            Fbs = fbs;
            BoxSTs = boxSTs;
        }

        public IReadOnlyList<string> GetVariablesAndParameters()
        {
            var symbols = new List<string>();
            symbols.AddRange(Contacts.Select(c => c.Variable));
            symbols.AddRange(Coils.Select(c => c.Variable));

            //Fb instance名は一旦のぞく。
            //symbols.AddRange(Fbs.Select(f => f.InstanceName));
            foreach (var args in Fbs.Select(f => f.Args))
            {
                symbols.AddRange(args.Select(a => a.ActualArg));
                symbols.AddRange(args.Select(a => a.Name));
            }
            return symbols.Where(s => !string.IsNullOrEmpty(s)).ToList();
        }

        public IReadOnlyDictionary<string, int> GetFbAndCnt()
        {
            var dic = new Dictionary<string, int>();
            var fbNameList = Fbs.Select(f => f.FbName);
            foreach (var f in fbNameList)
            {
                if (dic.ContainsKey(f)) continue;
                var count = fbNameList.Count(fb => fb == f);
                dic.Add(f, count);
            }
            return dic;
        }

        public IReadOnlyList<string> GetAllSymbols()
        {
            var symbols = new List<string>();
            symbols.AddRange(Contacts.Select(c => c.Variable));
            symbols.AddRange(Coils.Select(c => c.Variable));
            symbols.AddRange(Fbs.Select(f => f.InstanceName));

            // FB/FUN　引数名 / 実引数
            foreach (var args in Fbs.Select(f => f.Args))
            {
                symbols.AddRange(args.Select(a => a.Name));
                symbols.AddRange(args.Select(a => a.ActualArg));
            }
            return symbols.Where(s => !string.IsNullOrEmpty(s)).ToList();
        }

        // Network内での行/列を表現して出力 key = 行 value =その行にあるシンボル
        public IReadOnlyList<string> GetRowMnemonics(int fbOffset)
        {
            // 何行あるかまず検索。
            var yList = new List<int>();
            var mnemonics = new List<string>();

            yList.AddRange(Contacts.Select(c => c.Y));
            yList.AddRange(Coils.Select(c => c.Y));
            yList.AddRange(Fbs.Select(c => c.Y));
            if (yList.Count == 0) return mnemonics;
            var yMax = yList.Max();

            for (var y = 0; y <= yMax; y++)
            {
                var dicYComponents = new Dictionary<int, string>();
                // y行目にいるコイル
                var yCoils = Coils.Where(c => c.Y == y);

                // y行目にいるコイル
                var yContacts = Contacts.Where(c => c.Y == y);

                // y行目にいるFB FUN
                var yFbs = Fbs.Where(c => c.Y == y);

                yContacts.Where(c => c.Y == y).ForEach(c => dicYComponents.Add(c.X, "LD"));
                yCoils.Where(c => c.Y == y).ForEach(c => dicYComponents.Add(c.X, "OUT"));
                yFbs.Where(c => c.Y == y).ForEach(f => dicYComponents.Add(f.X, f.FbName));

                var xList = new List<int>();
                xList.AddRange(yCoils.Select(c => c.X));
                xList.AddRange(yContacts.Select(c => c.X));
                xList.AddRange(yFbs.Select(c => c.X));
                var mnemonic = new List<string>();
                for (var x = 0; x <= xList.Max(); x++)
                {
                    var offset = 0;

                    if (!xList.Contains(x))
                    {
                        var dicXComponents = new Dictionary<int, string>();
                        Contacts.Where(c => c.X == x).ForEach(c => dicXComponents.Add(x, "LD"));
                        Coils.Where(c => c.X == x).ForEach(c => dicXComponents.Add(x, "OUT"));
                        Fbs.Where(f => f.X == x).ForEach(f => dicXComponents.Add(x, f.FbName));

                        if (dicXComponents.Values.Any(ele => !ele.Equals("LD") && !ele.Equals("OUT")))
                        {
                            offset += fbOffset;
                        }
                        else
                        {
                            offset++;
                        }
                        for (var cnt = 0; cnt < offset; cnt++)
                        {
                            mnemonic.Add(string.Empty);
                        }
                        continue;
                    }
                    var num = dicYComponents[x].Equals("LD") || dicYComponents[x].Equals("OUT") ? 1 : 4;
                    for(var cnt = 0; cnt < num; cnt++)
                    {
                        mnemonic.Add(dicYComponents[x]);
                    }
                }
                mnemonics.Add(string.Join(",", mnemonic));
            }
            return mnemonics;
        }

        // for Debug
        public IReadOnlyDictionary<RowInfo, int> GetNewworkReqCellNumInfo()
        {
            // 何行あるかまず検索。
            var yList = new List<int>();
            var dic = new Dictionary<RowInfo, int>();

            yList.AddRange(Contacts.Select(c => c.Y));
            yList.AddRange(Coils.Select(c => c.Y));
            yList.AddRange(Fbs.Select(c => c.Y));
            if (yList.Count == 0) return dic;
            var yMax = yList.Max();

            for (var y = 0; y <= yMax; y++)
            {
                var count = 0;

                // y行目にいる接点
                count += (Contacts.Where(c => c.Y == y).Count()) * 1;

                // y行目にいるコイル
                count += (Coils.Where(c => c.Y == y).Count()) * 1;

                // y行目にいるFB FUN
                var targetFb = Fbs.Where(c => c.Y == y);

                count += Fbs.Where(c => c.Y == y).Count() * 4;

                dic.Add(new RowInfo(y), count);
            }
            return dic;
        }

        // rung 内での行番号
        public class RowInfo
        {
            public string Offset { get; }
            public int Value { get; }
            public RowInfo(int idx, string offset = "")
            {
                Value = idx;
                Offset = offset;
            }
        }
    }
}
