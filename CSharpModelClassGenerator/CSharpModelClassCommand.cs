using System;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace CSharpModelClassGenerator
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class CSharpModelClassCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("3cb56297-957a-43ed-948e-e35da5bbf64c");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="CSharpModelClassCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private CSharpModelClassCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static CSharpModelClassCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in CSharpModelClassCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new CSharpModelClassCommand(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var serviceProvider = this.package as System.IServiceProvider;
            var dte = serviceProvider?.GetService(typeof(DTE)) as DTE2;
            string selectedFolder = GetSelectedFolder(dte);

            if (string.IsNullOrEmpty(selectedFolder))
            {
                VsShellUtilities.ShowMessageBox(
                    this.package, "Seleziona una cartella valida all'interno della soluzione.", "ConvertToProtoCommand",
                    OLEMSGICON.OLEMSGICON_WARNING, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                return;
            }

            CSharpClassParser parser = new CSharpClassParser();
            var classMap = parser.ParseFolder(selectedFolder);

            var selectionWindow = new ClassSelectionWindow(classMap.Keys.ToList());
            if (selectionWindow.ShowDialog() == true)
            {
                string selectedClass = selectionWindow.SelectedClass;
                if (!string.IsNullOrEmpty(selectedClass))
                {
                    var csharpGenerator = new CSharpClassGenerator();
                    string generatedCode = csharpGenerator.GenerateClassCode(classMap, selectedClass);

                    // Ottieni il percorso del file della classe
                    string filePath = Path.Combine(selectedFolder, $"{selectedClass}.cs");

                    // Salva il codice nel file
                    csharpGenerator.SaveClassCodeToFile(filePath, generatedCode);
                }
            }

        }

        private string GetSelectedFolder(DTE2 dte)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            UIHierarchyItem selectedItem = (dte.ToolWindows.SolutionExplorer.SelectedItems as object[])?.FirstOrDefault() as UIHierarchyItem;
            if (selectedItem?.Object is ProjectItem projectItem && projectItem.Kind == EnvDTE.Constants.vsProjectItemKindPhysicalFolder)
            {
                return projectItem.FileNames[1];
            }
            return null;
        }
    }
}
