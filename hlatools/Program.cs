using hlatools.core;
using hlatools.core.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace hlatools
{
    class Program
    {
        static readonly string version = "0.0.0.0";

        static void PrintTopLevelHelp(IEnumerable<CommandVerb> cmds)
        {
            var hlpStr = string.Join("\n",
                string.Format("amgcompbio version {0} on {1}", version, DateTime.Now),
                "",
                "Commands:",
                string.Join("\n", cmds.OrderBy(kvp => kvp.Verb).Select(cmd => string.Format("  {0}\t{1}", cmd.Verb, cmd.GetShortDescription())))
                );
            Console.Out.WriteLine(hlpStr);
        }

        static void PrintCommandHelp(CommandVerb cmd)
        {
            var hlpStr = string.Join("\n",
                string.Format("{0} (version {1}): {2}", cmd.Verb, cmd.Version, cmd.GetShortDescription()),
                cmd.GetUsage(),
                "",
                cmd.GetParameterHelp()
                );
            Console.Out.WriteLine(hlpStr);
        }

        static bool IsHelpSignal(string arg)
        {
            return (string.Compare(arg, "-h", true) == 0
                    || (string.Compare(arg, "--h", true) == 0
                    || string.Compare(arg, "help", true) == 0
                    || string.Compare(arg, "-help", true) == 0
                    || string.Compare(arg, "--help", true) == 0));
        }

        static void RunCommand(CommandVerb cmd, Dictionary<string, string> paramKvps)
        {
            var stopWtch = new Stopwatch();
            stopWtch.Start();
            string errorMsg = null;
            string successMsg = null;
            successMsg = cmd.Run(paramKvps);
            try
            {
                //successMsg = cmd.Run(paramKvps);
            }
            catch (Exception ex)
            {
                errorMsg = ex.ToString();
            }
            finally
            {
                stopWtch.Stop();
                var msg = successMsg ?? errorMsg;
                var status = successMsg == null ? "FAILED" : "SUCCEEDED";
                //Console.WriteLine("Call to '{0}' {1} in {2} seconds. Summary:\n{3}",
                //    cmd.Verb,
                //    status,
                //    stopWtch.Elapsed.TotalSeconds,
                //    msg);

            }
        }

        static Dictionary<string, CommandVerb> ComposeCommands()
        {
            var allCmds = new List<CommandVerb>()
            {
                new HmmerConvert(),
                new AssignXLoc(),
                new BuildTabletAssembly()
            };
            var cmdsDict = new Dictionary<string, CommandVerb>(allCmds.Count, StringComparer.InvariantCultureIgnoreCase);
            foreach (var cmd in allCmds)
            {
                cmdsDict.Add(cmd.Verb, cmd);
            }
            return cmdsDict;
        }

        static CommandVerb GetRequestedCmd(Dictionary<string, CommandVerb> cmdsDict, IEnumerable<string> args)
        {
            string verbName = args.FirstOrDefault();
            if (verbName == null)
            {
                return null;
            }
            cmdsDict.TryGetValue(verbName, out CommandVerb userCmd);
            return userCmd;
        }

        static bool TryPrintHelp(IEnumerable<CommandVerb> cmdsDict, CommandVerb selectedCmd, IList<string> args)
        {
            var hlp = false;
            if (args.Count == 0 || (args.Count == 1 && IsHelpSignal(args[0])))
            {
                PrintTopLevelHelp(cmdsDict);
                hlp = true;
            }
            else if (selectedCmd == null && args.Count > 0)
            {
                Console.Error.WriteLine("ERROR: '{0}' is an unknown command", args[0]);
                PrintTopLevelHelp(cmdsDict);
                hlp = true;
            }
            else if (selectedCmd == null)
            {
                Console.Error.WriteLine("ERROR: Did not identify command");
                PrintTopLevelHelp(cmdsDict);
                hlp = true;
            }
            else if (args.Count == 1 || (args.Count == 2 && IsHelpSignal(args[1])))
            {
                PrintCommandHelp(selectedCmd);
                hlp = true;
            }
            return hlp;
        }

        static Dictionary<string, string> ParseInputParams(IEnumerable<string> args)
        {

            int n = 0;
            string key = null;
            var q = new Queue<string>(args);
            var inputParams = new Dictionary<string, string>();
            while (q.Count > 0)
            {
                var arg = q.Dequeue();
                var argTag = arg.StartsWith("-") ? (arg.StartsWith("--") ? 2 : 1) : 0;
                if (argTag > 0 || key != null)
                {
                    if (key != null)
                    {
                        Dict.Appendsert(inputParams, key, arg);
                        while (q.Count > 0 && !q.Peek().StartsWith("-"))
                        {
                            arg = q.Dequeue();
                            Dict.Appendsert(inputParams, key, arg);
                        }
                        key = null;
                    }
                    else
                    {
                        key = arg.Substring(argTag);
                    }
                }
                else
                {
                    inputParams.Add(string.Format("p_{0}", n), arg);
                    key = null;
                }
                n++;
            }
            return inputParams;
        }

        static void Main(string[] args)
        {
            var cmdsDict = ComposeCommands();
            var cmd = GetRequestedCmd(cmdsDict, args);
            if (TryPrintHelp(cmdsDict.Values, cmd, args))
            {
                return;
            }
            var paramKvps = ParseInputParams(args.Skip(1));
            RunCommand(cmd, paramKvps);

            //var stpWtch = new Stopwatch();stpWtch.Start();
            //using (var readPool = new ObjPool<SamSeq>(()=>new SamSeq()))
            //using (var samPrsr = SamParserCore.FromFilepath(args[0], readPool.Pop))
            //using (var walker = new PhasingReadWalker<SamSeq>(samPrsr.GetRecords().GetEnumerator(), readPool.Push))
            //{
            //    var hdr = samPrsr.Header;
            //    walker.RNameComparer = samPrsr.Header.GetSqOrder();
            //    //var allChrReads = walker.GetNextPileup("chr2");
            //    //allChrReads = walker.GetNextPileup("chr3");
            //    //var reads = walker.GetNextPileup("chr2", 219294535, 219303094);
            //    //var nxtReads = walker.GetNextPileup("chr2", 219376123, 219376275);
            //    //var nxtNxtReads = walker.GetNextPileup("chr2", 219376123, 219376125);
            //    var nxtNxtNxtReads = walker.GetNextPileup("chr2", 235667632, 235667711);
            //    stpWtch.Stop();
            //}
            //stpWtch.Stop();
            //var duration = stpWtch.Elapsed.TotalSeconds;
        }
    }
}
