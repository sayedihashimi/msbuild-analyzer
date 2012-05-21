namespace MsbuildAnalyzer {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    internal class CommandLineOptionsBuilder {
        public CommandLineOptions GetOptions(string[] args) {
            CommandLineOptions options = new CommandLineOptions();

            // get project file path            
            if (args != null && args.Length >=1 && args[0] != null){
                options.ProjectFilePath = args[0];
            }

            return options;
        }
    }

    internal class CommandLineOptions {
        private bool IsValidationCurrent;
        private bool areOptionsValid { get; set; }
        private string projectFilePath{get;set;}

        private string validationErrorMessage;
        public string ValidationErrorMessage {
            get {
                if (!IsValidationCurrent) {
                }
                return validationErrorMessage;
            }
            set { this.projectFilePath = value; }
        }


        public bool AreOptionsValid {
            get {
                if (!this.IsValidationCurrent) {
                    StringBuilder sb = new StringBuilder();

                    if (!string.IsNullOrWhiteSpace(this.ProjectFilePath)) {
                        this.areOptionsValid = true;
                    }
                    else {
                        sb.AppendFormat("ProjectFilePath is null or empty{0}",Environment.NewLine);
                        this.areOptionsValid = false;
                    }

                    this.IsValidationCurrent = true;
                }

                return this.areOptionsValid;
            }
        }

        public string ProjectFilePath {
            get { return this.projectFilePath; }
            set {
                this.projectFilePath = value;
                this.IsValidationCurrent = false;
            }
        }
    }
}
