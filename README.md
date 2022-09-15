# Biosero Syntax Editor
The Biosero Syntax Editor is a text editor control specific to Biosero Base objects with built in, code completion. The application uses intelli sense to detect the word being typed and displays a list of objects with the root contained in them. The purpose of this is to make it easier for customers to use GBG without needing to know every object and function in Green Button Go.

Live Demo in documentation file

Getting Started:
To use this software, visual studio must have a Telerik license because it uses the Telerik for WPF Syntax Editor for the UI. This also must be run on .NET Core or later. 
Dependencies:
-	Microsoft.CodeAnalysis.CSharp
-	System.IO.Ports
-	Telerik.Windows.Controls.SyntaxEditor.for.Wpf.Xaml
Assemblies:
-	Biosero.GreenButtonGo.Base.dll
-	System.IO.Ports.dll
-	System.ComponentModel.Composition.dll
Build and Test:
The solution can either be built by hitting the Build button or Run WpfApp5 button in Visual Studio.
 
Testing can be done by setting breakpoints in the different methods which can be categorized. If there is a need to test the parsing or format of the text, breakpoints can be placed in the syntaxEditor_DocumentContentChanged and syntaxEditor_PreviewSyntaxEditorKeyDown methods. If the completion window and process of text replacement needs to be tested it can be done by setting breakpoints in the syntaxEditor_PreviewSyntaxEditorKeyDown, CompletionListWindow_TextInserting, GetWordOnCaret methods. To test the API and assemblies used breakpoints can be set in the GetAvailibleApi method.
Contribute:
Developers can add breakpoints and test to see if the completion works for all objects as well as changing the assembly being used. One thing that needs to be improved is that the format of the text must be exact for the completion to work. Making the recognition more generic so mistakes can be made and autocorrected would greatly improve the quality.

