using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObfuscatorUnitTests.Tests
{
    /// <summary>
    /// Lazy Hardcoded Path List.
    /// TODO: Move this into a config file eventually
    /// </summary>
    public static class GhostPackSolutionPaths
    {
        public const string SeatbeltSlnPath = @"S:\projects\malware-dropper\Seatbelt-master\Seatbelt.sln";
        public const string SeatbeltSrcPath = @"S:\projects\malware-dropper\Seatbelt-master\Seatbelt\Program.cs";

        public const string SafetyKatzSlnPath = @"S:\projects\malware-dropper\SafetyKatz-master\SafetyKatz.sln";

        public const string SharpDumpSlnPath = @"S:\projects\malware-dropper\SharpDump-master\Original\SharpDump-master\SharpDump.sln";
        public const string SharpDumpSrcPath =
            @"S:\projects\malware-dropper\SharpDump-master\Original\SharpDump-master\SharpDump\Program.cs";
    }
}
