
using System;
using Starcounter;
using System.Linq;

namespace WhatsNotReferenced
{
    class Program
    {
        static void Main(string[] args)
        {
            RunStartupActions(args);

            Handle.GET("/whatsnotreferenced", () =>
            {
                var assets = UnreferencedAssets.Detect();

                var apps = string.Join(Environment.NewLine, assets.Apps.Select(a => a.DisplayName));
                var tables = string.Join(Environment.NewLine, assets.Tables.Select(c => c.FullName));
                var columns = string.Join(Environment.NewLine, assets.Columns.Select(c => c.TableName + "." + c.Name));

                var result = "Apps (exluding this):";
                result += Environment.NewLine + apps;
                result += Environment.NewLine + Environment.NewLine + "Tables:";
                result += Environment.NewLine + tables;
                result += Environment.NewLine + Environment.NewLine + "Columns:";
                result += Environment.NewLine + columns;

                return result;
            });
        }

        static void RunStartupActions(string[] args)
        {
            // Eventually:
            // args[n]: please:[action] supporting `star WhatsNotReferenced please:drop`

            var actions = args.Select(a =>
            {
                if (!a.StartsWith("please:")) throw new ArgumentException($"Invalid argument: {a}");
                return a.Substring("please:".Length);
            });

            foreach (var action in actions)
            {
                switch (action.ToLowerInvariant())
                {
                    case "drop":
                        throw new NotImplementedException("Dropping will be supported soon");
                    default:
                        throw new ArgumentOutOfRangeException(action, $"Don't know how to perform action {action}");
                }
            }
        }
    }
}