
using System;
using Starcounter;
using System.Linq;

namespace WhatsNotReferenced
{
    class Program
    {
        static void Main(string[] args)
        {
            // Eventually:
            // args[n]: please:[action] supporting `star WhatsNotReferenced please:drop`

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
    }
}