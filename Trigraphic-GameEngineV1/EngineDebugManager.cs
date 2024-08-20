using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trigraphic_GameEngineV1
{
    internal static class EngineDebugManager
    {
        // warnings are used in situations where a user of the engine through
        // custom code and use in the bounds intended by the engine might create
        // logic using engine methods in a way that is not ideal but doesnt
        // inhibit the functionality of engine logic itself or break the logic.
        // in these cases the engine method triggering this warning will still
        // execute as normal past the warning.

        public enum Warning
        {
            Default,
            OperationRedundancy
        }
        public static void throwNewOperationRedundancyWarning(string? message = null)
            => throwNewWarning(message, Warning.OperationRedundancy);
        public static void throwNewWarning(string? message = null, Warning type = Warning.Default)
        {

        }
    }
}
