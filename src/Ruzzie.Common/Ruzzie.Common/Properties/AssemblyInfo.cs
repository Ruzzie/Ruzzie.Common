using System;
using System.Resources;
using System.Reflection;
using System.Runtime.CompilerServices;
#if !PORTABLE
using System.Runtime.InteropServices;
#endif
// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Ruzzie.Common")]
[assembly: AssemblyDescription("Common libraries used in other projects")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("D Crafting")]
[assembly: AssemblyProduct("Ruzzie.Common")]
[assembly: AssemblyCopyright("Copyright © Dorus Verhoeckx 2020")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: NeutralResourcesLanguage("en")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: CLSCompliant(true)]
[assembly: InternalsVisibleTo("Ruzzie.Common.NET40.UnitTests")]
[assembly: InternalsVisibleTo("Ruzzie.Common.NET461.UnitTests")]
[assembly: InternalsVisibleTo("Ruzzie.Common.NetCore.UnitTests")]
[assembly: InternalsVisibleTo("Ruzzie.Common.NetCore31.UnitTests")]
[assembly: InternalsVisibleTo("Ruzzie.Common.UnitTests")]
#if !PORTABLE
[assembly: ComVisible(false)]
#endif