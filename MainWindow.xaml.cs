using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.SyntaxEditor.Tagging.Taggers;
using Telerik.Windows.Controls.SyntaxEditor.UI;
using Telerik.Windows.Controls.SyntaxEditor.UI.IntelliPrompt.Completion;
using Telerik.Windows.SyntaxEditor.Core.Text;
using System.IO.Ports;
using System.Xml;

namespace UpdatedBioseroSyntaxEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CompletionInfoCollection fullCompletionListItems;
        private bool periodFound = false;
        private string obj = "";
        private bool actionInWindow = false;
        List<Assembly> allAssemblies = new List<Assembly>();
        private Assembly baseAssembly;
        private string assemblyPath;

        public MainWindow()
        {
            InitializeComponent();

            allAssemblies.Add(Assembly.LoadFrom(@"C:\Program Files (x86)\Progress\Telerik UI for WPF R2 2022\Binaries\WPF60\Telerik.Windows.Controls.dll"));
            baseAssembly = allAssemblies[0];
            CSharpTagger? cSharptagger = new(syntaxEditor);
            syntaxEditor.TaggersRegistry.RegisterTagger(cSharptagger);
            using (StreamReader reader = new(@"..\..\..\SyntaxEditorTemplate.txt"))
            {
                this.syntaxEditor.Document = new Telerik.Windows.SyntaxEditor.Core.Text.TextDocument(reader);
            }

            this.syntaxEditor.DocumentContentChanged += syntaxEditor_DocumentContentChanged;
            this.syntaxEditor.IntelliPrompts.CompletionListWindow.TextInserting += CompletionListWindow_TextInserting;

            CreateDefaultWindow();

        }

        private void CreateDefaultWindow()
        {
            List<string> api = GetAvailableAPI();
            this.fullCompletionListItems = new CompletionInfoCollection();
            foreach (string member in api)
            {
                fullCompletionListItems.Add(new CompletionInfo(member));
            }
            this.syntaxEditor.IntelliPrompts.CompletionListWindow.Presenter.CompletionListItems = fullCompletionListItems;

        }

        private void syntaxEditor_PreviewSyntaxEditorKeyDown(object sender, Telerik.Windows.Controls.SyntaxEditor.UI.PreviewSyntaxEditorKeyEventArgs e) //works
        {
            if (((e.Key == Key.Space || e.Key == Key.OemPeriod) && KeyboardModifiers.IsControlDown))
            {
                e.OriginalArgs.Handled = true;
            }
            else if (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Escape ||
                e.Key == Key.Home || e.Key == Key.End || e.Key == Key.CapsLock || e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                return;
            }
            else if (e.Key == Key.Enter && !this.syntaxEditor.IntelliPrompts.CompletionListWindow.IsOpen)
            {
                obj = "";
                periodFound = false;
                CreateDefaultWindow();
                return;
            }
            else if (e.Key == Key.OemPeriod)
            {
                string word = GetWordOnCaret(e.Key);
                obj = word;
                List<string> api = GetAvailableAPI(obj);
                this.fullCompletionListItems = new CompletionInfoCollection();
                foreach (string member in api)
                {
                    fullCompletionListItems.Add(new CompletionInfo(member));
                }
                this.syntaxEditor.IntelliPrompts.CompletionListWindow.Presenter.CompletionListItems = ConvertToCompletionInfoCollection(fullCompletionListItems);

                this.syntaxEditor.CompleteCode();
            }
            else if (!this.syntaxEditor.IntelliPrompts.CompletionListWindow.IsOpen)
            {
                string word = GetWordOnCaret();
                if (periodFound == true)
                {
                    List<string> api = GetAvailableAPI(obj);
                    this.fullCompletionListItems = new CompletionInfoCollection();
                    foreach (string member in api)
                    {
                        fullCompletionListItems.Add(new CompletionInfo(member));
                    }
                    this.syntaxEditor.IntelliPrompts.CompletionListWindow.Presenter.CompletionListItems = ConvertToCompletionInfoCollection(fullCompletionListItems.Where(x => x.Text.Contains(word)));
                }
                else if (!string.IsNullOrEmpty(word))
                {
                    this.syntaxEditor.IntelliPrompts.CompletionListWindow.Presenter.CompletionListItems = ConvertToCompletionInfoCollection(fullCompletionListItems.Where(x => x.Text.Contains(word)));
                }
                else
                {
                    this.syntaxEditor.IntelliPrompts.CompletionListWindow.Presenter.CompletionListItems = fullCompletionListItems;
                }

                this.syntaxEditor.CompleteCode();
            }

        }


        private void CompletionListWindow_TextInserting(object sender, Telerik.Windows.Controls.SyntaxEditor.CompletionListTextInsertingEventArgs e)
        {
            actionInWindow = true;
            string lineText = this.syntaxEditor.Document.CurrentSnapshot.GetLineFromPosition(this.syntaxEditor.CaretPosition.Index).GetText();
            string[] splitted = lineText.Split(new char[] { ' ', '.' });
            string lastWord = splitted.Last().Trim();
            if (string.IsNullOrEmpty(lastWord))
            {
                e.SpanToReplace = Span.FromBounds(e.SpanToReplace.Start + 1, e.SpanToReplace.Start + 1);
            }
            else if (e.TextToInsert.ToLower().StartsWith(lastWord.ToLower()))
            {
                int wordLength = lastWord.Length;
                int spanEnd = e.SpanToReplace.Start;
                e.SpanToReplace = Span.FromBounds(spanEnd - wordLength, spanEnd);
            }

        }

        private void syntaxEditor_DocumentContentChanged(object sender, TextContentChangedEventArgs e)
        {
            var editor = sender as RadSyntaxEditor;
            var startingPosition = new CaretPosition(editor.CaretPosition);
            editor.CaretPosition.MoveToPreviousWord();
            var startOfWord = new CaretPosition(editor.CaretPosition);
            editor.CaretPosition.MoveToNextWord();
            var endOfWord = new CaretPosition(editor.CaretPosition);
            var text = editor.Document.CurrentSnapshot.GetText(new Telerik.Windows.SyntaxEditor.Core.Text.Span(startOfWord.Index, endOfWord.Index - startOfWord.Index));

            if (syntaxEditor.CaretPosition.IsAtWordStart() && !string.IsNullOrWhiteSpace(text) && fullCompletionListItems.Any(t => t.Text.StartsWith(text, StringComparison.InvariantCultureIgnoreCase)))
            {
                var matches = fullCompletionListItems.Where(t => t.Text.StartsWith(text, StringComparison.InvariantCultureIgnoreCase)).Select(t => new CompletionInfo(t.Text));
                var list = new CompletionInfoCollection();
                foreach (var item in matches)
                {
                    list.Add(item);
                }

                list.Add(new CompletionInfo(" ", " "));

                var completionListWindow = editor.IntelliPrompts.CompletionListWindow;

                completionListWindow.Presenter.CompletionListItems = list;


                Dispatcher.BeginInvoke(new Action(() =>
                {
                    var listBox = completionListWindow.Presenter.ChildrenOfType<RadListBox>().First();
                    listBox.SelectedItem = list.FirstOrDefault();
                }));

                if (!completionListWindow.Presenter.IsLoaded)
                {
                    editor.CompleteCode();
                }
                else
                {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    _ = completionListWindow.GetType().BaseType.GetMethod("SetStartEndPosition",
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(completionListWindow,
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                        new object[] { new CaretPosition(editor.CaretPosition), new CaretPosition(editor.CaretPosition) });
                }

            }

            editor.CaretPosition.MoveToPosition(startingPosition);
        }

        private string GetWordOnCaret()
        {
            var editor = this.syntaxEditor;
            var doc = editor.Document.CurrentSnapshot;
            var startingPosition = new CaretPosition(editor.CaretPosition);
            TextSnapshotLine currentLine = doc.Lines.ElementAt(startingPosition.LineNumber);
            string lineText = currentLine.GetText();
            // implement logic that gets the word on caret here

            string trimmedLine = "";
            if (lineText.Contains('.'))
            {
                editor.CaretPosition.MoveToPreviousWord();
                var startOfWord = new CaretPosition(editor.CaretPosition);
                editor.CaretPosition.MoveToNextWord();
                var endOfWord = new CaretPosition(editor.CaretPosition);
                var text = editor.Document.CurrentSnapshot.GetText(new Telerik.Windows.SyntaxEditor.Core.Text.Span(startOfWord.Index, endOfWord.Index - startOfWord.Index));
                periodFound = true;
                string temp = lineText.Trim();
                int index = temp.IndexOf('.');
                obj = temp.Substring(0, index);
                return text;
            }
            else
            {
                trimmedLine = lineText.Trim();
            }

            return trimmedLine;
        }

        private string GetWordOnCaret(Key e)
        {
            var editor = this.syntaxEditor;
            var doc = editor.Document.CurrentSnapshot;
            var startingPosition = new CaretPosition(editor.CaretPosition);
            TextSnapshotLine currentLine = doc.Lines.ElementAt(startingPosition.LineNumber);
            string lineText = currentLine.GetText();

            editor.CaretPosition.MoveToPreviousWord();
            var startOfWord = new CaretPosition(editor.CaretPosition);
            editor.CaretPosition.MoveToNextWord();
            var endOfWord = new CaretPosition(editor.CaretPosition);
            var text = editor.Document.CurrentSnapshot.GetText(new Telerik.Windows.SyntaxEditor.Core.Text.Span(startOfWord.Index, endOfWord.Index - startOfWord.Index));
            return text;

        }
        private List<string> GetAvailableAPI()
        {
            List<string> api = new();

            var types = baseAssembly.GetTypes();
            foreach (var type in types)
            {
                if (type.IsClass)
                {
                    api.Add(type.Name);
                }
            }

            api = GetUnique(api);
            api.Sort();
            return api;
        }
        private List<string> GetAvailableAPI(string typeName)
        {
            List<string> api = new List<string>();
            /**
             * To get BioseroAssemblies to add to current list of assemblies
             */

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                allAssemblies.Add(assembly);
            }


            
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            var types = (from asm in allAssemblies
                         from t in asm.GetTypes()
                         where t.IsClass && t.Name == typeName
                         && t.IsPublic
                         orderby t.Name
                         select t
                        );
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            foreach (var type in types)
            {
                var methods = type.GetMethods().Where(m => m.IsPublic).Select(m => m.Name);
                var properties = type.GetProperties().Select(p => p.Name);
                api.AddRange(methods.Concat(properties).ToList());
            }

            var filtered = GetUnique(api);
            filtered.Sort();
            return filtered;
        }


        private List<string> GetUnique(List<string> api)
        {
            string[] filtered_api = api.Distinct().ToArray();
            return (from item in filtered_api
                    where Char.IsUpper(item[0])
                    select item).ToList();
        }
        private static CompletionInfoCollection ConvertToCompletionInfoCollection(IEnumerable<CompletionInfo> items)
        {
            var result = new CompletionInfoCollection();
            foreach (var item in items)
            {
                result.Add(item);
            }
            return result;
        }

        private void AddSignatures()
        {
            //string docPath = assemblyPath.Substring(0, assemblyPath.LastIndexOf('.')) + ".XML";
            //XmlDocument doc = new XmlDocument();

            //if (File.Exists(docPath))
            //{

            //    doc.Load(docPath);
            //}


            //string path = "M:" + mi.DeclaringType.FullName + "." + mi.Name;

            //XmlNode xmlDocuOfMethod = doc.SelectSingleNode(
            //    "//member[starts-with(@name, '" + path + "')]");
        }


    }
}
