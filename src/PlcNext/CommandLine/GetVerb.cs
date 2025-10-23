#region Copyright
///////////////////////////////////////////////////////////////////////////////
//
//  Copyright PHOENIX CONTACT GmbH & Co. KG
//
///////////////////////////////////////////////////////////////////////////////
#endregion

using System.Reflection;
using CommandLine;

namespace PlcNext.CommandLine
{
    [Verb(CommandLineConstants.GetVerb, HelpText = "Returns information about projects / targets / ...")]
    [ChildVerbs(typeof(GetTargetsVerb), typeof(GetSdksVerb), typeof(GetProjectSdksVerb), 
        typeof(GetSettingVerb), typeof(GetUpdateVersionsVerb), typeof(GetCompilerSpecificationsVerb),
        typeof(GetProjectInformationVerb))]
    [Obfuscation(Exclude = true, ApplyToMembers = true)]
    internal abstract class GetVerb : VerbBase
    {
    }
}
