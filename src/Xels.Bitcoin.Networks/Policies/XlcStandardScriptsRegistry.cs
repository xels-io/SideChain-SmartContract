﻿using System.Collections.Generic;
using NBitcoin;

namespace Xels.Bitcoin.Networks.Policies
{
    /// <summary>
    /// Xlc-specific standard transaction definitions.
    /// </summary>
    public class XlcStandardScriptsRegistry : StandardScriptsRegistry
    {
        public const int MaxOpReturnRelay = 83;

        // Need a network-specific version of the template list
        private static readonly List<ScriptTemplate> standardTemplates = new List<ScriptTemplate>
        {
            new PayToPubkeyHashTemplate(),
            new PayToPubkeyTemplate(),
            new PayToScriptHashTemplate(),
            new PayToMultiSigTemplate(),
            new PayToFederationTemplate(),
            new TxNullDataTemplate(MaxOpReturnRelay),
            new PayToWitTemplate()
        };

        public override List<ScriptTemplate> GetScriptTemplates => standardTemplates;        
    }
}
