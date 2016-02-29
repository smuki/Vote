#region Using directives

using System;
using System.Collections.Generic;
using System.Text;

#endregion

using Igs.Hcms.Volt.Tokens;
namespace Igs.Hcms.Volt
{
    public interface ITmplHandler {
        void BeforeProcess(TmplManager manager);

        void AfterProcess(TmplManager manager);

        void BeforeProcess(TmplManager manager, Tag tag, ref bool processInnerTokens, ref bool captureInnerContent);

        void AfterProcess(TmplManager manager, Tag tag, string innerContent);

    }
}
