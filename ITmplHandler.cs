#region Using directives

using System;
using System.Collections.Generic;
using System.Text;

#endregion

using Igs.Hcms.Tmpl.Tokens;
namespace Igs.Hcms.Tmpl
{
    public interface ITmplHandler {
        void BeforeProcess(TmplManager manager);

        void AfterProcess(TmplManager manager);

        void BeforeProcess(TmplManager manager, Tag tag, ref bool processInnerElements, ref bool captureInnerContent);

        void AfterProcess(TmplManager manager, Tag tag, string innerContent);

    }
}
